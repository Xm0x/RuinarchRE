namespace Goap.Job_Checkers;

public class CanTakeBuryJob : CanTakeJobChecker
{
	public override string key => "CanTakeBury";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (character.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD, JOB_TYPE.PRODUCE_FOOD_FOR_CAMP, jobQueueItem.poiTarget))
		{
			return false;
		}
		if (character.traitComponent.IsCharacterTargetOfObsession(jobQueueItem.poiTarget as Character))
		{
			return false;
		}
		if (!character.traitContainer.HasTrait("Criminal") && character.isAtHomeRegion && character.isPartOfHomeFaction)
		{
			return true;
		}
		return false;
	}
}
