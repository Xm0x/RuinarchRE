using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Object_Pools;
using Quests;
using UnityEngine.Localization;
using UtilityScripts;

public class PlayerSkillComponent : LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	public const int GetBonusChargeCooldownInHours = 6;

	public List<PLAYER_SKILL_TYPE> removedSkills;

	public int allPowersCooldownMultiplier { get; private set; }

	public List<PLAYER_SKILL_TYPE> spells { get; protected set; }

	public List<PLAYER_SKILL_TYPE> afflictions { get; protected set; }

	public List<PLAYER_SKILL_TYPE> schemes { get; protected set; }

	public List<PLAYER_SKILL_TYPE> raidActions { get; protected set; }

	public List<PLAYER_SKILL_TYPE> playerActions { get; protected set; }

	public List<PLAYER_SKILL_TYPE> demonicStructuresSkills { get; protected set; }

	public List<PLAYER_SKILL_TYPE> buildSkills { get; protected set; }

	public List<PLAYER_SKILL_TYPE> minionsSkills { get; protected set; }

	public List<PLAYER_SKILL_TYPE> summonsSkills { get; protected set; }

	public List<PASSIVE_SKILL> passiveSkills { get; protected set; }

	public int chaosOrbExpulsionThreshold { get; private set; }

	public int chaosOrbExpulsionThresholdFromRaid { get; private set; }

	public int tier1Count { get; protected set; }

	public int tier2Count { get; protected set; }

	public int tier3Count { get; protected set; }

	public PLAYER_SKILL_TYPE currentSpellBeingUnlocked { get; private set; }

	public PLAYER_SKILL_TYPE lastUnlockedSpell { get; private set; }

	public int currentSpellUnlockCost { get; private set; }

	public RuinarchTimer timerUnlockSpell { get; private set; }

	public GenericTextBookmarkable spellUnlockedBookmark { get; private set; }

	public string lastSpellUnlockSummary { get; private set; }

	public bool isSpellUnlockedBookmarked { get; private set; }

	public List<PLAYER_SKILL_TYPE> currentSpellChoices { get; private set; }

	public Dictionary<int, PortalUpgradeItem[]> portalUpgradeItems { get; private set; }

	public Cost[] currentPortalUpgradeCost { get; private set; }

	public RuinarchTimer timerUpgradePortal { get; private set; }

	public GenericTextBookmarkable previousPortalUpgradedBookmark { get; private set; }

	public string lastPortalUpgradeSummary { get; private set; }

	public bool isPreviousPortalUpgradeBookmarked { get; private set; }

	public MovingTileObject latestCastMovingTileObject { get; private set; }

	public List<PLAYER_SKILL_TYPE> lockedSkills { get; private set; }

	public PrismEvent[] prismEvents { get; private set; }

	public PLAYER_SKILL_TYPE schemeInCooldown { get; private set; }

	public PlayerSkillComponent()
	{
		spells = new List<PLAYER_SKILL_TYPE>(41);
		afflictions = new List<PLAYER_SKILL_TYPE>(17);
		schemes = new List<PLAYER_SKILL_TYPE>(9);
		raidActions = new List<PLAYER_SKILL_TYPE>(4);
		playerActions = new List<PLAYER_SKILL_TYPE>(63);
		demonicStructuresSkills = new List<PLAYER_SKILL_TYPE>(13);
		buildSkills = new List<PLAYER_SKILL_TYPE>(4);
		minionsSkills = new List<PLAYER_SKILL_TYPE>(7);
		summonsSkills = new List<PLAYER_SKILL_TYPE>(57);
		passiveSkills = new List<PASSIVE_SKILL>(15);
		lockedSkills = new List<PLAYER_SKILL_TYPE>(10);
		removedSkills = new List<PLAYER_SKILL_TYPE>(10);
		allPowersCooldownMultiplier = 1;
		currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
		lastUnlockedSpell = PLAYER_SKILL_TYPE.NONE;
		timerUnlockSpell = new RuinarchTimer("Spell Unlock");
		spellUnlockedBookmark = new GenericTextBookmarkable(GetSpellUnlockedString, () => BOOKMARK_TYPE.Special, OnSelectSpellUnlockedBookmark, RemoveSpellUnlockedBookmark, null, null);
		currentSpellChoices = new List<PLAYER_SKILL_TYPE>();
		portalUpgradeItems = new Dictionary<int, PortalUpgradeItem[]>();
		timerUpgradePortal = new RuinarchTimer("Summon Demon");
		previousPortalUpgradedBookmark = new GenericTextBookmarkable(GetPortalUpgradedSummary, () => BOOKMARK_TYPE.Special, OnSelectPortalUpgradedBookmark, RemovePortalUpgradedBookmark, null, null);
		ConstructPrismEvents();
		timerUnlockSpell.SetOnHoverOverAction(OnHoverOverReleaseAbilitiesBookmark);
		timerUnlockSpell.SetOnHoverOutAction(OnHoverOutReleaseAbilitiesBookmark);
		timerUpgradePortal.SetOnHoverOverAction(OnHoverOverUpgradePortalBookmark);
		timerUpgradePortal.SetOnHoverOutAction(OnHoverOutUpgradePortalBookmark);
		chaosOrbExpulsionThreshold = EditableValuesManager.Instance.defaultChaosOrbExpulsionThreshold;
		chaosOrbExpulsionThresholdFromRaid = EditableValuesManager.Instance.defaultChaosOrbExpulsionThresholdFromRaid;
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, OnBonusChargesAdjusted);
		Messenger.AddListener(PlayerSkillSignals.UPDATE_SKILL_UNLOCK_COSTS, UpdateUnlockCosts);
		Messenger.AddListener(PlayerSkillSignals.UPDATE_SKILL_COOLDOWNS, UpdateCooldowns);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
	}

	public PlayerSkillComponent(SaveDataPlayerSkillComponent p_data)
	{
		spells = new List<PLAYER_SKILL_TYPE>(41);
		afflictions = new List<PLAYER_SKILL_TYPE>(17);
		schemes = new List<PLAYER_SKILL_TYPE>(9);
		raidActions = new List<PLAYER_SKILL_TYPE>(4);
		playerActions = new List<PLAYER_SKILL_TYPE>(63);
		demonicStructuresSkills = new List<PLAYER_SKILL_TYPE>(13);
		buildSkills = new List<PLAYER_SKILL_TYPE>(4);
		minionsSkills = new List<PLAYER_SKILL_TYPE>(7);
		summonsSkills = new List<PLAYER_SKILL_TYPE>(57);
		passiveSkills = new List<PASSIVE_SKILL>(15);
		lockedSkills = new List<PLAYER_SKILL_TYPE>(10);
		removedSkills = new List<PLAYER_SKILL_TYPE>(10);
		allPowersCooldownMultiplier = 1;
		currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
		lastUnlockedSpell = PLAYER_SKILL_TYPE.NONE;
		schemeInCooldown = p_data.schemeInCooldown;
	}

	private void OnStructurePlaced(LocationStructure p_structure)
	{
		if (GameManager.Instance.gameHasStarted && p_structure.structureType == STRUCTURE_TYPE.BIOLAB)
		{
			UnlockPlagueSkills();
		}
	}

	private void OnStructureDestroyed(LocationStructure p_structure)
	{
		if (GameManager.Instance.gameHasStarted && p_structure.structureType == STRUCTURE_TYPE.BIOLAB && !PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB))
		{
			LockPlagueSkills();
		}
	}

	private void OnBonusChargesAdjusted(SkillData p_skillData)
	{
		if (p_skillData.hasBonusCharges)
		{
			AddAndCategorizeTemporaryPlayerSkill(p_skillData);
		}
		else if (!p_skillData.isInUse)
		{
			RemovePlayerSkill(p_skillData);
		}
	}

	private void OnSpellCooldownFinished(SkillData p_skill)
	{
		if (p_skill.type == schemeInCooldown)
		{
			schemeInCooldown = PLAYER_SKILL_TYPE.NONE;
		}
	}

	private void OnSpellCooldownStarted(SkillData p_skill)
	{
		if (schemeInCooldown == PLAYER_SKILL_TYPE.NONE && p_skill is SchemeData)
		{
			schemeInCooldown = p_skill.type;
		}
	}

	private void UnlockPlagueSkills()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PLAGUED_RAT);
		skillData.SetIsUnlockBaseOnRequirements(p_isUnlocked: true);
		AddAndCategorizePlayerSkill(skillData);
		skillData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PLAGUE);
		skillData.SetIsUnlockBaseOnRequirements(p_isUnlocked: true);
		AddAndCategorizePlayerSkill(skillData);
	}

	private void LockPlagueSkills()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PLAGUE);
		skillData.SetIsUnlockBaseOnRequirements(p_isUnlocked: false);
		skillData.ResetDataExceptLevel();
		RemovePlayerSkill(skillData);
		skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PLAGUED_RAT);
		skillData.SetIsUnlockBaseOnRequirements(p_isUnlocked: false);
		RemovePlayerSkill(skillData);
	}

	public void PlayerChoseSkillToAddBonusCharge(SkillData p_skillData, int p_unlockCost)
	{
		currentSpellBeingUnlocked = p_skillData.type;
		currentSpellUnlockCost = p_unlockCost;
		timerUnlockSpell.SetTimerName(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_release_ability") + ": " + p_skillData.localizedName);
		timerUnlockSpell.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(6)), OnCompleteSpellUnlockTimer);
		timerUnlockSpell.SetOnSelectAction(delegate
		{
			UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL));
		});
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockSpell, BOOKMARK_CATEGORY.Portal);
		if (isSpellUnlockedBookmarked)
		{
			RemoveSpellUnlockedBookmark();
		}
		Messenger.Broadcast(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, p_skillData, p_unlockCost);
	}

	public void CancelCurrentPlayerSkillUnlock()
	{
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergyWithoutAffectingSpiritEnergy(currentSpellUnlockCost);
		currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
		currentSpellUnlockCost = 0;
		timerUnlockSpell.Stop();
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockSpell);
		Messenger.Broadcast(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED);
	}

	private void OnCompleteSpellUnlockTimer()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(currentSpellBeingUnlocked);
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(currentSpellBeingUnlocked);
		skillData.AdjustBonusCharges(scriptableObjPlayerSkillData.bonusChargeWhenUnlocked);
		ResetPlayerSpellChoices();
		Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, currentSpellBeingUnlocked, currentSpellUnlockCost);
		ProduceLogForUnlockedSkills(skillData, scriptableObjPlayerSkillData);
		lastUnlockedSpell = currentSpellBeingUnlocked;
		currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
		currentSpellUnlockCost = 0;
		UpdateLastSpellUnlockSummary();
		AddSpellUnlockedBookmark();
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockSpell);
		AkSoundEngine.PostEvent("Play_Unlock_Bonus_Power", InnerMapCameraMove.Instance.gameObject);
	}

	private void UpdateLastSpellUnlockSummary()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(lastUnlockedSpell);
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(lastUnlockedSpell);
		lastSpellUnlockSummary = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Gained") + " " + scriptableObjPlayerSkillData.bonusChargeWhenUnlocked + Utilities.BonusChargesIcon() + " <b>" + skillData.localizedName + "</b>";
		spellUnlockedBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(spellUnlockedBookmark);
	}

	private void ProduceLogForUnlockedSkills(SkillData p_skillData, PlayerSkillData p_playerSkillData)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Skills", "PlayerPowerAlerts_Table", "Unlock Skill skill_unlocked", LOG_TAG.Player);
		log.AddTag(LOG_TAG.Major);
		log.AddToFillers(null, string.Format("{0} {1}", p_playerSkillData.bonusChargeWhenUnlocked, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Charges")), LOG_IDENTIFIER.STRING_1);
		log.AddToFillers(null, p_skillData.localizedName, LOG_IDENTIFIER.STRING_2);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	public void OnRerollUsed()
	{
		ResetPlayerSpellChoices();
	}

	public void PlayerStartedPortalUpgrade(Cost[] p_upgradeCost, PortalUpgradeTier p_upgradeTier)
	{
		currentPortalUpgradeCost = p_upgradeCost;
		ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		timerUpgradePortal.SetTimerName(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_upgrade_portal_active") + " " + (portal.level + 1));
		timerUpgradePortal.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(p_upgradeTier.upgradeTime), OnCompletePortalUpgrade);
		timerUpgradePortal.SetOnSelectAction(delegate
		{
			UIManager.Instance.ShowStructureInfo(portal);
		});
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUpgradePortal, BOOKMARK_CATEGORY.Portal);
		if (isPreviousPortalUpgradeBookmarked)
		{
			RemovePortalUpgradedBookmark();
		}
		Messenger.Broadcast(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE);
	}

	public void CancelPortalUpgrade()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUpgradePortal);
		for (int i = 0; i < currentPortalUpgradeCost.Length; i++)
		{
			PlayerManager.Instance.player.currenciesComponent.AddCurrency(currentPortalUpgradeCost[i]);
		}
		currentPortalUpgradeCost = null;
		timerUpgradePortal.Stop();
		Messenger.Broadcast(PlayerSignals.PORTAL_UPGRADE_CANCELLED);
	}

	public bool IsCurrentlyUpgradingPortal()
	{
		return timerUpgradePortal.hasStarted;
	}

	private void OnCompletePortalUpgrade()
	{
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		PortalUpgradeTier currentTier = thePortal.currentTier;
		thePortal.IncreaseLevel();
		thePortal.GainEssentialPowers(thePortal.currentTier, list);
		PreselectOptionalPowersInNextUpgradeTier(thePortal);
		ProduceLogForPortalUpgrade(thePortal.level);
		PlayerManager.Instance.player.partyStructureDataHandler.UpdateSummonCountForStructureType(STRUCTURE_TYPE.PRISM);
		Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, thePortal.level);
		Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
		currentPortalUpgradeCost = null;
		UpdatePortalUpgradedLog();
		AddPortalUpgradedBookmark();
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUpgradePortal);
		AkSoundEngine.PostEvent("Play_Portal_Upgrade_Complete", InnerMapCameraMove.Instance.gameObject);
		if (list.Count > 0)
		{
			UIManager.Instance.ShowGainedPowersPopup(list, ShowOptionalPowersPopup);
		}
		else
		{
			ShowOptionalPowersPopup();
		}
		RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
		if (!currentTier.isForProgressionWin)
		{
			for (int i = 0; i < buildSkills.Count; i++)
			{
				PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(buildSkills[i]);
				BuildPlayerSkill buildSkillData = PlayerSkillManager.Instance.GetBuildSkillData(buildSkills[i]);
				int modifiedSpellCost = SpellUtilities.GetModifiedSpellCost(scriptableObjPlayerSkillData.unlockChargeOnPortalUpgrade, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetChargeModificationBasedOnSkillType(buildSkillData.type));
				buildSkillData.AdjustMaxCharges(modifiedSpellCost);
				buildSkillData.AdjustCharges(modifiedSpellCost);
			}
		}
	}

	public bool CanPlayerChoosePowerFromOptionalPowers(PLAYER_SKILL_TYPE p_skillType)
	{
		if (PlayerSkillLoadout.All_Common_Structures.Contains(p_skillType))
		{
			return true;
		}
		return !PlayerSkillManager.Instance.GetSkillData(p_skillType).isInUse;
	}

	private void ShowOptionalPowersPopup()
	{
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		if (!portalUpgradeItems.ContainsKey(thePortal.level))
		{
			return;
		}
		Dictionary<PortalUpgradeItem, PortalUpgradeTier> dictionary = MaccimaDictionaryPool<PortalUpgradeItem, PortalUpgradeTier>.Claim();
		PortalUpgradeItem[] array = portalUpgradeItems[thePortal.level];
		foreach (PortalUpgradeItem portalUpgradeItem in array)
		{
			if (portalUpgradeItem.portalUpgradeType == Portal_Upgrade_Type.Essential)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < portalUpgradeItem.powerChoices.Length; j++)
			{
				PLAYER_SKILL_TYPE p_skillType = portalUpgradeItem.powerChoices[j];
				if (CanPlayerChoosePowerFromOptionalPowers(p_skillType))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				dictionary.Add(portalUpgradeItem, thePortal.currentTier);
			}
			else
			{
				portalUpgradeItem.SetChosenPowerForUpgrade(CollectionUtilities.GetRandomElement(portalUpgradeItem.powerChoices));
			}
		}
		if (dictionary.Count > 0)
		{
			UIManager.Instance.ShowChooseSkillsPopup(dictionary);
			MaccimaDictionaryPool<PortalUpgradeItem, PortalUpgradeTier>.Release(dictionary);
		}
	}

	public void UpdateBuildSkillChargesBasedOnCurrentPortalLevel(int p_level)
	{
		for (int i = 0; i < buildSkills.Count; i++)
		{
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(buildSkills[i]);
			BuildPlayerSkill buildSkillData = PlayerSkillManager.Instance.GetBuildSkillData(buildSkills[i]);
			int modifiedSpellCost = SpellUtilities.GetModifiedSpellCost(scriptableObjPlayerSkillData.unlockChargeOnPortalUpgrade, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetChargeModificationBasedOnSkillType(buildSkillData.type));
			int amount = (p_level - 1) * modifiedSpellCost;
			buildSkillData.AdjustMaxCharges(amount);
			buildSkillData.AdjustCharges(amount);
		}
	}

	public void UpdateDefenderSlotsBasedOnCurrentPortalLevel(int p_level)
	{
		int num = p_level - 1;
		for (int i = 0; i < num; i++)
		{
			PlayerManager.Instance.player.partyStructureDataHandler.UpdateSummonCountForStructureType(STRUCTURE_TYPE.PRISM);
		}
	}

	private void UpdatePortalUpgradedLog()
	{
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "portal_upgrade_done", LOG_TAG.Player);
		log.AddToFillers(null, thePortal.level.ToString(), LOG_IDENTIFIER.STRING_1);
		lastPortalUpgradeSummary = log.logText;
		LogPool.Release(log);
		previousPortalUpgradedBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(previousPortalUpgradedBookmark);
	}

	private bool HasUnclaimedPowers()
	{
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		for (int i = 1; i <= thePortal.level; i++)
		{
			if (!portalUpgradeItems.ContainsKey(i))
			{
				continue;
			}
			PortalUpgradeItem[] array = portalUpgradeItems[i];
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].chosenPowerForUpgrade == PLAYER_SKILL_TYPE.NONE)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PreselectOptionalPowersInNextUpgradeTier(ThePortal portal)
	{
		int key = portal.level + 1;
		if (portalUpgradeItems.ContainsKey(key))
		{
			PortalUpgradeItem[] array = portalUpgradeItems[key];
			List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].PreselectOptionalPowers(portal.nextTier, portal.level, list);
			}
			RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
		}
	}

	private void ProduceLogForPortalUpgrade(int level)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "player_portal_upgraded", LOG_TAG.Player);
		log.AddTag(LOG_TAG.Major);
		log.AddToFillers(null, "Level " + level, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private void ResetPlayerSpellChoices()
	{
		currentSpellChoices.Clear();
	}

	public void AddCurrentPlayerSpellChoice(PLAYER_SKILL_TYPE p_skillType)
	{
		currentSpellChoices.Add(p_skillType);
	}

	private void OnHoverOverReleaseAbilitiesBookmark(UIHoverPosition position)
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Will_Finish_On") + " " + timerUnlockSpell.GetTimerEndString() + ".";
		UIManager.Instance.ShowSmallInfo(info, position);
	}

	private void OnHoverOutReleaseAbilitiesBookmark()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverUpgradePortalBookmark(UIHoverPosition position)
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Will_Finish_On") + " " + timerUpgradePortal.GetTimerEndString() + ".";
		UIManager.Instance.ShowSmallInfo(info, position);
	}

	private void OnHoverOutUpgradePortalBookmark()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void SetChosenPowerForUpgradeItem(PortalUpgradeTier p_tier, PortalUpgradeItem p_upgradeItem, PLAYER_SKILL_TYPE p_skillType)
	{
		if (portalUpgradeItems.ContainsKey(p_tier.level))
		{
			p_upgradeItem.SetChosenPowerForUpgrade(p_skillType);
			if (!HasUnclaimedPowers())
			{
				RemovePortalUpgradedBookmark();
			}
			Messenger.Broadcast(PlayerSkillSignals.PORTAL_UPGRADE_ITEM_CHOSEN, p_tier, p_upgradeItem);
		}
	}

	public void LoadPlayerSkillTreeOrLoadout(SaveDataPlayer save)
	{
		PlayerSkillLoadout selectedLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
		if (PlayerSkillManager.Instance.unlockAllSkills)
		{
			PopulateDevModeSkills();
			PopulatePassiveSkills(selectedLoadout.passiveSkills);
		}
		else
		{
			PopulateAllSkills(selectedLoadout.spells.fixedSkills);
			PopulateAllSkills(selectedLoadout.afflictions.fixedSkills);
			PopulateAllSkills(selectedLoadout.minions.fixedSkills);
			PopulateAllSkills(selectedLoadout.structures.fixedSkills);
			PopulateAllSkills(selectedLoadout.miscs.fixedSkills);
			PopulatePassiveSkills(selectedLoadout.passiveSkills);
			LoadoutSaveData loadout = save.GetLoadout(PlayerSkillManager.Instance.selectedArchetype);
			if (loadout != null)
			{
				PopulateAllSkills(loadout.extraSpells);
				PopulateAllSkills(loadout.extraAfflictions);
				PopulateAllSkills(loadout.extraMinions);
				PopulateAllSkills(loadout.extraStructures);
				PopulateAllSkills(loadout.extraMiscs);
			}
			PopulateAllSkills(PlayerSkillManager.Instance.constantSkills);
		}
		for (int i = 0; i < selectedLoadout.portalUpgradeTiers.Length; i++)
		{
			PortalUpgradeTier portalUpgradeTier = selectedLoadout.portalUpgradeTiers[i];
			if (portalUpgradeTier.ShouldTierBeIncludedGivenVictoryCondition(QuestManager.Instance.victoryCondition.type))
			{
				PortalUpgradeItem[] defaultUpgradeItems = portalUpgradeTier.defaultUpgradeItems;
				PortalUpgradeItem[] array = new PortalUpgradeItem[defaultUpgradeItems.Length];
				for (int j = 0; j < defaultUpgradeItems.Length; j++)
				{
					PortalUpgradeItem portalUpgradeItem = new PortalUpgradeItem(defaultUpgradeItems[j], portalUpgradeTier);
					array[j] = portalUpgradeItem;
				}
				portalUpgradeItems.Add(portalUpgradeTier.level, array);
			}
		}
		int numberOfSkillsThatShouldBeLocked = CharacterManager.Instance.GetNumberOfSkillsThatShouldBeLocked();
		for (int k = 0; k < numberOfSkillsThatShouldBeLocked; k++)
		{
			CheckIfSkillShouldBeLocked();
		}
	}

	public void AddTierCount(PlayerSkillData playerSkillData)
	{
		switch (playerSkillData.tier)
		{
		case 1:
			tier1Count++;
			break;
		case 2:
			tier2Count++;
			break;
		case 3:
			tier3Count++;
			break;
		}
	}

	public void RemoveTierCount(PlayerSkillData playerSkillData)
	{
		switch (playerSkillData.tier)
		{
		case 1:
			tier1Count--;
			break;
		case 2:
			tier2Count--;
			break;
		case 3:
			tier3Count--;
			break;
		}
	}

	public int GetLevelOfSkill(SkillData p_targetSkill)
	{
		int currentLevel = 0;
		switch (p_targetSkill.category)
		{
		case PLAYER_SKILL_CATEGORY.PLAYER_ACTION:
			playerActions.ForEach(delegate(PLAYER_SKILL_TYPE eachSkill)
			{
				if (eachSkill == p_targetSkill.type)
				{
					currentLevel = PlayerSkillManager.Instance.GetSkillData(eachSkill).currentLevel;
				}
			});
			break;
		case PLAYER_SKILL_CATEGORY.SPELL:
			spells.ForEach(delegate(PLAYER_SKILL_TYPE eachSkill)
			{
				if (eachSkill == p_targetSkill.type)
				{
					currentLevel = PlayerSkillManager.Instance.GetSkillData(eachSkill).currentLevel;
				}
			});
			break;
		case PLAYER_SKILL_CATEGORY.AFFLICTION:
			afflictions.ForEach(delegate(PLAYER_SKILL_TYPE eachSkill)
			{
				if (eachSkill == p_targetSkill.type)
				{
					currentLevel = PlayerSkillManager.Instance.GetSkillData(eachSkill).currentLevel;
				}
			});
			break;
		}
		return currentLevel;
	}

	public bool CheckIfSkillIsAvailable(PLAYER_SKILL_TYPE p_targetSkill)
	{
		return PlayerSkillManager.Instance.GetSkillData(p_targetSkill).isInUse;
	}

	public bool CanDoSkill(PLAYER_SKILL_TYPE type)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(type);
		if (!skillData.isInUse)
		{
			return skillData.isTemporarilyInUse;
		}
		return true;
	}

	public bool CanBuildDemonicStructure(PLAYER_SKILL_TYPE type)
	{
		DemonicStructurePlayerSkill demonicStructureSkillData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(type);
		if (!demonicStructureSkillData.isInUse)
		{
			return demonicStructureSkillData.isTemporarilyInUse;
		}
		return true;
	}

	public bool HasAnyAvailableAffliction()
	{
		for (int i = 0; i < afflictions.Count; i++)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(afflictions[i]);
			if (!skillData.hasCharges)
			{
				return true;
			}
			if (skillData.charges > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAfflictions()
	{
		return afflictions.Count > 0;
	}

	private void PopulateDevModeSkills()
	{
		foreach (PlayerSkillData value in PlayerSkillManager.Instance.playerSkillDataDictionary.Values)
		{
			if (PlayerSkillManager.Instance.GetSkillData(value.skill) != null && value.skill != PLAYER_SKILL_TYPE.OSTRACIZER && value.skill != PLAYER_SKILL_TYPE.SKELETON && value.skill != PLAYER_SKILL_TYPE.DEFILER)
			{
				AddAndCategorizePlayerSkill(value.skill, testScene: false, isDevMode: true);
			}
		}
	}

	private void PopulateAllSkills(List<PLAYER_SKILL_TYPE> skillTypes)
	{
		if (skillTypes != null)
		{
			for (int i = 0; i < skillTypes.Count; i++)
			{
				PLAYER_SKILL_TYPE p_skillType = skillTypes[i];
				AddAndCategorizePlayerSkill(p_skillType);
			}
		}
	}

	public void AddAndCategorizePlayerSkill(PLAYER_SKILL_TYPE p_skillType, bool testScene = false, bool isDevMode = false)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
		AddAndCategorizePlayerSkill(skillData, testScene, isDevMode);
	}

	public void AddAndCategorizePlayerSkill(SkillData p_skillData, bool testScene = false, bool isDevMode = false)
	{
		if (p_skillData.isTemporarilyInUse)
		{
			p_skillData.SetIsInUse(state: true);
		}
		else if (!p_skillData.isInUse)
		{
			p_skillData.SetIsInUse(state: true);
			SetPlayerSkillData(p_skillData, testScene, isDevMode);
			AddAndCategorizePlayerSkillBase(p_skillData);
			if (GameManager.Instance.gameHasStarted)
			{
				CheckIfSkillShouldBeLocked();
			}
		}
	}

	public void AddAndCategorizeTemporaryPlayerSkill(SkillData p_skillData, bool testScene = false, bool isDevMode = false)
	{
		if (!p_skillData.isTemporarilyInUse && !p_skillData.isInUse)
		{
			p_skillData.SetIsTemporarilyInUse(p_state: true);
			SetPlayerSkillData(p_skillData, testScene, isDevMode);
			AddAndCategorizePlayerSkillBase(p_skillData);
			if (GameManager.Instance.gameHasStarted)
			{
				CheckIfSkillShouldBeLocked();
			}
		}
	}

	public void RemovePlayerSkill(PLAYER_SKILL_TYPE p_skillType)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
		RemovePlayerSkill(skillData);
	}

	public void RemovePlayerSkill(SkillData p_skillData)
	{
		if (p_skillData.hasBonusCharges)
		{
			p_skillData.SetIsInUse(state: false);
			p_skillData.SetIsTemporarilyInUse(p_state: true);
		}
		else
		{
			RemovePlayerSkillBase(p_skillData);
			p_skillData.ResetDataExceptLevel();
		}
	}

	private void AddAndCategorizePlayerSkillBase(SkillData p_skillData)
	{
		if (p_skillData.category == PLAYER_SKILL_CATEGORY.AFFLICTION)
		{
			afflictions.Add(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SCHEME)
		{
			schemes.Add(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.RAID)
		{
			raidActions.Add(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE)
		{
			demonicStructuresSkills.Add(p_skillData.type);
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.BUILD)
		{
			buildSkills.Add(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.MINION)
		{
			minionsSkills.Add(p_skillData.type);
			Messenger.Broadcast(PlayerSkillSignals.ADDED_PLAYER_MINION_SKILL, p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION)
		{
			playerActions.Add(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SPELL)
		{
			spells.Add(p_skillData.type);
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_GAINED_SPELL, p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SUMMON)
		{
			summonsSkills.Add(p_skillData.type);
			Messenger.Broadcast(PlayerSkillSignals.ADDED_PLAYER_SUMMON_SKILL, p_skillData.type);
		}
		RemoveFromRemovedSkills(p_skillData.type);
	}

	private void RemovePlayerSkillBase(SkillData p_skillData)
	{
		if (p_skillData.category == PLAYER_SKILL_CATEGORY.AFFLICTION)
		{
			afflictions.Remove(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SCHEME)
		{
			schemes.Remove(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.RAID)
		{
			raidActions.Remove(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE)
		{
			demonicStructuresSkills.Remove(p_skillData.type);
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_LOST_DEMONIC_STRUCTURE, p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.BUILD)
		{
			buildSkills.Remove(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.MINION)
		{
			minionsSkills.Remove(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION)
		{
			playerActions.Remove(p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SPELL)
		{
			spells.Remove(p_skillData.type);
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_LOST_SPELL, p_skillData.type);
		}
		else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SUMMON)
		{
			summonsSkills.Remove(p_skillData.type);
		}
		if (lockedSkills.Remove(p_skillData.type))
		{
			CheckIfSkillShouldBeLocked();
		}
		AddToRemovedSkills(p_skillData.type);
	}

	private void SetPlayerSkillData(SkillData p_skillData, bool testScene, bool isDevMode)
	{
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillData.type);
		SetPlayerSkillDataBase(p_skillData, scriptableObjPlayerSkillData, testScene, isDevMode);
	}

	private void SetPlayerSkillDataBase(SkillData p_skillData, PlayerSkillData p_playerSkillData, bool testScene, bool isDevMode)
	{
		p_skillData.SetMaxCharges(p_playerSkillData.GetMaxChargesBaseOnLevel(p_skillData.currentLevel));
		p_skillData.SetCharges(p_skillData.maxCharges);
		p_skillData.SetCooldown(p_playerSkillData.GetCoolDownBaseOnLevel(p_skillData.currentLevel));
		p_skillData.SetPierce(PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(p_skillData.type));
		p_skillData.SetUnlockCost(p_playerSkillData.GetUnlockCost());
		p_skillData.SetManaCost(p_playerSkillData.GetManaCostBaseOnLevel(p_skillData.currentLevel));
		p_skillData.SetBaseSpiritEnergyCost(p_playerSkillData.GetSpiritEnergyCostBaseOnLevel(p_skillData.currentLevel));
		p_skillData.SetRemainingChaosOrbs(p_playerSkillData.chaosOrbLimit);
	}

	private void UpdateUnlockCosts()
	{
		UpdateUnlockCosts(spells);
		UpdateUnlockCosts(afflictions);
		UpdateUnlockCosts(schemes);
		UpdateUnlockCosts(playerActions);
	}

	private void UpdateCooldowns()
	{
		UpdateCooldowns(spells);
		UpdateCooldowns(afflictions);
		UpdateCooldowns(schemes);
		UpdateCooldowns(playerActions);
	}

	private void UpdateUnlockCosts(List<PLAYER_SKILL_TYPE> p_spells)
	{
		for (int i = 0; i < p_spells.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = p_spells[i];
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(pLAYER_SKILL_TYPE);
			PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE).SetUnlockCost(scriptableObjPlayerSkillData.GetUnlockCost());
		}
	}

	private void UpdateCooldowns(List<PLAYER_SKILL_TYPE> p_powers)
	{
		for (int i = 0; i < p_powers.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = p_powers[i];
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(pLAYER_SKILL_TYPE);
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
			if (skillData.isInCooldown)
			{
				skillData.SetBaseCooldownOnly(scriptableObjPlayerSkillData.GetCoolDownBaseOnLevel(skillData.currentLevel));
			}
			else
			{
				skillData.SetCooldown(scriptableObjPlayerSkillData.GetCoolDownBaseOnLevel(skillData.currentLevel));
			}
			scriptableObjPlayerSkillData.ResetAllBonusUIText();
			Messenger.Broadcast(PlayerSkillSignals.UPDATE_PLAYER_SKILL, skillData);
		}
	}

	private void AddToRemovedSkills(PLAYER_SKILL_TYPE p_type)
	{
		if (!removedSkills.Contains(p_type))
		{
			removedSkills.Add(p_type);
		}
	}

	private void RemoveFromRemovedSkills(PLAYER_SKILL_TYPE p_type)
	{
		removedSkills.Remove(p_type);
	}

	public int GetNumberOfUpgradeableSpells()
	{
		int num = 0;
		for (int i = 0; i < spells.Count; i++)
		{
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spells[i]).isNonUpgradeable)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfUpgradeableAfflictions()
	{
		int num = 0;
		for (int i = 0; i < afflictions.Count; i++)
		{
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(afflictions[i]).isNonUpgradeable)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfUpgradeableAbilities()
	{
		int num = 0;
		for (int i = 0; i < playerActions.Count; i++)
		{
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerActions[i]).isNonUpgradeable)
			{
				num++;
			}
		}
		return num;
	}

	private void PopulatePassiveSkills(PASSIVE_SKILL[] passiveSkills)
	{
		foreach (PASSIVE_SKILL pASSIVE_SKILL in passiveSkills)
		{
			AddPassiveSkills(pASSIVE_SKILL);
		}
	}

	private void AddPassiveSkills(PASSIVE_SKILL passiveSkills)
	{
		PlayerSkillManager.Instance.GetPassiveSkill(passiveSkills).ActivateSkill();
		this.passiveSkills.Add(passiveSkills);
	}

	public bool AlreadyHasBlackmail(Character p_character)
	{
		return PlayerManager.Instance.player.HasIsImprisonedIntel(p_character);
	}

	private string GetSpellUnlockedString()
	{
		return lastSpellUnlockSummary;
	}

	private void OnSelectSpellUnlockedBookmark()
	{
		UIManager.Instance.ShowPurchaseSkillUI();
	}

	private void RemoveSpellUnlockedBookmark()
	{
		isSpellUnlockedBookmarked = false;
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(spellUnlockedBookmark);
	}

	private void AddSpellUnlockedBookmark()
	{
		isSpellUnlockedBookmarked = true;
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(spellUnlockedBookmark, BOOKMARK_CATEGORY.Portal);
	}

	private string GetPortalUpgradedSummary()
	{
		return lastPortalUpgradeSummary;
	}

	private void OnSelectPortalUpgradedBookmark()
	{
		UIManager.Instance.ShowUpgradePortalUI(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal);
	}

	private void RemovePortalUpgradedBookmark()
	{
		isPreviousPortalUpgradeBookmarked = false;
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(previousPortalUpgradedBookmark);
	}

	private void AddPortalUpgradedBookmark()
	{
		isPreviousPortalUpgradeBookmarked = true;
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(previousPortalUpgradedBookmark, BOOKMARK_CATEGORY.Portal);
	}

	public void OverrideDefaultChaosOrbExpulsionThreshold(int p_threshold)
	{
		chaosOrbExpulsionThreshold = p_threshold;
	}

	public void OverrideDefaultChaosOrbExpulsionThresholdFromRaid(int p_threshold)
	{
		chaosOrbExpulsionThresholdFromRaid = p_threshold;
	}

	private void CheckIfSkillShouldBeLocked()
	{
		if (lockedSkills.Count >= CharacterManager.Instance.GetNumberOfSkillsThatShouldBeLocked())
		{
			return;
		}
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < CharacterManager.Instance.powerLockers.Count; i++)
		{
			Character character = CharacterManager.Instance.powerLockers[i];
			if (character.traitContainer.GetPowerLockerTraitWithNoAssignedPower() != null)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			Character randomElement = CollectionUtilities.GetRandomElement(list);
			randomElement.traitContainer.GetPowerLockerTraitWithNoAssignedPower().LockRandomPlayerSkill(randomElement);
		}
		RuinarchListPool<Character>.Release(list);
	}

	public void LockSkill(PLAYER_SKILL_TYPE p_type, Character p_character)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		skillData.DisableSkill(p_character);
		lockedSkills.Add(p_type);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Skills", "PlayerPowerAlerts_Table", "skill_locked", LOG_TAG.Player, LOG_TAG.Major);
		log.AddToFillers(null, skillData.localizedName, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	public void LockSkillFromNullchild(PLAYER_SKILL_TYPE p_type, Character p_character)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		skillData.DisableSkill(p_character);
		lockedSkills.Add(p_type);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Skills", "PlayerPowerAlerts_Table", "skill_locked_nullchild", LOG_TAG.Player, LOG_TAG.Major);
		log.AddToFillers(null, skillData.localizedName, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	public void UnlockSkill(PLAYER_SKILL_TYPE p_type)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		if (lockedSkills.Remove(p_type))
		{
			skillData.EnableSkill();
		}
	}

	public PLAYER_SKILL_TYPE GetRandomSkillTypeToLock()
	{
		List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		PopulateUsableSkills(spells, list);
		PopulateUsableSkills(afflictions, list);
		PopulateUsableSkills(playerActions, list);
		if (list.Count > 0)
		{
			PLAYER_SKILL_TYPE randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
			return randomElement;
		}
		RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
		return PLAYER_SKILL_TYPE.NONE;
	}

	private void PopulateUsableSkills(List<PLAYER_SKILL_TYPE> p_skillTypes, List<PLAYER_SKILL_TYPE> p_usableSkills)
	{
		for (int i = 0; i < p_skillTypes.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = p_skillTypes[i];
			if (PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE).isUsable && !PlayerSkillManager.Instance.constantSkills.Contains(pLAYER_SKILL_TYPE))
			{
				p_usableSkills.Add(pLAYER_SKILL_TYPE);
			}
		}
	}

	public void Load(SaveDataPlayerSkillComponent data)
	{
		timerUnlockSpell = new RuinarchTimer("Spell Unlock");
		spellUnlockedBookmark = new GenericTextBookmarkable(GetSpellUnlockedString, () => BOOKMARK_TYPE.Special, OnSelectSpellUnlockedBookmark, RemoveSpellUnlockedBookmark, null, null);
		currentSpellChoices = new List<PLAYER_SKILL_TYPE>();
		portalUpgradeItems = new Dictionary<int, PortalUpgradeItem[]>();
		timerUpgradePortal = new RuinarchTimer("Summon Demon");
		previousPortalUpgradedBookmark = new GenericTextBookmarkable(GetPortalUpgradedSummary, () => BOOKMARK_TYPE.Special, OnSelectPortalUpgradedBookmark, RemovePortalUpgradedBookmark, null, null);
		ConstructPrismEvents();
		timerUnlockSpell.SetOnHoverOverAction(OnHoverOverReleaseAbilitiesBookmark);
		timerUnlockSpell.SetOnHoverOutAction(OnHoverOutReleaseAbilitiesBookmark);
		timerUpgradePortal.SetOnHoverOverAction(OnHoverOverUpgradePortalBookmark);
		timerUpgradePortal.SetOnHoverOutAction(OnHoverOutUpgradePortalBookmark);
		chaosOrbExpulsionThreshold = EditableValuesManager.Instance.defaultChaosOrbExpulsionThreshold;
		chaosOrbExpulsionThresholdFromRaid = EditableValuesManager.Instance.defaultChaosOrbExpulsionThresholdFromRaid;
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, OnBonusChargesAdjusted);
		Messenger.AddListener(PlayerSkillSignals.UPDATE_SKILL_UNLOCK_COSTS, UpdateUnlockCosts);
		Messenger.AddListener(PlayerSkillSignals.UPDATE_SKILL_COOLDOWNS, UpdateCooldowns);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		if (data.skills != null)
		{
			for (int num = 0; num < data.skills.Count; num++)
			{
				SkillData p_skillData = data.skills[num].Load();
				AddAndCategorizePlayerSkillBase(p_skillData);
			}
		}
		if (data.removedSkills != null)
		{
			for (int num2 = 0; num2 < data.removedSkills.Count; num2++)
			{
				data.removedSkills[num2].Load();
			}
		}
		for (int num3 = 0; num3 < prismEvents.Length; num3++)
		{
			PrismEvent prismEvent = prismEvents[num3];
			if (prismEvent is BanditsEvent banditsEvent)
			{
				banditsEvent.SetIsActivated(data.isBanditEventActivated);
				banditsEvent.SetNumberOfAliveVagrantsWithSeriousOrHeinousCrime(data.numOfAliveVagrantsWithSeriousOrHeinousCrime);
			}
			else if (prismEvent is WolfClanEvent wolfClanEvent)
			{
				wolfClanEvent.SetIsActivated(data.isWolfEventActivated);
				wolfClanEvent.SetNumberOfAliveMasterLycan(data.numberOfAliveMasterLycan);
			}
			else if (prismEvent is CultLeaderEvent cultLeaderEvent)
			{
				cultLeaderEvent.SetIsActivated(data.isCultLeaderEventActivated);
				cultLeaderEvent.SetNumberOfAliveDemonCultists(data.numberOfAliveDemonCultists);
				cultLeaderEvent.SetNumberOfAliveCultLeaders(data.numberOfAliveCultLeaders);
				cultLeaderEvent.SetRetaliationMeter(data.retaliationMeter);
			}
			else if (prismEvent is RatmenEvent ratmenEvent)
			{
				ratmenEvent.SetIsActivated(data.isRatmenEventActivated);
				ratmenEvent.SetNumberOfAbandonedVillages(data.numberOfAbandonedVillages);
				ratmenEvent.SetNumberOfPlaguedVillagers(data.numberOfAlivePlaguedVillagers);
			}
			else if (prismEvent is UndeadInvasionEvent undeadInvasionEvent)
			{
				undeadInvasionEvent.SetIsActivated(data.isUndeadInvasionEventActivated);
			}
		}
	}

	public void OnLoadSaveData()
	{
		for (int i = 0; i < spells.Count; i++)
		{
			PLAYER_SKILL_TYPE type = spells[i];
			PlayerSkillManager.Instance.GetSpellData(type).OnLoadSpell();
		}
		for (int j = 0; j < demonicStructuresSkills.Count; j++)
		{
			PLAYER_SKILL_TYPE type2 = demonicStructuresSkills[j];
			PlayerSkillManager.Instance.GetDemonicStructureSkillData(type2).OnLoadSpell();
		}
		for (int k = 0; k < buildSkills.Count; k++)
		{
			PLAYER_SKILL_TYPE type3 = buildSkills[k];
			PlayerSkillManager.Instance.GetBuildSkillData(type3).OnLoadSpell();
		}
		for (int l = 0; l < minionsSkills.Count; l++)
		{
			PLAYER_SKILL_TYPE type4 = minionsSkills[l];
			PlayerSkillManager.Instance.GetMinionPlayerSkillData(type4).OnLoadSpell();
		}
		for (int m = 0; m < summonsSkills.Count; m++)
		{
			PLAYER_SKILL_TYPE type5 = summonsSkills[m];
			PlayerSkillManager.Instance.GetSummonPlayerSkillData(type5).OnLoadSpell();
		}
		for (int n = 0; n < playerActions.Count; n++)
		{
			PLAYER_SKILL_TYPE type6 = playerActions[n];
			PlayerSkillManager.Instance.GetPlayerActionData(type6).OnLoadSpell();
		}
		for (int num = 0; num < afflictions.Count; num++)
		{
			PLAYER_SKILL_TYPE type7 = afflictions[num];
			PlayerSkillManager.Instance.GetAfflictionData(type7).OnLoadSpell();
		}
		for (int num2 = 0; num2 < schemes.Count; num2++)
		{
			PLAYER_SKILL_TYPE type8 = schemes[num2];
			PlayerSkillManager.Instance.GetSchemeData(type8).OnLoadSpell();
		}
		PlayerSkillLoadout selectedLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
		PopulatePassiveSkills(selectedLoadout.passiveSkills);
	}

	public void LoadReferencesInMainThread(SaveDataPlayerSkillComponent data)
	{
		Load(data);
		currentSpellBeingUnlocked = data.currentSpellBeingUnlocked;
		lastUnlockedSpell = data.lastUnlockedSpell;
		currentSpellUnlockCost = data.currentSpellUnlockCost;
		timerUnlockSpell = data.timerUnlockSpell;
		timerUnlockSpell.SetOnHoverOverAction(OnHoverOverReleaseAbilitiesBookmark);
		timerUnlockSpell.SetOnHoverOutAction(OnHoverOutReleaseAbilitiesBookmark);
		if (currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE)
		{
			timerUnlockSpell.LoadStart(OnCompleteSpellUnlockTimer);
			timerUnlockSpell.SetOnSelectAction(delegate
			{
				UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL));
			});
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockSpell, BOOKMARK_CATEGORY.Portal);
		}
		lastSpellUnlockSummary = data.lastSpellUnlockSummary;
		isSpellUnlockedBookmarked = data.isSpellUnlockedBookmarked;
		if (isSpellUnlockedBookmarked)
		{
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(spellUnlockedBookmark, BOOKMARK_CATEGORY.Portal);
		}
		currentPortalUpgradeCost = data.currentPortalUpgradeCost;
		timerUpgradePortal = data.timerUpgradePortal;
		timerUpgradePortal.SetOnHoverOverAction(OnHoverOverUpgradePortalBookmark);
		timerUpgradePortal.SetOnHoverOutAction(OnHoverOutUpgradePortalBookmark);
		if (currentPortalUpgradeCost != null)
		{
			timerUpgradePortal.LoadStart(OnCompletePortalUpgrade);
			timerUpgradePortal.SetOnSelectAction(delegate
			{
				UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL));
			});
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUpgradePortal, BOOKMARK_CATEGORY.Portal);
		}
		lastPortalUpgradeSummary = data.lastPortalUpgradeSummary;
		isPreviousPortalUpgradeBookmarked = data.isPreviousPortalUpgradeBookmarked;
		if (isPreviousPortalUpgradeBookmarked)
		{
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(previousPortalUpgradedBookmark, BOOKMARK_CATEGORY.Portal);
		}
		currentSpellChoices = data.currentSpellChoices;
		portalUpgradeItems = data.chosenPowersPerUpgradeTier;
		lockedSkills = data.lockedSkills;
		allPowersCooldownMultiplier = data.allPowersCooldownMultiplier;
		if (!string.IsNullOrEmpty(data.latestCastMovingTileObject))
		{
			latestCastMovingTileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(data.latestCastMovingTileObject) as MovingTileObject;
		}
	}

	public void SetAllPowersCooldownMultiplier(int p_amount)
	{
		allPowersCooldownMultiplier = p_amount;
		Messenger.Broadcast(PlayerSkillSignals.UPDATE_SKILL_COOLDOWNS);
	}

	public void SetLatestCastMovingObject(MovingTileObject p_tileObject)
	{
		if (latestCastMovingTileObject != null)
		{
			latestCastMovingTileObject.OnNoLongerLatestCast();
		}
		latestCastMovingTileObject = p_tileObject;
	}

	public void ClearLatestCastMovingObject()
	{
		latestCastMovingTileObject = null;
	}

	public void OnLocaleChanged(Locale locale)
	{
		if (lastUnlockedSpell != PLAYER_SKILL_TYPE.NONE)
		{
			UpdateLastSpellUnlockSummary();
		}
		if (!string.IsNullOrEmpty(lastPortalUpgradeSummary))
		{
			UpdatePortalUpgradedLog();
		}
	}

	private void ConstructPrismEvents()
	{
		prismEvents = new PrismEvent[5]
		{
			new BanditsEvent(ScriptableObjectsManager.Instance.GetPrismEventData(PRISM_EVENT.Bandits)),
			new WolfClanEvent(ScriptableObjectsManager.Instance.GetPrismEventData(PRISM_EVENT.Wolf_Clan)),
			new CultLeaderEvent(ScriptableObjectsManager.Instance.GetPrismEventData(PRISM_EVENT.Cult_Leader)),
			new UndeadInvasionEvent(ScriptableObjectsManager.Instance.GetPrismEventData(PRISM_EVENT.Undead_Invasion)),
			new RatmenEvent(ScriptableObjectsManager.Instance.GetPrismEventData(PRISM_EVENT.Ratmen))
		};
		UIManager.Instance.InitializePrismEventsUI(prismEvents);
	}

	public T GetPrismEvent<T>() where T : PrismEvent
	{
		for (int i = 0; i < prismEvents.Length; i++)
		{
			PrismEvent prismEvent = prismEvents[i];
			if (prismEvent is T)
			{
				return prismEvent as T;
			}
		}
		return null;
	}
}
