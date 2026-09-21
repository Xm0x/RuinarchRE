using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Factions.Faction_Types;

public class LycanClan : FactionType
{
	public override RESOURCE mainResource => RESOURCE.WOOD;

	public override bool usesBothWoodAndStoneResources => true;

	public LycanClan()
		: base(FACTION_TYPE.Lycan_Clan)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public LycanClan(SaveDataFactionType saveData)
		: base(FACTION_TYPE.Lycan_Clan, saveData)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public override void SetAsDefault(Faction p_faction)
	{
		Warmonger ideology = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
		AddIdeology(ideology, p_faction);
		ReveresWerewolves ideology2 = FactionManager.Instance.CreateIdeology<ReveresWerewolves>(FACTION_IDEOLOGY.Reveres_Werewolves);
		AddIdeology(ideology2, p_faction);
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
		AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
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
			CRIME_TYPE.Cannibalism => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Murder => CRIME_SEVERITY.Serious, 
			CRIME_TYPE.Vampire => CRIME_SEVERITY.Heinous, 
			_ => CRIME_SEVERITY.None, 
		};
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
		case CHARACTER_CATEGORY.Humanoid:
			if (GameUtilities.RollChance(80))
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUTCHER, INTERACTION_TYPE.BUTCHER, p_target, p_actor);
				goapPlanJob.SetCancelOnDeath(state: false);
				p_producedJob = goapPlanJob;
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
		default:
			KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			break;
		}
	}
}
