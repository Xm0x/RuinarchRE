using EZObjectPools;
using Inner_Maps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMarkerNameplate : PooledObject
{
	[SerializeField]
	private RectTransform thisRect;

	[SerializeField]
	private GameObject visualsParent;

	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private GameObject nameBGGO;

	[SerializeField]
	private Image actionIcon;

	[SerializeField]
	private CanvasGroup actionIconCanvasGroup;

	[SerializeField]
	private CanvasGroup nameCanvasGroup;

	[SerializeField]
	private CanvasGroup nameBGCanvasGroup;

	[Header("Thoughts")]
	[SerializeField]
	private GameObject thoughtGO;

	[SerializeField]
	private TextMeshProUGUI thoughtLbl;

	[SerializeField]
	private RectTransform thoughtsRectTransform;

	[SerializeField]
	private ContentSizeFitter contentSizeFitter;

	[Header("Intel Helper")]
	[SerializeField]
	private TextMeshProUGUI intelHelperLbl;

	[SerializeField]
	private GameObject intelHelperGO;

	[SerializeField]
	private GameObject highlightGO;

	private CharacterMarker _parentMarker;

	private const float DefaultSize = 80f;

	public void Initialize(CharacterMarker characterMarker)
	{
		base.name = characterMarker.character.name + " Marker Nameplate";
		_parentMarker = characterMarker;
		UpdateName();
		UpdateSizeBasedOnZoom();
		Messenger.AddListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
		Messenger.AddListener(UISignals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
		Messenger.AddListener<Character>(FactionSignals.FACTION_SET, OnCharacterSetFaction);
	}

	private void OnCharacterSetFaction(Character p_character)
	{
		if (p_character == _parentMarker.character)
		{
			UpdateName();
		}
	}

	private void OnCameraZoomChanged(Camera camera, float amount)
	{
		if (camera == InnerMapCameraMove.Instance.camera)
		{
			UpdateSizeBasedOnZoom();
		}
	}

	private void Update()
	{
		if (thoughtGO.activeSelf)
		{
			UpdateThoughtText();
		}
	}

	private void LateUpdate()
	{
		Vector3 position = InnerMapCameraMove.Instance.camera.WorldToScreenPoint(_parentMarker.transform.position);
		position.z = 0f;
		if (_parentMarker.character != null && _parentMarker.character.grave != null)
		{
			position.y += 15f;
		}
		base.transform.position = position;
	}

	public void UpdateName()
	{
		string characterStringIcon = _parentMarker.character.visuals.GetCharacterStringIcon();
		string firstNameWithColor = _parentMarker.character.firstNameWithColor;
		nameLbl.text = characterStringIcon + firstNameWithColor;
	}

	public override void Reset()
	{
		base.Reset();
		HideThoughts();
		HideIntelHelper();
		SetHighlighterState(state: false);
		_parentMarker = null;
		Messenger.RemoveListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
		Messenger.RemoveListener(UISignals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
		Messenger.RemoveListener<Character>(FactionSignals.FACTION_SET, OnCharacterSetFaction);
	}

	public void UpdateNameActiveState()
	{
		if (_parentMarker != null && _parentMarker.character != null && CharacterManager.Instance != null && InnerMapManager.Instance != null)
		{
			SetNameActiveState(CharacterManager.Instance.toggleCharacterMarkerName || (_parentMarker != null && _parentMarker.character != null && (_parentMarker.character.isStoredAsTarget || InnerMapManager.Instance.IsPOIConsideredTheCurrentHoveredPOI(_parentMarker.character))));
		}
	}

	public void SetNameActiveState(bool state)
	{
		nameCanvasGroup.alpha = (state ? 1f : 0f);
		nameBGCanvasGroup.alpha = (state ? 1f : 0f);
	}

	private void SetGameObjectActiveState(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void SetVisualsState(bool state)
	{
		visualsParent.gameObject.SetActive(state);
	}

	private void UpdateSizeBasedOnZoom()
	{
		float num = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
		float num2 = _parentMarker.character.visuals.selectableSize.y * 100f;
		num2 = ((_parentMarker.character.grave != null) ? (80f - num * 4f) : ((!(_parentMarker.character is Dragon)) ? (num2 - 4f * num) : (num2 - 12f * num)));
		float num3 = num2;
		thisRect.sizeDelta = new Vector2(num3, num3);
	}

	public void UpdateElementsStateBasedOnActiveCharacter()
	{
		Character currentlySelectedCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
		if (UIManager.Instance.gameObject.activeSelf)
		{
			if (currentlySelectedCharacter == _parentMarker.character)
			{
				ShowThoughts();
			}
			else
			{
				HideThoughts();
			}
		}
		else if (currentlySelectedCharacter == _parentMarker.character)
		{
			ShowThoughts();
		}
		else
		{
			HideThoughts();
		}
	}

	public void UpdateActionIcon()
	{
		if (_parentMarker == null)
		{
			return;
		}
		Character character = _parentMarker.character;
		if (character == null)
		{
			return;
		}
		if (character.isDead)
		{
			SetActionIconState(state: false);
			return;
		}
		if (character.isConversing && !character.combatComponent.isInCombat)
		{
			actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Social_Icon];
			SetActionIconState(state: true);
			return;
		}
		if (character.interruptComponent.isInterrupted)
		{
			if (character.interruptComponent.currentInterrupt.interrupt.interruptIconString != GoapActionStateDB.No_Icon)
			{
				actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.currentInterrupt.interrupt.interruptIconString];
				SetActionIconState(state: true);
			}
			else
			{
				SetActionIconState(state: false);
			}
			return;
		}
		if (character.interruptComponent.hasTriggeredSimultaneousInterrupt)
		{
			if (character.interruptComponent.triggeredSimultaneousInterrupt.interrupt.interruptIconString != GoapActionStateDB.No_Icon)
			{
				actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.triggeredSimultaneousInterrupt.interrupt.interruptIconString];
				SetActionIconState(state: true);
			}
			else
			{
				SetActionIconState(state: false);
			}
			return;
		}
		SetActionIconState(state: false);
		if (character.currentActionNode != null)
		{
			string actionIconString = character.currentActionNode.action.GetActionIconString(character.currentActionNode);
			if (actionIconString != GoapActionStateDB.No_Icon)
			{
				actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[actionIconString];
				SetActionIconState(state: true);
			}
			else
			{
				SetActionIconState(state: false);
			}
		}
		else if (_parentMarker.hasFleePath)
		{
			actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Flee_Icon];
			SetActionIconState(state: true);
		}
		else if (character.combatComponent.isInActualCombat)
		{
			SetActionIconState(state: false);
		}
		else if (character.stateComponent.currentState != null)
		{
			string actionIconString2 = character.stateComponent.currentState.actionIconString;
			if (actionIconString2 != GoapActionStateDB.No_Icon)
			{
				actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[actionIconString2];
				SetActionIconState(state: true);
			}
			else
			{
				SetActionIconState(state: false);
			}
		}
		else
		{
			SetActionIconState(state: false);
		}
	}

	private void SetActionIconState(bool state)
	{
		actionIconCanvasGroup.alpha = (state ? 1f : 0f);
	}

	public void ShowThoughts()
	{
		thoughtGO.SetActive(value: true);
		UpdateThoughtText();
	}

	public void HideThoughts()
	{
		thoughtGO.SetActive(value: false);
		thoughtLbl.text = string.Empty;
	}

	private void UpdateThoughtText()
	{
		string thoughtBubble = _parentMarker.character.visuals.GetThoughtBubble();
		if (!thoughtLbl.text.Equals(thoughtBubble))
		{
			thoughtLbl.text = thoughtBubble;
			LayoutRebuilder.ForceRebuildLayoutImmediate(thoughtsRectTransform);
		}
	}

	public void ShowIntelHelper(string text)
	{
		intelHelperLbl.text = text;
		intelHelperGO.SetActive(value: true);
	}

	public void HideIntelHelper()
	{
		intelHelperGO.SetActive(value: false);
	}

	public void SetHighlighterState(bool state)
	{
		highlightGO.SetActive(state);
	}
}
