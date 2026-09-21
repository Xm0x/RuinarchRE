using Characters.Villager_Wants;
using Inner_Maps;

public class HealingPotion : TileObject
{
	public HealingPotion()
	{
		Initialize(TILE_OBJECT_TYPE.HEALING_POTION, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
	}

	public HealingPotion(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation != null && (gridTileLocation.structure.structureType == STRUCTURE_TYPE.HOSPICE || gridTileLocation.structure.structureType == STRUCTURE_TYPE.TAVERN))
		{
			AddAdvertisedAction(INTERACTION_TYPE.BUY_ITEM);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.BUY_ITEM);
		}
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		RemoveAdvertisedAction(INTERACTION_TYPE.BUY_ITEM);
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (actor.villagerWantsComponent != null && actor.villagerWantsComponent.IsWantToggledOn<HealingPotionWant>() && !actor.jobQueue.HasJob(JOB_TYPE.OBTAIN_WANTED_ITEM) && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.OBTAIN_WANTED_ITEM) && !actor.movementComponent.structuresToAvoid.Contains(base.structureLocation))
		{
			bool flag = false;
			if ((!base.structureLocation.structureType.IsVillageStructure() || base.structureLocation.structureType == STRUCTURE_TYPE.CITY_CENTER || base.structureLocation.structureType == STRUCTURE_TYPE.CEMETERY) ? (base.characterOwner == null || IsOwnedBy(actor)) : (IsOwnedBy(actor) && base.structureLocation != actor.homeStructure))
			{
				actor.jobComponent.CreateTakeItemOnSightJob(this, JOB_TYPE.OBTAIN_WANTED_ITEM);
			}
		}
	}
}
