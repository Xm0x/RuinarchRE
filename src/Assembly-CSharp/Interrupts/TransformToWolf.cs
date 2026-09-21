using System.Collections.Generic;

namespace Interrupts;

public class TransformToWolf : Interrupt
{
	public TransformToWolf()
		: base(INTERRUPT.Transform_To_Wolf)
	{
		base.duration = 6;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Lycan_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[3]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Crimes,
			LOG_TAG.Player
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Transforming");
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		Character actor = interruptHolder.actor;
		if ((bool)actor.marker)
		{
			_ = actor.currentRegion;
		}
		if (actor.isLycanthrope)
		{
			actor.lycanData.TurnToWolf();
		}
		else
		{
			actor.traitContainer.RemoveTrait(actor, "Transforming");
		}
		return base.ExecuteInterruptEndEffect(interruptHolder);
	}

	public override bool OnForceEndInterrupt(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Transforming");
		return base.OnForceEndInterrupt(interruptHolder);
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		if (actor.isLycanthrope)
		{
			actor.lycanData.AddAwareCharacter(witness);
		}
		return base.ReactionToActor(actor, target, witness, interrupt, status);
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		Character character = actor;
		if (actor.isLycanthrope)
		{
			character = actor.lycanData.originalForm;
		}
		if (CrimeManager.Instance.GetCrimeSeverity(witness, character, target, CRIME_TYPE.Werewolf).IsConsideredACrime())
		{
			if (witness.traitContainer.HasTrait("Coward", "Lycanphobic"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Threatened);
				if (witness.relationshipContainer.GetOpinionLabel(character) == "Close Friend")
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
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, character))
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
