namespace Interrupts;

public class Grieving : Interrupt
{
	public Grieving()
		: base(INTERRUPT.Grieving)
	{
		base.duration = 5;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Sad_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Social
		};
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
		interruptHolder.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, interruptHolder.target, "", null, "Cry_Grieving");
		if (interruptHolder.actor.religionComponent.religion != RELIGION.None && interruptHolder.actor.religionComponent.GetBeliefPoints(interruptHolder.actor.religionComponent.religion) > ReligionComponent.Base_Belief_Points)
		{
			interruptHolder.actor.religionComponent.DecreaseBeliefPoints(interruptHolder.actor.religionComponent.religion);
		}
		return true;
	}
}
