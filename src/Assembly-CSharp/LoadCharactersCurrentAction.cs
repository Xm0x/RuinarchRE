using System.Collections;

public class LoadCharactersCurrentAction : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadActions(data, saveData));
	}

	private IEnumerator LoadActions(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Preparing_Character_Actions");
		saveData.LoadCharactersCurrentAction();
		yield return null;
	}
}
