using System;
using System.Collections;
using System.Collections.Generic;
using Databases;
using Events.World_Events;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

[Serializable]
public class WorldMapSave
{
	public WorldSettingsData.World_Type worldType;

	public WorldMapTemplate worldMapTemplate;

	public SaveDataRegion regionSave;

	public List<SaveDataArea> areaSaves;

	public List<SaveDataBaseSettlement> settlementSaves;

	public List<SaveDataLocationStructure> structureSaves;

	public List<SaveDataWorldEvent> worldEventSaves;

	public IEnumerator SaveWorldCoroutine(WorldMapTemplate _worldMapTemplate, AreaDatabase hexTileDatabase, RegionDatabase regionDatabase, SettlementDatabase settlementDatabase, LocationStructureDatabase structureDatabase, List<WorldEvent> activeEvents)
	{
		worldType = WorldSettings.Instance.worldSettingsData.worldType;
		worldMapTemplate = _worldMapTemplate;
		yield return SaveManager.Instance.StartCoroutine(SaveHexTilesCoroutine(hexTileDatabase.allAreas));
		yield return SaveManager.Instance.StartCoroutine(SaveRegionsCoroutine(regionDatabase.mainRegion));
		yield return SaveManager.Instance.StartCoroutine(SaveSettlementsCoroutine(settlementDatabase.allSettlements));
		yield return SaveManager.Instance.StartCoroutine(SaveStructuresCoroutine(structureDatabase.allStructures));
		yield return SaveManager.Instance.StartCoroutine(SaveWorldEventsCoroutine(activeEvents));
	}

	public void SaveWorld(WorldMapTemplate _worldMapTemplate, AreaDatabase hexTileDatabase, RegionDatabase regionDatabase, SettlementDatabase settlementDatabase, LocationStructureDatabase structureDatabase, List<WorldEvent> activeEvents)
	{
		worldType = WorldSettings.Instance.worldSettingsData.worldType;
		worldMapTemplate = _worldMapTemplate;
		SaveHexTiles(hexTileDatabase.allAreas);
		SaveRegions(regionDatabase.mainRegion);
		SaveSettlements(settlementDatabase.allSettlements);
		SaveStructures(structureDatabase.allStructures);
		SaveWorldEvents(activeEvents);
	}

	public IEnumerator SaveHexTilesCoroutine(List<Area> tiles)
	{
		int batchCount = 0;
		areaSaves = RuinarchListPool<SaveDataArea>.Claim();
		for (int i = 0; i < tiles.Count; i++)
		{
			Area data = tiles[i];
			SaveDataArea saveDataArea = new SaveDataArea();
			saveDataArea.Save(data);
			areaSaves.Add(saveDataArea);
			batchCount++;
			if (batchCount >= 200)
			{
				batchCount = 0;
				yield return null;
			}
		}
	}

	public void SaveHexTiles(List<Area> tiles)
	{
		areaSaves = RuinarchListPool<SaveDataArea>.Claim();
		for (int i = 0; i < tiles.Count; i++)
		{
			Area data = tiles[i];
			SaveDataArea saveDataArea = new SaveDataArea();
			saveDataArea.Save(data);
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

	private void SaveRegions(Region region)
	{
		SaveDataRegion saveDataRegion = new SaveDataRegion();
		saveDataRegion.Save(region);
		regionSave = saveDataRegion;
	}

	private IEnumerator SaveRegionsCoroutine(Region region)
	{
		SaveDataRegion saveDataRegion = new SaveDataRegion();
		saveDataRegion.Save(region);
		regionSave = saveDataRegion;
		yield return null;
	}

	public void SaveSettlements(List<BaseSettlement> allSettlements)
	{
		settlementSaves = RuinarchListPool<SaveDataBaseSettlement>.Claim();
		for (int i = 0; i < allSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = allSettlements[i];
			SaveDataBaseSettlement saveDataBaseSettlement = CreateNewSettlementSaveData(baseSettlement);
			saveDataBaseSettlement.Save(baseSettlement);
			settlementSaves.Add(saveDataBaseSettlement);
		}
	}

	public IEnumerator SaveSettlementsCoroutine(List<BaseSettlement> allSettlements)
	{
		int batchCount = 0;
		settlementSaves = RuinarchListPool<SaveDataBaseSettlement>.Claim();
		for (int i = 0; i < allSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = allSettlements[i];
			SaveDataBaseSettlement saveDataBaseSettlement = CreateNewSettlementSaveData(baseSettlement);
			saveDataBaseSettlement.Save(baseSettlement);
			settlementSaves.Add(saveDataBaseSettlement);
			batchCount++;
			if (batchCount >= 200)
			{
				batchCount = 0;
				yield return null;
			}
		}
	}

	private SaveDataBaseSettlement CreateNewSettlementSaveData(BaseSettlement settlement)
	{
		if (settlement is PlayerSettlement)
		{
			return new SaveDataPlayerSettlement();
		}
		return new SaveDataNPCSettlement();
	}

	private SaveDataLocationStructure CreateNewSaveDataFor(LocationStructure structure)
	{
		return Activator.CreateInstance(structure.serializedData) as SaveDataLocationStructure;
	}

	private void SaveStructures(List<LocationStructure> structures)
	{
		structureSaves = RuinarchListPool<SaveDataLocationStructure>.Claim();
		for (int i = 0; i < structures.Count; i++)
		{
			LocationStructure locationStructure = structures[i];
			SaveDataLocationStructure saveDataLocationStructure = CreateNewSaveDataFor(locationStructure);
			saveDataLocationStructure.Save(locationStructure);
			structureSaves.Add(saveDataLocationStructure);
		}
	}

	private IEnumerator SaveStructuresCoroutine(List<LocationStructure> structures)
	{
		int batchCount = 0;
		structureSaves = RuinarchListPool<SaveDataLocationStructure>.Claim();
		for (int i = 0; i < structures.Count; i++)
		{
			LocationStructure locationStructure = structures[i];
			SaveDataLocationStructure saveDataLocationStructure = CreateNewSaveDataFor(locationStructure);
			saveDataLocationStructure.Save(locationStructure);
			structureSaves.Add(saveDataLocationStructure);
			batchCount++;
			if (batchCount >= 200)
			{
				batchCount = 0;
				yield return null;
			}
		}
	}

	private IEnumerator SaveWorldEventsCoroutine(List<WorldEvent> worldEvents)
	{
		worldEventSaves = RuinarchListPool<SaveDataWorldEvent>.Claim();
		for (int i = 0; i < worldEvents.Count; i++)
		{
			WorldEvent worldEvent = worldEvents[i];
			worldEventSaves.Add(worldEvent.Save());
		}
		yield return null;
	}

	private void SaveWorldEvents(List<WorldEvent> worldEvents)
	{
		worldEventSaves = RuinarchListPool<SaveDataWorldEvent>.Claim();
		for (int i = 0; i < worldEvents.Count; i++)
		{
			WorldEvent worldEvent = worldEvents[i];
			worldEventSaves.Add(worldEvent.Save());
		}
	}

	public void CleanUp()
	{
		regionSave.CleanUp();
		regionSave = null;
		for (int i = 0; i < areaSaves.Count; i++)
		{
			areaSaves[i].CleanUp();
		}
		RuinarchListPool<SaveDataArea>.Release(areaSaves);
		areaSaves = null;
		for (int j = 0; j < settlementSaves.Count; j++)
		{
			settlementSaves[j].CleanUp();
		}
		RuinarchListPool<SaveDataBaseSettlement>.Release(settlementSaves);
		settlementSaves = null;
		for (int k = 0; k < structureSaves.Count; k++)
		{
			structureSaves[k].CleanUp();
		}
		RuinarchListPool<SaveDataLocationStructure>.Release(structureSaves);
		structureSaves = null;
		for (int l = 0; l < worldEventSaves.Count; l++)
		{
			worldEventSaves[l].CleanUp();
		}
		RuinarchListPool<SaveDataWorldEvent>.Release(worldEventSaves);
		worldEventSaves = null;
	}
}
