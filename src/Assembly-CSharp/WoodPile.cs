using Inner_Maps;
using Inner_Maps.Location_Structures;

public class WoodPile : ResourcePile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Wood;

	public WoodPile()
		: base(RESOURCE.WOOD)
	{
		Initialize(TILE_OBJECT_TYPE.WOOD_PILE, shouldAddCommonAdvertisements: false);
		SetResourceInPile(100);
	}

	public WoodPile(SaveDataTileObject data)
		: base(data, RESOURCE.WOOD)
	{
	}

	public override string ToString()
	{
		return "Wood Pile " + base.id;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.LUMBERYARD)
		{
			AddAdvertisedAction(INTERACTION_TYPE.BUY_WOOD);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.BUY_WOOD);
		}
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		RemoveAdvertisedAction(INTERACTION_TYPE.BUY_WOOD);
	}

	protected override string GetFlavorText()
	{
		return LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "Basic Resource_Flavor_Text");
	}

	public override void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		base.GeneralReactionToTileObject(actor, ref debugLog);
		if (actor is Troll)
		{
			if (actor.homeStructure != null && gridTileLocation.structure != actor.homeStructure && !actor.jobQueue.HasJob(JOB_TYPE.DROP_ITEM))
			{
				actor.jobComponent.CreateHoardItemJob(this, actor.homeStructure, doNotRecalculate: true);
			}
		}
		else if (gridTileLocation != null && gridTileLocation.structure != null && actor.homeSettlement != null && actor.faction != null && actor.faction.isMajorNonPlayer && (actor.faction.factionType.mainResource == RESOURCE.WOOD || actor.faction.factionType.usesBothWoodAndStoneResources) && base.characterOwner == null && gridTileLocation.structure.structureType != STRUCTURE_TYPE.CITY_CENTER && gridTileLocation.structure.structureType != STRUCTURE_TYPE.WORKSHOP && gridTileLocation.structure.structureType != STRUCTURE_TYPE.LUMBERYARD && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.HAUL))
		{
			LocationStructure dropStructure = ((!(actor.structureComponent.workPlaceStructure is Lumberyard)) ? actor.homeSettlement.mainStorage : actor.structureComponent.workPlaceStructure);
			actor.jobComponent.TryCreateHaulJobForOnSightResourcePile(this, dropStructure);
		}
	}
}
