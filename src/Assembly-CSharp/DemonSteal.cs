using Inner_Maps;
using Inner_Maps.Location_Structures;

public class DemonSteal : GoapAction
{
	public DemonSteal()
		: base(INTERACTION_TYPE.DEMON_STEAL)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
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
		bool isInvalid = IsTargetMissingOverride(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unreachable";
		return invalidity;
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

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		if (node.associatedJobType == JOB_TYPE.DEMON_STEAL && node.actor.partyComponent.isMemberThatJoinedQuest && !CanAPartyMemberReachTile(node.poiTarget.gridTileLocation, node.actor.partyComponent.currentParty))
		{
			node.actor.partyComponent.currentParty.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Unreachable"));
		}
	}

	private bool IsCarrierKnockedOut(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is TileObject { isBeingCarriedBy: not null } tileObject)
		{
			return tileObject.isBeingCarriedBy.traitContainer.HasTrait("Unconscious");
		}
		return false;
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
			if (tileObject.isBeingCarriedBy != null)
			{
				return !tileObject.isBeingCarriedBy.limiterComponent.canPerform;
			}
			return false;
		}
		return false;
	}

	public void AfterStealSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject, changeCharacterOwnership: false, setOwnership: false, equipItem: false);
		if (goapNode.actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			goapNode.actor.needsComponent.AdjustHappiness(10f);
		}
		if (goapNode.associatedJobType == JOB_TYPE.DEMON_STEAL && goapNode.poiTarget is ResourcePile resourcePile)
		{
			resourcePile.SetCharacterOwner(null);
		}
	}

	private bool CanAPartyMemberReachTile(LocationGridTile tileLocation, Party party)
	{
		for (int i = 0; i < party.members.Count; i++)
		{
			if (party.members[i].movementComponent.HasPathToEvenIfDiffRegion(tileLocation))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsTargetMissingOverride(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		bool num = poiTarget.IsAvailable();
		LocationGridTile locationGridTile = poiTarget.gridTileLocation;
		if (locationGridTile == null)
		{
			locationGridTile = poiTarget.isBeingCarriedBy?.gridTileLocation;
		}
		if ((!num && !base.canBeAdvertisedEvenIfTargetIsUnavailable) || (locationGridTile == null && !base.canBePerformedEvenIfTargetHasNoTileLocation))
		{
			return true;
		}
		if (actor.gridTileLocation != locationGridTile && !actor.gridTileLocation.IsNeighbour(locationGridTile, sameStructureOnly: true))
		{
			if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
