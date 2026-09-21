using UtilityScripts;

public class Rage : Emotion
{
	public Rage()
		: base(EMOTION.Rage)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character character)
		{
			int num = -35;
			p_totalOpinionReduction += num;
			p_lastStrawReasonKey = _localizedResponseKey;
			p_witness.relationshipContainer.AdjustOpinion(p_witness, character, "Base", num, _localizedResponseKey, p_triggerOpinionChangesEffect);
			if (p_witness.partyComponent.hasParty && character.partyComponent.hasParty && p_witness.partyComponent.currentParty == character.partyComponent.currentParty)
			{
				if (p_witness.partyComponent.currentParty.partyLeader == p_witness)
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Removed_From_Party, character, "", null, "Removed_From_Party_Enraged_Leader");
				}
				else
				{
					p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, p_witness, "", null, "Left_Party_Enraged_Party_Member");
				}
			}
			p_witness.traitContainer.AddTrait(p_witness, "Angry", character);
			if (p_witness.hasMarker && character.hasMarker && p_witness.marker.IsPOIInVision(character))
			{
				bool isLethal = false;
				if (GameUtilities.RollChance(50))
				{
					isLethal = true;
				}
				p_witness.combatComponent.Fight(character, "Rage", null, isLethal);
			}
			else
			{
				p_witness.relationshipContainer.SetHasGrudgeAgainst(p_witness, character, p_state: true);
			}
		}
		else if (p_target is TileObject tileObject)
		{
			p_witness.traitContainer.AddTrait(p_witness, "Angry");
			if (p_witness.hasMarker && p_witness.marker.IsPOIInVision(tileObject))
			{
				p_witness.combatComponent.Fight(tileObject, "Rage");
			}
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
