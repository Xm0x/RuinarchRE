using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class BloodHuntBehaviour : CharacterBehaviour
{
	public BloodHuntBehaviour()
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
			BaseSettlement targetSettlement = (currentParty.currentQuest as BloodHuntPartyQuest).targetSettlement;
			List<Character> list = RuinarchListPool<Character>.Claim();
			PopulateDrinkBloodTargets(list, targetSettlement);
			Character randomElement = CollectionUtilities.GetRandomElement(list);
			if (randomElement != null)
			{
				if (ChanceData.RollChance(CHANCE_TYPE.Blood_Hunt_Imprison_Blood_Source))
				{
					LocationStructure dropStructure = ((character.homeSettlement != null) ? character.homeSettlement.mainStorage : character.homeStructure);
					flag = character.jobComponent.TriggerImprisonBloodSource(randomElement, dropStructure, out producedJob);
				}
				else
				{
					flag = character.jobComponent.CreateDrinkBloodJobForBloodHunt(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, randomElement, out producedJob);
				}
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

	private void PopulateDrinkBloodTargets(List<Character> p_list, BaseSettlement p_settlement)
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
