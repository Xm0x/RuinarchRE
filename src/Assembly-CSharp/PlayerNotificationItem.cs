using System;
using System.Collections;
using DG.Tweening;
using EZObjectPools;
using Object_Pools;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PlayerNotificationItem : PooledObject
{
	private string _involvedObjects;

	[SerializeField]
	protected TextMeshProUGUI logLbl;

	[SerializeField]
	private TextMeshProUGUI dateLbl;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private LogsTagButton _logsTagButton;

	[SerializeField]
	private LayoutElement _layoutElement;

	[SerializeField]
	private Image _bg;

	[SerializeField]
	private Sprite _normalSprite;

	[SerializeField]
	private Sprite _importantSprite;

	[SerializeField]
	private EventLabel _logEventLbl;

	[SerializeField]
	private HoverHandler _dateHoverHandler;

	protected UIHoverPosition _hoverPosition;

	private Action<PlayerNotificationItem> onDestroyAction;

	private bool _adjustHeightOnEnable;

	private GameDate _logDate;

	public int tickShown { get; private set; }

	public string fromActionID { get; private set; }

	public string logPersistentID { get; private set; }

	public string currentTextDisplayed => dateLbl.text + " - " + logLbl.text;

	private void Awake()
	{
		_logEventLbl.SetOnRightClickAction(OnRightClickLog);
	}

	private void OnEnable()
	{
		if (_adjustHeightOnEnable)
		{
			StartCoroutine(InstantHeight());
			_adjustHeightOnEnable = false;
		}
	}

	public void Initialize(Log log, Action<PlayerNotificationItem> onDestroyAction = null)
	{
		logPersistentID = log.persistentID;
		tickShown = GameManager.Instance.Today().tick;
		dateLbl.text = log.gameDate.ConvertToTime();
		logLbl.text = log.logText;
		fromActionID = log.actionID;
		_involvedObjects = log.allInvolvedObjectIDs;
		_bg.sprite = (log.IsImportant() ? _importantSprite : _normalSprite);
		_logDate = log.gameDate;
		this.onDestroyAction = onDestroyAction;
		_logsTagButton.SetTags(log.tags);
		_dateHoverHandler.AddOnHoverOverAction(OnHoverOverDateLabel);
		_dateHoverHandler.AddOnHoverOutAction(OnHoverOutDateLabel);
		Messenger.AddListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	public void Initialize(Log log, int tick, Action<PlayerNotificationItem> onDestroyAction = null)
	{
		logPersistentID = log.persistentID;
		tickShown = tick;
		dateLbl.text = log.gameDate.ConvertToTime();
		logLbl.text = log.logText;
		fromActionID = log.actionID;
		_involvedObjects = log.allInvolvedObjectIDs;
		_bg.sprite = (log.IsImportant() ? _importantSprite : _normalSprite);
		_logDate = log.gameDate;
		this.onDestroyAction = onDestroyAction;
		_logsTagButton.SetTags(log.tags);
		_dateHoverHandler.AddOnHoverOverAction(OnHoverOverDateLabel);
		_dateHoverHandler.AddOnHoverOutAction(OnHoverOutDateLabel);
		Messenger.AddListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	public void SetHoverPosition(UIHoverPosition hoverPosition)
	{
		_hoverPosition = hoverPosition;
	}

	public void DoTweenHeight()
	{
		StartCoroutine(TweenHeight());
	}

	public void QueueAdjustHeightOnEnable()
	{
		_adjustHeightOnEnable = true;
	}

	private IEnumerator TweenHeight()
	{
		yield return null;
		_layoutElement.DOPreferredSize(new Vector2(0f, (logLbl.transform as RectTransform).sizeDelta.y), 0.5f);
	}

	protected IEnumerator InstantHeight()
	{
		yield return GameUtilities.waitForEndOfFrame;
		Vector2 sizeDelta = (logLbl.transform as RectTransform).sizeDelta;
		_layoutElement.preferredHeight = sizeDelta.y;
	}

	public void DeleteNotification()
	{
		onDestroyAction?.Invoke(this);
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	public virtual void DeleteOldestNotification()
	{
		DeleteNotification();
	}

	public void TweenIn()
	{
		_container.anchoredPosition = new Vector2(450f, 0f);
		_container.DOAnchorPosX(0f, 0.5f);
	}

	public void OnHoverOverLog(object obj)
	{
		if (obj is Character character && _hoverPosition != null)
		{
			Character character2 = character;
			if (character.isLycanthrope)
			{
				character2 = character.lycanData.activeForm;
			}
			UIManager.Instance.ShowCharacterNameplateTooltip(character2, _hoverPosition);
		}
	}

	public void OnHoverOutLog()
	{
		UIManager.Instance.HideCharacterNameplateTooltip();
	}

	private void OnRightClickLog(object obj)
	{
		IPlayerActionTarget playerActionTarget = obj as IPlayerActionTarget;
		if (playerActionTarget != null)
		{
			if (playerActionTarget is Character { isLycanthrope: not false } character)
			{
				playerActionTarget = character.lycanData.activeForm;
			}
			UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	private void OnLogRemovedFromDatabase(Log log)
	{
		if (log.persistentID == logPersistentID)
		{
			DeleteNotification();
		}
	}

	protected virtual void OnCharacterChangedName(Character character)
	{
		if (character == null || string.IsNullOrEmpty(_involvedObjects) || !_involvedObjects.Contains(character.persistentID))
		{
			return;
		}
		Log fullLogWithPersistentID = DatabaseManager.Instance.mainSQLDatabase.GetFullLogWithPersistentID(logPersistentID);
		if (fullLogWithPersistentID != null)
		{
			fullLogWithPersistentID.TryUpdateLogAfterRename(character);
			logLbl.text = fullLogWithPersistentID.logText;
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(InstantHeight());
			}
		}
		LogPool.Release(fullLogWithPersistentID);
	}

	public override void Reset()
	{
		base.Reset();
		_logsTagButton.Reset();
		_adjustHeightOnEnable = false;
		_container.anchoredPosition = Vector2.zero;
		base.transform.localScale = Vector3.one;
		fromActionID = string.Empty;
		logPersistentID = string.Empty;
		_dateHoverHandler.RemoveOnHoverOverAction(OnHoverOverDateLabel);
		_dateHoverHandler.RemoveOnHoverOutAction(OnHoverOutDateLabel);
		Messenger.RemoveListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	private void OnHoverOverDateLabel()
	{
	}

	private void OnHoverOutDateLabel()
	{
	}
}
