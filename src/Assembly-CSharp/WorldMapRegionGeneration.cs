using System.Collections;
using UtilityScripts;

public class WorldMapRegionGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating_Regions");
		WorldMapTemplate chosenWorldMapTemplate = data.chosenWorldMapTemplate;
		yield return MapGenerator.Instance.StartCoroutine(DivideToRegions(chosenWorldMapTemplate, data));
		CreateBiomeDivisions();
		yield return null;
	}

	private void CreateBiomeDivisions()
	{
		BIOMES[] enumValues = CollectionUtilities.GetEnumValues<BIOMES>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			BiomeDivision p_division = new BiomeDivision(enumValues[i]);
			GridMap.Instance.mainRegion.biomeDivisionComponent.AddBiomeDivision(p_division);
		}
	}

	private IEnumerator DivideToRegions(WorldMapTemplate mapTemplate, MapGenerationData data)
	{
		int num = mapTemplate.worldMapWidth / 2;
		int num2 = mapTemplate.worldMapHeight / 2;
		Area coreTile = GridMap.Instance.map[num, num2];
		string empty = string.Empty;
		Region region = new Region(coreTile, empty);
		for (int i = 0; i < GridMap.Instance.allAreas.Count; i++)
		{
			Area tile = GridMap.Instance.allAreas[i];
			region.AddTile(tile);
		}
		DatabaseManager.Instance.regionDatabase.RegisterRegion(region);
		yield return null;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Regions");
		yield return MapGenerator.Instance.StartCoroutine(LoadRegions(saveData));
	}

	private IEnumerator LoadRegions(SaveDataCurrentProgress saveData)
	{
		Region region = new Region(saveData.worldMapSave.regionSave);
		DatabaseManager.Instance.regionDatabase.RegisterRegion(region);
		yield return null;
	}
}
