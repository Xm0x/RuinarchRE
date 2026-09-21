using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class WorldSettings : MonoBehaviour
{
	public static WorldSettings Instance;

	public GameObject settingsGO;

	public WorldGenOptionsUIController worldGenOptionsUIController;

	public Button btnContinue;

	public GameObject goContinue;

	public Transform parentDisplay;

	public WorldSettingsData worldSettingsData { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			worldSettingsData = new WorldSettingsData();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
			Instance = null;
		}
	}

	private void Start()
	{
		if (Instance == this)
		{
			worldGenOptionsUIController.InitUI(OnUpdateVillageCount);
			worldGenOptionsUIController.HideUI();
			worldGenOptionsUIController.SetParent(parentDisplay);
			LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
		}
	}

	public void Open()
	{
		settingsGO.SetActive(value: true);
		worldGenOptionsUIController.ShowUI();
		UpdateContinueBtnInteractable();
	}

	public void Close()
	{
		settingsGO.SetActive(value: false);
	}

	public void SetWorldSettingsData(WorldSettingsData data)
	{
		worldSettingsData = data;
	}

	private void OnUpdateVillageCount()
	{
		UpdateContinueBtnInteractable();
	}

	private void UpdateContinueBtnInteractable()
	{
		if (worldGenOptionsUIController.IsUIShowing())
		{
			goContinue.SetActive(value: true);
			btnContinue.interactable = worldSettingsData.AreSettingsValid(out var _);
		}
		else
		{
			goContinue.SetActive(value: false);
			btnContinue.interactable = true;
		}
	}

	public void OnClickContinue()
	{
		if (worldGenOptionsUIController.IsUIShowing())
		{
			worldSettingsData.SetWorldType(WorldSettingsData.World_Type.Custom);
			worldGenOptionsUIController.ApplyCurrentSettingsToData();
			worldSettingsData.ApplyCustomWorldSettings();
			if (worldSettingsData.AreSettingsValid(out var _))
			{
				AudioManager.Instance.TryPlayUISFX("Play_Click_Continue");
				Close();
				MainMenuManager.Instance.StartGame();
			}
		}
		UpdateContinueBtnInteractable();
	}

	public void OnClickBack()
	{
		if (worldGenOptionsUIController.IsUIShowing())
		{
			worldGenOptionsUIController.HideUI();
			Close();
		}
		UpdateContinueBtnInteractable();
	}

	private void OnLocaleChanged(Locale p_newLang)
	{
		worldGenOptionsUIController.OnLocaleChanged(p_newLang);
	}
}
