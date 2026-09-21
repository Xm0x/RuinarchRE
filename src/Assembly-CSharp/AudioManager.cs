using AK.Wwise;
using Ruinarch.Custom_UI;
using Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	private const int MaxAudioObjects = 30;

	public static float Minimum_Volume_Level = 0f;

	public static float Maximum_Volume_Level = 100f;

	[Header("Ambient Music")]
	public AK.Wwise.Event playAmbientMusicEvent;

	[Header("UI Audio Sources")]
	[SerializeField]
	private AK.Wwise.Event toggleClick;

	[SerializeField]
	private AK.Wwise.Event alertNotification;

	[SerializeField]
	private AK.Wwise.Event positiveNotification;

	[SerializeField]
	private AK.Wwise.Event negativeNotification;

	[SerializeField]
	private AK.Wwise.Event conversationMenuOpened;

	[SerializeField]
	private AK.Wwise.Event particleMagnet;

	[SerializeField]
	private AK.Wwise.Event worldSelectableClick;

	[SerializeField]
	private AK.Wwise.Event buttonClick;

	private bool isPlayingThreatMusic;

	private int _activeAudioObjects;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			Initialize();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener<float>(SettingsSignals.MASTER_VOLUME_CHANGED, SetMasterVolume);
		Messenger.RemoveListener<float>(SettingsSignals.MUSIC_VOLUME_CHANGED, SetMusicVolume);
		Messenger.RemoveListener<float>(SettingsSignals.SFX_VOLUME_CHANGED, SetSFXVolume);
		Messenger.RemoveListener<RuinarchButton>(UISignals.BUTTON_CLICKED, OnButtonClicked);
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClicked);
		Messenger.RemoveListener<string>(UISignals.STARTED_LOADING_SCENE, OnSceneStartedLoading);
	}

	private void Initialize()
	{
		AkSoundEngine.LoadBank("Characters", out var out_bankID);
		AkSoundEngine.LoadBank("Combat", out out_bankID);
		AkSoundEngine.LoadBank("Misc", out out_bankID);
		AkSoundEngine.LoadBank("Music", out out_bankID);
		AkSoundEngine.LoadBank("Spells", out out_bankID);
		AkSoundEngine.LoadBank("Spells_2", out out_bankID);
		AkSoundEngine.LoadBank("Structures", out out_bankID);
		AkSoundEngine.LoadBank("Tile_Objects", out out_bankID);
		AkSoundEngine.LoadBank("UI", out out_bankID);
		AkSoundEngine.LoadBank("World_Events", out out_bankID);
		SetMasterVolume(SettingsManager.Instance.settings.masterVolume);
		SetMusicVolume(SettingsManager.Instance.settings.musicVolume);
		SetSFXVolume(SettingsManager.Instance.settings.sfxVolume);
		AkSoundEngine.SetState("Ambient_Music_States", "Main_Menu");
		Messenger.MarkAsPermanent(SettingsSignals.MASTER_VOLUME_CHANGED);
		Messenger.MarkAsPermanent(SettingsSignals.MUSIC_VOLUME_CHANGED);
		Messenger.MarkAsPermanent(SettingsSignals.SFX_VOLUME_CHANGED);
		Messenger.MarkAsPermanent(UISignals.BUTTON_CLICKED);
		Messenger.MarkAsPermanent(UISignals.TOGGLE_CLICKED);
		Messenger.MarkAsPermanent(UISignals.STARTED_LOADING_SCENE);
		Messenger.AddListener<float>(SettingsSignals.MASTER_VOLUME_CHANGED, SetMasterVolume);
		Messenger.AddListener<float>(SettingsSignals.MUSIC_VOLUME_CHANGED, SetMusicVolume);
		Messenger.AddListener<float>(SettingsSignals.SFX_VOLUME_CHANGED, SetSFXVolume);
		Messenger.AddListener<RuinarchButton>(UISignals.BUTTON_CLICKED, OnButtonClicked);
		Messenger.AddListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClicked);
		Messenger.AddListener<string>(UISignals.STARTED_LOADING_SCENE, OnSceneStartedLoading);
	}

	public void OnGameLoaded()
	{
		Messenger.AddListener(PlayerSignals.START_THREAT_EFFECT, OnStartThreatEffect);
		Messenger.AddListener(PlayerSignals.STOP_THREAT_EFFECT, OnStopThreatEffect);
		Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnObtainIntel);
		Messenger.AddListener<ISelectable>(ControlsSignals.SELECTABLE_LEFT_CLICKED, WorldSelectableLeftClicked);
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	public void OnLoadoutSelected()
	{
	}

	private void OnGamePaused(bool p_isPaused)
	{
		if (p_isPaused)
		{
			AkSoundEngine.PostEvent("Pause_Spells_SFX", base.gameObject);
		}
		else
		{
			AkSoundEngine.PostEvent("Resume_Spells_SFX", base.gameObject);
		}
	}

	private void PlayThreatMusic()
	{
		if (!isPlayingThreatMusic)
		{
			isPlayingThreatMusic = true;
			AkSoundEngine.SetState("Ambient_Music_States", "Retaliation");
		}
	}

	private void StopThreatMusic()
	{
		if (isPlayingThreatMusic)
		{
			isPlayingThreatMusic = false;
			AkSoundEngine.SetState("Ambient_Music_States", "In_Game");
		}
	}

	private void OnStopThreatEffect()
	{
		StopThreatMusic();
	}

	private void OnStartThreatEffect()
	{
		PlayThreatMusic();
	}

	private void SetMasterVolume(float volume)
	{
		AkSoundEngine.SetRTPCValue("Master_Volume", volume);
	}

	private void SetMusicVolume(float volume)
	{
		AkSoundEngine.SetRTPCValue("Music_Volume", volume);
	}

	private void SetSFXVolume(float volume)
	{
		AkSoundEngine.SetRTPCValue("SFX_Volume", volume);
	}

	private void OnButtonClicked(RuinarchButton button)
	{
		if (button.playClickAudio)
		{
			buttonClick.Post(base.gameObject);
		}
	}

	private void OnToggleClicked(RuinarchToggle toggle)
	{
		toggleClick.Post(base.gameObject);
	}

	public void OnErrorSoundPlay()
	{
		negativeNotification.Post(base.gameObject);
	}

	public void OnTextPopUpSoundPlay()
	{
		positiveNotification.Post(base.gameObject);
	}

	private void OnObtainIntel(IIntel intel)
	{
		particleMagnet.Post(base.gameObject);
	}

	public void PlayConversationMenuOpenedSFX()
	{
		conversationMenuOpened.Post(base.gameObject);
	}

	public void PlayParticleMagnet()
	{
		particleMagnet.Post(base.gameObject);
	}

	public void PlayAlertNotificationSound()
	{
		if ((!(GameManager.Instance != null) || GameManager.Instance.gameHasStarted) && (!(LevelLoaderManager.Instance != null) || !LevelLoaderManager.Instance.IsLoadingScreenActive()))
		{
			alertNotification.Post(base.gameObject);
		}
	}

	private void OnSceneStartedLoading(string sceneName)
	{
		UpdateAmbientSoundStateBasedOnCurrentScene();
		if (sceneName == "MainMenu")
		{
			base.transform.SetParent(null);
		}
	}

	public void UpdateAmbientSoundStateBasedOnCurrentScene()
	{
		if (LevelLoaderManager.Instance.IsLoadingScreenActive())
		{
			AkSoundEngine.SetState("Ambient_Music_States", "Loading_Menu");
			return;
		}
		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene.name == "MainMenu")
		{
			AkSoundEngine.SetState("Ambient_Music_States", "Main_Menu");
		}
		else if (activeScene.name == "Game")
		{
			AkSoundEngine.SetState("Ambient_Music_States", "In_Game");
		}
	}

	private void WorldSelectableLeftClicked(ISelectable selectable)
	{
		worldSelectableClick.Post(base.gameObject);
	}

	public void SetCameraParent(BaseCameraMove cameraMove)
	{
		if (cameraMove == null)
		{
			base.transform.SetParent(null);
		}
		else
		{
			base.transform.SetParent(cameraMove.transform);
		}
		base.transform.localPosition = Vector3.zero;
	}

	public uint PlaySpellSFXAndUpdateBasedOnTimeState(string p_sfxEvent, GameObject p_go)
	{
		uint num = AkSoundEngine.PostEvent(p_sfxEvent, p_go);
		if (GameManager.Instance.isPaused)
		{
			AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Pause, num);
		}
		return num;
	}

	public void TryPlayUISFX(string p_event)
	{
		if ((!(GameManager.Instance != null) || GameManager.Instance.gameHasStarted) && (!(LevelLoaderManager.Instance != null) || !LevelLoaderManager.Instance.IsLoadingScreenActive()))
		{
			AkSoundEngine.PostEvent(p_event, base.gameObject);
		}
	}
}
