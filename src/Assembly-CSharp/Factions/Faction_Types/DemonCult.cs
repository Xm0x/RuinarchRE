using Inner_Maps.Location_Structures;

namespace Factions.Faction_Types;

public class DemonCult : CultFaction
{
	public override RESOURCE mainResource => RESOURCE.STONE;

	public override bool usesCorruptedStructures => true;

	public override RELIGION cultReligion => RELIGION.Demon_Worship;

	public DemonCult()
		: base(FACTION_TYPE.Demon_Cult)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public DemonCult(SaveDataFactionType saveData)
		: base(saveData, FACTION_TYPE.Demon_Cult)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public override void SetAsDefault(Faction p_faction)
	{
		Warmonger ideology = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
		AddIdeology(ideology, p_faction);
		DemonWorship ideology2 = FactionManager.Instance.CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship);
		AddIdeology(ideology2, p_faction);
		Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
		exclusive.SetRequirement(RELIGION.Demon_Worship);
		AddIdeology(exclusive, p_faction);
		BoneGolemMakers ideology3 = FactionManager.Instance.CreateIdeology<BoneGolemMakers>(FACTION_IDEOLOGY.Bone_Golem_Makers);
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
		AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
		AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Heinous);
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
			CRIME_TYPE.Theft => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Assault => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Cannibalism => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Murder => CRIME_SEVERITY.Serious, 
			CRIME_TYPE.Nature_Worship => CRIME_SEVERITY.Heinous, 
			CRIME_TYPE.Divine_Worship => CRIME_SEVERITY.Heinous, 
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

	public override void ProcessNewMember(Character character)
	{
		base.ProcessNewMember(character);
		if (ShouldChangeReligionOnJoinCult(character))
		{
			int beliefPoints = character.religionComponent.GetBeliefPoints(cultReligion);
			if (beliefPoints < ReligionComponent.Religious_Cultist_Belief_Threshold)
			{
				int p_amount = ReligionComponent.Religious_Cultist_Belief_Threshold - beliefPoints;
				character.religionComponent.IncreaseBeliefPoints(cultReligion, p_amount);
			}
			character.traitContainer.AddTrait(character, cultReligion.GetCultistTraitNameForReligion());
		}
	}

	public override void RecruitProcess(Character p_actor, Character p_target, Character p_factionLeader, out JobQueueItem p_producedJob)
	{
		_ = p_target.raceSetting.category;
		if (p_target.traitContainer.HasTrait("Devout") && p_target.traitContainer.IsReligiousCultist(out var p_religion) && p_religion != RELIGION.Demon_Worship)
		{
			p_actor.jobComponent.TriggerSacrificeJob(p_target, out p_producedJob);
		}
		else if (ChanceData.RollChance(CHANCE_TYPE.Usually_Recruits))
		{
			p_actor.jobComponent.TriggerRecruitJob(p_target, out p_producedJob);
		}
		else
		{
			p_actor.jobComponent.TriggerSacrificeJob(p_target, out p_producedJob);
		}
	}
}
