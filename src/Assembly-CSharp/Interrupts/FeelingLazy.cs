using UtilityScripts;

namespace Interrupts;

public class FeelingLazy : Interrupt
{
	public FeelingLazy()
		: base(INTERRUPT.Feeling_Lazy)
	{
		base.duration = 0;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Flirt_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
		base.shouldShowNotif = false;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		if (!actor.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), interruptHolder.actor, interruptHolder.actor);
			JobUtilities.PopulatePriorityLocationsForHappinessRecovery(actor, goapPlanJob);
			goapPlanJob.SetDoNotRecalculate(state: true);
			interruptHolder.actor.jobQueue.AddJobInQueue(goapPlanJob);
			return true;
		}
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}
}
