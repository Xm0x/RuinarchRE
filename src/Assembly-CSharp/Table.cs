using System;
using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class Table : TileObject
{
	public int food => base.resourceStorageComponent.storedResources[RESOURCE.FOOD];

	public CONCRETE_RESOURCES lastAddedFoodType { get; private set; }

	public override Type serializedData => typeof(SaveDataTable);

	public Table()
	{
		Initialize(TILE_OBJECT_TYPE.TABLE);
		AddAdvertisedAction(INTERACTION_TYPE.DRINK);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_RESOURCE);
		AddAdvertisedAction(INTERACTION_TYPE.SIT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		SetFood(CONCRETE_RESOURCES.Animal_Meat, UnityEngine.Random.Range(20, 81));
		base.traitContainer.AddTrait(this, "Edible");
	}

	public Table(SaveDataTileObject data)
		: base(data)
	{
		SaveDataTable saveDataTable = data as SaveDataTable;
		lastAddedFoodType = saveDataTable.lastAddedFoodType;
	}

	public override void SetPOIState(POI_STATE state)
	{
		base.SetPOIState(state);
	}

	protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true)
	{
		base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
	}

	public override string ToString()
	{
		return "Table " + base.id;
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		INTERACTION_TYPE goapType = action.goapType;
		if (goapType == INTERACTION_TYPE.EAT || goapType == INTERACTION_TYPE.DRINK || goapType == INTERACTION_TYPE.SIT)
		{
			AddUser(action.actor);
		}
	}

	public override void OnDoneActionToObject(ActualGoapNode action)
	{
		base.OnDoneActionToObject(action);
		INTERACTION_TYPE goapType = action.goapType;
		if (goapType == INTERACTION_TYPE.EAT || goapType == INTERACTION_TYPE.DRINK || goapType == INTERACTION_TYPE.SIT)
		{
			RemoveUser(action.actor);
		}
	}

	public override void OnCancelActionTowardsObject(ActualGoapNode action)
	{
		base.OnCancelActionTowardsObject(action);
		INTERACTION_TYPE goapType = action.goapType;
		if (goapType == INTERACTION_TYPE.EAT || goapType == INTERACTION_TYPE.DRINK || goapType == INTERACTION_TYPE.SIT)
		{
			RemoveUser(action.actor);
		}
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		base.OnPlaceTileObjectAtTile(tile);
		if (mapVisual.usedSprite.name.Contains("bartop"))
		{
			mapVisual.InitializeGUS(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), tile);
		}
		else
		{
			mapVisual.InitializeGUS(Vector2.zero, new Vector2(0.5f, 0.5f), tile);
		}
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		mapVisual.DestroyExistingGUS();
	}

	public override string GetAdditionalTestingData()
	{
		return string.Concat(base.GetAdditionalTestingData() + "\n\tFood in Table: " + food, "\n\tLast Added Food Type: ", lastAddedFoodType.ToString());
	}

	protected override void OnSetObjectAsUnbuilt()
	{
		base.OnSetObjectAsUnbuilt();
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	protected override void OnSetObjectAsBuilt()
	{
		base.OnSetObjectAsBuilt();
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	public bool CanAccommodateCharacter(Character p_character)
	{
		return HasUnoccupiedSlot();
	}

	public void AdjustFood(CONCRETE_RESOURCES p_foodType, int p_amount)
	{
		lastAddedFoodType = p_foodType;
		base.resourceStorageComponent.AdjustResource(p_foodType, p_amount);
		if (gridTileLocation != null && base.structureLocation is Dwelling)
		{
			Messenger.Broadcast(StructureSignals.FOOD_IN_DWELLING_CHANGED, this);
		}
	}

	public void SetFood(CONCRETE_RESOURCES p_foodType, int p_amount)
	{
		lastAddedFoodType = p_foodType;
		base.resourceStorageComponent.SetResource(p_foodType, p_amount);
		if (gridTileLocation != null && base.structureLocation is Dwelling)
		{
			Messenger.Broadcast(StructureSignals.FOOD_IN_DWELLING_CHANGED, this);
		}
	}

	public void ApplyFoodEffectsToConsumer(Character p_consumer)
	{
		switch (lastAddedFoodType)
		{
		case CONCRETE_RESOURCES.Corn:
			p_consumer.traitContainer.AddTrait(p_consumer, "Corn Fed");
			break;
		case CONCRETE_RESOURCES.Potato:
			p_consumer.traitContainer.AddTrait(p_consumer, "Potato Fed");
			break;
		case CONCRETE_RESOURCES.Pineapple:
			p_consumer.traitContainer.AddTrait(p_consumer, "Pineapple Fed");
			break;
		case CONCRETE_RESOURCES.Iceberry:
			p_consumer.traitContainer.AddTrait(p_consumer, "Iceberry Fed");
			break;
		case CONCRETE_RESOURCES.Fish:
			p_consumer.traitContainer.AddTrait(p_consumer, "Fish Fed");
			break;
		case CONCRETE_RESOURCES.Animal_Meat:
			p_consumer.traitContainer.AddTrait(p_consumer, "Animal Fed");
			break;
		}
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (!actor.partyComponent.isMemberThatJoinedQuest)
		{
			TryCreateObtainFurnitureWantOnReactionJob<TableWant>(actor);
		}
	}
}
