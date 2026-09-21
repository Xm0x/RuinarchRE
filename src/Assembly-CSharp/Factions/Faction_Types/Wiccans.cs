using Inner_Maps.Location_Structures;

namespace Factions.Faction_Types;

public class Wiccans : CultFaction
{
	public override RESOURCE mainResource => RESOURCE.WOOD;

	public override RELIGION cultReligion => RELIGION.Nature_Worship;

	public Wiccans()
		: base(FACTION_TYPE.Wiccans)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public Wiccans(SaveDataFactionType saveData)
		: base(saveData, FACTION_TYPE.Wiccans)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public override void SetAsDefault(Faction p_faction)
	{
		Warmonger ideology = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
		AddIdeology(ideology, p_faction);
		NatureWorship ideology2 = FactionManager.Instance.CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
		AddIdeology(ideology2, p_faction);
		Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
		exclusive.SetRequirement(RELIGION.Nature_Worship);
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
		AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Heinous);
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
		case CRIME_TYPE.Divine_Worship:
		case CRIME_TYPE.Vampire:
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
		if (p_settlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.WOOD))
		{
			return new StructureSetting(structureType, RESOURCE.WOOD);
		}
		return new StructureSetting(structureType, RESOURCE.STONE);
	}

	public override int GetAdditionalMigrationMeterGain(NPCSettlement p_settlement)
	{
		return GetMigrationMeterGainBasedOnUnoccupiedDwellings(p_settlement);
	}

	public override void RecruitProcess(Character p_actor, Character p_target, Character p_factionLeader, out JobQueueItem p_producedJob)
	{
		if (p_target.traitContainer.HasTrait("Devout") && p_target.traitContainer.IsReligiousCultist(out var p_religion) && p_religion != RELIGION.Demon_Worship)
		{
			KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			return;
		}
		switch (p_target.raceSetting.category)
		{
		case CHARACTER_CATEGORY.Beast:
		case CHARACTER_CATEGORY.Humanoid:
			if (ChanceData.RollChance(CHANCE_TYPE.Usually_Recruits))
			{
				p_actor.jobComponent.TriggerRecruitJob(p_target, out p_producedJob);
			}
			else
			{
				KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			}
			break;
		case CHARACTER_CATEGORY.Demonic:
		case CHARACTER_CATEGORY.Undead:
			KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			break;
		default:
			KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			break;
		}
	}
}
