using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Skill Loadout", menuName = "Scriptable Objects/Player Skill Loadout")]
public class PlayerSkillLoadout : ScriptableObject
{
	public const int MAX_UPGRADES_PER_UPGRADE_TIER = 8;

	public static readonly PLAYER_SKILL_TYPE[] All_Common_Structures = new PLAYER_SKILL_TYPE[7]
	{
		PLAYER_SKILL_TYPE.KENNEL,
		PLAYER_SKILL_TYPE.CRYPT,
		PLAYER_SKILL_TYPE.TORTURE_CHAMBERS,
		PLAYER_SKILL_TYPE.IMP_HUT,
		PLAYER_SKILL_TYPE.WATCHER,
		PLAYER_SKILL_TYPE.MANA_PIT,
		PLAYER_SKILL_TYPE.MARAUD
	};

	public PLAYER_ARCHETYPE archetype;

	public PlayerSkillLoadoutData spells;

	public PlayerSkillLoadoutData afflictions;

	public PlayerSkillLoadoutData minions;

	public PlayerSkillLoadoutData structures;

	public PlayerSkillLoadoutData miscs;

	public PASSIVE_SKILL[] passiveSkills;

	public PLAYER_SKILL_TYPE[] availableSpells;

	public PLAYER_SKILL_TYPE[] availableAfflictions;

	public PLAYER_SKILL_TYPE[] availableMinions;

	public PLAYER_SKILL_TYPE[] availableStructures;

	public PLAYER_SKILL_TYPE[] availableMiscs;

	[Header("Portal Upgrades")]
	public PortalUpgradeTier[] portalUpgradeTiers;

	public PortalUpgradeTier GetUpgradeForTier(int p_tier)
	{
		if (portalUpgradeTiers.IsIndexInArray(p_tier - 1))
		{
			return portalUpgradeTiers[p_tier - 1];
		}
		throw new Exception("No upgrade tier for " + p_tier + " in " + base.name);
	}

	public void PopulateValidSkillsForWildcard(List<PLAYER_SKILL_TYPE> p_skills, int p_currentPortalLevel, List<PLAYER_SKILL_TYPE> p_chosenWildcardPowers)
	{
		for (int i = 0; i < portalUpgradeTiers.Length; i++)
		{
			PortalUpgradeTier portalUpgradeTier = portalUpgradeTiers[i];
			if (i + 1 < p_currentPortalLevel)
			{
				portalUpgradeTier.PopulateUnlearnedSkillsForWildcard(p_skills, p_chosenWildcardPowers);
			}
		}
	}

	public void ResetDataBeforeGameStart()
	{
		for (int i = 0; i < portalUpgradeTiers.Length; i++)
		{
			portalUpgradeTiers[i].ResetDataBeforeGameStart();
		}
	}
}
