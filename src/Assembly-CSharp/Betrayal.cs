public class Betrayal : Emotion
{
	public Betrayal()
		: base(EMOTION.Betrayal)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character)
		{
			Character character = p_target as Character;
			int num = -60;
			p_totalOpinionReduction += num;
			p_lastStrawReasonKey = _localizedResponseKey;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, character, "Betrayal", num, _localizedResponseKey, p_triggerOpinionChangesEffect);
			p_witness.traitContainer.AddTrait(p_witness, "Betrayed", character);
			p_witness.relationshipContainer.SetHasGrudgeAgainst(p_witness, character, p_state: true);
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
