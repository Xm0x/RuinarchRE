using BayatGames.SaveGameFree;
using UnityEngine;
using UnityEngine.Events;
using UtilityScripts;

public class AchievementManager : MonoBehaviour
{
	public static AchievementManager Instance;

	public UnityEvent<Achievement> onFulfillAchievement = new UnityEvent<Achievement>();

	public UnityEvent<Achievement> onResetAchievement = new UnityEvent<Achievement>();

	public UnityEvent onResetAllAchievements = new UnityEvent();

	public UnityEvent onInitializeAchievementManager = new UnityEvent();

	private Achievements _achievementsWrapper;

	private string _achievementsSavePath;

	private bool _hasInitialized;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!_hasInitialized)
		{
			_achievementsSavePath = Application.persistentDataPath + "/Achievements";
			onFulfillAchievement.RemoveAllListeners();
			onResetAchievement.RemoveAllListeners();
			onResetAllAchievements.RemoveAllListeners();
			LoadStatsAndAchievements();
			onInitializeAchievementManager?.Invoke();
			_hasInitialized = true;
		}
	}

	public void LoadStatsAndAchievements()
	{
		if (Utilities.DoesFileExist(_achievementsSavePath))
		{
			_achievementsWrapper = SaveGame.Load<Achievements>(_achievementsSavePath);
		}
		else
		{
			_achievementsWrapper = new Achievements();
			_achievementsWrapper.CreateNewAchievements();
		}
		SteamAchievementsManager.Instance.LoadStatsAndAchievements(_achievementsWrapper.achievements);
	}

	public Achievement GetAchievement(string p_achievementID)
	{
		for (int i = 0; i < _achievementsWrapper.achievements.Length; i++)
		{
			Achievement achievement = _achievementsWrapper.achievements[i];
			if (achievement.achievementID.Equals(p_achievementID))
			{
				return achievement;
			}
		}
		return null;
	}

	public Achievement GetAchievement(ACHIEVEMENT p_achievementIDEnum)
	{
		for (int i = 0; i < _achievementsWrapper.achievements.Length; i++)
		{
			Achievement achievement = _achievementsWrapper.achievements[i];
			if (achievement.achievementIDEnum == p_achievementIDEnum)
			{
				return achievement;
			}
		}
		return null;
	}

	public Achievement GetAchievement(ACHIEVEMENT_STAT p_statIDEnum)
	{
		for (int i = 0; i < _achievementsWrapper.achievements.Length; i++)
		{
			Achievement achievement = _achievementsWrapper.achievements[i];
			if (achievement.statIDEnum == p_statIDEnum)
			{
				return achievement;
			}
		}
		return null;
	}

	public void AdjustAchievementStatProgress(ACHIEVEMENT_STAT p_statIDEnum, int p_amount)
	{
		Achievement achievement = GetAchievement(p_statIDEnum);
		achievement.SetCurrentProgress(achievement.currentProgress + p_amount);
		bool flag = false;
		if (achievement.currentProgress >= achievement.maxProgress)
		{
			achievement.SetCurrentProgress(achievement.maxProgress);
			flag = true;
		}
		SteamAchievementsManager.Instance.SetAchievementStat(achievement.statID, achievement.currentProgress, p_shouldStoreStats: false);
		if (flag)
		{
			FulfillAchievement(achievement);
			return;
		}
		SaveAchievementsLocally();
		SteamAchievementsManager.Instance.StoreStatsAndAchievements();
	}

	public void FulfillAchievement(ACHIEVEMENT p_achievementIDEnum, bool p_shouldStoreStats = true)
	{
		Achievement achievement = GetAchievement(p_achievementIDEnum);
		FulfillAchievement(achievement, p_shouldStoreStats);
	}

	public void FulfillAchievement(string p_achievementID, bool p_shouldStoreStats = true)
	{
		Achievement achievement = GetAchievement(p_achievementID);
		FulfillAchievement(achievement, p_shouldStoreStats);
	}

	private void FulfillAchievement(Achievement p_achievement, bool p_shouldStoreStats = true)
	{
		if (!p_achievement.isAchieved)
		{
			p_achievement.SetIsAchieved(p_state: true);
			if (p_shouldStoreStats)
			{
				SaveAchievementsLocally();
			}
			SteamAchievementsManager.Instance.FulfillAchievement(p_achievement.achievementID, p_shouldStoreStats);
			onFulfillAchievement?.Invoke(p_achievement);
		}
	}

	public void ResetAchievement(ACHIEVEMENT p_achievementIDEnum, bool p_shouldStoreStats = true)
	{
		Achievement achievement = GetAchievement(p_achievementIDEnum);
		ResetAchievement(achievement, p_shouldStoreStats);
	}

	public void ResetAchievement(string p_achievementID, bool p_shouldStoreStats = true)
	{
		Achievement achievement = GetAchievement(p_achievementID);
		ResetAchievement(achievement, p_shouldStoreStats);
	}

	private void ResetAchievement(Achievement p_achievement, bool p_shouldStoreStats = true, bool p_shouldInvokeCallback = true)
	{
		if (p_achievement.isAchieved)
		{
			p_achievement.ResetAchievement();
			if (p_shouldStoreStats)
			{
				SaveAchievementsLocally();
			}
			SteamAchievementsManager.Instance.ResetAchievement(p_achievement.achievementID, p_shouldStoreStats);
			if (!string.IsNullOrEmpty(p_achievement.statID))
			{
				SteamAchievementsManager.Instance.ResetAchievementStat(p_achievement.statID, p_shouldStoreStats);
			}
			if (p_shouldInvokeCallback)
			{
				onResetAchievement?.Invoke(p_achievement);
			}
		}
	}

	public void ResetAllAchievements()
	{
		for (int i = 0; i < _achievementsWrapper.achievements.Length; i++)
		{
			Achievement p_achievement = _achievementsWrapper.achievements[i];
			ResetAchievement(p_achievement, p_shouldStoreStats: false, p_shouldInvokeCallback: false);
		}
		SaveAchievementsLocally();
		SteamAchievementsManager.Instance.StoreStatsAndAchievements();
		onResetAllAchievements?.Invoke();
	}

	public void ResetAllStatsAndAchievements(bool p_shouldStoreStats = true)
	{
		for (int i = 0; i < _achievementsWrapper.achievements.Length; i++)
		{
			_achievementsWrapper.achievements[i].ResetAchievement();
		}
		SteamAchievementsManager.Instance.ResetAllStatsAndAchievements(p_shouldStoreStats);
		SaveAchievementsLocally();
		onResetAllAchievements?.Invoke();
	}

	private void SaveAchievementsLocally()
	{
	}
}
