public class Resentment : Emotion
{
	public Resentment()
		: base(EMOTION.Resentment)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character character)
		{
			int num = -15;
			p_totalOpinionReduction += num;
			p_lastStrawReasonKey = _localizedResponseKey;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, character, "Resentment", num, _localizedResponseKey, p_triggerOpinionChangesEffect);
			if (p_witness.moodComponent.moodState != MOOD_STATE.Normal)
			{
				p_witness.relationshipContainer.SetHasGrudgeAgainst(p_witness, character, p_state: true);
			}
			else
			{
				p_witness.traitContainer.AddTrait(p_witness, "Annoyed", character);
			}
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
