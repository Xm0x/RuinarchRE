public class DefaultOtherStructure : CharacterBehaviour
{
	public DefaultOtherStructure()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.currentStructure.isInterior && character.currentStructure != character.homeStructure && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea())
		{
			if ((character.homeStructure != null && !character.homeStructure.hasBeenDestroyed) || character.HasTerritory())
			{
				return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			}
			return character.jobComponent.TriggerStand(out producedJob);
		}
		producedJob = null;
		return false;
	}
}
