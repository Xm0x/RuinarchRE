using UtilityScripts;

public class Despair : Emotion
{
	public Despair()
		: base(EMOTION.Despair)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (GameUtilities.RollChance(80))
		{
			p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, p_target, "", null, "Cry_Despair");
		}
		p_witness.traitContainer.AddTrait(p_witness, "Despairing");
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
