using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class VillageSettings
{
	public List<FACTION_TYPE> disabledFactionMigrations;

	public MIGRATION_SPEED migrationSpeed;

	public bool disableNewVillages;

	public bool blessedMigrants;

	public int divineCultistPointThreshold;

	public int natureCultistPointThreshold;

	public VillageSettings()
	{
		migrationSpeed = MIGRATION_SPEED.Normal;
		disabledFactionMigrations = new List<FACTION_TYPE>();
		divineCultistPointThreshold = ReligionComponent.Religious_Cultist_Belief_Threshold;
		natureCultistPointThreshold = ReligionComponent.Religious_Cultist_Belief_Threshold;
	}

	public void SetMigrationSpeed(MIGRATION_SPEED p_value)
	{
		migrationSpeed = p_value;
	}

	public void AllowVillagerMigrationForFactionType(FACTION_TYPE p_factionType)
	{
		disabledFactionMigrations.Remove(p_factionType);
	}

	public void BlockVillagerMigrationForFactionType(FACTION_TYPE p_factionType)
	{
		disabledFactionMigrations.Add(p_factionType);
	}

	public bool IsMigrationAllowedForFaction(FACTION_TYPE p_factionType)
	{
		return !disabledFactionMigrations.Contains(p_factionType);
	}

	public void AllowAllFactionMigrations()
	{
		disabledFactionMigrations.Clear();
	}

	public void BlockAllFactionMigrations()
	{
		FACTION_TYPE[] enumValues = CollectionUtilities.GetEnumValues<FACTION_TYPE>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			disabledFactionMigrations.Add(enumValues[i]);
		}
	}

	public void AllowNewVillages()
	{
		disableNewVillages = false;
	}

	public void BlockNewVillages()
	{
		disableNewVillages = true;
	}

	public void SetBlessedMigrantsState(bool p_state)
	{
		blessedMigrants = p_state;
	}

	public void RandomizeCultistCreationThresholds()
	{
		divineCultistPointThreshold = (GameUtilities.RollChance(80) ? GameUtilities.RandomBetweenTwoNumbers(60, 120) : GameUtilities.RandomBetweenTwoNumbers(80, 400));
		natureCultistPointThreshold = (GameUtilities.RollChance(80) ? GameUtilities.RandomBetweenTwoNumbers(60, 120) : GameUtilities.RandomBetweenTwoNumbers(80, 400));
	}

	public int GetInitialCultistThresholdForReligion(RELIGION p_religion)
	{
		return p_religion switch
		{
			RELIGION.Divine_Worship => divineCultistPointThreshold, 
			RELIGION.Nature_Worship => natureCultistPointThreshold, 
			_ => ReligionComponent.Religious_Cultist_Belief_Threshold, 
		};
	}
}
