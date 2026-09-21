using Traits;

public class Threatened : Emotion
{
	public Threatened()
		: base(EMOTION.Threatened)
	{
		base.mutuallyExclusive = new string[1] { "Fear" };
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_witness.isNormalCharacter && p_target is Character target)
		{
			int num = -8;
			p_totalOpinionReduction += num;
			p_lastStrawReasonKey = _localizedResponseKey;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, target, "Threatened", num, _localizedResponseKey, p_triggerOpinionChangesEffect);
		}
		if (p_witness.hasMarker && p_witness.marker.IsPOIInVision(p_target))
		{
			bool willAttackBecauseOfCrime = false;
			if (p_goapNode != null && p_goapNode.crimeType != CRIME_TYPE.None && p_goapNode.crimeType != CRIME_TYPE.Unset)
			{
				willAttackBecauseOfCrime = true;
			}
			p_witness.combatComponent.FightOrFlight(p_target, "Threatened", null, isLethal: true, willAttackBecauseOfCrime);
		}
		else
		{
			p_witness.traitContainer.AddTrait(p_witness, "Anxious");
			p_witness.traitContainer.GetTraitOrStatus<Anxious>("Anxious").AddSourceOfAnxiety(p_target);
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
