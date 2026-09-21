using System;
using System.Collections.Generic;
using Tutorial;
using UnityEngine.Localization;
using UtilityScripts;

namespace Quests.Alerts;

public abstract class GameAlert : IBookmarkable, ISavable, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	protected const string LocalizationTableName = "GameAlertStrings_Table";

	protected string _displayName;

	protected bool _isActive;

	protected string _strGameAlertType;

	private readonly Game_Alert _alertType;

	private bool _isHoveredOver;

	public string persistentID { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	public GameDate expirationDate { get; private set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Game_Alert;

	public virtual Type serializedData => typeof(SaveDataGameAlert);

	public string displayName => _displayName;

	public virtual BOOKMARK_TYPE bookmarkType
	{
		get
		{
			if (!_alertType.IsTutorialTypeAlert())
			{
				return BOOKMARK_TYPE.Text_With_Cancel;
			}
			return BOOKMARK_TYPE.Special;
		}
	}

	public bool isActive => _isActive;

	public string bookmarkName => displayName;

	public Game_Alert alertType => _alertType;

	protected virtual BOOKMARK_CATEGORY bookmarkCategory => BOOKMARK_CATEGORY.Alerts;

	public GameAlert(Game_Alert p_alert)
	{
		persistentID = Utilities.GetNewUniqueID();
		_strGameAlertType = p_alert.ToStringEnum();
		_displayName = GetLocalizedString(_strGameAlertType + "_Title");
		_alertType = p_alert;
		_isActive = false;
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
	}

	public GameAlert(SaveDataGameAlert p_data, Game_Alert p_alert)
	{
		persistentID = p_data.persistentID;
		_displayName = p_data.displayName;
		_isActive = p_data.isActive;
		_alertType = p_alert;
		_strGameAlertType = p_alert.ToStringEnum();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
	}

	public abstract void SetAsSpawned();

	public virtual void SetAsActive()
	{
		_isActive = true;
		TutorialManager.Instance.OnAlertSetAsActive(this);
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(this, bookmarkCategory);
		Messenger.AddListener<string>(ControlsSignals.CONTROL_DEVICE_CHANGED, OnControlDeviceChanged);
		if (_alertType.IsTutorialTypeAlert())
		{
			ScheduleInitialExpiry();
		}
	}

	protected virtual void SetAsCleared()
	{
		_isActive = false;
		TutorialManager.Instance.OnAlertCleared(this);
		Messenger.RemoveListener<string>(ControlsSignals.CONTROL_DEVICE_CHANGED, OnControlDeviceChanged);
	}

	protected virtual void OnControlDeviceChanged(string p_device)
	{
	}

	public virtual void LoadReferences(SaveDataGameAlert data)
	{
		if (_isActive)
		{
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(this, bookmarkCategory);
			TutorialManager.Instance.OnAlertSetAsActive(this);
			if (data.expirationDate.hasValue)
			{
				LoadExpirationDate(data.expirationDate);
			}
		}
		else
		{
			SetAsSpawned();
		}
	}

	public virtual void OnSelectBookmark()
	{
	}

	public virtual void RemoveBookmark()
	{
		if (_alertType.IsTutorialTypeAlert())
		{
			SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(alertType);
		}
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
		SetAsCleared();
	}

	public virtual void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		_isHoveredOver = true;
	}

	public virtual void OnHoverOutBookmarkItem()
	{
		_isHoveredOver = false;
	}

	public virtual void CleanUp()
	{
		bookmarkEventDispatcher.ClearAll();
	}

	protected string GetLocalizedString(string p_key)
	{
		return LocalizationManager.Instance.GetLocalizedValue("GameAlertStrings_Table", p_key);
	}

	protected string GetLocalizedString(string p_key, Dictionary<string, string> p_args)
	{
		return LocalizationManager.Instance.GetLocalizedValue("GameAlertStrings_Table", p_key, p_args);
	}

	public virtual void OnLocaleChanged(Locale p_newLocale)
	{
		_displayName = GetLocalizedString(_strGameAlertType + "_Title");
		bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
	}

	private void ScheduleInitialExpiry()
	{
		expirationDate = GameManager.Instance.Today();
		expirationDate = expirationDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(24));
		SchedulingManager.Instance.AddEntry(expirationDate, TryExpireAlert, this);
	}

	private void LoadExpirationDate(GameDate p_date)
	{
		expirationDate = p_date;
		SchedulingManager.Instance.AddEntry(expirationDate, TryExpireAlert, this);
	}

	private void TryExpireAlert()
	{
		if (_isHoveredOver)
		{
			expirationDate = GameManager.Instance.Today();
			expirationDate = expirationDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(1));
			SchedulingManager.Instance.AddEntry(expirationDate, TryExpireAlert, this);
		}
		else
		{
			RemoveBookmark();
		}
	}

	public override string ToString()
	{
		return _strGameAlertType;
	}
}
