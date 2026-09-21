using Traits;

public class Fear : Emotion
{
	public Fear()
		: base(EMOTION.Fear)
	{
		base.mutuallyExclusive = new string[1] { "Threatened" };
	}

	public override string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		p_witness.traitContainer.AddTrait(p_witness, "Spooked", p_target as Character);
		Spooked traitOrStatus = p_witness.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
		if (p_target is GenericTileObject)
		{
			RemnantTrait remnantTrait = null;
			if (p_target.traitContainer.HasTrait("Danger Remnant"))
			{
				remnantTrait = p_target.traitContainer.GetTraitOrStatus<RemnantTrait>("Danger Remnant");
			}
			else if (p_target.traitContainer.HasTrait("Surprised Remnant"))
			{
				remnantTrait = p_target.traitContainer.GetTraitOrStatus<RemnantTrait>("Surprised Remnant");
			}
			else if (p_target.traitContainer.HasTrait("Lightning Remnant"))
			{
				remnantTrait = p_target.traitContainer.GetTraitOrStatus<RemnantTrait>("Lightning Remnant");
			}
			if (remnantTrait != null && remnantTrait.spellUsed != PLAYER_SKILL_TYPE.NONE)
			{
				traitOrStatus.AddSourceOfFear(remnantTrait.spellUsed);
				p_witness.moodComponent.UpdateMoodModificationLog(traitOrStatus, traitOrStatus.responsibleCharacter);
			}
		}
		else
		{
			traitOrStatus.AddSourceOfFear(p_target);
		}
		p_witness.combatComponent.Flight(p_target, "Saw_Frightening");
		return base.ProcessEmotion(p_witness, p_target, p_status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, p_goapNode, p_reason, p_triggerOpinionChangesEffect);
	}
}
