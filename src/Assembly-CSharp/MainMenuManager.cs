using System.Collections.Generic;
using System.Linq;
using Object_Pools;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class MainMenuManager : MonoBehaviour
{
	public static MainMenuManager Instance;

	[ContextMenu("Get Combinations")]
	public void GetCombinations()
	{
		List<List<int>> list = Utilities.ItemCombinations(new List<int> { 1, 2 }, 3, 3);
		for (int i = 0; i < list.Count; i++)
		{
			string text = "\n{";
			for (int j = 0; j < list[i].Count(); j++)
			{
				text += $" {list[i][j]},";
			}
			text += " }";
			Debug.Log(text);
		}
	}

	public void Awake()
	{
		Instance = this;
		StringEnumLookUp.Initialize();
	}

	private void Start()
	{
		Initialize();
		if (!ExternalFileManager.Instance.hasInitialized)
		{
			ExternalFileManager.Instance.InitializeAllFiles(StartMainMenu);
		}
		else
		{
			StartMainMenu();
		}
	}

	private void StartMainMenu()
	{
		MainMenuUI.Instance.ShowMenuButtons();
		MainMenuUI.Instance.ShowEarlyAccessAnnouncement();
		LevelLoaderManager.Instance.SetLoadingState(state: false);
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		AudioManager.Instance.UpdateAmbientSoundStateBasedOnCurrentScene();
		AudioManager.Instance.playAmbientMusicEvent.Post(base.gameObject);
	}

	private void Initialize()
	{
		SaveManager.Instance.LoadSaveDataPlayer();
		LogPool.WarmUp(5000);
	}

	public void LoadMainGameScene()
	{
		LevelLoaderManager.Instance.LoadLevel("Game");
	}

	public void OnUnlockPlayerSkill()
	{
	}

	public void StartGame()
	{
		LevelLoaderManager.Instance.SetLoadingState(state: true);
		LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing_Data");
		LevelLoaderManager.Instance.UpdateLoadingBar(0.1f, 3f);
		LoadMainGameScene();
	}
}
