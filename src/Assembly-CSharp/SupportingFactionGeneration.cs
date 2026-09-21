using System.Collections;

public class SupportingFactionGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating_Factions");
		FactionManager.Instance.CreateWildMonsterFaction();
		FactionManager.Instance.CreateVagrantFaction();
		FactionManager.Instance.CreateDisguisedFaction();
		FactionManager.Instance.CreateRetaliatorFaction();
		yield return null;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
}
