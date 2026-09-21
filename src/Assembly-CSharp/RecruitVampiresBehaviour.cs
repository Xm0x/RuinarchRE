using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

public class RecruitVampiresBehaviour : CharacterBehaviour
{
	public RecruitVampiresBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool flag = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.targetDestination.IsAtTargetDestination(character))
		{
			BaseSettlement targetSettlement = (currentParty.currentQuest as RecruitVampiresPartyQuest).targetSettlement;
			List<Character> list = RuinarchListPool<Character>.Claim();
			PopulateVampiricEmbraceTargets(list, targetSettlement);
			Character randomElement = CollectionUtilities.GetRandomElement(list);
			if (randomElement != null)
			{
				flag = character.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.VAMPIRIC_EMBRACE, randomElement, out producedJob);
			}
			if (!flag)
			{
				flag = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return flag;
	}

	private void PopulateVampiricEmbraceTargets(List<Character> p_list, BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.residents.Count; i++)
		{
			Character character = p_settlement.residents[i];
			if (character.currentSettlement == p_settlement && !character.traitContainer.HasTrait("Vampire"))
			{
				p_list.Add(character);
			}
		}
	}
}
