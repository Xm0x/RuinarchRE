public class MovementProcessing : CharacterBehaviour
{
	public MovementProcessing()
	{
		base.priority = 27;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.gridTileLocation != null && character.areaLocation == null)
		{
			Area nearestAreaWithinRegionThatCharacterHasPathTo = character.gridTileLocation.GetNearestAreaWithinRegionThatCharacterHasPathTo(character);
			character.jobComponent.TriggerMoveToArea(JOB_TYPE.IDLE, out producedJob, nearestAreaWithinRegionThatCharacterHasPathTo);
			return true;
		}
		producedJob = null;
		return false;
	}
}
