using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DropCorpse : GoapAction
{
	public DropCorpse()
		: base(INTERACTION_TYPE.DROP_CORPSE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Corpse", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory);
	}

	public override void Perform(ActualGoapNode actionNode)
	{
		base.Perform(actionNode);
		SetState("Drop Success", actionNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null)
		{
			if (otherData.Length == 1 && otherData[0].obj is LocationStructure)
			{
				return otherData[0].obj as LocationStructure;
			}
			if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
			{
				return otherData[0].obj as LocationStructure;
			}
		}
		return base.GetTargetStructure(node);
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
		{
			return otherData[1].obj as LocationGridTile;
		}
		return null;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		actor.UncarryPOI(poiTarget);
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		actor.UncarryPOI(poiTarget);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		_ = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = IsDropTargetMissing(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unavailable";
		return invalidity;
	}

	private bool IsDropTargetMissing(ActualGoapNode node)
	{
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget.isBeingSeized || (poiTarget.gridTileLocation == null && !node.actor.IsPOICarriedOrInInventory(poiTarget)))
		{
			return true;
		}
		return false;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor == poiTarget)
			{
				return false;
			}
			if (otherData != null)
			{
				if (otherData.Length == 1 && otherData[0].obj is LocationStructure locationStructure)
				{
					return actor.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(locationStructure.passableTiles));
				}
				if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile toTile)
				{
					return actor.movementComponent.HasPathToEvenIfDiffRegion(toTile);
				}
			}
			return true;
		}
		return false;
	}

	private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.IsPOICarriedOrInInventory(poiTarget);
	}

	public void AfterDropSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		LocationGridTile dropLocation = null;
		if (otherData != null && otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
		{
			dropLocation = otherData[1].obj as LocationGridTile;
		}
		goapNode.actor.UncarryPOI(goapNode.poiTarget, bringBackToInventory: false, addToLocation: true, dropLocation);
	}
}
