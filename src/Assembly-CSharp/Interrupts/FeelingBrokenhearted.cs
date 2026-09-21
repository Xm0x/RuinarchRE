namespace Interrupts;

public class FeelingBrokenhearted : Interrupt
{
	public FeelingBrokenhearted()
		: base(INTERRUPT.Feeling_Brokenhearted)
	{
		base.duration = 5;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Heartbroken_Icon;
		base.logTags = new LOG_TAG[3]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Needs,
			LOG_TAG.Social
		};
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
		if (interruptHolder.actor.religionComponent.religion != RELIGION.None && interruptHolder.actor.religionComponent.GetBeliefPoints(interruptHolder.actor.religionComponent.religion) > ReligionComponent.Base_Belief_Points)
		{
			interruptHolder.actor.religionComponent.DecreaseBeliefPoints(interruptHolder.actor.religionComponent.religion);
		}
		return true;
	}
}
