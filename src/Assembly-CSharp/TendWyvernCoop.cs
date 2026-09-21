using Inner_Maps;
using Inner_Maps.Location_Structures;

public class TendWyvernCoop : GoapAction
{
	public TendWyvernCoop()
		: base(INTERACTION_TYPE.TEND_WYVERN_COOP)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Tend Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			LocationGridTile gridTileLocation = target.gridTileLocation;
			if (gridTileLocation != null)
			{
				return !gridTileLocation.structure.hasBeenDestroyed;
			}
		}
		return false;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget.gridTileLocation.structure.hasBeenDestroyed)
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "target_unavailable";
		}
		return goapActionInvalidity;
	}

	public void AfterTendSuccess(ActualGoapNode goapNode)
	{
		LocationStructure locationStructure = goapNode.target.gridTileLocation?.structure;
		if (locationStructure != null && locationStructure is WyvernCoop wyvernCoop)
		{
			wyvernCoop.TendCoopBy(goapNode.actor);
		}
	}
}
