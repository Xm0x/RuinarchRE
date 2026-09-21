using System.Collections;
using Traits;

public class LoadSecondWaveTraitsInMainThread : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Traits");
		yield return MapGenerator.Instance.StartCoroutine(Load(data, saveData));
	}

	private IEnumerator Load(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadTraitsInMainThread(data, saveData));
	}

	private IEnumerator LoadTraitsInMainThread(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		int batchCount = 0;
		for (int i = 0; i < DatabaseManager.Instance.traitDatabase.traitsToBeLoadedOnMainThread.Count; i++)
		{
			Trait trait = DatabaseManager.Instance.traitDatabase.traitsToBeLoadedOnMainThread[i];
			SaveDataTrait fromSaveHub = saveData.GetFromSaveHub<SaveDataTrait>(OBJECT_TYPE.Trait, trait.persistentID);
			trait.LoadTraitSecondWaveInMainThread(fromSaveHub);
			batchCount++;
			if (batchCount == MapGenerationData.LocationGridTileSecondaryWaveBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
	}
}
