using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Localization.Settings;

public class ConvertSaveFileLanguage : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		yield return null;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return null;
	}

	private IEnumerator ConvertObjectsToCurrentLanguage(SaveDataCurrentProgress saveData)
	{
		foreach (KeyValuePair<string, Character> allCharacter in DatabaseManager.Instance.characterDatabase.allCharacters)
		{
			allCharacter.Value.OnLocaleChanged(LocalizationSettings.SelectedLocale);
		}
		foreach (LocationStructure allStructure in DatabaseManager.Instance.structureDatabase.allStructures)
		{
			allStructure.ReGenerateName();
		}
		yield return null;
	}
}
