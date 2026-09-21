using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class StealAnything : GoapAction
{
	public StealAnything()
		: base(INTERACTION_TYPE.STEAL_ANYTHING)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Steal Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is TileObject)
		{
			TileObject tileObject = goapNode.poiTarget as TileObject;
			if (tileObject.isBeingCarriedBy != null)
			{
				return tileObject.isBeingCarriedBy;
			}
		}
		return base.GetTargetToGoTo(goapNode);
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		TileObject tileObject = node.poiTarget as TileObject;
		if (tileObject.isBeingCarriedBy != null)
		{
			return tileObject.isBeingCarriedBy.currentStructure;
		}
		return base.GetTargetStructure(node);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = false;
		invalidity.stateName = stateName;
		invalidity.reason = string.Empty;
		return invalidity;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!witness.traitContainer.HasTrait("Demon Cultist"))
		{
			reactions.Add(EMOTION.Disapproval);
			if (witness.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Disappointment);
				reactions.Add(EMOTION.Shock);
			}
		}
		else if (witness == target || (target is TileObject tileObject && tileObject.IsOwnedBy(witness)))
		{
			reactions.Add(EMOTION.Betrayal);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Theft;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Theft;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			TileObject tileObject = poiTarget as TileObject;
			if (tileObject.gridTileLocation != null)
			{
				return true;
			}
			return tileObject.isBeingCarriedBy != null;
		}
		return false;
	}

	public void AfterStealSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject);
		if (goapNode.actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			goapNode.actor.needsComponent.AdjustHappiness(10f);
		}
	}
}
