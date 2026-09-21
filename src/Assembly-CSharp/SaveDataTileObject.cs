using System;
using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class SaveDataTileObject : SaveData<TileObject>, ISavableCounterpart
{
	public int id;

	public string name;

	public string internalName;

	public TILE_OBJECT_TYPE tileObjectType;

	public string characterOwnerID;

	public TileLocationSave tileLocationID;

	public bool isPreplaced;

	public string isBeingCarriedByID;

	public POI_STATE poiState;

	public List<INTERACTION_TYPE> advertisedActions;

	public MAP_OBJECT_STATE mapObjectState;

	public bool isDamageContributorToStructure;

	public bool isStoredAsTarget;

	public bool isDeadReference;

	public bool isBuiltByPlayerBaseBuilding;

	public string lastStolenByID;

	public SaveDataResourceStorageComponent resourceStorageComponent;

	public int currentHP;

	public int maxHP;

	public int spriteIndex;

	public QuaternionSave rotation;

	public SaveDataTraitContainer saveDataTraitContainer;

	public SaveDataLogComponent logComponent;

	public SaveDataTileObjectHiddenComponent hiddenComponent;

	public SaveDataTileObjectConstructionComponent constructionComponent;

	public SaveDataTileObjectPartyComponent partyComponent;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Tile_Object;

	public override void Save(TileObject data)
	{
		persistentID = data.persistentID;
		id = data.id;
		name = data.name;
		internalName = data.internalName;
		tileObjectType = data.tileObjectType;
		characterOwnerID = data.characterOwner?.persistentID ?? string.Empty;
		if (data.gridTileLocation != null)
		{
			tileLocationID = new TileLocationSave(data.gridTileLocation);
		}
		else
		{
			tileLocationID = default(TileLocationSave);
		}
		isPreplaced = data.isPreplaced;
		poiState = data.state;
		isDamageContributorToStructure = data.isDamageContributorToStructure;
		isStoredAsTarget = data.isStoredAsTarget;
		isDeadReference = data.isDeadReference;
		isBuiltByPlayerBaseBuilding = data.isBuiltByPlayerBaseBuilding;
		lastStolenByID = data.lastStolenBy?.persistentID ?? string.Empty;
		List<INTERACTION_TYPE> list = RuinarchListPool<INTERACTION_TYPE>.Claim(data.advertisedActions.Count);
		if (data.advertisedActions != null && data.advertisedActions.Count > 0)
		{
			list.AddRange(data.advertisedActions);
		}
		advertisedActions = list;
		mapObjectState = data.mapObjectState;
		currentHP = data.currentHP;
		maxHP = data.maxHP;
		if (data.mapObjectVisual == null)
		{
			spriteIndex = -1;
			rotation = Quaternion.identity;
		}
		else
		{
			spriteIndex = data.mapObjectVisual.usedSpriteIndex;
			rotation = data.mapObjectVisual.rotation;
		}
		resourceStorageComponent = new SaveDataResourceStorageComponent();
		resourceStorageComponent.Save(data.resourceStorageComponent);
		isBeingCarriedByID = ((data.isBeingCarriedBy != null) ? data.isBeingCarriedBy.persistentID : string.Empty);
		saveDataTraitContainer = new SaveDataTraitContainer();
		saveDataTraitContainer.Save(data.traitContainer);
		logComponent = new SaveDataLogComponent();
		logComponent.Save(data.logComponent);
		hiddenComponent = new SaveDataTileObjectHiddenComponent();
		hiddenComponent.Save(data.hiddenComponent);
		constructionComponent = new SaveDataTileObjectConstructionComponent();
		constructionComponent.Save(data.constructionComponent);
		partyComponent = new SaveDataTileObjectPartyComponent();
		partyComponent.Save(data.partyComponent);
	}

	public override TileObject Load()
	{
		TileObject tileObject = InnerMapManager.Instance.LoadTileObject<TileObject>(this);
		tileObject.Initialize(this);
		return tileObject;
	}

	public override void CleanUp()
	{
		if (advertisedActions != null)
		{
			RuinarchListPool<INTERACTION_TYPE>.Release(advertisedActions);
			advertisedActions = null;
		}
		resourceStorageComponent?.CleanUp();
		resourceStorageComponent = null;
		saveDataTraitContainer?.CleanUp();
		saveDataTraitContainer = null;
		logComponent?.CleanUp();
		logComponent = null;
		hiddenComponent?.CleanUp();
		hiddenComponent = null;
		constructionComponent?.CleanUp();
		constructionComponent = null;
		partyComponent?.CleanUp();
		partyComponent = null;
	}
}
