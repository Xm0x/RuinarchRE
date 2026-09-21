using Factions.Faction_Types;
using Inner_Maps.Location_Structures;

public class SettlementRulerBehaviour : CharacterBehaviour
{
	public SettlementRulerBehaviour()
	{
		base.priority = 32;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.partyComponent.isActiveMember)
		{
			producedJob = null;
			return false;
		}
		NPCSettlement homeSettlement = character.homeSettlement;
		if (homeSettlement != null && homeSettlement.prison != null && character.faction != null)
		{
			LocationStructure prison = homeSettlement.prison;
			if (ChanceData.RollChance(CHANCE_TYPE.Check_Recruit_Chance, ref log))
			{
				Character randomCharacterThatCanBeRecruitedBy = prison.GetRandomCharacterThatCanBeRecruitedBy(character);
				if (randomCharacterThatCanBeRecruitedBy != null && !randomCharacterThatCanBeRecruitedBy.crimeComponent.HasWantedCrimeBy(character.faction, CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous))
				{
					if (randomCharacterThatCanBeRecruitedBy.isNormalCharacter)
					{
						if (character.faction.factionType is CultFaction && randomCharacterThatCanBeRecruitedBy.traitContainer.HasTrait("Devout") && randomCharacterThatCanBeRecruitedBy.traitContainer.IsReligiousCultist(out var p_religion) && p_religion != RELIGION.Demon_Worship)
						{
							if (character.faction.factionType.type == FACTION_TYPE.Demon_Cult)
							{
								return character.jobComponent.TriggerSacrificeJob(randomCharacterThatCanBeRecruitedBy, out producedJob);
							}
							GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECRUIT, INTERACTION_TYPE.EXECUTE, randomCharacterThatCanBeRecruitedBy, character);
							goapPlanJob.SetCannotBePushedBack(state: true);
							goapPlanJob.SetDoNotRecalculate(state: true);
							producedJob = goapPlanJob;
							return true;
						}
						return character.jobComponent.TriggerRecruitJob(randomCharacterThatCanBeRecruitedBy, out producedJob);
					}
					if (character.faction.leader is Character p_factionLeader)
					{
						character.faction.factionType.RecruitProcess(character, randomCharacterThatCanBeRecruitedBy, p_factionLeader, out producedJob);
						if (producedJob != null)
						{
							return true;
						}
					}
				}
			}
		}
		producedJob = null;
		return false;
	}
}
