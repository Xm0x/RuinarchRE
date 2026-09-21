using System.Collections;

public class WorldMapBiomeGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating_Biomes");
		yield return null;
	}
}
