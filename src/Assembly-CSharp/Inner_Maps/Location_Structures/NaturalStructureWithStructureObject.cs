using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public abstract class NaturalStructureWithStructureObject : NaturalStructure
{
	public LocationStructureObject structureObj { get; private set; }

	public string templateName { get; private set; }

	public Vector3 structureObjectWorldPos { get; private set; }

	public override Vector2 selectableSize => structureObj.size;

	public override Type serializedData => typeof(SaveDataNaturalStructureWithStructureObject);

	protected NaturalStructureWithStructureObject(STRUCTURE_TYPE structureType, Region location)
		: base(structureType, location)
	{
	}

	protected NaturalStructureWithStructureObject(Region location, SaveDataNaturalStructureWithStructureObject data)
		: base(location, data)
	{
	}

	public virtual void SetStructureObject(LocationStructureObject structureObj)
	{
		this.structureObj = structureObj;
		templateName = structureObj.name;
		structureObjectWorldPos = structureObj.transform.position;
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		position.y -= 0.5f;
		worldPosition = position;
	}

	public override void CenterOnStructure()
	{
		if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != base.region.innerMap)
		{
			InnerMapManager.Instance.HideAreaMap();
		}
		if (!base.region.innerMap.isShowing)
		{
			InnerMapManager.Instance.ShowInnerMap(base.region);
		}
		if (structureObj != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(structureObj.gameObject);
		}
	}

	public override void ShowSelectorOnStructure()
	{
		Selector.Instance.Select(this);
	}

	public override LocationGridTile GetCenterTile()
	{
		if (structureObj != null)
		{
			return GridMap.Instance.mainRegion.innerMap.GetTileFromWorldPosition(structureObj.worldPosition);
		}
		return CollectionUtilities.GetRandomElement(base.tiles);
	}

	public override bool TryGetMinimapColorForTileInStructure(LocationGridTile p_tile, out Color p_color)
	{
		p_color = GameUtilities.StructureMinimapColor;
		return true;
	}

	protected override void OnTileAddedToStructure(LocationGridTile tile)
	{
		base.OnTileAddedToStructure(tile);
		tile.UpdateMinimapVisual(this);
	}

	protected override void OnTileRemovedFromStructure(LocationGridTile tile, LocationStructure removedFrom)
	{
		base.OnTileRemovedFromStructure(tile, removedFrom);
		tile.UpdateMinimapVisual(null);
	}

	public void PopulateBorderTiles(List<LocationGridTile> p_borderTiles)
	{
		LocationGridTile centerTile = GetCenterTile();
		int num = centerTile.localPlace.x - structureObj.center.x;
		int num2 = centerTile.localPlace.y - structureObj.center.y;
		List<Vector2Int> borderCoordinates = structureObj.borderCoordinates;
		for (int i = 0; i < borderCoordinates.Count; i++)
		{
			Vector2Int vector2Int = borderCoordinates[i];
			int xPos = num + vector2Int.x;
			int yPos = num2 + vector2Int.y;
			LocationGridTile tileFromMapCoordinates = GridMap.Instance.mainRegion.innerMap.GetTileFromMapCoordinates(xPos, yPos);
			if (tileFromMapCoordinates != null)
			{
				p_borderTiles.Add(tileFromMapCoordinates);
			}
		}
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		structureObj.OnOwnerStructureDestroyed(base.region.innerMap, this);
		Area area = base.occupiedArea;
		base.AfterStructureDestruction(p_responsibleCharacter);
		area?.TryRemoveAreaFromSettlementIfItIsNoLongerPartOfIt();
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			structureObj = null;
			base.CleanUp();
		}
	}
}
