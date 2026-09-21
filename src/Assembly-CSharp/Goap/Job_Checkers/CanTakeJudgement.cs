namespace Goap.Job_Checkers;

public class CanTakeJudgement : CanTakeJobChecker
{
	public override string key => "CanTakeJudgement";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (!character.isSettlementRuler)
		{
			return character.isFactionLeader;
		}
		return true;
	}
}
