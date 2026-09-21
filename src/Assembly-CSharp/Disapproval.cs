public class Disapproval : Emotion
{
	public Disapproval()
		: base(EMOTION.Disapproval)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character character)
		{
			int num = -9;
			p_totalOpinionReduction += num;
			p_lastStrawReasonKey = _localizedResponseKey;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, character, "Disapproval", num, _localizedResponseKey, p_triggerOpinionChangesEffect);
			if (p_status == REACTION_STATUS.WITNESSED && (p_goapNode == null || !p_goapNode.isAssumption) && !character.combatComponent.isInCombat && !character.interruptComponent.isInterrupted)
			{
				_ = p_goapNode?.currentState;
				if (p_goapNode == null || (p_goapNode.action.goapType != INTERACTION_TYPE.EVANGELIZE && p_goapNode.action.goapType != INTERACTION_TYPE.STRANGLE && p_goapNode.expectedActionStateDuration > 0 && p_goapNode.ticksPerformingCurrentState < p_goapNode.expectedActionStateDuration - 1))
				{
					p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, character, "", p_goapNode);
				}
			}
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
