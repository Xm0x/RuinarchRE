using System.Collections.Generic;
using Traits;

public class BoobyTrap : GoapAction
{
	public BoobyTrap()
		: base(INTERACTION_TYPE.BOOBY_TRAP)
	{
		base.actionIconString = GoapActionStateDB.Trap_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Trap Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		BoobyTrapped traitOrStatus = target.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
		if (traitOrStatus != null)
		{
			traitOrStatus.AddAwareCharacter(witness);
			return result;
		}
		return result;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is TileObject tileObject))
		{
			return;
		}
		if (tileObject.IsOwnedBy(witness))
		{
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				reactions.Add(EMOTION.Threatened);
			}
			if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
		else if (witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && actor.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
		{
			reactions.Add(EMOTION.Approval);
		}
		else if (tileObject.characterOwner != null)
		{
			Character characterOwner = tileObject.characterOwner;
			if (witness.relationshipContainer.HasRelationshipWith(characterOwner, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(characterOwner))
			{
				if (witness.traitContainer.HasTrait("Coward"))
				{
					reactions.Add(EMOTION.Fear);
					return;
				}
				reactions.Add(EMOTION.Disapproval);
				if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(actor))
				{
					reactions.Add(EMOTION.Anger);
				}
			}
			else
			{
				reactions.Add(EMOTION.Disapproval);
				if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(actor))
				{
					reactions.Add(EMOTION.Shock);
					reactions.Add(EMOTION.Disappointment);
				}
			}
		}
		else
		{
			reactions.Add(EMOTION.Disapproval);
			if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE) || witness.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Anger);
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		node.poiTarget.traitContainer.RemoveTrait(node.poiTarget, "Booby Trapped");
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		node.poiTarget.traitContainer.RemoveTrait(node.poiTarget, "Booby Trapped");
	}

	public override void OnStoppedInterrupt(ActualGoapNode node)
	{
		base.OnStoppedInterrupt(node);
		node.poiTarget.traitContainer.RemoveTrait(node.poiTarget, "Booby Trapped");
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Assault;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Assault;
	}

	public void PreTrapSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		IPointOfInterest poiTarget = goapNode.poiTarget;
		poiTarget.traitContainer.AddTrait(poiTarget, "Booby Trapped", out var trait, actor);
		if (trait != null)
		{
			(trait as BoobyTrapped).SetElementType(actor.combatComponent.currentElement.type);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return poiTarget.gridTileLocation != null;
		}
		return false;
	}
}
