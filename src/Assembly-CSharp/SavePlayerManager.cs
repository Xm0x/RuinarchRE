using BayatGames.SaveGameFree;
using UnityEngine;
using UtilityScripts;

public class SavePlayerManager : MonoBehaviour
{
	private const string savedPlayerDataFileName = "SAVED_PLAYER_DATA_2";

	public SaveDataPlayer currentSaveDataPlayer { get; private set; }

	public bool hasSavedDataPlayer => currentSaveDataPlayer != null;

	public void SetCurrentSaveDataPlayer(SaveDataPlayer save)
	{
		currentSaveDataPlayer = save;
	}

	public void SavePlayerData()
	{
		SaveDataPlayer obj = currentSaveDataPlayer;
		SaveGame.Save(Utilities.gameSavePath + "SAVED_PLAYER_DATA_2", obj);
	}

	public void LoadSaveDataPlayer()
	{
		if (Utilities.DoesFileExist(Utilities.gameSavePath + "SAVED_PLAYER_DATA_2"))
		{
			SaveDataPlayer saveDataPlayer = SaveGame.Load<SaveDataPlayer>(Utilities.gameSavePath + "SAVED_PLAYER_DATA_2");
			if (saveDataPlayer != null)
			{
				saveDataPlayer.ProcessOnLoad();
				SetCurrentSaveDataPlayer(saveDataPlayer);
			}
			else
			{
				CreateNewSaveDataPlayer();
			}
		}
	}

	public void CreateNewSaveDataPlayer()
	{
		SaveDataPlayer saveDataPlayer = new SaveDataPlayer();
		saveDataPlayer.InitializeInitialData();
		SetCurrentSaveDataPlayer(saveDataPlayer);
	}
}
