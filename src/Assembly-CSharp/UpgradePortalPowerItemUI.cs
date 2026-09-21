using System;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class UpgradePortalPowerItemUI : MonoBehaviour
{
	public Action<UpgradePortalPowerItemUI> onHoverOverItem;

	public Action<UpgradePortalPowerItemUI> onHoverOutItem;

	public Action<UpgradePortalPowerItemUI> onClickItem;

	[SerializeField]
	private Image imgBG;

	[SerializeField]
	private Image imgIcon;

	[SerializeField]
	private HoverHandler hoverHandler;

	[SerializeField]
	private GameObject goHover;

	[SerializeField]
	private RuinarchButton button;

	[SerializeField]
	private GameObject goCover;

	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[Header("Icon Sprites")]
	[SerializeField]
	private Sprite spriteLesserDemonActive;

	[SerializeField]
	private Sprite spriteActionActive;

	[SerializeField]
	private Sprite spriteAfflictionActive;

	[SerializeField]
	private Sprite spriteStructureActive;

	[SerializeField]
	private Sprite spriteSpellActive;

	[SerializeField]
	private Sprite spriteCommonStructureActive;

	[SerializeField]
	private Sprite spriteRandomLesserDemonActive;

	[SerializeField]
	private Sprite spriteUnselectedActive;

	public UIHoverPosition tooltipPosition;

	public UIHoverPosition bottomTooltipPosition;

	public UIHoverPosition topTooltipPosition;

	private PortalUpgradeItem _upgradeItem;

	private PortalUpgradeTier _parentTier;

	public PortalUpgradeItem upgradeItem => _upgradeItem;

	public PortalUpgradeTier parentTier => _parentTier;

	private void Awake()
	{
		hoverHandler.AddOnHoverOverAction(OnHoverOverItem);
		hoverHandler.AddOnHoverOutAction(OnHoverOutItem);
		button.onClick.AddListener(OnClickItem);
	}

	private void OnDestroy()
	{
		hoverHandler.RemoveOnHoverOverAction(OnHoverOverItem);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOutItem);
		button.onClick.RemoveListener(OnClickItem);
		_upgradeItem = null;
		_parentTier = null;
	}

	public void Initialize(PortalUpgradeItem p_item, PortalUpgradeTier p_parentTier, bool p_disabledBG, bool p_disabledIcon)
	{
		_upgradeItem = p_item;
		_parentTier = p_parentTier;
		UpdateIcon(p_disabledIcon);
		nameLbl.text = GetItemName(p_item);
	}

	private void UpdateIcon(bool p_disabled)
	{
		if (_upgradeItem.chosenPowerForUpgrade == PLAYER_SKILL_TYPE.NONE)
		{
			switch (_upgradeItem.portalUpgradeType)
			{
			case Portal_Upgrade_Type.Optional_Other:
				imgIcon.sprite = spriteActionActive;
				break;
			case Portal_Upgrade_Type.Optional_Self:
				imgIcon.sprite = spriteUnselectedActive;
				break;
			case Portal_Upgrade_Type.Common_Structures:
				imgIcon.sprite = spriteCommonStructureActive;
				break;
			case Portal_Upgrade_Type.Lesser_Demon:
				imgIcon.sprite = spriteRandomLesserDemonActive;
				break;
			case Portal_Upgrade_Type.Wildcard:
				imgIcon.sprite = spriteUnselectedActive;
				break;
			default:
				throw new ArgumentOutOfRangeException(_upgradeItem.ToString() ?? "");
			}
		}
		else
		{
			imgIcon.sprite = GetIconForPower(_upgradeItem.chosenPowerForUpgrade);
		}
		goCover.SetActive(p_disabled);
	}

	private Sprite GetIconForPower(PLAYER_SKILL_TYPE p_skillType)
	{
		return PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).skillIcon;
	}

	private void OnHoverOverItem()
	{
		onHoverOverItem?.Invoke(this);
		goHover.SetActive(value: true);
	}

	private void OnHoverOutItem()
	{
		onHoverOutItem?.Invoke(this);
		goHover.SetActive(value: false);
	}

	private void OnClickItem()
	{
		onClickItem?.Invoke(this);
	}

	public UIHoverPosition GetHoverPositionToUse()
	{
		int siblingIndex = base.transform.GetSiblingIndex();
		if (parentTier.level >= 7)
		{
			return topTooltipPosition;
		}
		if (siblingIndex >= 6)
		{
			return bottomTooltipPosition;
		}
		return tooltipPosition;
	}

	private string GetItemName(PortalUpgradeItem p_item)
	{
		if (p_item.chosenPowerForUpgrade != PLAYER_SKILL_TYPE.NONE)
		{
			return PlayerSkillManager.Instance.GetSkillData(p_item.chosenPowerForUpgrade).localizedName;
		}
		PlayerSkillLoadout selectedLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowers_Table", p_item.portalUpgradeType.ToStringEnum() + "_Title");
		string text;
		switch (selectedLoadout.archetype)
		{
		case PLAYER_ARCHETYPE.Progression_Puppet_Master:
		case PLAYER_ARCHETYPE.Attainment_Puppet_Master:
		case PLAYER_ARCHETYPE.Eradication_Puppet_Master:
			text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "The_Puppetmaster");
			break;
		case PLAYER_ARCHETYPE.Progression_Ravager:
		case PLAYER_ARCHETYPE.Attainment_Ravager:
		case PLAYER_ARCHETYPE.Eradication_Ravager:
			text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "The_Ravager");
			break;
		case PLAYER_ARCHETYPE.Progression_Lich:
		case PLAYER_ARCHETYPE.Attainment_Lich:
		case PLAYER_ARCHETYPE.Eradication_Lich:
			text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "The_Lich");
			break;
		default:
			text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "The_" + selectedLoadout.archetype.ToStringEnum());
			break;
		}
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)"))
		{
			text = text.Replace("The", "").Trim();
		}
		log.AddToFillers(null, text, LOG_IDENTIFIER.STRING_2);
		switch (p_item.portalUpgradeType)
		{
		case Portal_Upgrade_Type.Optional_Other:
			log.AddToFillers(null, p_item.otherArchetypeTier.ToString(), LOG_IDENTIFIER.STRING_1);
			break;
		case Portal_Upgrade_Type.Optional_Self:
		case Portal_Upgrade_Type.Wildcard:
			log.AddToFillers(null, p_item.tier.ToString(), LOG_IDENTIFIER.STRING_1);
			break;
		}
		return log.logText;
	}
}
