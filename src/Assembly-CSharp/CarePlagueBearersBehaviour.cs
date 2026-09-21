using System.Collections.Generic;

public class CarePlagueBearersBehaviour : CharacterBehaviour
{
	public CarePlagueBearersBehaviour()
	{
		base.priority = 790;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.homeSettlement == null || !character.homeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE))
		{
			producedJob = null;
			character.traitContainer.RemoveTrait(character, "Plague Caring");
			return false;
		}
		List<Character> list = new List<Character>();
		for (int i = 0; i < character.homeSettlement.residents.Count; i++)
		{
			Character character2 = character.homeSettlement.residents[i];
			if (IsCharacterQuarantinedAndUnTended(character2))
			{
				list.Add(character2);
			}
		}
		if (list.Count > 0)
		{
			Character character3 = list[0];
			if (character3.needsComponent.isHungry || character3.needsComponent.isStarving)
			{
				FoodPile resourcePileObjectWithLowestCount = character.homeSettlement.mainStorage.GetResourcePileObjectWithLowestCount<FoodPile>();
				if (resourcePileObjectWithLowestCount != null && resourcePileObjectWithLowestCount.resourceInPile >= 12 && character.jobComponent.TryTriggerFeed(character3, out producedJob))
				{
					return true;
				}
			}
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.CARE], character, character3, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, character3);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLAGUE_CARE, INTERACTION_TYPE.CARE, character3, character);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		character.traitContainer.RemoveTrait(character, "Plague Caring");
		return false;
	}

	private bool IsCharacterQuarantinedAndUnTended(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Quarantined") && !p_character.traitContainer.HasTrait("Plague Cared") && p_character.gridTileLocation != null)
		{
			return p_character.gridTileLocation.structure.settlementLocation == p_character.homeSettlement;
		}
		return false;
	}
}
