using System.Collections;

public class ElevationGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating_Elevation_Maps");
		yield return null;
	}
}
