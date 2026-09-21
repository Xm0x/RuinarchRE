using System.Collections.Generic;
using Traits;

namespace Interrupts;

public class TransformToBat : Interrupt
{
	public TransformToBat()
		: base(INTERRUPT.Transform_To_Bat)
	{
		base.duration = 1;
		base.shouldStopMovement = false;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Combat,
			LOG_TAG.Crimes
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		actor.RevertFromWerewolfForm();
		actor.TransformToVampireBatForm();
		return true;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AddAwareCharacter(witness);
		return base.ReactionToActor(actor, target, witness, interrupt, status);
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire).IsConsideredACrime())
		{
			if (witness.traitContainer.HasTrait("Coward", "Hemophobic"))
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
		else if (witness.traitContainer.HasTrait("Hemophiliac"))
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
		else if (witness.traitContainer.HasTrait("Hemophobic"))
		{
			reactions.Add(EMOTION.Threatened);
		}
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime)
	{
		return CRIME_TYPE.Vampire;
	}
}
