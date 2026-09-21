using System;
using EZObjectPools;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPortrait : PooledObject, IPointerClickHandler, IEventSystemHandler
{
	private Character _character;

	private PortraitSettings _portraitSettings;

	private Sprite _classPortraitSprite;

	private SUMMON_TYPE _summonType;

	public bool ignoreInteractions;

	[Header("BG")]
	[SerializeField]
	private Image baseBG;

	[SerializeField]
	private TextMeshProUGUI lvlTxt;

	[SerializeField]
	private GameObject lvlGO;

	[Header("Face")]
	[SerializeField]
	private Image faceImage;

	[Header("Name")]
	[SerializeField]
	private GameObject goName;

	[SerializeField]
	private TextMeshProUGUI lblName;

	[Header("Other")]
	[SerializeField]
	private FactionEmblem factionEmblem;

	[SerializeField]
	private GameObject hoverObj;

	[SerializeField]
	private GameObject leaderIcon;

	[SerializeField]
	private GameObject deadIcon;

	private Action _onClickAction;

	private Action<CharacterPortrait> _onHoverOverAction;

	private Action<CharacterPortrait> _onHoverOutAction;

	private bool _isSubscribedToListeners;

	public Character character => _character;

	private void OnEnable()
	{
		Messenger.AddListener(CharacterSignals.CHARACTER_INFO_REVEALED, UpdateLeaderIcon);
		SubscribeListeners();
		UpdateIcons();
	}

	public void GeneratePortrait(PortraitSettings portraitSettings)
	{
		_portraitSettings = portraitSettings;
		UpdatePortrait();
		UpdateIcons();
		UpdateWholeImageTint(portraitSettings.wholeImageColor);
		if (lblName != null)
		{
			lblName.text = string.Empty;
		}
	}

	public void GeneratePortrait(Character character)
	{
		_character = character;
		_portraitSettings = character.visuals.portraitSettings;
		_classPortraitSprite = null;
		UpdatePortrait();
		UpdateIcons();
		UpdateWholeImageTint(_portraitSettings.wholeImageColor);
		if (lblName != null)
		{
			lblName.text = character.name;
		}
	}

	public void GeneratePortrait(SUMMON_TYPE p_monsterType)
	{
		_summonType = p_monsterType;
		string p_className = p_monsterType.ClassnameToUseForPortrait();
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_className);
		_classPortraitSprite = characterClass?.portraitSprite;
		UpdatePortrait();
		UpdateIcons();
		UpdateWholeImageTint(p_monsterType.GetColorTintToUseForPortrait());
		if (lblName != null)
		{
			lblName.text = characterClass?.displayName ?? string.Empty;
		}
	}

	public void GeneratePortrait(MINION_TYPE p_demonType)
	{
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(CharacterManager.Instance.GetMinionSettings(p_demonType).className);
		_classPortraitSprite = characterClass?.portraitSprite;
		UpdatePortrait();
		UpdateIcons();
		UpdateWholeImageTint(Color.white);
		if (lblName != null)
		{
			lblName.text = characterClass?.displayName ?? string.Empty;
		}
	}

	private void UpdatePortrait()
	{
		string classToUseForPortrait = _portraitSettings.GetClassToUseForPortrait();
		if (!string.IsNullOrEmpty(classToUseForPortrait))
		{
			_classPortraitSprite = CharacterManager.Instance.GetCharacterClass(classToUseForPortrait)?.portraitSprite;
		}
		if (_classPortraitSprite != null)
		{
			SetWholeImageSprite(_classPortraitSprite);
		}
		else
		{
			Sprite portraitAsset = CharacterManager.Instance.portraitCollection.GetPortraitAsset(_portraitSettings);
			SetWholeImageSprite(portraitAsset);
		}
		UpdateFactionEmblem();
		lvlGO.SetActive(value: false);
	}

	private void UpdateWholeImageTint(Color p_color)
	{
		faceImage.color = p_color;
	}

	private void UpdateIcons()
	{
		UpdateDeadIcon();
		UpdateLeaderIcon();
	}

	private void SetWholeImageSprite(Sprite sprite)
	{
		faceImage.sprite = sprite;
	}

	public void SetAsDefaultMinion()
	{
		_character = null;
		SetWholeImageSprite(CharacterManager.Instance.GetCharacterClass("Wrath").portraitSprite);
		lvlGO.SetActive(value: false);
		factionEmblem.SetFaction(PlayerManager.Instance.player.playerFaction);
		leaderIcon.SetActive(value: false);
	}

	private void UpdateFrame()
	{
		if (_character != null)
		{
			PortraitFrame portraitFrame = null;
			portraitFrame = ((!_character.isFactionLeader && !_character.isSettlementRuler) ? CharacterManager.Instance.GetPortraitFrame(CHARACTER_ROLE.SOLDIER) : CharacterManager.Instance.GetPortraitFrame(CHARACTER_ROLE.LEADER));
			baseBG.sprite = portraitFrame.baseBG;
			SetBaseBGState(state: true);
		}
	}

	public void SetBaseBGState(bool state)
	{
		baseBG.gameObject.SetActive(state);
	}

	public void ShowCharacterInfo()
	{
		if (_character != null)
		{
			UIManager.Instance.ShowSmallInfo(_character.name);
		}
	}

	public void HideCharacterInfo()
	{
		if (_character != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	public void SetImageRaycastTargetState(bool state)
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].raycastTarget = state;
		}
	}

	public void AddPointerClickAction(Action p_action)
	{
		_onClickAction = (Action)Delegate.Combine(_onClickAction, p_action);
	}

	public void RemovePointerClickAction(Action p_action)
	{
		_onClickAction = (Action)Delegate.Remove(_onClickAction, p_action);
	}

	public void ClearPointerClickAction()
	{
		_onClickAction = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!ignoreInteractions)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				OnLeftClick();
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				OnRightClick();
			}
		}
	}

	public void OnLeftClick()
	{
		if (_onClickAction != null)
		{
			_onClickAction?.Invoke();
		}
		else
		{
			ShowCharacterMenu();
		}
	}

	private void OnRightClick()
	{
		if (_character != null)
		{
			UIManager.Instance.ShowPlayerActionContextMenu(_character.isLycanthrope ? _character.lycanData.activeForm : _character, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	public void SetHoverHighlightState(bool state)
	{
		hoverObj.SetActive(state);
	}

	private void ShowCharacterMenu()
	{
		if (_character != null)
		{
			UIManager.Instance.ShowCharacterInfo(_character, centerOnCharacter: true);
		}
	}

	public void OnHoverEnter()
	{
		SetHoverHighlightState(state: true);
		if (_character != null && _character.hasMarker)
		{
			CharacterPointerUI.Instance.SetTargetTransform(_character.marker.transform);
		}
		_onHoverOverAction?.Invoke(this);
	}

	public void OnHoverExit()
	{
		SetHoverHighlightState(state: false);
		CharacterPointerUI.Instance.SetTargetTransform(null);
		_onHoverOutAction?.Invoke(this);
	}

	public void OnHoverEnterSuccessor()
	{
		if (!ignoreInteractions)
		{
			SetHoverHighlightState(state: true);
			if (character != null && character.faction != null)
			{
				int totalWeightsOfSuccessors = character.faction.successionComponent.GetTotalWeightsOfSuccessors();
				string info = ((float)character.faction.successionComponent.GetWeightOfSuccessor(character) / (float)totalWeightsOfSuccessors * 100f).ToString("N1") + "% " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Successor_Chance");
				UIManager.Instance.ShowSmallInfo(info, character.visuals.GetCharacterNameWithIconAndColor(), autoReplaceText: false);
			}
		}
	}

	public void OnHoverExitSuccessor()
	{
		if (!ignoreInteractions)
		{
			SetHoverHighlightState(state: false);
			UIManager.Instance.HideSmallInfo();
		}
	}

	public void SetHoverActions(Action<CharacterPortrait> p_onHoverOver, Action<CharacterPortrait> p_onHoverOut)
	{
		_onHoverOverAction = p_onHoverOver;
		_onHoverOutAction = p_onHoverOut;
	}

	public void SetNameState(bool p_state)
	{
		goName?.SetActive(p_state);
	}

	private void SubscribeListeners()
	{
		if (!_isSubscribedToListeners)
		{
			_isSubscribedToListeners = true;
			Messenger.AddListener<Character>(FactionSignals.FACTION_SET, OnFactionSet);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
			Messenger.AddListener<Character>(CharacterSignals.ROLE_CHANGED, OnCharacterChangedRole);
			Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
			Messenger.AddListener<Character, Character>(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
			Messenger.AddListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
			Messenger.AddListener<NPCSettlement, Character>(CharacterSignals.ON_SETTLEMENT_RULER_REMOVED, OnSettlementRulerRemoved);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
			Messenger.AddListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
			Messenger.AddListener<Character>(CharacterSignals.UPDATE_CHARACTER_PORTRAITS, ForceUpdatePortrait);
		}
	}

	private void RemoveListeners()
	{
		_isSubscribedToListeners = false;
		Messenger.RemoveListener<Character>(FactionSignals.FACTION_SET, OnFactionSet);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
		Messenger.RemoveListener<Character>(CharacterSignals.ROLE_CHANGED, OnCharacterChangedRole);
		Messenger.RemoveListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
		Messenger.RemoveListener<Character, Character>(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
		Messenger.RemoveListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
		Messenger.RemoveListener<NPCSettlement, Character>(CharacterSignals.ON_SETTLEMENT_RULER_REMOVED, OnSettlementRulerRemoved);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
		Messenger.RemoveListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
		Messenger.RemoveListener<Character>(CharacterSignals.UPDATE_CHARACTER_PORTRAITS, ForceUpdatePortrait);
	}

	public override void Reset()
	{
		base.Reset();
		leaderIcon.gameObject.SetActive(value: true);
		factionEmblem.gameObject.SetActive(value: false);
		goName?.SetActive(value: false);
		_character = null;
		_onClickAction = null;
		_onHoverOverAction = null;
		_onHoverOutAction = null;
		ignoreInteractions = false;
		_classPortraitSprite = null;
		_summonType = SUMMON_TYPE.None;
		RemoveListeners();
	}

	public void OnFactionSet(Character character)
	{
		if (_character != null && _character == character)
		{
			UpdateFactionEmblem();
		}
	}

	private void UpdateFactionEmblem()
	{
		if (_character != null)
		{
			factionEmblem.SetFaction(_character.faction);
		}
		else
		{
			factionEmblem.gameObject.SetActive(value: false);
		}
	}

	public void SetFactionEmblemState(bool p_state)
	{
		factionEmblem.gameObject.SetActive(p_state);
	}

	private void UpdateDeadIcon()
	{
		deadIcon.SetActive(character != null && character.isDead);
	}

	private void UpdateLeaderIcon()
	{
		if (character != null)
		{
			leaderIcon.SetActive(character.isFactionLeader || character.isSettlementRuler);
		}
		else
		{
			leaderIcon.SetActive(value: false);
		}
	}

	public void SetLeaderIconState(bool p_state)
	{
		leaderIcon.SetActive(p_state);
	}

	public void OnHoverLeaderIcon()
	{
		if (character != null)
		{
			string text = string.Empty;
			if (character.isSettlementRuler)
			{
				text = "<b>" + character.name + "</b> " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Settlement_Ruler_Tooltip") + " <b>" + character.homeSettlement.name + "</b>\n";
			}
			if (character.isFactionLeader)
			{
				text = text + "<b>" + character.name + "</b> " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Faction_Leader_Tooltip") + " <b>" + character.faction.name + "</b>";
			}
			UIManager.Instance.ShowSmallInfo(text);
		}
	}

	public void OnHoverExitLeaderIcon()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnCharacterChangedRace(Character character)
	{
		if (_character != null && _character == character)
		{
			GeneratePortrait(character);
		}
	}

	private void OnCharacterChangedRole(Character character)
	{
		if (_character != null && _character == character)
		{
			GeneratePortrait(character);
		}
	}

	private void OnCharacterSetAsFactionLeader(Character character, ILeader previousLeader)
	{
		if (_character != null && _character == character)
		{
			GeneratePortrait(character);
		}
	}

	private void OnCharacterSetAsSettlementRuler(Character character, Character previousRuler)
	{
		if (_character != null && _character == character)
		{
			GeneratePortrait(character);
		}
	}

	private void OnFactionLeaderRemoved(Faction faction, ILeader newLeader)
	{
		if (_character != null && _character == newLeader)
		{
			UpdateLeaderIcon();
		}
	}

	private void OnSettlementRulerRemoved(NPCSettlement settlement, Character previousLeader)
	{
		if (previousLeader == character)
		{
			UpdateLeaderIcon();
		}
	}

	private void OnCharacterDeath(Character p_character)
	{
		if (p_character == character)
		{
			UpdateDeadIcon();
		}
	}

	private void OnArachnophobiaToggled(bool p_isOn)
	{
		if (_summonType != SUMMON_TYPE.None && _summonType.IsSpiderType())
		{
			GeneratePortrait(_summonType);
		}
		else if (_character != null && _character.race == RACE.SPIDER)
		{
			GeneratePortrait(_character);
		}
	}

	private void ForceUpdatePortrait(Character p_character)
	{
		if (p_character == _character)
		{
			GeneratePortrait(p_character);
		}
	}
}
