using System.Collections.Generic;
using Traits;

namespace Interrupts;

public class BecomeLycanthrope : Interrupt
{
	public BecomeLycanthrope()
		: base(INTERRUPT.Become_Lycanthrope)
	{
		base.duration = 0;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		new LycanthropeData(interruptHolder.actor);
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
		interruptHolder.actor.UnobtainItem(TILE_OBJECT_TYPE.WEREWOLF_PELT);
		if (interruptHolder.target is WerewolfPelt)
		{
			Messenger.Broadcast(CharacterSignals.BECAME_WEREWOLF_VIA_PELT, interruptHolder.actor);
		}
		return base.ExecuteInterruptEndEffect(interruptHolder);
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		if (status == REACTION_STATUS.INFORMED)
		{
			actor.lycanData?.AddAwareCharacter(witness);
		}
		return base.ReactionToActor(actor, target, witness, interrupt, status);
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		if (status != REACTION_STATUS.INFORMED)
		{
			return;
		}
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Werewolf).IsConsideredACrime())
		{
			if (witness.traitContainer.HasTrait("Coward", "Lycanphobic"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Threatened);
				if (witness.relationshipContainer.GetOpinionLabel(actor) == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else
				{
					reactions.Add(EMOTION.Shock);
				}
			}
		}
		else if (witness.traitContainer.HasTrait("Lycanphiliac"))
		{
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor))
			{
				reactions.Add(EMOTION.Arousal);
			}
			else
			{
				reactions.Add(EMOTION.Approval);
			}
		}
		else if (witness.traitContainer.HasTrait("Lycanphobic"))
		{
			reactions.Add(EMOTION.Threatened);
		}
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime)
	{
		return CRIME_TYPE.Werewolf;
	}
}
