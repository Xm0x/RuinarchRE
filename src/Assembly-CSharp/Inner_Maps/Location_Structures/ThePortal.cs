using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class ThePortal : DemonicStructure
{
	public int level { get; private set; }

	public PortalUpgradeTier currentTier => PlayerSkillManager.Instance.GetSelectedLoadout().portalUpgradeTiers[level - 1];

	public PortalUpgradeTier nextTier => PlayerSkillManager.Instance.GetSelectedLoadout().portalUpgradeTiers[level];

	public override Type serializedData => typeof(SaveDataThePortal);

	public ThePortal(Region location)
		: base(STRUCTURE_TYPE.THE_PORTAL, location)
	{
		base.name = LocalizationManager.Instance.GetLocalizedValue("Structures_Table", base.structureType.ToStringEnum());
		SetMaxHPAndReset(13666);
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled)
		{
			level = 7;
		}
		else
		{
			level = WorldSettings.Instance.worldSettingsData.playerSkillSettings.startingPortalLevel;
		}
	}

	public ThePortal(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
		if (data is SaveDataThePortal saveDataThePortal)
		{
			level = saveDataThePortal.level;
		}
		else
		{
			level = 1;
		}
	}

	protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false, bool shouldBeCleanedUp = true)
	{
		base.DestroyStructure(p_responsibleCharacter, isPlayerSource, shouldBeCleanedUp: false);
		PlayerUI.Instance.LoseGameOver(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Lose_Game_Default"));
		if (shouldBeCleanedUp)
		{
			MarkForCleanup();
		}
	}

	public override string GetTestingInfo()
	{
		string testingInfo = base.GetTestingInfo();
		testingInfo = testingInfo + "\nLevel: " + level;
		testingInfo += "\nFaction Quests:\n";
		for (int i = 0; i < PlayerManager.Instance.player.playerFaction.partyQuestBoard.availablePartyQuests.Count; i++)
		{
			PartyQuest partyQuest = PlayerManager.Instance.player.playerFaction.partyQuestBoard.availablePartyQuests[i];
			testingInfo = testingInfo + "\n" + partyQuest.GetPartyQuestName() + " - " + partyQuest.assignedParty?.name;
		}
		return testingInfo;
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		position.y += 0.1f;
		worldPosition = position;
	}

	public void GainPowersFromStartingLevel()
	{
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled)
		{
			return;
		}
		if (level > 1)
		{
			PlayerSkillLoadout selectedLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
			for (int i = 2; i <= level; i++)
			{
				PortalUpgradeTier p_parentTier = selectedLoadout.portalUpgradeTiers[i - 1];
				List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
				PortalUpgradeItem[] array = PlayerManager.Instance.player.playerSkillComponent.portalUpgradeItems[i];
				foreach (PortalUpgradeItem portalUpgradeItem in array)
				{
					portalUpgradeItem.PreselectOptionalPowers(p_parentTier, i, list);
					if (portalUpgradeItem.portalUpgradeType == Portal_Upgrade_Type.Essential)
					{
						GainPowerFromPortalUpgrade(portalUpgradeItem.essentialSkillToUnlock);
						continue;
					}
					PLAYER_SKILL_TYPE randomElement = CollectionUtilities.GetRandomElement(portalUpgradeItem.powerChoices);
					portalUpgradeItem.SetChosenPowerForUpgrade(randomElement);
					GainPowerFromPortalUpgrade(randomElement);
				}
				RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
			}
		}
		if (!IsMaxLevel())
		{
			PlayerManager.Instance.player.playerSkillComponent.PreselectOptionalPowersInNextUpgradeTier(this);
		}
	}

	public bool IsMaxLevel()
	{
		return level >= PlayerManager.Instance.player.playerSkillComponent.portalUpgradeItems.Last().Key;
	}

	public void IncreaseLevel()
	{
		level++;
	}

	public void GainEssentialPowers(PortalUpgradeTier p_tier, List<PLAYER_SKILL_TYPE> p_gainedPowers)
	{
		PortalUpgradeItem[] array = PlayerManager.Instance.player.playerSkillComponent.portalUpgradeItems[p_tier.level];
		foreach (PortalUpgradeItem portalUpgradeItem in array)
		{
			if (portalUpgradeItem.portalUpgradeType == Portal_Upgrade_Type.Essential)
			{
				p_gainedPowers.Add(portalUpgradeItem.essentialSkillToUnlock);
				GainPowerFromPortalUpgrade(portalUpgradeItem.essentialSkillToUnlock);
			}
		}
	}

	public void GainPowerFromPortalUpgrade(PLAYER_SKILL_TYPE p_skillType, bool p_addChargesIfLearned = true)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
		if (skillData.isInUse)
		{
			if (p_addChargesIfLearned)
			{
				skillData.AdjustMaxCharges(scriptableObjPlayerSkillData.unlockChargeOnPortalUpgrade);
				skillData.AdjustCharges(scriptableObjPlayerSkillData.unlockChargeOnPortalUpgrade);
			}
		}
		else
		{
			PlayerManager.Instance.player.playerSkillComponent.AddAndCategorizePlayerSkill(p_skillType);
		}
	}

	public void PayForUpgrade(PortalUpgradeTier p_tier)
	{
		for (int i = 0; i < p_tier.upgradeCost.Length; i++)
		{
			PlayerManager.Instance.player.currenciesComponent.ReduceCurrency(p_tier.upgradeCost[i]);
		}
	}
}
