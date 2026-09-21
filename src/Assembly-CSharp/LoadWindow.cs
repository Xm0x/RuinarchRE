using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UtilityScripts;

public class LoadWindow : PopupMenuBase
{
	[Header("Load Game")]
	[SerializeField]
	private ScrollRect loadGameScrollRect;

	[SerializeField]
	private GameObject saveItemPrefab;

	[SerializeField]
	private GameObject fetchSavesCover;

	private bool isFetchingSaves;

	public override void Open()
	{
		if (UIManager.Instance != null && !UIManager.Instance.optionsMenu.isShowing)
		{
			UIManager.Instance.Pause();
			UIManager.Instance.SetSpeedTogglesState(state: false);
		}
		Messenger.AddListener<string>(UISignals.LOAD_SAVE_FILE, OnLoadFileChosen);
		isFetchingSaves = false;
		fetchSavesCover.gameObject.SetActive(value: false);
		base.Open();
		LoadSavedGameItems();
	}

	public override void Close()
	{
		if (!isFetchingSaves)
		{
			if (UIManager.Instance != null && !UIManager.Instance.optionsMenu.isShowing)
			{
				UIManager.Instance.ResumeLastProgressionSpeed();
			}
			base.Close();
			isFetchingSaves = false;
			fetchSavesCover.gameObject.SetActive(value: false);
			Messenger.RemoveListener<string>(UISignals.LOAD_SAVE_FILE, OnLoadFileChosen);
		}
	}

	private void OnLoadFileChosen(string path)
	{
		SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(path);
		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene.name == "MainMenu")
		{
			MainMenuManager.Instance.StartGame();
		}
		else if (activeScene.name == "Game")
		{
			UIManager.Instance.optionsMenu.LoadSave();
		}
	}

	private void LoadSavedGameItems()
	{
		Utilities.DestroyChildren(loadGameScrollRect.content);
		string[] files = Directory.GetFiles(Utilities.autosavePath, "*.zip");
		files = files.OrderBy(File.GetLastWriteTime).ToArray();
		foreach (string saveFile in files)
		{
			SaveItem component = Object.Instantiate(saveItemPrefab, loadGameScrollRect.content).GetComponent<SaveItem>();
			component.SetSaveFile(saveFile);
			(component.transform as RectTransform)?.SetAsFirstSibling();
		}
		string[] files2 = Directory.GetFiles(Utilities.gameSavePath, "*.zip");
		files2 = files2.OrderBy(File.GetLastWriteTime).ToArray();
		foreach (string saveFile2 in files2)
		{
			SaveItem component2 = Object.Instantiate(saveItemPrefab, loadGameScrollRect.content).GetComponent<SaveItem>();
			component2.SetSaveFile(saveFile2);
			(component2.transform as RectTransform)?.SetAsFirstSibling();
		}
	}
}
