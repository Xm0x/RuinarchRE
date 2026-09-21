using UtilityScripts;

public class Shock : Emotion
{
	public Shock()
		: base(EMOTION.Shock)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		p_witness.needsComponent.AdjustHappiness(-10f);
		if (p_status == REACTION_STATUS.WITNESSED)
		{
			if (GameUtilities.RollChance(30))
			{
				p_witness.combatComponent.Flight(p_target, "Shocked");
			}
			else
			{
				string text = p_reason;
				if (string.IsNullOrEmpty(text))
				{
					text = "Shocked_Witness_Reason";
				}
				p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, p_target, "", null, text);
			}
		}
		else
		{
			string text2 = p_reason;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "Shocked_Informed_Reason";
			}
			p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, p_target, "", null, text2);
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
