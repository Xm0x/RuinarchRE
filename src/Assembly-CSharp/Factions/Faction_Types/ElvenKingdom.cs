using Inner_Maps.Location_Structures;

namespace Factions.Faction_Types;

public class ElvenKingdom : FactionType
{
	public override RESOURCE mainResource => RESOURCE.WOOD;

	public ElvenKingdom()
		: base(FACTION_TYPE.Elven_Kingdom)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Popularity);
	}

	public ElvenKingdom(SaveDataFactionType saveData)
		: base(FACTION_TYPE.Elven_Kingdom, saveData)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Popularity);
	}

	public override void SetAsDefault(Faction p_faction)
	{
		Peaceful ideology = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
		AddIdeology(ideology, p_faction);
		Inclusive ideology2 = FactionManager.Instance.CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
		AddIdeology(ideology2, p_faction);
		NatureWorship ideology3 = FactionManager.Instance.CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
		AddIdeology(ideology3, p_faction);
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
		return crimeType switch
		{
			CRIME_TYPE.Infidelity => CRIME_SEVERITY.Infraction, 
			CRIME_TYPE.Theft => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Disturbances => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Assault => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Arson => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Trespassing => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Murder => CRIME_SEVERITY.Serious, 
			CRIME_TYPE.Cannibalism => CRIME_SEVERITY.Serious, 
			CRIME_TYPE.Vampire => CRIME_SEVERITY.Heinous, 
			_ => CRIME_SEVERITY.None, 
		};
	}

	public override int GetAdditionalMigrationMeterGain(NPCSettlement p_settlement)
	{
		return GetMigrationMeterGainBasedOnUnoccupiedDwellings(p_settlement);
	}

	public override StructureSetting CreateStructureSettingForStructure(STRUCTURE_TYPE structureType, NPCSettlement p_settlement)
	{
		RESOURCE resource = (structureType.RequiresResourceToBuild(RESOURCE.WOOD) ? RESOURCE.WOOD : RESOURCE.NONE);
		if (structureType == STRUCTURE_TYPE.VAMPIRE_CASTLE)
		{
			return new StructureSetting(structureType, RESOURCE.STONE);
		}
		return new StructureSetting(structureType, resource);
	}

	public override void RecruitProcess(Character p_actor, Character p_target, Character p_factionLeader, out JobQueueItem p_producedJob)
	{
		switch (p_target.raceSetting.category)
		{
		case CHARACTER_CATEGORY.Beast:
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
			if (ChanceData.RollChance(CHANCE_TYPE.Rarely_Recruits) || p_factionLeader.religionComponent.religion == RELIGION.Demon_Worship)
			{
				p_actor.jobComponent.TriggerRecruitJob(p_target, out p_producedJob);
			}
			else
			{
				KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			}
			break;
		case CHARACTER_CATEGORY.Undead:
			if (p_factionLeader.characterClass.className == "Necromancer")
			{
				p_actor.jobComponent.TriggerRecruitJob(p_target, out p_producedJob);
			}
			else
			{
				KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			}
			break;
		case CHARACTER_CATEGORY.Humanoid:
			KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			break;
		default:
			KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			break;
		}
	}
}
