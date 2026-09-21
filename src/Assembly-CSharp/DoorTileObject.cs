using System;
using Inner_Maps;
using UnityEngine;

public class DoorTileObject : TileObject
{
	private DoorGameObject _doorGameObject;

	public bool isOpen { get; private set; }

	public override Type serializedData => typeof(SaveDataDoorTileObject);

	public DoorTileObject()
	{
		Initialize(TILE_OBJECT_TYPE.DOOR_TILE_OBJECT);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public DoorTileObject(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void LoadAdditionalInfo(SaveDataTileObject data)
	{
		base.LoadAdditionalInfo(data);
		if ((data as SaveDataDoorTileObject).isOpen)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_doorGameObject = mapVisual as DoorGameObject;
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		mapVisual.DestroyExistingGUS();
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		base.OnPlaceTileObjectAtTile(tile);
		mapVisual.InitializeGUS(Vector2.zero, Vector2.one, tile);
	}

	public override bool IsUnpassable()
	{
		return !isOpen;
	}

	public override bool IsValidCombatTargetFor(IPointOfInterest source)
	{
		if (gridTileLocation == null)
		{
			return false;
		}
		if (source.gridTileLocation == null)
		{
			return false;
		}
		return true;
	}

	public void Open()
	{
		if (!(mapVisual == null))
		{
			isOpen = true;
			_doorGameObject.SetBlockerState(state: false);
			mapVisual.SetVisualAlpha(0f);
			mapVisual.DestroyExistingGUS();
		}
	}

	public void Close()
	{
		if (!(mapVisual == null))
		{
			isOpen = false;
			_doorGameObject.SetBlockerState(state: true);
			mapVisual.SetVisualAlpha(1f);
			mapVisual.InitializeGUS(Vector2.zero, Vector2.one, gridTileLocation);
		}
	}
}
