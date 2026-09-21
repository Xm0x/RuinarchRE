public class Embarassment : Emotion
{
	public Embarassment()
		: base(EMOTION.Embarassment)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		p_witness.needsComponent.AdjustHope(-5f);
		p_witness.traitContainer.AddTrait(p_witness, "Ashamed");
		if (p_witness.marker.IsPOIInVision(p_target))
		{
			p_witness.combatComponent.Flight(p_target, "Felt_Embarrassed");
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
