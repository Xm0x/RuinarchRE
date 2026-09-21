using System.Collections.Generic;

public class SlayCharacter : GoapAction
{
	public SlayCharacter()
		: base(INTERACTION_TYPE.SLAY_CHARACTER)
	{
		base.doesNotStopTargetCharacter = true;
		base.actionIconString = GoapActionStateDB.Death_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Slay Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && !(poiTarget as Character).isDead)
		{
			goapActionInvalidity.isInvalid = true;
		}
		return goapActionInvalidity;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (actor == target)
		{
			return CRIME_TYPE.None;
		}
		return CRIME_TYPE.Murder;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Murder;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is Character))
		{
			return;
		}
		Character character = target as Character;
		if (actor != character)
		{
			if (witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Approval);
			}
			else if (!actor.IsHostileWith(character))
			{
				if (witness.traitContainer.HasTrait("Coward"))
				{
					reactions.Add(EMOTION.Fear);
					return;
				}
				string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
				if (opinionLabel == "Rival")
				{
					reactions.Add(EMOTION.Approval);
				}
				else if (witness.traitContainer.HasTrait("Diplomatic"))
				{
					reactions.Add(EMOTION.Disapproval);
				}
				else if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
				{
					reactions.Add(EMOTION.Rage);
				}
				else if (node.crimeType == CRIME_TYPE.Murder)
				{
					reactions.Add(EMOTION.Shock);
					reactions.Add(EMOTION.Disapproval);
				}
				else
				{
					reactions.Add(EMOTION.Shock);
				}
			}
			else if (witness.IsHostileWith(character))
			{
				reactions.Add(EMOTION.Approval);
			}
			else if (witness.traitContainer.HasTrait("Diplomatic"))
			{
				reactions.Add(EMOTION.Disapproval);
			}
			else if (witness.relationshipContainer.IsFriendsWith(character))
			{
				reactions.Add(EMOTION.Rage);
			}
			else
			{
				reactions.Add(EMOTION.Shock);
			}
		}
		else
		{
			reactions.Add(EMOTION.Shock);
			if (witness.traitContainer.HasTrait("Psychopath") || witness.relationshipContainer.IsEnemiesWith(actor))
			{
				reactions.Add(EMOTION.Scorn);
			}
			else
			{
				reactions.Add(EMOTION.Disapproval);
			}
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (target is Character character && actor != character && witness.relationshipContainer.HasRelationshipWith(character))
		{
			if (witness.relationshipContainer.GetOpinionLabel(character) == "Rival")
			{
				reactions.Add(EMOTION.Scorn);
			}
			else
			{
				reactions.Add(EMOTION.Concern);
			}
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (target is Character character && actor != character)
		{
			reactions.Add(EMOTION.Anger);
			if (character.relationshipContainer.IsFriendsWith(actor) && !character.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
	}

	public void AfterSlaySuccess(ActualGoapNode goapNode)
	{
		(goapNode.poiTarget as Character).Death("normal", goapNode, goapNode.actor, null, null, null, null, isPlayerSource: false, goapNode.actor);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget)
			{
				return !(poiTarget as Character).limiterComponent.canPerform;
			}
			return false;
		}
		return false;
	}
}
