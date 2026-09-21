public class Sadness : Emotion
{
	public Sadness()
		: base(EMOTION.Sadness)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		p_witness.needsComponent.AdjustHappiness(-10f);
		p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, p_target, "", null, "Cry_Sadness");
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
