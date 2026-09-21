using System.Collections.Generic;
using UtilityScripts;

namespace Interrupts;

public class RevertToNormal : Interrupt
{
	public RevertToNormal()
		: base(INTERRUPT.Revert_To_Normal)
	{
		base.duration = 6;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
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
		if (actor.isLycanthrope)
		{
			actor.lycanData.RevertToNormal();
			if (!actor.lycanData.isMaster && GameUtilities.RollChance(25))
			{
				actor.lycanData.SetIsMaster(p_state: true);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " mastered", LOG_TAG.Life_Changes);
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
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
