using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Cave : NaturalStructure
{
	private static WeightedDictionary<CONCRETE_RESOURCES> _resourceWeights;

	public CONCRETE_RESOURCES producedResource { get; }

	public List<LocationGridTile> stoneSpots { get; }

	public List<LocationGridTile> oreSpots { get; }

	public List<LocationGridTile> mushroomSpots { get; }

	public List<LocationStructure> connectedMines { get; private set; }

	public override Type serializedData => typeof(SaveDataCave);

	public bool hasConnectedMine => connectedMines.Count > 0;

	public Cave(Region location)
		: base(STRUCTURE_TYPE.CAVE, location)
	{
		if (_resourceWeights == null)
		{
			_resourceWeights = new WeightedDictionary<CONCRETE_RESOURCES>();
			_resourceWeights.AddElement(CONCRETE_RESOURCES.Copper, 100);
			_resourceWeights.AddElement(CONCRETE_RESOURCES.Iron, 80);
			_resourceWeights.AddElement(CONCRETE_RESOURCES.Mithril, 40);
			_resourceWeights.AddElement(CONCRETE_RESOURCES.Orichalcum, 20);
		}
		producedResource = _resourceWeights.PickRandomElementGivenWeights();
		stoneSpots = new List<LocationGridTile>();
		oreSpots = new List<LocationGridTile>();
		mushroomSpots = new List<LocationGridTile>();
		connectedMines = new List<LocationStructure>();
	}

	public Cave(Region location, SaveDataCave data)
		: base(location, data)
	{
		producedResource = data.producedResource;
		oreSpots = new List<LocationGridTile>();
		stoneSpots = new List<LocationGridTile>();
		mushroomSpots = new List<LocationGridTile>();
		connectedMines = new List<LocationStructure>();
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataCave saveDataCave = saveDataLocationStructure as SaveDataCave;
		if (saveDataCave.oreSpots != null)
		{
			for (int i = 0; i < saveDataCave.oreSpots.Length; i++)
			{
				TileLocationSave tileLocationSave = saveDataCave.oreSpots[i];
				LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
				oreSpots.Add(tileBySavedData);
			}
		}
		if (saveDataCave.stoneSpots != null)
		{
			for (int j = 0; j < saveDataCave.stoneSpots.Length; j++)
			{
				TileLocationSave tileLocationSave2 = saveDataCave.stoneSpots[j];
				LocationGridTile tileBySavedData2 = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave2);
				stoneSpots.Add(tileBySavedData2);
			}
		}
		if (saveDataCave.mushroomSpots != null)
		{
			for (int k = 0; k < saveDataCave.mushroomSpots.Length; k++)
			{
				TileLocationSave tileLocationSave3 = saveDataCave.mushroomSpots[k];
				LocationGridTile tileBySavedData3 = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave3);
				mushroomSpots.Add(tileBySavedData3);
			}
		}
		if (saveDataCave.connectedMines != null)
		{
			for (int l = 0; l < saveDataCave.connectedMines.Length; l++)
			{
				string text = saveDataCave.connectedMines[l];
				LocationStructure structureByPersistentID = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(text);
				connectedMines.Add(structureByPersistentID);
			}
		}
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
		if (base.occupiedArea != null)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < base.occupiedAreas.Keys.Count; i++)
			{
				Vector2 vector = base.occupiedAreas.Keys.ElementAt(i).gridTileComponent.centerGridTile.centeredWorldLocation;
				num += vector.x;
				num2 += vector.y;
			}
			Vector2 vector2 = new Vector2(num / (float)base.occupiedAreas.Count, num2 / (float)base.occupiedAreas.Count);
			InnerMapCameraMove.Instance.CenterCameraOn(vector2);
		}
	}

	public override void ShowSelectorOnStructure()
	{
	}

	public void AddStoneSpot(LocationGridTile p_tile)
	{
		if (!stoneSpots.Contains(p_tile))
		{
			stoneSpots.Add(p_tile);
			TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK);
			AddPOI(poi, p_tile);
		}
	}

	public void AddOreSpot(LocationGridTile p_tile)
	{
		if (!oreSpots.Contains(p_tile))
		{
			oreSpots.Add(p_tile);
			Ore ore = InnerMapManager.Instance.CreateNewTileObject<Ore>(TILE_OBJECT_TYPE.ORE);
			ore.SetProvidedMetal(producedResource);
			AddPOI(ore, p_tile);
		}
	}

	public void AddMushroomSpot(LocationGridTile p_tile)
	{
		if (!mushroomSpots.Contains(p_tile))
		{
			mushroomSpots.Add(p_tile);
			TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MUSHROOM);
			AddPOI(poi, p_tile);
		}
	}

	public void ConnectMine(Mine p_mine)
	{
		if (!connectedMines.Contains(p_mine))
		{
			connectedMines.Add(p_mine);
			if (base.linkedSettlement != null)
			{
				base.linkedSettlement.structureComponent.RemoveLinkedStructure(this);
				LinkThisStructureToAVillage();
			}
		}
	}

	public void DisconnectMine(Mine p_mine)
	{
		connectedMines.Remove(p_mine);
	}

	public bool IsConnectedToSettlement(NPCSettlement p_settlement)
	{
		for (int i = 0; i < connectedMines.Count; i++)
		{
			if (connectedMines[i].settlementLocation == p_settlement)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasConnectedMines()
	{
		if (connectedMines != null)
		{
			return connectedMines.Count > 0;
		}
		return false;
	}

	public override string GetTestingInfo()
	{
		string testingInfo = base.GetTestingInfo();
		testingInfo = testingInfo + "\nProduced Resource: " + producedResource;
		testingInfo = testingInfo + "\nStone Spots: " + stoneSpots.ComafyList();
		testingInfo = testingInfo + "\nOre Spots: " + oreSpots.ComafyList();
		testingInfo = testingInfo + "\nMushroom Spots: " + mushroomSpots.ComafyList();
		return testingInfo + "\nConnected Mines (" + connectedMines.Count + "): " + connectedMines.ComafyList();
	}

	public override bool TryGetMinimapColorForTileInStructure(LocationGridTile p_tile, out Color p_color)
	{
		if (p_tile.tileObjectComponent.objHere is BlockWall || p_tile.tileObjectComponent.objHere is OreVein)
		{
			p_color = GameUtilities.CaveWallMinimapColor;
		}
		else
		{
			p_color = GameUtilities.CaveGroundMinimapColor;
		}
		return true;
	}

	protected override void OnTileAddedToStructure(LocationGridTile tile)
	{
		base.OnTileAddedToStructure(tile);
		tile.SetElevation(ELEVATION.MOUNTAIN);
		tile.UpdateMinimapVisual(this);
	}

	protected override void OnTileRemovedFromStructure(LocationGridTile tile, LocationStructure removedFrom)
	{
		base.OnTileRemovedFromStructure(tile, removedFrom);
		tile.UpdateMinimapVisual(null);
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		connectedMines.Contains(p_structure);
	}
}
