using UnityEngine;

public class Scorn : Emotion
{
	public Scorn()
		: base(EMOTION.Scorn)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character targetPOI)
		{
			if (p_status == REACTION_STATUS.INFORMED || Random.Range(0, 2) == 0)
			{
				p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetPOI);
			}
			else
			{
				p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetPOI);
			}
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
