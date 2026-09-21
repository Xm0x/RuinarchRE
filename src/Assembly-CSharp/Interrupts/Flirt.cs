using System.Collections.Generic;
using Traits;

namespace Interrupts;

public class Flirt : Interrupt
{
	public Flirt()
		: base(INTERRUPT.Flirt)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Flirt_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.nonActionEventsComponent.NormalFlirtCharacter(interruptHolder.target as Character, interruptHolder, ref overrideEffectLog);
		return true;
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.nonActionEventsComponent.CreateRelationshipBasedOnFlirtResult(interruptHolder.identifier, interruptHolder.target as Character);
		return base.ExecuteInterruptEndEffect(interruptHolder);
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		if (!(target is Character character))
		{
			return;
		}
		if (target != witness)
		{
			bool num = witness.relationshipContainer.IsLoverOrAffair(actor);
			bool flag = witness.relationshipContainer.IsLoverOrAffair(character);
			if (witness.traitContainer.HasTrait("Hemophobic"))
			{
				Vampire traitOrStatus = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus != null && traitOrStatus.DoesCharacterKnowThisVampire(witness))
				{
					reactions.Add(EMOTION.Disgust);
				}
			}
			if (witness.traitContainer.HasTrait("Lycanphobic") && actor.isLycanthrope && actor.lycanData.DoesCharacterKnowThisLycan(witness))
			{
				reactions.Add(EMOTION.Disgust);
			}
			if (num)
			{
				reactions.Add(EMOTION.Rage);
				reactions.Add(EMOTION.Betrayal);
			}
			else if (flag)
			{
				reactions.Add(EMOTION.Rage);
				if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor))
				{
					reactions.Add(EMOTION.Betrayal);
				}
			}
			else
			{
				Character firstCharacterWithRelationship = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
				if (firstCharacterWithRelationship != null && firstCharacterWithRelationship != character)
				{
					reactions.Add(EMOTION.Disapproval);
					reactions.Add(EMOTION.Disgust);
				}
				else if (witness.relationshipContainer.IsFriendsWith(actor))
				{
					reactions.Add(EMOTION.Scorn);
				}
			}
			if (witness != actor && witness.traitContainer.HasTrait("Obsessed") && witness.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == character && !reactions.Contains(EMOTION.Rage))
			{
				reactions.Add(EMOTION.Rage);
			}
		}
		else if (status == REACTION_STATUS.INFORMED)
		{
			reactions.Add(EMOTION.Embarassment);
		}
	}

	public override void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToTarget(reactions, actor, target, witness, interrupt, status);
		if (target is Character character && witness != actor && witness != character && witness.traitContainer.HasTrait("Obsessed") && witness.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == actor)
		{
			reactions.Add(EMOTION.Rage);
		}
	}
}
