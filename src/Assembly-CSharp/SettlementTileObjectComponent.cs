using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SettlementTileObjectComponent : NPCSettlementComponent, TileObjectEventDispatcher.IDestroyedListener
{
	public List<LocationGridTile> wardLightLocations { get; private set; }

	public List<WardLight> wardLights { get; }

	public SettlementTileObjectComponent()
	{
		wardLightLocations = new List<LocationGridTile>();
		wardLights = new List<WardLight>();
	}

	public SettlementTileObjectComponent(SaveDataSettlementTileObjectComponent p_data)
	{
		wardLightLocations = new List<LocationGridTile>();
		wardLights = new List<WardLight>();
	}

	public void LoadReferences(SaveDataSettlementTileObjectComponent p_data)
	{
		for (int i = 0; i < p_data.wardLightLocations.Length; i++)
		{
			Point point = p_data.wardLightLocations[i];
			LocationGridTile item = GridMap.Instance.mainRegion.innerMap.map[point.X, point.Y];
			wardLightLocations.Add(item);
		}
		for (int j = 0; j < p_data.wardLightIDs.Length; j++)
		{
			string id = p_data.wardLightIDs[j];
			WardLight item2 = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(id) as WardLight;
			wardLights.Add(item2);
		}
	}

	public void AddWardLightLocation(LocationGridTile p_tile)
	{
		wardLightLocations.Add(p_tile);
	}

	public void RemoveWardLightLocations(LocationGridTile p_tile)
	{
		wardLightLocations.Remove(p_tile);
	}

	public void AddWardLight(WardLight p_wardLight)
	{
		if (!wardLights.Contains(p_wardLight))
		{
			wardLights.Add(p_wardLight);
		}
	}

	public void RemoveWardLight(WardLight p_wardLight)
	{
		wardLights.Remove(p_wardLight);
	}

	public bool IsCharacterInProximityOfWardLight(Character p_character)
	{
		for (int i = 0; i < wardLights.Count; i++)
		{
			WardLight wardLight = wardLights[i];
			if (wardLight != null && wardLight.charactersInRange != null && wardLight.charactersInRange.Contains(p_character))
			{
				return true;
			}
		}
		return false;
	}

	public void OnTileObjectDestroyed(TileObject p_tileObject)
	{
		if (p_tileObject is WardLight p_wardLight)
		{
			RemoveWardLight(p_wardLight);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
