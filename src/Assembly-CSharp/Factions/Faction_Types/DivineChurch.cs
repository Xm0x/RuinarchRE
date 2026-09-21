using Inner_Maps.Location_Structures;

namespace Factions.Faction_Types;

public class DivineChurch : CultFaction
{
	public override RESOURCE mainResource => RESOURCE.STONE;

	public override RELIGION cultReligion => RELIGION.Divine_Worship;

	public DivineChurch()
		: base(FACTION_TYPE.Divine_Church)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public DivineChurch(SaveDataFactionType saveData)
		: base(saveData, FACTION_TYPE.Divine_Church)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public override void SetAsDefault(Faction p_faction)
	{
		Peaceful ideology = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
		AddIdeology(ideology, p_faction);
		DivineWorship ideology2 = FactionManager.Instance.CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
		AddIdeology(ideology2, p_faction);
		Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
		exclusive.SetRequirement(RELIGION.Divine_Worship);
		AddIdeology(exclusive, p_faction);
		AddCombatantClass("Archer");
		AddCombatantClass("Hunter");
		AddCombatantClass("Druid");
		AddCombatantClass("Shaman");
		AddCivilianClass("Miner");
		AddCivilianClass("Crafter");
		AddCivilianClass("Farmer");
		AddCivilianClass("Fisher");
		AddCivilianClass("Logger");
		AddCivilianClass("Merchant");
		AddCivilianClass("Butcher");
		AddCivilianClass("Skinner");
		base.hasCrimes = true;
		AddCrime(CRIME_TYPE.Infidelity, CRIME_SEVERITY.Infraction);
		AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Disturbances, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Trespassing, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
		AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Serious);
		AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
	}

	public override void SetFixedData()
	{
		AddCombatantClass("Archer");
		AddCombatantClass("Hunter");
		AddCombatantClass("Druid");
		AddCombatantClass("Shaman");
		AddCivilianClass("Miner");
		AddCivilianClass("Crafter");
		AddCivilianClass("Farmer");
		AddCivilianClass("Fisher");
		AddCivilianClass("Logger");
		AddCivilianClass("Merchant");
		AddCivilianClass("Butcher");
		AddCivilianClass("Skinner");
	}

	public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType)
	{
		switch (crimeType)
		{
		case CRIME_TYPE.Infidelity:
			return CRIME_SEVERITY.Infraction;
		case CRIME_TYPE.Disturbances:
		case CRIME_TYPE.Theft:
		case CRIME_TYPE.Assault:
		case CRIME_TYPE.Arson:
		case CRIME_TYPE.Trespassing:
			return CRIME_SEVERITY.Misdemeanor;
		case CRIME_TYPE.Murder:
		case CRIME_TYPE.Cannibalism:
			return CRIME_SEVERITY.Serious;
		case CRIME_TYPE.Demon_Worship:
		case CRIME_TYPE.Nature_Worship:
		case CRIME_TYPE.Vampire:
		case CRIME_TYPE.Werewolf:
			return CRIME_SEVERITY.Heinous;
		default:
			return CRIME_SEVERITY.None;
		}
	}

	public override StructureSetting ProcessStructureSetting(StructureSetting p_setting, NPCSettlement p_settlement)
	{
		if (p_settlement.settlementJobTriggerComponent.HasAccessToResource(p_setting.resource))
		{
			return p_setting;
		}
		RESOURCE resource = ((p_setting.resource != RESOURCE.WOOD) ? RESOURCE.WOOD : RESOURCE.STONE);
		return new StructureSetting(p_setting.structureType, resource);
	}

	public override StructureSetting CreateStructureSettingForStructure(STRUCTURE_TYPE structureType, NPCSettlement p_settlement)
	{
		if (!structureType.RequiresResourceToBuild(RESOURCE.NONE))
		{
			return new StructureSetting(structureType, RESOURCE.NONE);
		}
		if (structureType == STRUCTURE_TYPE.VAMPIRE_CASTLE)
		{
			return new StructureSetting(structureType, RESOURCE.STONE);
		}
		if (p_settlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.STONE))
		{
			return new StructureSetting(structureType, RESOURCE.STONE);
		}
		return new StructureSetting(structureType, RESOURCE.WOOD);
	}

	public override int GetAdditionalMigrationMeterGain(NPCSettlement p_settlement)
	{
		return GetMigrationMeterGainBasedOnUnoccupiedDwellings(p_settlement);
	}

	public override void RecruitProcess(Character p_actor, Character p_target, Character p_factionLeader, out JobQueueItem p_producedJob)
	{
		KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
	}
}
