using System;
using UtilityScripts;

[Serializable]
public class WorldSettingsData
{
	public enum World_Type
	{
		Custom = 2
	}

	public World_Type worldType;

	public VICTORY_CONDITION victoryCondition;

	public PlayerSkillSettings playerSkillSettings;

	public MapSettings mapSettings;

	public VillageSettings villageSettings;

	public FactionSettings factionSettings;

	public WorldSettingsData()
	{
		worldType = World_Type.Custom;
		victoryCondition = VICTORY_CONDITION.Eradication;
		playerSkillSettings = new PlayerSkillSettings();
		mapSettings = new MapSettings();
		villageSettings = new VillageSettings();
		factionSettings = new FactionSettings();
	}

	public void SetWorldType(World_Type type)
	{
		worldType = type;
	}

	public void SetDefaultSettings()
	{
		villageSettings.SetMigrationSpeed(MIGRATION_SPEED.Normal);
		playerSkillSettings.SetCooldownSpeed(SKILL_COOLDOWN_SPEED.Normal);
		playerSkillSettings.SetManaCostAmount(SKILL_COST_AMOUNT.Normal);
		playerSkillSettings.SetChargeAmount(SKILL_CHARGE_AMOUNT.Normal);
		playerSkillSettings.SetRetaliationState(RETALIATION.Normal);
		playerSkillSettings.SetOmnipotentMode(OMNIPOTENT_MODE.Disabled);
		playerSkillSettings.SetCorruptionChargeAmount(CORRUPTION_CHARGE_AMOUNT.Normal);
		playerSkillSettings.SetStartingPortalLevel(1);
	}

	public bool AreSettingsValid(out string invalidityReason)
	{
		if (factionSettings.GetCurrentTotalVillageCountBasedOnFactions() > mapSettings.GetMaxStartingVillages())
		{
			invalidityReason = Utilities.NotNormalizedConversionEnumToString(mapSettings.mapSize.ToString()) + " maps can only have up to " + mapSettings.GetMaxStartingVillages() + " Village/s!";
			return false;
		}
		invalidityReason = string.Empty;
		return true;
	}

	public void ApplyCustomWorldSettings()
	{
		villageSettings.AllowAllFactionMigrations();
		villageSettings.AllowNewVillages();
		factionSettings.AllowNewFactions();
		villageSettings.SetBlessedMigrantsState(p_state: false);
		factionSettings.AllowFactionIdeologyChanges();
		PLAYER_ARCHETYPE[] forcedArchetype = victoryCondition switch
		{
			VICTORY_CONDITION.Progression => new PLAYER_ARCHETYPE[3]
			{
				PLAYER_ARCHETYPE.Progression_Ravager,
				PLAYER_ARCHETYPE.Progression_Lich,
				PLAYER_ARCHETYPE.Progression_Puppet_Master
			}, 
			VICTORY_CONDITION.Attainment => new PLAYER_ARCHETYPE[3]
			{
				PLAYER_ARCHETYPE.Attainment_Ravager,
				PLAYER_ARCHETYPE.Attainment_Lich,
				PLAYER_ARCHETYPE.Attainment_Puppet_Master
			}, 
			VICTORY_CONDITION.Eradication => new PLAYER_ARCHETYPE[3]
			{
				PLAYER_ARCHETYPE.Eradication_Ravager,
				PLAYER_ARCHETYPE.Eradication_Lich,
				PLAYER_ARCHETYPE.Eradication_Puppet_Master
			}, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		playerSkillSettings.SetForcedArchetype(forcedArchetype);
	}

	public bool HasReachedMaxStartingFactionCount()
	{
		return factionSettings.factionTemplates.Count >= mapSettings.GetMaxStartingFactions();
	}

	public bool IsRetaliationAllowed()
	{
		return playerSkillSettings.retaliation != RETALIATION.Disabled;
	}

	public void SetVictoryCondition(VICTORY_CONDITION p_victoryCondition)
	{
		victoryCondition = p_victoryCondition;
	}

	public bool IsSpiritEnergyEnabledBasedOnVictoryCondition()
	{
		return victoryCondition != VICTORY_CONDITION.Eradication;
	}

	public void SetPresetSettingsForVictoryCondition(VICTORY_CONDITION p_victoryCondition)
	{
		switch (p_victoryCondition)
		{
		case VICTORY_CONDITION.Progression:
			playerSkillSettings.SetOmnipotentMode(OMNIPOTENT_MODE.Disabled);
			break;
		case VICTORY_CONDITION.Eradication:
			mapSettings.SetMapSize(MAP_SIZE.Small);
			playerSkillSettings.SetCooldownSpeed(SKILL_COOLDOWN_SPEED.None);
			playerSkillSettings.SetManaCostAmount(SKILL_COST_AMOUNT.None);
			playerSkillSettings.SetChargeAmount(SKILL_CHARGE_AMOUNT.Unlimited);
			playerSkillSettings.SetCorruptionChargeAmount(CORRUPTION_CHARGE_AMOUNT.Unlimited);
			villageSettings.SetMigrationSpeed(MIGRATION_SPEED.Normal);
			playerSkillSettings.SetOmnipotentMode(OMNIPOTENT_MODE.Enabled);
			playerSkillSettings.SetRetaliationState(RETALIATION.Disabled);
			break;
		}
	}
}
