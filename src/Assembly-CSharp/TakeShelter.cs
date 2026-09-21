using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class TakeShelter : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public TakeShelter()
		: base(INTERACTION_TYPE.TAKE_SHELTER)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		base.actionIconString = GoapActionStateDB.Cowering_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 2 && otherData[0].obj is LocationStructure)
		{
			return otherData[0].obj as LocationStructure;
		}
		return null;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 2)
		{
			if (otherData[0].obj is LocationStructure locationStructure)
			{
				log.AddToFillers(locationStructure, locationStructure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
			}
			if (otherData[1] is StringOtherData stringOtherData)
			{
				log.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait(stringOtherData.str), LOG_IDENTIFIER.STRING_1);
			}
		}
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Take Shelter Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void AfterTakeShelterSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.gridTileLocation != null && (goapNode.actor.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT) || goapNode.actor.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT)))
		{
			if (goapNode.actor.traitContainer.HasTrait("Freezing"))
			{
				goapNode.actor.traitContainer.GetTraitOrStatus<Freezing>("Freezing").SetCurrentShelterStructure(goapNode.targetStructure);
			}
			if (goapNode.actor.traitContainer.HasTrait("Overheating"))
			{
				goapNode.actor.traitContainer.GetTraitOrStatus<Overheating>("Overheating").SetCurrentShelterStructure(goapNode.targetStructure);
			}
			goapNode.actor.trapStructure.SetForcedStructure(goapNode.targetStructure);
		}
	}
}
