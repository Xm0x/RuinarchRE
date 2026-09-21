namespace Goap.Job_Checkers;

public class CanTakeTendWyvernCoopJob : CanTakeJobChecker
{
	public override string key => "CanTakeTendWyvernCoop";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		RaceData raceData = RaceManager.Instance.GetRaceData(character.race);
		if (raceData.category == CHARACTER_CATEGORY.Humanoid || raceData.category == CHARACTER_CATEGORY.Demonic || character.race.IsSapient())
		{
			return true;
		}
		return false;
	}
}
