using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class CraftLegendaryEquipment : GoapAction
{
	public CraftLegendaryEquipment()
		: base(INTERACTION_TYPE.CRAFT_LEGENDARY_EQUIPMENT)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string text = GetEquipmentToCreate(node).LocalizedName();
		log.AddToFillers(null, Utilities.GetArticleForWord(text), LOG_IDENTIFIER.STRING_1);
		log.AddToFillers(null, text, LOG_IDENTIFIER.ITEM_1);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Craft Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.IsAvailable())
			{
				return poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 1;
			}
			return false;
		}
		return false;
	}

	public void AfterCraftSuccess(ActualGoapNode p_node)
	{
		TileObject tileObject = p_node.target as TileObject;
		TILE_OBJECT_TYPE equipmentToCreate = GetEquipmentToCreate(p_node);
		EquipmentItem equipmentItem = InnerMapManager.Instance.CreateNewTileObject<EquipmentItem>(equipmentToCreate);
		if (equipmentItem.tileObjectType.IsStaff())
		{
			equipmentItem.AddRandomElementalElementPrefix();
		}
		else if (equipmentItem.tileObjectType.IsBow())
		{
			equipmentItem.AddRandomSecondaryElementPrefix();
		}
		p_node.actor.PickUpItem(equipmentItem, changeCharacterOwnership: false, setOwnership: false, equipItem: false);
		p_node.actor.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM_TO_WORKPLACE, equipmentItem, p_node.actor.structureComponent.workPlaceStructure);
		if (p_node.actor.structureComponent.workPlaceStructure is Workshop workshop)
		{
			workshop.RemoveFirstRequestThatIsFulfilledBy(tileObject);
		}
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Crafting).AdjustExperience(25, p_node.actor);
		tileObject.gridTileLocation?.structure.RemovePOI(tileObject);
	}

	private TILE_OBJECT_TYPE GetEquipmentToCreate(ActualGoapNode p_node)
	{
		OtherData[] otherData = p_node.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0] is StringOtherData stringOtherData && Enum.TryParse<TILE_OBJECT_TYPE>(stringOtherData.str, out var result))
		{
			return result;
		}
		throw new Exception("Could not get legendary equipment to create for " + p_node.actor.name);
	}
}
