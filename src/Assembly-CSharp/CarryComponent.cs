using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class CarryComponent : CharacterComponent
{
	private Character _isBeingCarriedBy;

	public IPointOfInterest carriedPOI { get; private set; }

	public Character isBeingCarriedBy
	{
		get
		{
			if (base.owner.grave != null && base.owner.grave.isBeingCarriedBy != null)
			{
				return base.owner.grave.isBeingCarriedBy;
			}
			return _isBeingCarriedBy;
		}
		private set
		{
			_isBeingCarriedBy = value;
		}
	}

	public Character prevCarriedBy { get; private set; }

	public bool isCarryingAnyPOI => carriedPOI != null;

	public Character masterCharacter
	{
		get
		{
			if (isBeingCarriedBy == null)
			{
				return base.owner;
			}
			return isBeingCarriedBy;
		}
	}

	public Character baseIsBeingCarriedBy => _isBeingCarriedBy;

	public CarryComponent()
	{
	}

	public CarryComponent(SaveDataCarryComponent data)
	{
	}

	public void SetIsBeingCarriedBy(Character carrier)
	{
		if (isBeingCarriedBy == carrier)
		{
			return;
		}
		prevCarriedBy = isBeingCarriedBy;
		isBeingCarriedBy = carrier;
		if ((bool)base.owner.marker)
		{
			if (isBeingCarriedBy != null)
			{
				base.owner.marker.visionTrigger?.SetAllCollidersState(state: false);
			}
			else
			{
				base.owner.marker.visionTrigger?.SetAllCollidersState(state: true);
			}
			base.owner.marker.UpdateAnimation();
		}
	}

	public bool CarryPOI(IPointOfInterest poi, bool isOwner = false, bool isFromSave = false)
	{
		if (isCarryingAnyPOI && poi != carriedPOI)
		{
			if (carriedPOI is TileObject item && base.owner.HasItemOrEquipment(item))
			{
				base.owner.DropItem(item);
			}
			else
			{
				UncarryPOI(carriedPOI);
			}
		}
		if (poi is Character)
		{
			return CarryCharacter(poi as Character, isOwner, isFromSave);
		}
		if (poi is TileObject)
		{
			return CarryTileObject(poi as TileObject);
		}
		return false;
	}

	private bool CarryTileObject(TileObject tileObject)
	{
		if (carriedPOI == null)
		{
			carriedPOI = tileObject;
			if (tileObject.gridTileLocation != null)
			{
				tileObject.gridTileLocation.structure.RemovePOIWithoutDestroying(tileObject);
			}
			if (tileObject.mapVisual == null)
			{
				tileObject.InitializeMapObject(tileObject);
			}
			tileObject.visionTrigger.SetAllCollidersState(state: false);
			Transform transform = tileObject.mapVisual.transform;
			transform.SetParent(base.owner.marker.visualsParent);
			transform.localPosition = new Vector3(0f, 0.5f, 0f);
			transform.eulerAngles = Vector3.zero;
			tileObject.mapVisual.UpdateSortingOrders(tileObject);
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)tileObject);
			return true;
		}
		return false;
	}

	private bool CarryCharacter(Character character, bool isOwner, bool isFromSave)
	{
		if (carriedPOI == null)
		{
			if (character == base.owner)
			{
				return false;
			}
			if (isBeingCarriedBy != null && isBeingCarriedBy == character)
			{
				return false;
			}
			character.eventDispatcher.ExecuteCarried(character, base.owner);
			carriedPOI = character;
			character.carryComponent.SetIsBeingCarriedBy(base.owner);
			character.SetGridTileLocation(base.owner.gridTileLocation);
			character.SetCurrentStructureLocation(base.owner.currentStructure);
			if (!character.marker)
			{
				character.CreateMarker();
			}
			character.marker.transform.SetParent(base.owner.marker.visualsParent);
			character.marker.transform.localPosition = new Vector3(0f, 0.5f, 0f);
			character.marker.visualsParent.eulerAngles = Vector3.zero;
			character.marker.transform.eulerAngles = Vector3.zero;
			return true;
		}
		return false;
	}

	public void UncarryPOI(IPointOfInterest poi, bool addToLocation = true, LocationGridTile dropLocation = null)
	{
		if (IsPOICarried(poi))
		{
			if (poi is Character)
			{
				RemoveCharacter(poi as Character, dropLocation);
			}
			else if (poi is TileObject)
			{
				RemoveTileObject(poi as TileObject, addToLocation, dropLocation);
			}
		}
	}

	private void RemoveTileObject(TileObject tileObject, bool addToLocation, LocationGridTile dropLocation)
	{
		carriedPOI = null;
		if (addToLocation)
		{
			if (dropLocation == null)
			{
				if (base.owner.gridTileLocation.isOccupied)
				{
					LocationGridTile firstNoObjectNeighbor = base.owner.gridTileLocation.GetFirstNoObjectNeighbor();
					if (firstNoObjectNeighbor != null)
					{
						base.owner.gridTileLocation.structure.AddPOI(tileObject, firstNoObjectNeighbor);
					}
					else
					{
						firstNoObjectNeighbor = base.owner.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
						base.owner.gridTileLocation.structure.AddPOI(tileObject, firstNoObjectNeighbor);
					}
				}
				else
				{
					base.owner.gridTileLocation.structure.AddPOI(tileObject, base.owner.gridTileLocation);
				}
			}
			else
			{
				base.owner.gridTileLocation.structure.AddPOI(tileObject, dropLocation);
			}
		}
		else if (tileObject.gridTileLocation != null)
		{
			tileObject.gridTileLocation.structure.RemovePOIDestroyVisualOnly(tileObject, base.owner);
		}
		else if (tileObject.mapVisual != null)
		{
			tileObject.DestroyMapVisualGameObject();
		}
		if (tileObject.mapVisual != null)
		{
			tileObject.mapVisual.transform.eulerAngles = Vector3.zero;
		}
	}

	private void RemoveCharacter(Character character, LocationGridTile dropLocation)
	{
		if (base.owner == character || character == null)
		{
			return;
		}
		carriedPOI = null;
		character.carryComponent.SetIsBeingCarriedBy(null);
		if (character.hasMarker)
		{
			if (dropLocation == null)
			{
				character.marker.PlaceMarkerAt(base.owner.gridTileLocation);
			}
			else
			{
				character.marker.PlaceMarkerAt(dropLocation);
			}
			character.marker.transform.eulerAngles = Vector3.zero;
			character.carryComponent.OnCharacterUncarried();
		}
	}

	private void OnCharacterUncarried()
	{
		if (base.owner is Dragon dragon)
		{
			dragon.Awaken();
		}
		if (base.owner.isDead)
		{
			base.owner.jobComponent.TriggerBuryMe();
		}
	}

	public bool IsPOICarried(IPointOfInterest poi)
	{
		if (carriedPOI != null)
		{
			return carriedPOI == poi;
		}
		return false;
	}

	public bool IsPOICarried(string name)
	{
		if (carriedPOI != null)
		{
			return carriedPOI.name == name;
		}
		return false;
	}

	public bool IsNotBeingCarried()
	{
		return isBeingCarriedBy == null;
	}

	public bool IsCurrentlyPartOf(Character character)
	{
		if (character != null)
		{
			if (base.owner != character)
			{
				return isBeingCarriedBy == character;
			}
			return true;
		}
		return false;
	}

	public void SetPrevCarriedBy(Character character)
	{
		prevCarriedBy = character;
	}

	public void LoadReferences(SaveDataCarryComponent data)
	{
		if (!string.IsNullOrEmpty(data.isBeingCarriedBy))
		{
			isBeingCarriedBy = CharacterManager.Instance.GetCharacterByPersistentID(data.isBeingCarriedBy);
		}
		if (!string.IsNullOrEmpty(data.prevCarriedBy))
		{
			prevCarriedBy = CharacterManager.Instance.GetCharacterByPersistentID(data.prevCarriedBy);
		}
	}

	public void LoadCarryReference(SaveDataCarryComponent data)
	{
		if (!string.IsNullOrEmpty(data.carriedPOI))
		{
			IPointOfInterest poi = null;
			if (data.carriedPOIType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				poi = CharacterManager.Instance.GetCharacterByPersistentID(data.carriedPOI);
			}
			else if (data.carriedPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				poi = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.carriedPOI);
			}
			CarryPOI(poi, isOwner: false, isFromSave: true);
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (prevCarriedBy == p_character)
		{
			prevCarriedBy = null;
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = carriedPOI;
		_ = prevCarriedBy;
		_ = _isBeingCarriedBy;
	}
}
