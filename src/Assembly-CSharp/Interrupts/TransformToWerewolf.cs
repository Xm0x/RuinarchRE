using System.Collections.Generic;

namespace Interrupts;

public class TransformToWerewolf : Interrupt
{
	public TransformToWerewolf()
		: base(INTERRUPT.Transform_To_Werewolf)
	{
		base.duration = 1;
		base.shouldStopMovement = false;
		base.interruptIconString = GoapActionStateDB.Lycan_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Combat,
			LOG_TAG.Crimes
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		actor.RevertFromVampireBatForm();
		actor.TransformToWerewolfForm();
		return true;
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
