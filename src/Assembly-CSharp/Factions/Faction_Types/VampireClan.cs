using Inner_Maps.Location_Structures;

namespace Factions.Faction_Types;

public class VampireClan : FactionType
{
	public override RESOURCE mainResource => RESOURCE.STONE;

	public override bool usesBothWoodAndStoneResources => true;

	public VampireClan()
		: base(FACTION_TYPE.Vampire_Clan)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Popularity);
	}

	public VampireClan(SaveDataFactionType saveData)
		: base(FACTION_TYPE.Vampire_Clan, saveData)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Popularity);
	}

	public override void SetAsDefault(Faction p_faction)
	{
		ReveresVampires ideology = FactionManager.Instance.CreateIdeology<ReveresVampires>(FACTION_IDEOLOGY.Reveres_Vampires);
		AddIdeology(ideology, p_faction);
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
			CRIME_TYPE.Cannibalism => CRIME_SEVERITY.Heinous, 
			CRIME_TYPE.Werewolf => CRIME_SEVERITY.Heinous, 
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
		CHARACTER_CATEGORY category = p_target.raceSetting.category;
		p_producedJob = null;
		if (category == CHARACTER_CATEGORY.Undead)
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Usually_Recruits))
			{
				p_actor.jobComponent.TriggerRecruitJob(p_target, out p_producedJob);
			}
			else
			{
				KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			}
			return;
		}
		if (p_target.traitContainer.HasTrait("Vampire"))
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Usually_Recruits))
			{
				p_actor.jobComponent.TriggerRecruitJob(p_target, out p_producedJob);
			}
			else
			{
				KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			}
			return;
		}
		switch (category)
		{
		case CHARACTER_CATEGORY.Humanoid:
			if (GetAlreadyKeptPrisonersCount(p_actor) >= 2)
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

	private int GetAlreadyKeptPrisonersCount(Character p_actor)
	{
		if (p_actor.homeSettlement != null && p_actor.homeSettlement.prison != null)
		{
			int num = 0;
			for (int i = 0; i < p_actor.homeSettlement.prison.charactersHere.Count; i++)
			{
				Character character = p_actor.homeSettlement.prison.charactersHere[i];
				if (character.behaviourComponent.CanCharacterBeRecruitedBy(p_actor) && !character.traitContainer.HasTrait("Vampire"))
				{
					num++;
				}
			}
			return num;
		}
		return 0;
	}
}
