namespace Goap.Job_Checkers;

public class CanTakeApprehend : CanTakeJobChecker
{
	public override string key => "CanTakeApprehend";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		Character character2 = (jobQueueItem as GoapPlanJob).targetPOI as Character;
		if (character2 == character)
		{
			return false;
		}
		return InteractionManager.Instance.CanCharacterTakeApprehendJob(character, character2);
	}
}
