using Inner_Maps;

public class TransformCentaur : GoapAction
{
	public TransformCentaur()
		: base(INTERACTION_TYPE.TRANSFORM_CENTAUR)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Transform Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && (node.poiTarget.gridTileLocation == null || node.poiTarget.isBeingSeized))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "target_missing";
		}
		return goapActionInvalidity;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.gridTileLocation != null)
			{
				return actor != poiTarget;
			}
			return false;
		}
		return false;
	}

	public void AfterTransformSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		LocationGridTile gridTileLocation = character.gridTileLocation;
		if (gridTileLocation != null)
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Centaur, actor.faction, actor.homeSettlement, homeStructure: actor.homeStructure, homeRegion: actor.homeRegion, className: "", bypassIdeologyChecking: true);
			summon.SetFirstName(character.name);
			CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
			character.TransferAllInventoryItemsAndEquippableEquipmentsTo(summon, transferOwnership: true);
			character.SetDestroyMarkerOnDeath(state: true);
			character.Death();
		}
	}
}
