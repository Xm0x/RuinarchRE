using UtilityScripts;

public class Anger : Emotion
{
	public Anger()
		: base(EMOTION.Anger)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character character)
		{
			int num = -15;
			p_totalOpinionReduction += num;
			p_lastStrawReasonKey = _localizedResponseKey;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, character, "Base", num, _localizedResponseKey, p_triggerOpinionChangesEffect);
			p_witness.traitContainer.AddTrait(p_witness, "Angry", character);
			if ((bool)p_witness.marker && p_witness.marker.IsPOIInVision(character))
			{
				p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Angry, character);
			}
		}
		else if (p_target is TileObject tileObject)
		{
			p_witness.traitContainer.AddTrait(p_witness, "Angry");
			if (GameUtilities.RollChance(50))
			{
				p_witness.combatComponent.Fight(tileObject, "Anger");
			}
			if ((bool)p_witness.marker && p_witness.marker.IsPOIInVision(tileObject))
			{
				p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Angry, tileObject);
			}
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
