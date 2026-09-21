namespace Goap.Job_Checkers;

public class BuryApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsBuryApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		Character character = (job as GoapPlanJob).targetPOI as Character;
		if (character.gridTileLocation != null)
		{
			return character.hasMarker;
		}
		return false;
	}
}
