namespace Interrupts;

public class Stopped : Interrupt
{
	public Stopped()
		: base(INTERRUPT.Stopped)
	{
		base.duration = 0;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.isSimulateneous = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode node = null)
	{
		bool result = base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, node);
		Character character = interruptHolder.target as Character;
		GoapAction goapAction = node?.action;
		if (goapAction != null)
		{
			goapAction.OnStoppedInterrupt(node);
			if (node.associatedJob != null && !node.associatedJob.hasBeenReset && node.associatedJob.originalOwner != null)
			{
				node.associatedJob.CancelJob();
			}
			result = true;
		}
		character.currentJob?.CancelJob();
		character.currentJob?.StopJobNotDrop();
		if (interruptHolder.actor != character && goapAction != null)
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect_with_action", base.logTags);
			overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			overrideEffectLog.AddToFillers(null, goapAction.localizedName, LOG_IDENTIFIER.STRING_1);
		}
		return result;
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		if (actor == target)
		{
			return null;
		}
		return base.CreateEffectLog(actor, target);
	}
}
