using System;
using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine;

public class SeizeComponent
{
	private Sprite _seizedPOISprite;

	private int _seizedPOIVisionTriggerVotes;

	private int _seizedCharacterVisionVotes;

	private bool _seizedPOIVisionTriggerState;

	private Vector3 followOffset;

	private Tween tween;

	public IPointOfInterest seizedPOI { get; private set; }

	public bool isPreparingToBeUnseized { get; private set; }

	public bool hasSeizedPOI => seizedPOI != null;

	public SeizeComponent()
	{
		followOffset = new Vector3(1f, -1f, 10f);
	}

	public void SeizePOI(IPointOfInterest poi)
	{
		if (seizedPOI != null)
		{
			return;
		}
		bool wasUnseizedFromCharacter = false;
		if (poi.isBeingCarriedBy != null)
		{
			wasUnseizedFromCharacter = true;
			if (poi.isBeingCarriedBy.carryComponent.isCarryingAnyPOI && poi.isBeingCarriedBy.carryComponent.carriedPOI == poi)
			{
				poi.isBeingCarriedBy.UncarryPOI(bringBackToInventory: false, addToLocation: true, null, isUncarriedFromSeize: true);
			}
			else
			{
				poi.isBeingCarriedBy.UncarryPOI(poi, bringBackToInventory: false, addToLocation: true, null, isUncarriedFromSeize: true);
			}
		}
		if (poi.gridTileLocation != null)
		{
			Messenger.Broadcast(CharacterSignals.BEFORE_SEIZING_POI, poi);
			seizedPOI = poi;
			_seizedPOISprite = poi.mapObjectVisual.GetSeizeSprite(poi);
			poi.mapObjectVisual.SetVisual(_seizedPOISprite);
			poi.mapObjectVisual.OnSeizeVisual(poi);
			if (poi is BaseMapObject baseMapObject)
			{
				baseMapObject.OnManipulatedBy(PlayerManager.Instance.player);
			}
			if (poi.mapObjectVisual.visionTrigger != null)
			{
				_seizedPOIVisionTriggerVotes = poi.mapObjectVisual.visionTrigger.filterVotes;
				_seizedPOIVisionTriggerState = poi.mapObjectVisual.visionTrigger.mainCollider.enabled;
			}
			if (seizedPOI is Character character)
			{
				_seizedCharacterVisionVotes = character.marker.visionColliderComponent.filterVotes;
			}
			poi.OnSeizePOI(wasUnseizedFromCharacter);
			Messenger.Broadcast(CharacterSignals.ON_SEIZE_POI, poi);
			PrepareToUnseize();
			Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
		}
		else
		{
			Debug.LogError("Cannot seize. " + poi.name + " has no tile");
		}
	}

	private void PrepareToUnseize()
	{
		isPreparingToBeUnseized = true;
		PlayerManager.Instance.AddPlayerInputModule(PlayerManager.seizeInputModule);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	private void DoneUnseize()
	{
		isPreparingToBeUnseized = false;
		PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.seizeInputModule);
		Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click)
		{
			TryToUnseize();
		}
	}

	private void TryToUnseize()
	{
		if (isPreparingToBeUnseized && UnseizePOI())
		{
			DoneUnseize();
		}
	}

	private bool UnseizePOI()
	{
		if (!CanUnseize())
		{
			return false;
		}
		IPointOfInterest currentlyHoveredPoi = InnerMapManager.Instance.currentlyHoveredPoi;
		LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
		if (!CanUnseizeHere(tileFromMousePosition, currentlyHoveredPoi))
		{
			return false;
		}
		if (currentlyHoveredPoi is Character targetCharacter && seizedPOI is TileObject seizedObject)
		{
			UnseizeTileObjectOnACharacter(seizedObject, targetCharacter);
		}
		else
		{
			UnseizePOIBase(tileFromMousePosition);
		}
		return true;
	}

	public void UnseizePOIOnCharacterDeath()
	{
		if (seizedPOI != null)
		{
			UnseizePOIBase(seizedPOI.gridTileLocation);
			DoneUnseize();
		}
	}

	public void UnseizeTileObjectThenDestroyIt()
	{
		if (seizedPOI != null)
		{
			LocationGridTile firstUnoccupiedGridTileInWilderness = GridMap.Instance.mainRegion.GetFirstUnoccupiedGridTileInWilderness();
			if (firstUnoccupiedGridTileInWilderness == null)
			{
				throw new Exception("Trying to unseize " + seizedPOI.name + " on destroy but there are no unoccupied tiles in the region");
			}
			IPointOfInterest poi = seizedPOI;
			UnseizePOIBase(firstUnoccupiedGridTileInWilderness);
			DoneUnseize();
			firstUnoccupiedGridTileInWilderness.structure.RemovePOI(poi);
		}
	}

	private void UnseizeTileObjectOnACharacter(TileObject seizedObject, Character targetCharacter)
	{
		LocationGridTile firstUnoccupiedGridTileInWilderness = GridMap.Instance.mainRegion.GetFirstUnoccupiedGridTileInWilderness();
		if (firstUnoccupiedGridTileInWilderness != null)
		{
			UnseizePOIBase(firstUnoccupiedGridTileInWilderness, broadcastReloadPlayerActions: false);
			targetCharacter.PickUpItem(seizedObject);
			Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
			return;
		}
		throw new Exception("Trying to unseize " + seizedPOI.name + " on destroy but there are no unoccupied tiles in the region");
	}

	private void UnseizePOIBase(LocationGridTile tileLocation, bool broadcastReloadPlayerActions = true)
	{
		IPointOfInterest pointOfInterest = seizedPOI;
		DisableFollowMousePosition();
		seizedPOI = null;
		pointOfInterest.OnUnseizePOI(tileLocation);
		if (pointOfInterest.mapObjectVisual != null)
		{
			pointOfInterest.mapObjectVisual.SetVisual(_seizedPOISprite);
			if (pointOfInterest.mapObjectVisual.visionTrigger != null)
			{
				pointOfInterest.mapObjectVisual.visionTrigger.SetFilterVotes(_seizedPOIVisionTriggerVotes);
				pointOfInterest.mapObjectVisual.visionTrigger.SetVisionTriggerCollidersState(_seizedPOIVisionTriggerState);
			}
		}
		if (pointOfInterest is Character character)
		{
			character.marker.visionColliderComponent.SetFilterVisionVotes(_seizedCharacterVisionVotes);
		}
		_seizedPOISprite = null;
		_seizedPOIVisionTriggerVotes = 0;
		_seizedCharacterVisionVotes = 0;
		_seizedPOIVisionTriggerState = false;
		Messenger.Broadcast(CharacterSignals.ON_UNSEIZE_POI, pointOfInterest);
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		if (broadcastReloadPlayerActions)
		{
			Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
		}
	}

	public bool CanUnseize()
	{
		if (!hasSeizedPOI)
		{
			return false;
		}
		if (!InnerMapManager.Instance.isAnInnerMapShowing || UIManager.Instance.IsMouseOnUI())
		{
			return false;
		}
		return true;
	}

	public bool CanUnseizeHere(LocationGridTile tileLocation, IPointOfInterest hoveredPOI = null)
	{
		if (hoveredPOI is Character character && seizedPOI is TileObject tileObject)
		{
			if (tileObject is EquipmentItem equipmentItem && (character.equipmentComponent.HasEquipmentInSlotFor(equipmentItem) || !character.equipmentComponent.CanEquipItem(equipmentItem, character)))
			{
				return false;
			}
			return tileObject.canBeDirectlyUnseizedToCharacter;
		}
		if (tileLocation == null)
		{
			return false;
		}
		if (!tileLocation.IsPassable())
		{
			return false;
		}
		if (!tileLocation.HasWalkableNode())
		{
			return false;
		}
		if (tileLocation.IsTileConsideredProtected(0))
		{
			return false;
		}
		if (seizedPOI.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT && tileLocation.tileObjectComponent.objHere != null)
		{
			return false;
		}
		if (seizedPOI is MonsterSpawner && tileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS && tileLocation.structure.HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER))
		{
			return false;
		}
		if (tileLocation.structure is Kennel kennel)
		{
			if (tileLocation.structure.IsTilePartOfARoom(tileLocation, out var _))
			{
				if (seizedPOI is Summon summon)
				{
					if (!summon.isDead && !kennel.HasReachedKennelCapacity() && (kennel.preOccupiedBy == null || kennel.preOccupiedBy == seizedPOI))
					{
						if (summon.faction != null)
						{
							return !summon.faction.isPlayerFaction;
						}
						return true;
					}
					return false;
				}
				return false;
			}
			return false;
		}
		if (tileLocation.structure is TortureChambers tortureChambers)
		{
			if (tileLocation.structure.IsTilePartOfARoom(tileLocation, out var room2))
			{
				if (seizedPOI is Character character2)
				{
					if (room2.CanUnseizeCharacterInRoom(character2))
					{
						if (tortureChambers.preOccupiedBy != null)
						{
							return tortureChambers.preOccupiedBy == seizedPOI;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private int GetManaCost(IPointOfInterest poi)
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			return 50;
		}
		return 20;
	}

	public void EnableFollowMousePosition()
	{
		if (!seizedPOI.visualGO.activeSelf)
		{
			seizedPOI.visualGO.transform.position = InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(InputManager.Instance.mousePosition) + followOffset;
			seizedPOI.visualGO.SetActive(value: true);
		}
	}

	public void FollowMousePosition()
	{
		if (seizedPOI.visualGO.activeSelf && InnerMapManager.Instance.isAnInnerMapShowing)
		{
			Vector3 position = InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(InputManager.Instance.mousePosition) + followOffset;
			iTween.MoveUpdate(seizedPOI.visualGO, position, 0.5f);
		}
	}

	public void DisableFollowMousePosition()
	{
		if (seizedPOI.visualGO.activeSelf)
		{
			seizedPOI.visualGO.SetActive(value: false);
			iTween.Stop(seizedPOI.visualGO);
		}
	}

	public void LoadSeizedPOI(SaveDataPlayerGame p_data)
	{
		if (string.IsNullOrEmpty(p_data.seizedPOIID))
		{
			return;
		}
		IPointOfInterest pointOfInterest = null;
		if (p_data.seizedPOIType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			pointOfInterest = CharacterManager.Instance.GetCharacterByPersistentID(p_data.seizedPOIID);
			if (pointOfInterest is Character character)
			{
				character.marker.pathfindingAI.UpdateMe();
			}
		}
		else
		{
			LocationGridTile firstPassableUnoccupiedGridTile = GridMap.Instance.GetFirstPassableUnoccupiedGridTile();
			if (firstPassableUnoccupiedGridTile != null)
			{
				pointOfInterest = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(p_data.seizedPOIID);
				if (pointOfInterest != null)
				{
					TileObject tileObject = pointOfInterest as TileObject;
					SaveDataTileObject fromSaveHub = SaveManager.Instance.currentSaveDataProgress.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, tileObject.persistentID);
					if (tileObject is Tombstone tombstone)
					{
						tileObject.LoadSecondWave(fromSaveHub);
						if (fromSaveHub.tileLocationID.hasValue)
						{
							if (tombstone.character == null || tombstone.character.marker == null)
							{
								Debug.LogWarning($"{tombstone} with persistent id {tombstone.persistentID} does not have a character inside it, but has a tile location. Not placing it to prevent errors, but this case should not happen!");
								return;
							}
							firstPassableUnoccupiedGridTile.structure.LoadPOI(tileObject, firstPassableUnoccupiedGridTile);
							if (tileObject.mapObjectVisual != null)
							{
								TileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
								tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject.GetSpriteByIndex(fromSaveHub.spriteIndex), fromSaveHub.spriteIndex);
								tileObject.mapObjectVisual.SetRotation(fromSaveHub.rotation);
							}
						}
					}
					else
					{
						firstPassableUnoccupiedGridTile.structure.LoadPOI(tileObject, firstPassableUnoccupiedGridTile);
						if (tileObject.mapObjectVisual != null)
						{
							TileObjectScriptableObject tileObjectScriptableObject2 = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
							Sprite spriteByIndex = tileObjectScriptableObject2.GetSpriteByIndex(fromSaveHub.spriteIndex);
							if (spriteByIndex == null)
							{
								tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject2.defaultSprite);
								if (tileObjectScriptableObject2.defaultSprite != null && tileObject is Table)
								{
									tileObject.RevalidateTileObjectSlots();
								}
							}
							else
							{
								tileObject.mapObjectVisual.SetVisual(spriteByIndex);
								if (tileObject is Table)
								{
									tileObject.RevalidateTileObjectSlots();
								}
							}
							tileObject.mapObjectVisual.SetRotation(fromSaveHub.rotation);
						}
						tileObject.LoadSecondWave(fromSaveHub);
						tileObject.LoadAdditionalInfo(fromSaveHub);
					}
				}
			}
		}
		if (pointOfInterest != null)
		{
			PlayerManager.Instance.player.seizeComponent.SeizePOI(pointOfInterest);
		}
	}
}
