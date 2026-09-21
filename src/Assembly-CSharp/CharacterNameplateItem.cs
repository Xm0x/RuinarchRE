using UnityEngine;
using UnityEngine.UI;

public class CharacterNameplateItem : NameplateItem<Character>
{
	[Header("Character Nameplate Attributes")]
	[SerializeField]
	private CharacterPortrait portrait;

	[SerializeField]
	private GameObject travellingIcon;

	[SerializeField]
	private GameObject arrivedIcon;

	[SerializeField]
	private GameObject restrainedIcon;

	[SerializeField]
	private GameObject leaderIcon;

	[SerializeField]
	private Image raceIcon;

	[SerializeField]
	private HoverHandler hoverHandlerClassName;

	[Space(10f)]
	[Header("Store Target")]
	[SerializeField]
	private StoreTargetButton btnStoreTarget;

	public bool isActive { get; private set; }

	public Character character { get; private set; }

	private void OnEnable()
	{
		Messenger.AddListener(Signals.TICK_ENDED, UpdateAllTextsAndIcon);
		if (character != null)
		{
			UpdateAllTextsAndIcon();
			btnStoreTarget.UpdateInteractableState();
		}
		hoverHandlerClassName.AddOnHoverOverAction(OnHoverOverClassName);
		hoverHandlerClassName.AddOnHoverOutAction(OnHoverOutClassName);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener(Signals.TICK_ENDED, UpdateAllTextsAndIcon);
		hoverHandlerClassName.RemoveOnHoverOverAction(OnHoverOverClassName);
		hoverHandlerClassName.RemoveOnHoverOutAction(OnHoverOutClassName);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	private void OnCharacterChangedName(Character p_character)
	{
		if (character != null && p_character == character && base.gameObject.activeInHierarchy && base.gameObject.activeSelf)
		{
			UpdateMainAndActionText();
		}
	}

	public override void SetObject(Character p_character)
	{
		if (p_character.isInLimbo && p_character.isLycanthrope)
		{
			p_character = p_character.lycanData.activeForm;
		}
		base.SetObject(p_character);
		character = p_character;
		portrait.GeneratePortrait(p_character);
		btnStoreTarget.SetTarget(p_character);
		UpdateAllTextsAndIcon();
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchFromLimbo);
	}

	public override void UpdateObject(Character character)
	{
		base.UpdateObject(character);
		this.character = character;
		portrait.GeneratePortrait(character);
		UpdateAllTextsAndIcon();
	}

	public override void OnHoverEnter()
	{
		portrait.SetHoverHighlightState(state: true);
		base.OnHoverEnter();
	}

	public override void OnHoverExit()
	{
		portrait.SetHoverHighlightState(state: false);
		base.OnHoverExit();
	}

	public override void Reset()
	{
		base.Reset();
		Messenger.RemoveListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchFromLimbo);
		SetPortraitInteractableState(state: true);
		character = null;
	}

	public void SetPosition(UIHoverPosition position)
	{
		UIManager.Instance.PositionTooltip(position, base.gameObject, base.transform as RectTransform);
	}

	public void SetIsActive(bool state)
	{
		isActive = state;
	}

	private void OnCharacterSwitchFromLimbo(Character toLimbo, Character fromLimbo)
	{
		if (toLimbo == character && toLimbo.isLycanthrope)
		{
			UpdateObject(fromLimbo);
		}
	}

	public void SetAsDefaultBehaviour()
	{
		SetAsButton();
		ClearAllOnClickActions();
		AddOnClickAction(delegate(Character character)
		{
			UIManager.Instance.ShowCharacterInfo(character, centerOnCharacter: true);
		});
		SetSupportingLabelState(state: false);
	}

	public void SetPortraitInteractableState(bool state)
	{
		portrait.ignoreInteractions = !state;
	}

	private void UpdateAllTextsAndIcon()
	{
		UpdateMainAndActionText();
		UpdateSubTextAndIcon();
	}

	private void UpdateMainAndActionText()
	{
		mainLbl.text = "<b>" + character.firstNameWithColor + "</b>";
		supportingLbl.text = character.visuals.GetThoughtBubble();
		SetSupportingLabelState(state: true);
	}

	private void UpdateSubTextAndIcon()
	{
		if (!character.isNormalCharacter)
		{
			subLbl.gameObject.SetActive(value: false);
			return;
		}
		CharacterClass characterClass = character.characterClass;
		subLbl.text = characterClass.displayName;
		raceIcon.sprite = character.raceSetting.nameplateIcon;
		raceIcon.gameObject.SetActive(character.raceSetting.nameplateIcon != null);
		subLbl.gameObject.SetActive(value: true);
	}

	private void OnHoverOverClassName()
	{
		if (character != null)
		{
			CharacterClass characterClass = character.characterClass;
			if (!string.IsNullOrEmpty(characterClass.displayDescription))
			{
				UIManager.Instance.ShowSmallInfo(characterClass.displayDescription);
			}
		}
	}

	private void OnHoverOutClassName()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverLeaderIcon()
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

	public void OnHoverExitLeaderIcon()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverRaceIcon()
	{
	}

	public void OnHoverExitRaceIcon()
	{
	}
}
