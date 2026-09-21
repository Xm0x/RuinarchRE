public class SlaveBehaviour : CharacterBehaviour
{
	public SlaveBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool flag = character.IsAtHome();
		if (flag)
		{
			if (character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob))
			{
				return true;
			}
			if (character.homeSettlement != null && !HasEnoughFoodAtHome(character) && !AlreadyHasResidentProducingFood(character) && character.jobComponent.CreateProduceFoodJob(out producedJob))
			{
				return true;
			}
		}
		if (character.movementComponent.isStationary)
		{
			return character.jobComponent.PlanIdleLongStandStill(out producedJob);
		}
		if (!flag)
		{
			return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundTile(out producedJob);
	}

	private bool HasEnoughFoodAtHome(Character p_character)
	{
		int num = 0;
		if (p_character.homeSettlement != null)
		{
			num = p_character.homeSettlement.GetNumberOfFoodInWholeSettlement();
		}
		else if (p_character.homeStructure != null)
		{
			num = p_character.homeStructure.GetTotalResourceInStructure(RESOURCE.FOOD);
		}
		else if (p_character.territory != null)
		{
			num = p_character.territory.tileObjectComponent.GetTotalResourceCount(RESOURCE.FOOD);
		}
		return num > 50;
	}

	private bool AlreadyHasResidentProducingFood(Character p_character)
	{
		for (int i = 0; i < p_character.homeSettlement.residents.Count; i++)
		{
			if (p_character.homeSettlement.residents[i].jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD))
			{
				return true;
			}
		}
		return false;
	}
}
