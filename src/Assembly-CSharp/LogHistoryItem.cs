using Ruinarch;
using TMPro;
using UnityEngine;

public class LogHistoryItem : LogItem
{
	[SerializeField]
	private TextMeshProUGUI logLbl;

	[SerializeField]
	private TextMeshProUGUI dateLbl;

	[SerializeField]
	private EventLabel eventLabel;

	[SerializeField]
	private LogsTagButton logsTagButton;

	private UIHoverPosition _hoverPosition;

	private bool _isHovered;

	public void SetLog(Log log)
	{
		base.name = log.persistentID;
		dateLbl.text = log.gameDate.ConvertToTime();
		logLbl.text = log.logText;
		logLbl.Refresh();
		logsTagButton.SetTags(log.tags);
		logsTagButton.logID = log.persistentID;
		eventLabel.SetOnLeftClickAction(OnLeftClickObjectInLog);
		eventLabel.SetOnRightClickAction(OnRightClickObjectInLog);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetHoverPosition(UIHoverPosition hoverPosition)
	{
		_hoverPosition = hoverPosition;
	}

	private void OnLeftClickObjectInLog(object obj)
	{
		IPointOfInterest currentlySelectedPOI = UIManager.Instance.GetCurrentlySelectedPOI();
		if (currentlySelectedPOI != null)
		{
			Messenger.Broadcast(UISignals.LOG_HISTORY_OBJECT_CLICKED, obj, logLbl.text, currentlySelectedPOI);
		}
		UIManager.Instance.OpenObjectUI(obj);
	}

	private void OnRightClickObjectInLog(object obj)
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

	public void OnHoverOverLog(object obj)
	{
		if (obj is Character character && _hoverPosition != null)
		{
			_isHovered = true;
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
		if (_isHovered)
		{
			_isHovered = false;
			UIManager.Instance.HideCharacterNameplateTooltip();
		}
	}

	public void ManualReset()
	{
		base.name = "LogHistoryItem";
		logsTagButton.Reset();
		OnHoverOutLog();
	}

	public void SetLogLblWidth(float p_width)
	{
		logLbl.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, p_width);
	}

	public override void Reset()
	{
		base.Reset();
		ManualReset();
	}
}
