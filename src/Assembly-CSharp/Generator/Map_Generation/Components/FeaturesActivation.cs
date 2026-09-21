using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace Generator.Map_Generation.Components;

public class FeaturesActivation : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		Stopwatch stopwatch = new Stopwatch();
		yield return MapGenerator.Instance.StartCoroutine(ExecuteFeatureInitialActions(stopwatch));
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}

	private IEnumerator ExecuteFeatureInitialActions(Stopwatch stopwatch)
	{
		stopwatch.Reset();
		stopwatch.Start();
		for (int i = 0; i < GridMap.Instance.allAreas.Count; i++)
		{
			Area area = GridMap.Instance.allAreas[i];
			for (int j = 0; j < area.featureComponent.features.Count; j++)
			{
				area.featureComponent.features[j].GameStartActions(area);
			}
			yield return null;
		}
		stopwatch.Stop();
		AddLog(GridMap.Instance.mainRegion.name + " ExecuteFeatureInitialActions took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
	}
}
