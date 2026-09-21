public class Gratefulness : Emotion
{
	public Gratefulness()
		: base(EMOTION.Gratefulness)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character)
		{
			Character targetCharacter = p_target as Character;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, targetCharacter, "Gratefulness", 10, "", p_triggerOpinionChangesEffect);
			p_witness.relationshipContainer.AdjustOpinion(p_witness, targetCharacter, "Gratefulness", 40, "", p_triggerOpinionChangesEffect);
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(24));
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				p_witness.relationshipContainer.AdjustOpinion(p_witness, targetCharacter, "Gratefulness", -40, "", createJobsOnReduce: false);
			}, p_witness);
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
