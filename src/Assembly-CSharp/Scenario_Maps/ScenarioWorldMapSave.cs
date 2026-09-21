using System;
using System.Collections.Generic;
using Perlin_Noise;

namespace Scenario_Maps;

[Serializable]
public class ScenarioWorldMapSave
{
	public WorldMapTemplate worldMapTemplate;

	public List<SaveDataArea> areaSaves;

	public PerlinNoiseSettings elevationPerlinNoiseSettings;

	public int xSeed;

	public int ySeed;

	public float warpWeight;

	public float temperatureSeed;

	public List<SpecialStructureSetting> specialStructureSaves;

	public List<SaveDataVillageSpot> villageSpots;

	public void SaveWorld(WorldMapTemplate p_worldMapTemplate, List<Area> p_areas, PerlinNoiseSettings p_elevationSettings, float p_warpWeight, float p_temperatureSeed, List<VillageSpot> villageSpots)
	{
		worldMapTemplate = p_worldMapTemplate;
		elevationPerlinNoiseSettings = p_elevationSettings;
		warpWeight = p_warpWeight;
		temperatureSeed = p_temperatureSeed;
		SaveAreas(p_areas);
		SaveVillageSpots(villageSpots);
	}

	public void SaveAreas(List<Area> p_tiles)
	{
		areaSaves = new List<SaveDataArea>();
		specialStructureSaves = new List<SpecialStructureSetting>();
		for (int i = 0; i < p_tiles.Count; i++)
		{
			Area area = p_tiles[i];
			SaveDataArea saveDataArea = new SaveDataArea();
			saveDataArea.Save(area);
			if (area.primaryStructureInArea != null && area.primaryStructureInArea.structureType.IsSpecialStructure() && area.primaryStructureInArea.structureType != STRUCTURE_TYPE.CAVE)
			{
				SpecialStructureSetting item = new SpecialStructureSetting(new Point(area.areaData.xCoordinate, area.areaData.yCoordinate), area.primaryStructureInArea.structureType);
				specialStructureSaves.Add(item);
			}
			areaSaves.Add(saveDataArea);
		}
	}

	public SaveDataArea[,] GetSaveDataMap()
	{
		SaveDataArea[,] array = new SaveDataArea[worldMapTemplate.worldMapWidth, worldMapTemplate.worldMapHeight];
		for (int i = 0; i < areaSaves.Count; i++)
		{
			SaveDataArea saveDataArea = areaSaves[i];
			array[saveDataArea.areaData.xCoordinate, saveDataArea.areaData.yCoordinate] = saveDataArea;
		}
		return array;
	}

	private void SaveVillageSpots(List<VillageSpot> p_villageSpots)
	{
		villageSpots = new List<SaveDataVillageSpot>();
		for (int i = 0; i < p_villageSpots.Count; i++)
		{
			VillageSpot data = p_villageSpots[i];
			SaveDataVillageSpot saveDataVillageSpot = new SaveDataVillageSpot();
			saveDataVillageSpot.Save(data);
			villageSpots.Add(saveDataVillageSpot);
		}
	}
}
