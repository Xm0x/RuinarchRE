using Traits;

public class Arousal : Emotion
{
	public Arousal()
		: base(EMOTION.Arousal)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character character)
		{
			p_witness.relationshipContainer.AdjustOpinion(p_witness, character, "Arousal", 10, "", p_triggerOpinionChangesEffect);
			p_witness.traitContainer.AddTrait(p_witness, "Aroused");
			p_witness.traitContainer.GetTraitOrStatus<Aroused>("Aroused").AddArousedTarget(character);
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
