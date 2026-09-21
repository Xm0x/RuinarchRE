using System;
using System.Collections;
using DG.Tweening;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoaderManager : MonoBehaviour
{
	public static LevelLoaderManager Instance;

	[SerializeField]
	private GameObject loaderGO;

	[SerializeField]
	private Image loadingBG;

	[SerializeField]
	private TextMeshProUGUI loaderInfoText;

	[SerializeField]
	private Slider _progressBar;

	[SerializeField]
	private TextMeshProUGUI _additionalLoadingText;

	public bool isLoadingNewScene;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void LoadLevel(string sceneName, bool updateSceneProgress = false)
	{
		_progressBar.value = 0f;
		Messenger.Broadcast(UISignals.STARTED_LOADING_SCENE, sceneName);
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		isLoadingNewScene = true;
		StartCoroutine(LoadLevelAsynchronously(sceneName, updateSceneProgress));
	}

	private IEnumerator LoadLevelAsynchronously(string sceneName, bool updateSceneProgress)
	{
		SetLoadingState(state: true);
		AsyncOperation unloader = Resources.UnloadUnusedAssets();
		while (!unloader.isDone)
		{
			yield return null;
		}
		GC.Collect();
		yield return null;
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		asyncOperation.allowSceneActivation = false;
		while (asyncOperation.progress < 0.9f)
		{
			if (updateSceneProgress)
			{
				UpdateLoadingBar(asyncOperation.progress, 2f);
			}
		}
		asyncOperation.allowSceneActivation = true;
		isLoadingNewScene = false;
	}

	public void UpdateLoadingBar(float value, float duration)
	{
		_progressBar.DOValue(value, duration);
	}

	public void UpdateLoadingInfo(string info)
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", info);
		if (string.IsNullOrEmpty(text))
		{
			text = info;
		}
		loaderInfoText.text = text;
	}

	public bool IsLoadingScreenActive()
	{
		return loaderGO.activeInHierarchy;
	}

	public void SetLoadingState(bool state)
	{
		loaderGO.SetActive(state);
	}
}
