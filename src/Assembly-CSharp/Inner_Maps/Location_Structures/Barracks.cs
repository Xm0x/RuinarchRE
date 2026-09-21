using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Barracks : ManMadeStructure
{
	private LocationGridTile _unseizedObjectFrom;

	private const int Needed_Dummies = 3;

	private const int Needed_Archery_Target = 2;

	public Barracks(Region location)
		: base(STRUCTURE_TYPE.BARRACKS, location)
	{
		SetMaxHPAndReset(8000);
	}

	public Barracks(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(8000);
	}

	public override bool RemovePOI(IPointOfInterest poi, Character removedBy = null, bool isPlayerSource = false)
	{
		LocationGridTile removedFrom = null;
		TileObject removedObject = null;
		NPCSettlement npcSettlement = null;
		Sprite visual = null;
		Quaternion rotation = Quaternion.identity;
		TryGetNeededDataForCraftingOnPOIRemoved(poi, ref removedFrom, ref removedObject, ref npcSettlement, ref visual, ref rotation);
		bool num = base.RemovePOI(poi, removedBy, isPlayerSource);
		if (num && npcSettlement != null)
		{
			TryCreateCraftObjectJob(removedFrom, removedObject, npcSettlement, visual, rotation);
		}
		return num;
	}

	public override bool RemovePOIWithoutDestroying(IPointOfInterest poi)
	{
		bool flag;
		if (poi.isBeingSeized)
		{
			_unseizedObjectFrom = poi.gridTileLocation;
			flag = base.RemovePOIWithoutDestroying(poi);
			Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnPOIUnseized);
		}
		else
		{
			LocationGridTile removedFrom = null;
			TileObject removedObject = null;
			NPCSettlement npcSettlement = null;
			Sprite visual = null;
			Quaternion rotation = Quaternion.identity;
			TryGetNeededDataForCraftingOnPOIRemoved(poi, ref removedFrom, ref removedObject, ref npcSettlement, ref visual, ref rotation);
			flag = base.RemovePOIWithoutDestroying(poi);
			if (flag && npcSettlement != null)
			{
				TryCreateCraftObjectJob(removedFrom, removedObject, npcSettlement, visual, rotation);
			}
		}
		return flag;
	}

	public override bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null)
	{
		bool flag;
		if (poi.isBeingSeized)
		{
			_unseizedObjectFrom = poi.gridTileLocation;
			flag = base.RemovePOIDestroyVisualOnly(poi, (Character)null);
			Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnPOIUnseized);
		}
		else
		{
			LocationGridTile removedFrom = null;
			TileObject removedObject = null;
			NPCSettlement npcSettlement = null;
			Sprite visual = null;
			Quaternion rotation = Quaternion.identity;
			TryGetNeededDataForCraftingOnPOIRemoved(poi, ref removedFrom, ref removedObject, ref npcSettlement, ref visual, ref rotation);
			flag = base.RemovePOIDestroyVisualOnly(poi, remover);
			if (flag && npcSettlement != null)
			{
				TryCreateCraftObjectJob(removedFrom, removedObject, npcSettlement, visual, rotation);
			}
		}
		return flag;
	}

	public override bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null)
	{
		bool num = base.AddPOI(poi, tileLocation);
		if (num && poi is TileObject { mapObjectState: MAP_OBJECT_STATE.BUILT })
		{
			Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, JOB_TYPE.CRAFT_OBJECT);
		}
		return num;
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		if (_unseizedObjectFrom != null)
		{
			Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnPOIUnseized);
			_unseizedObjectFrom = null;
		}
	}

	private void OnPOIUnseized(IPointOfInterest p_poi)
	{
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnPOIUnseized);
		if (p_poi is TileObject { gridTileLocation: not null } tileObject && tileObject.gridTileLocation.structure != this)
		{
			LocationGridTile removedFrom = null;
			TileObject removedObject = null;
			NPCSettlement npcSettlement = null;
			Sprite visual = null;
			Quaternion rotation = Quaternion.identity;
			TryGetNeededDataForCraftingOnPOIRemoved(p_poi, ref removedFrom, ref removedObject, ref npcSettlement, ref visual, ref rotation);
			removedFrom = _unseizedObjectFrom;
			if (npcSettlement != null)
			{
				TryCreateCraftObjectJob(removedFrom, removedObject, npcSettlement, visual, rotation);
			}
			_unseizedObjectFrom = null;
		}
	}

	private void TryGetNeededDataForCraftingOnPOIRemoved(IPointOfInterest poi, ref LocationGridTile removedFrom, ref TileObject removedObject, ref NPCSettlement npcSettlement, ref Sprite visual, ref Quaternion rotation)
	{
		if (poi is TileObject { gridTileLocation: not null } tileObject && base.settlementLocation is NPCSettlement nPCSettlement)
		{
			removedFrom = tileObject.gridTileLocation;
			removedObject = tileObject;
			npcSettlement = nPCSettlement;
			visual = tileObject.mapObjectVisual.usedSprite;
			rotation = tileObject.mapObjectVisual.rotation;
		}
	}

	private void TryCreateCraftObjectJob(LocationGridTile removedFrom, TileObject removedObject, NPCSettlement npcSettlement, Sprite visual, Quaternion rotation)
	{
		if (!base.hasBeenDestroyed && removedObject.mapObjectState == MAP_OBJECT_STATE.BUILT && !HasEnoughOfObject(removedObject) && (removedObject is TrainingDummy || removedObject is ArcheryTarget))
		{
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(removedObject.tileObjectType);
			AddPOI(tileObject, removedFrom);
			tileObject.mapObjectVisual.SetVisual(visual);
			tileObject.mapObjectVisual.SetRotation(rotation);
			tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
			TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(removedObject.tileObjectType);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, tileObject, npcSettlement);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(npcSettlement, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			tileObjectData.TryGetPossibleRecipe(npcSettlement, out var possibleRecipe);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { possibleRecipe });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CRAFT_TILE_OBJECT, new object[1] { possibleRecipe });
			goapPlanJob.SetStillApplicableChecker("IsBarracksCraftStillApplicable");
			npcSettlement.AddToAvailableJobs(goapPlanJob);
		}
	}

	public bool HasEnoughOfObject(TileObject p_tileObject)
	{
		return p_tileObject.tileObjectType switch
		{
			TILE_OBJECT_TYPE.TRAINING_DUMMY => GetNumberOfBuiltTileObjects(p_tileObject.tileObjectType) >= 3, 
			TILE_OBJECT_TYPE.ARCHERY_TARGET => GetNumberOfBuiltTileObjects(p_tileObject.tileObjectType) >= 2, 
			_ => true, 
		};
	}
}
