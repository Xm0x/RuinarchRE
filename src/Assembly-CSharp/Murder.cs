using System.Collections.Generic;

public class Murder : GoapAction
{
	public Murder()
		: base(INTERACTION_TYPE.MURDER)
	{
		base.actionIconString = GoapActionStateDB.Death_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Murder Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
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

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		if (target is Character character && actor.faction != null && witness.faction != null && witness.faction.isMajorNonPlayerOrBandits && actor.faction.isMajorNonPlayerOrBandits && actor.faction != witness.faction && witness.faction == character.faction)
		{
			if (witness.isFactionLeader || witness.isSettlementRuler)
			{
				witness.faction.FactionProcessingAbductionOrMurder(actor, character, node);
				return result;
			}
			if (witness.faction.leader is Character || (witness.homeSettlement != null && witness.homeSettlement.ruler != null))
			{
				witness.jobComponent.TryCreateReportMurderOrAbduct(node);
			}
		}
		return result;
	}

	public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToTarget(actor, target, witness, node, status);
		if (target is Character character && (witness.relationshipContainer.GetOpinionLabel(character) == "Close Friend" || (witness.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.SIBLING) && !witness.relationshipContainer.IsEnemiesWith(character))))
		{
			witness.traitContainer.AddTrait(witness, "Griefstricken", character);
		}
		return result;
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

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (actor == target)
		{
			return CRIME_TYPE.None;
		}
		if (crime.isAssumption)
		{
			return CRIME_TYPE.Murder;
		}
		Character character = target as Character;
		CombatData combatData = actor.combatComponent.GetCombatData(target);
		if (character != null && !actor.IsHostileWith(character) && combatData != null)
		{
			if (combatData.reasonForCombat == "Retaliation")
			{
				return CRIME_TYPE.None;
			}
			if (combatData.reasonForCombat == "Action" && combatData.connectedAction != null && combatData.connectedAction.associatedJob != null && (combatData.connectedAction.associatedJob.jobType.IsApprehendTypeJob() || combatData.connectedAction.associatedJob.jobType == JOB_TYPE.RESTRAIN))
			{
				return CRIME_TYPE.None;
			}
			return CRIME_TYPE.Murder;
		}
		return CRIME_TYPE.None;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Murder;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public void AfterMurderSuccess(ActualGoapNode goapNode)
	{
		(goapNode.poiTarget as Character).Death("normal", goapNode, goapNode.actor, null, null, null, null, isPlayerSource: false, goapNode.actor);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget)
			{
				return !(poiTarget as Character).isDead;
			}
			return false;
		}
		return false;
	}
}
