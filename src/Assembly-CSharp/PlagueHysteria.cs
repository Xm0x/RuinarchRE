public class PlagueHysteria : Emotion
{
	public PlagueHysteria()
		: base(EMOTION.Plague_Hysteria)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character character)
		{
			int num = -10;
			p_totalOpinionReduction += num;
			p_lastStrawReasonKey = _localizedResponseKey;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, character, "Plague_Hysteria", num, _localizedResponseKey, p_triggerOpinionChangesEffect);
			if (p_witness.relationshipContainer.GetAwarenessState(p_witness, character) == AWARENESS_STATE.Available && !character.isDead)
			{
				p_witness.assumptionComponent.CreateAndReactToNewAssumption(character, character, INTERACTION_TYPE.IS_PLAGUED, REACTION_STATUS.WITNESSED, !character.traitContainer.HasTrait("Plagued"));
			}
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
