using JetBrains.Annotations;

public class Dig : GoapAction
{
	public Dig()
		: base(INTERACTION_TYPE.DIG)
	{
		base.actionIconString = GoapActionStateDB.Bury_Icon;
		base.canBePerformedEvenIfPathImpossible = true;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Dig Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 0;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return target.gridTileLocation != null;
		}
		return false;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		_ = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissingDig(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unavailable";
		return invalidity;
	}

	[UsedImplicitly]
	public void AfterDigSuccess(ActualGoapNode goapNode)
	{
		goapNode.poiTarget.AdjustHP(-goapNode.poiTarget.maxHP, ELEMENTAL_TYPE.Normal, triggerDeath: true, null, null, showHPBar: false, 0f, isPlayerSource: false, isTrueDamage: true);
	}

	private bool IsTargetMissingDig(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if ((!poiTarget.IsAvailable() && !base.canBeAdvertisedEvenIfTargetIsUnavailable) || poiTarget.gridTileLocation == null)
		{
			return true;
		}
		if (base.actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE && actor.currentRegion != poiTarget.gridTileLocation.structure.region)
		{
			return true;
		}
		if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET)
		{
			if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation))
			{
				return true;
			}
		}
		else if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET && actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile))
		{
			return true;
		}
		return false;
	}
}
