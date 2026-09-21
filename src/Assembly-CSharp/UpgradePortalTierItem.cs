using System;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class UpgradePortalTierItem : MonoBehaviour
{
	[SerializeField]
	private Image imgLevelUpBG;

	[SerializeField]
	private TextMeshProUGUI lblLevelUnlocked;

	[SerializeField]
	private TextMeshProUGUI lblLevelLocked;

	[SerializeField]
	private TextMeshProUGUI lblLevelUpCost;

	[SerializeField]
	private HoverHandler hoverHandlerLevelUp;

	[SerializeField]
	private UpgradePortalPowerItemUI[] upgradePortalPowerItems;

	[SerializeField]
	private GameObject goLevelUnlocked;

	[SerializeField]
	private GameObject goLevelLocked;

	[SerializeField]
	private RuinarchButton btnLevelUp;

	[SerializeField]
	private GameObject goHorizontalBarLevelUp;

	[SerializeField]
	private GameObject goHorizontalBarPowers;

	[SerializeField]
	private GameObject goPowerContainer;

	[SerializeField]
	private GameObject goAwakenRuinarch;

	[Header("BG Sprites")]
	[SerializeField]
	private Sprite spriteUnlocked;

	[SerializeField]
	private Sprite spriteCanBeUpgraded;

	[SerializeField]
	private Sprite spriteCanBeUpgradedHover;

	[SerializeField]
	private Sprite spriteLocked;

	private int _level;

	private PortalUpgradeTier _portalUpgradeTier;

	private ThePortal _portal;

	private Action<UpgradePortalTierItem> _onClickLevelUpTier;

	public PortalUpgradeTier portalUpgradeTier => _portalUpgradeTier;

	private void Awake()
	{
		hoverHandlerLevelUp.AddOnHoverOverAction(OnHoverOverLevelUp);
		hoverHandlerLevelUp.AddOnHoverOutAction(OnHoverOutLevelUp);
		for (int i = 0; i < upgradePortalPowerItems.Length; i++)
		{
			UpgradePortalPowerItemUI obj = upgradePortalPowerItems[i];
			obj.onHoverOverItem = (Action<UpgradePortalPowerItemUI>)Delegate.Combine(obj.onHoverOverItem, new Action<UpgradePortalPowerItemUI>(OnHoverOverUpgradePowerItem));
			UpgradePortalPowerItemUI obj2 = upgradePortalPowerItems[i];
			obj2.onHoverOutItem = (Action<UpgradePortalPowerItemUI>)Delegate.Combine(obj2.onHoverOutItem, new Action<UpgradePortalPowerItemUI>(OnHoverOutUpgradePowerItem));
		}
		btnLevelUp.onClick.AddListener(OnClickLevelUp);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyAdjusted);
		Messenger.AddListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
		Messenger.AddListener<PortalUpgradeTier, PortalUpgradeItem>(PlayerSkillSignals.PORTAL_UPGRADE_ITEM_CHOSEN, OnPortalUpgradeItemChosen);
	}

	private void OnDestroy()
	{
		_portal = null;
		_portalUpgradeTier = null;
		_onClickLevelUpTier = null;
		hoverHandlerLevelUp.RemoveOnHoverOverAction(OnHoverOverLevelUp);
		hoverHandlerLevelUp.RemoveOnHoverOutAction(OnHoverOutLevelUp);
		for (int i = 0; i < upgradePortalPowerItems.Length; i++)
		{
			UpgradePortalPowerItemUI obj = upgradePortalPowerItems[i];
			obj.onHoverOverItem = (Action<UpgradePortalPowerItemUI>)Delegate.Remove(obj.onHoverOverItem, new Action<UpgradePortalPowerItemUI>(OnHoverOverUpgradePowerItem));
			UpgradePortalPowerItemUI obj2 = upgradePortalPowerItems[i];
			obj2.onHoverOutItem = (Action<UpgradePortalPowerItemUI>)Delegate.Remove(obj2.onHoverOutItem, new Action<UpgradePortalPowerItemUI>(OnHoverOutUpgradePowerItem));
		}
		btnLevelUp.onClick.RemoveListener(OnClickLevelUp);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyAdjusted);
		Messenger.RemoveListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
		Messenger.RemoveListener<PortalUpgradeTier, PortalUpgradeItem>(PlayerSkillSignals.PORTAL_UPGRADE_ITEM_CHOSEN, OnPortalUpgradeItemChosen);
	}

	private void OnSpiritEnergyAdjusted(int p_amountGained, int p_currentSpiritEnergy)
	{
		UpdateLevelUpBtnInteractable();
	}

	private void OnPlayerCancelledPortalUpgrade()
	{
		UpdateLevelUpBtnInteractable();
	}

	private void OnPortalUpgradeItemChosen(PortalUpgradeTier p_tier, PortalUpgradeItem p_item)
	{
		if (p_tier == _portalUpgradeTier)
		{
			LoadUpgradePowers();
		}
	}

	public void Initialize(int p_level, PortalUpgradeTier p_tier)
	{
		_level = p_level;
		_portalUpgradeTier = p_tier;
		lblLevelUnlocked.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Level") + " " + _level;
		lblLevelLocked.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Level") + " " + _level;
		lblLevelUpCost.text = p_tier.GetUpgradeCostString();
		_portal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		bool flag = _portal.level >= _level;
		goLevelLocked.SetActive(!flag);
		goLevelUnlocked.SetActive(flag);
		UpdateLevelUpBG(_portal.level);
		LoadUpgradePowers();
		UpdateLevelUpBtnInteractable();
		goHorizontalBarLevelUp.SetActive(value: true);
		goPowerContainer.SetActive(!p_tier.isForProgressionWin);
		goAwakenRuinarch.SetActive(p_tier.isForProgressionWin);
	}

	private void UpdateLevelUpBG(int p_portalLevel)
	{
		if (p_portalLevel >= _level)
		{
			imgLevelUpBG.sprite = spriteUnlocked;
		}
		else if (!btnLevelUp.interactable)
		{
			imgLevelUpBG.sprite = spriteLocked;
		}
		else if (_level == p_portalLevel + 1)
		{
			imgLevelUpBG.sprite = spriteCanBeUpgraded;
		}
		else
		{
			imgLevelUpBG.sprite = spriteLocked;
		}
	}

	private void UpdateLevelUpBtnInteractable()
	{
		if (_portalUpgradeTier != null)
		{
			bool flag = PlayerManager.Instance.player.currenciesComponent.CanAfford(_portalUpgradeTier.upgradeCost);
			bool flag2 = PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.hasStarted && !PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.IsFinished();
			bool flag3 = _portal.level + 1 == _level;
			btnLevelUp.interactable = flag && flag3 && !flag2;
			UpdateLevelUpBG(_portal.level);
		}
	}

	private void OnClickLevelUp()
	{
		if (btnLevelUp.interactable)
		{
			_onClickLevelUpTier?.Invoke(this);
		}
	}

	public void SetLevelUpAction(Action<UpgradePortalTierItem> p_levelUpAction)
	{
		_onClickLevelUpTier = p_levelUpAction;
	}

	private void LoadUpgradePowers()
	{
		PortalUpgradeItem[] array = PlayerManager.Instance.player.playerSkillComponent.portalUpgradeItems[_portalUpgradeTier.level];
		for (int i = 0; i < upgradePortalPowerItems.Length; i++)
		{
			UpgradePortalPowerItemUI upgradePortalPowerItemUI = upgradePortalPowerItems[i];
			PortalUpgradeItem portalUpgradeItem = array.ElementAtOrDefault(i);
			if (portalUpgradeItem == null)
			{
				upgradePortalPowerItemUI.gameObject.SetActive(value: false);
				continue;
			}
			upgradePortalPowerItemUI.gameObject.SetActive(value: true);
			upgradePortalPowerItemUI.Initialize(portalUpgradeItem, _portalUpgradeTier, _level > _portal.level, _level > _portal.level);
		}
		goHorizontalBarPowers.SetActive(array.Length != 0);
	}

	private void OnHoverOverLevelUp()
	{
		if (_portal != null && _level == _portal.level + 1 && btnLevelUp.interactable)
		{
			imgLevelUpBG.sprite = spriteCanBeUpgradedHover;
		}
	}

	private void OnHoverOutLevelUp()
	{
		if (_portal != null && _level == _portal.level + 1 && btnLevelUp.interactable)
		{
			imgLevelUpBG.sprite = spriteCanBeUpgraded;
		}
	}

	private void OnHoverOverUpgradePowerItem(UpgradePortalPowerItemUI p_item)
	{
		if (!(PlayerManager.Instance != null))
		{
			return;
		}
		if (p_item.upgradeItem.chosenPowerForUpgrade != PLAYER_SKILL_TYPE.NONE)
		{
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_item.upgradeItem.chosenPowerForUpgrade);
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(scriptableObjPlayerSkillData.skill);
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(skillData.localizedName, skillData.localizedDescription, p_item.GetHoverPositionToUse());
			return;
		}
		PlayerSkillLoadout selectedLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowers_Table", p_item.upgradeItem.portalUpgradeType.ToStringEnum() + "_Title");
		Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowers_Table", p_item.upgradeItem.portalUpgradeType.ToStringEnum() + "_Description");
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
		log2.AddToFillers(null, text, LOG_IDENTIFIER.STRING_2);
		switch (p_item.upgradeItem.portalUpgradeType)
		{
		case Portal_Upgrade_Type.Optional_Other:
			log2.AddToFillers(null, p_item.upgradeItem.otherArchetypeTier.ToString(), LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(null, p_item.upgradeItem.otherArchetypeTier.ToString(), LOG_IDENTIFIER.STRING_1);
			break;
		case Portal_Upgrade_Type.Optional_Self:
		case Portal_Upgrade_Type.Wildcard:
			log2.AddToFillers(null, p_item.upgradeItem.tier.ToString(), LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(null, p_item.upgradeItem.tier.ToString(), LOG_IDENTIFIER.STRING_1);
			break;
		}
		string logText = log2.logText;
		if (p_item.parentTier.level == _portal.level + 1 || p_item.parentTier.level <= _portal.level || p_item.upgradeItem.portalUpgradeType == Portal_Upgrade_Type.Optional_Self || p_item.upgradeItem.portalUpgradeType == Portal_Upgrade_Type.Common_Structures)
		{
			Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowers_Table", "Portal_Upgrade_Reveal_Choices");
			string text2 = string.Empty;
			PLAYER_SKILL_TYPE[] array = ((p_item.upgradeItem.portalUpgradeType == Portal_Upgrade_Type.Common_Structures) ? p_item.upgradeItem.commonStructureChoices : p_item.upgradeItem.powerChoices);
			foreach (PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE in array)
			{
				if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
				{
					SkillData skillData2 = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
					if (skillData2 != null)
					{
						text2 = text2 + "\n\t- " + skillData2.localizedName;
					}
				}
			}
			log3.AddToFillers(null, text2, LOG_IDENTIFIER.STRING_1);
			logText = logText + "\n\n" + log3.logText;
		}
		else
		{
			Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowers_Table", "Portal_Upgrade_Hide_Choices");
			log4.AddToFillers(null, (p_item.parentTier.level - 1).ToString(), LOG_IDENTIFIER.STRING_1);
			logText = logText + "\n\n<b>" + log4.logText + "</b>";
		}
		PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(log.logText, logText, p_item.GetHoverPositionToUse(), autoReplaceText: false);
	}

	private void OnHoverOutUpgradePowerItem(UpgradePortalPowerItemUI p_item)
	{
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}
}
