using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataPlayerSkillComponent : SaveData<PlayerSkillComponent>
{
	public List<SaveDataSkillData> skills;

	public List<SaveDataSkillData> removedSkills;

	public PLAYER_SKILL_TYPE currentSpellBeingUnlocked;

	public PLAYER_SKILL_TYPE lastUnlockedSpell;

	public int currentSpellUnlockCost;

	public RuinarchTimer timerUnlockSpell;

	public string lastSpellUnlockSummary;

	public bool isSpellUnlockedBookmarked;

	public RuinarchTimer cooldownReroll;

	public List<PLAYER_SKILL_TYPE> currentSpellChoices;

	public Dictionary<int, PortalUpgradeItem[]> chosenPowersPerUpgradeTier;

	public Cost[] currentPortalUpgradeCost;

	public RuinarchTimer timerUpgradePortal;

	public string lastPortalUpgradeSummary;

	public bool isPreviousPortalUpgradeBookmarked;

	public List<PLAYER_SKILL_TYPE> lockedSkills;

	public int allPowersCooldownMultiplier;

	public string latestCastMovingTileObject;

	public PLAYER_SKILL_TYPE schemeInCooldown;

	public bool isBanditEventActivated;

	public bool isWolfEventActivated;

	public bool isCultLeaderEventActivated;

	public bool isUndeadInvasionEventActivated;

	public bool isRatmenEventActivated;

	public int numOfAliveVagrantsWithSeriousOrHeinousCrime;

	public int numberOfAliveMasterLycan;

	public int numberOfAliveDemonCultists;

	public int numberOfAliveCultLeaders;

	public int retaliationMeter;

	public int numberOfAbandonedVillages;

	public int numberOfAlivePlaguedVillagers;

	public override void Save(PlayerSkillComponent component)
	{
		skills = new List<SaveDataSkillData>();
		for (int i = 0; i < component.spells.Count; i++)
		{
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(component.spells[i]);
			SaveDataSkillData saveDataSkillData = new SaveDataSkillData();
			saveDataSkillData.Save(spellData);
			skills.Add(saveDataSkillData);
		}
		for (int j = 0; j < component.afflictions.Count; j++)
		{
			SkillData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(component.afflictions[j]);
			SaveDataSkillData saveDataSkillData2 = new SaveDataSkillData();
			saveDataSkillData2.Save(afflictionData);
			skills.Add(saveDataSkillData2);
		}
		for (int k = 0; k < component.schemes.Count; k++)
		{
			SkillData schemeData = PlayerSkillManager.Instance.GetSchemeData(component.schemes[k]);
			SaveDataSkillData saveDataSkillData3 = new SaveDataSkillData();
			saveDataSkillData3.Save(schemeData);
			skills.Add(saveDataSkillData3);
		}
		for (int l = 0; l < component.raidActions.Count; l++)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(component.raidActions[l]);
			SaveDataSkillData saveDataSkillData4 = new SaveDataSkillData();
			saveDataSkillData4.Save(skillData);
			skills.Add(saveDataSkillData4);
		}
		for (int m = 0; m < component.playerActions.Count; m++)
		{
			PlayerAction playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(component.playerActions[m]);
			SaveDataSkillData saveDataSkillData5 = new SaveDataSkillData();
			saveDataSkillData5.Save(playerActionData);
			skills.Add(saveDataSkillData5);
		}
		for (int n = 0; n < component.demonicStructuresSkills.Count; n++)
		{
			DemonicStructurePlayerSkill demonicStructureSkillData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(component.demonicStructuresSkills[n]);
			SaveDataSkillData saveDataSkillData6 = new SaveDataSkillData();
			saveDataSkillData6.Save(demonicStructureSkillData);
			skills.Add(saveDataSkillData6);
		}
		for (int num = 0; num < component.buildSkills.Count; num++)
		{
			BuildPlayerSkill buildSkillData = PlayerSkillManager.Instance.GetBuildSkillData(component.buildSkills[num]);
			SaveDataSkillData saveDataSkillData7 = new SaveDataSkillData();
			saveDataSkillData7.Save(buildSkillData);
			skills.Add(saveDataSkillData7);
		}
		for (int num2 = 0; num2 < component.minionsSkills.Count; num2++)
		{
			MinionPlayerSkill minionPlayerSkillData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(component.minionsSkills[num2]);
			SaveDataSkillData saveDataSkillData8 = new SaveDataSkillData();
			saveDataSkillData8.Save(minionPlayerSkillData);
			skills.Add(saveDataSkillData8);
		}
		for (int num3 = 0; num3 < component.summonsSkills.Count; num3++)
		{
			SummonPlayerSkill summonPlayerSkillData = PlayerSkillManager.Instance.GetSummonPlayerSkillData(component.summonsSkills[num3]);
			SaveDataSkillData saveDataSkillData9 = new SaveDataSkillData();
			saveDataSkillData9.Save(summonPlayerSkillData);
			skills.Add(saveDataSkillData9);
		}
		removedSkills = new List<SaveDataSkillData>();
		for (int num4 = 0; num4 < component.removedSkills.Count; num4++)
		{
			SkillData skillData2 = PlayerSkillManager.Instance.GetSkillData(component.removedSkills[num4]);
			SaveDataSkillData saveDataSkillData10 = new SaveDataSkillData();
			saveDataSkillData10.Save(skillData2);
			removedSkills.Add(saveDataSkillData10);
		}
		currentSpellBeingUnlocked = component.currentSpellBeingUnlocked;
		lastUnlockedSpell = component.lastUnlockedSpell;
		currentSpellUnlockCost = component.currentSpellUnlockCost;
		lastSpellUnlockSummary = component.lastSpellUnlockSummary;
		isSpellUnlockedBookmarked = component.isSpellUnlockedBookmarked;
		timerUnlockSpell = component.timerUnlockSpell;
		currentSpellChoices = new List<PLAYER_SKILL_TYPE>(component.currentSpellChoices);
		chosenPowersPerUpgradeTier = new Dictionary<int, PortalUpgradeItem[]>(component.portalUpgradeItems);
		currentPortalUpgradeCost = component.currentPortalUpgradeCost;
		timerUpgradePortal = component.timerUpgradePortal;
		lastPortalUpgradeSummary = component.lastPortalUpgradeSummary;
		isPreviousPortalUpgradeBookmarked = component.isPreviousPortalUpgradeBookmarked;
		lockedSkills = new List<PLAYER_SKILL_TYPE>(component.lockedSkills);
		allPowersCooldownMultiplier = component.allPowersCooldownMultiplier;
		schemeInCooldown = component.schemeInCooldown;
		if (component.latestCastMovingTileObject != null)
		{
			latestCastMovingTileObject = component.latestCastMovingTileObject.persistentID;
		}
		for (int num5 = 0; num5 < component.prismEvents.Length; num5++)
		{
			PrismEvent prismEvent = component.prismEvents[num5];
			if (prismEvent is BanditsEvent banditsEvent)
			{
				isBanditEventActivated = banditsEvent.isActivated;
				numOfAliveVagrantsWithSeriousOrHeinousCrime = banditsEvent.numberOfAliveVagrantsWithSeriousOrHeinousCrime;
			}
			else if (prismEvent is WolfClanEvent wolfClanEvent)
			{
				isWolfEventActivated = wolfClanEvent.isActivated;
				numberOfAliveMasterLycan = wolfClanEvent.numberOfAliveMasterLycan;
			}
			else if (prismEvent is CultLeaderEvent cultLeaderEvent)
			{
				isCultLeaderEventActivated = cultLeaderEvent.isActivated;
				numberOfAliveDemonCultists = cultLeaderEvent.numberOfAliveDemonCultists;
				numberOfAliveCultLeaders = cultLeaderEvent.numberOfAliveCultLeaders;
				retaliationMeter = cultLeaderEvent.retaliationMeter;
			}
			else if (prismEvent is RatmenEvent ratmenEvent)
			{
				isRatmenEventActivated = ratmenEvent.isActivated;
				numberOfAbandonedVillages = ratmenEvent.numberOfAbandonedVillages;
				numberOfAlivePlaguedVillagers = ratmenEvent.numberOfAlivePlaguedVillagers;
			}
			else if (prismEvent is UndeadInvasionEvent undeadInvasionEvent)
			{
				isUndeadInvasionEventActivated = undeadInvasionEvent.isActivated;
			}
		}
	}

	public override PlayerSkillComponent Load()
	{
		return new PlayerSkillComponent(this);
	}
}
