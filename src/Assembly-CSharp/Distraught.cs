using Traits;

public class Distraught : Emotion
{
	public Distraught()
		: base(EMOTION.Distraught)
	{
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		if (p_target is Character character)
		{
			if (character.IsConsideredInDangerBy(p_witness))
			{
				p_witness.traitContainer.AddTrait(p_witness, "Worried", character);
				bool flag = false;
				if (p_witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
				{
					Prisoner traitOrStatus = character.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
					if (traitOrStatus != null && traitOrStatus.IsFactionPrisonerOf(PlayerManager.Instance.player.playerFaction))
					{
						p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Cry_Distraught");
						flag = true;
					}
				}
				if (!flag && p_witness.faction != null && !p_witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, character) && !p_witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, character))
				{
					p_witness.faction.partyQuestBoard.CreateRescuePartyQuest(p_witness, p_witness.homeSettlement, character);
				}
			}
			else
			{
				p_witness.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, character);
			}
		}
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}

	public static bool WillWitnessCreateRescueQuestWhenFelt(Character p_witness, IPointOfInterest p_target)
	{
		if (p_target is Character character && character.IsConsideredInDangerBy(p_witness))
		{
			p_witness.traitContainer.AddTrait(p_witness, "Worried", character);
			if (p_witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
			{
				Prisoner traitOrStatus = character.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
				if (traitOrStatus != null && traitOrStatus.IsFactionPrisonerOf(PlayerManager.Instance.player.playerFaction))
				{
					return false;
				}
			}
			if (p_witness.faction != null)
			{
				return true;
			}
		}
		return false;
	}
}
