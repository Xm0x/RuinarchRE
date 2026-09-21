using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class StealthTransform : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public StealthTransform()
		: base(INTERACTION_TYPE.STEALTH_TRANSFORM)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Crimes,
			LOG_TAG.Life_Changes
		};
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1)
		{
			if (otherData[0].obj is LocationStructure result)
			{
				return result;
			}
			if (otherData[0].obj is Area area)
			{
				return area.primaryStructureInArea;
			}
			if (otherData[0].obj is BaseSettlement baseSettlement)
			{
				if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE)
				{
					return baseSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
				}
				if (baseSettlement.allStructures.Count > 0)
				{
					return baseSettlement.allStructures[0];
				}
			}
		}
		return null;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1)
		{
			if (otherData[0].obj is LocationStructure locationStructure)
			{
				if (locationStructure.passableTiles.Count > 0)
				{
					return locationStructure.GetRandomPassableTile();
				}
				return locationStructure.GetRandomTile();
			}
			if (otherData[0].obj is Area area)
			{
				return area.GetRandomPassableTile();
			}
			if (otherData[0].obj is BaseSettlement baseSettlement)
			{
				if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE)
				{
					List<Area> list = RuinarchListPool<Area>.Claim();
					baseSettlement.PopulateSurroundingAreas(list);
					return list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)].GetRandomPassableTile();
				}
				return baseSettlement.GetRandomPassableTile();
			}
		}
		return base.GetTargetTileToGoTo(goapNode);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Transform Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void AfterTransformSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.GetTraitOrStatus<Lycanthrope>("Lycanthrope")?.CheckIfAlone();
	}
}
