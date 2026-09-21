using Steamworks;
using UnityEngine;

public class SteamAchievementsManager : MonoBehaviour
{
	public static SteamAchievementsManager Instance;

	private CGameID _gameID;

	protected Steamworks.Callback<UserStatsReceived_t> onUserStatsReceived;

	protected Steamworks.Callback<UserStatsStored_t> onUserStatsStored;

	protected Steamworks.Callback<UserAchievementStored_t> onUserAchievementStored;

	public bool hasSuccessfullyLoadStatsAndAchievements { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			SteamManager.OnSteamAPIInitialized.AddListener(OnSteamAPIInitialized);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void LoadStatsAndAchievements(Achievement[] p_achievements)
	{
		if (!hasSuccessfullyLoadStatsAndAchievements)
		{
			return;
		}
		foreach (Achievement achievement in p_achievements)
		{
			if (!string.IsNullOrEmpty(achievement.statID))
			{
				SteamUserStats.GetStat(achievement.statID, out int pData);
				achievement.SetCurrentProgress(pData);
			}
			SteamUserStats.GetAchievement(achievement.achievementID, out var pbAchieved);
			achievement.SetIsAchieved(pbAchieved);
		}
	}

	public void FulfillAchievement(string p_achievementID, bool p_shouldStoreStats)
	{
		if (hasSuccessfullyLoadStatsAndAchievements && SteamUserStats.SetAchievement(p_achievementID) && p_shouldStoreStats)
		{
			SteamUserStats.StoreStats();
		}
	}

	public void SetAchievementStat(string p_statID, int p_currentProgress, bool p_shouldStoreStats)
	{
		if (hasSuccessfullyLoadStatsAndAchievements && SteamUserStats.SetStat(p_statID, p_currentProgress) && p_shouldStoreStats)
		{
			SteamUserStats.StoreStats();
		}
	}

	public void ResetAllStatsAndAchievements(bool p_shouldStoreStats)
	{
		if (hasSuccessfullyLoadStatsAndAchievements)
		{
			SteamUserStats.ResetAllStats(bAchievementsToo: true);
			if (p_shouldStoreStats)
			{
				SteamUserStats.StoreStats();
			}
		}
	}

	public void ResetAchievement(string p_achievementID, bool p_shouldStoreStats)
	{
		if (hasSuccessfullyLoadStatsAndAchievements)
		{
			SteamUserStats.ClearAchievement(p_achievementID);
			if (p_shouldStoreStats)
			{
				SteamUserStats.StoreStats();
			}
		}
	}

	public void ResetAchievementStat(string p_statID, bool p_shouldStoreStats)
	{
		if (hasSuccessfullyLoadStatsAndAchievements)
		{
			SteamUserStats.SetStat(p_statID, 0);
			if (p_shouldStoreStats)
			{
				SteamUserStats.StoreStats();
			}
		}
	}

	public void StoreStatsAndAchievements()
	{
		if (hasSuccessfullyLoadStatsAndAchievements)
		{
			SteamUserStats.StoreStats();
		}
	}

	private void OnSteamAPIInitialized()
	{
		_gameID = new CGameID(SteamUtils.GetAppID());
		onUserStatsReceived = Steamworks.Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		onUserStatsStored = Steamworks.Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		onUserAchievementStored = Steamworks.Callback<UserAchievementStored_t>.Create(OnAchievementStored);
		SteamUserStats.RequestCurrentStats();
	}

	private void OnUserStatsReceived(UserStatsReceived_t p_stats)
	{
		if ((ulong)_gameID == p_stats.m_nGameID && EResult.k_EResultOK == p_stats.m_eResult)
		{
			hasSuccessfullyLoadStatsAndAchievements = true;
		}
	}

	private void OnUserStatsStored(UserStatsStored_t p_stats)
	{
		if ((ulong)_gameID == p_stats.m_nGameID && EResult.k_EResultOK != p_stats.m_eResult && EResult.k_EResultInvalidParam == p_stats.m_eResult)
		{
			OnUserStatsReceived(new UserStatsReceived_t
			{
				m_eResult = EResult.k_EResultOK,
				m_nGameID = (ulong)_gameID
			});
		}
	}

	private void OnAchievementStored(UserAchievementStored_t p_achievement)
	{
		if ((ulong)_gameID == p_achievement.m_nGameID)
		{
			_ = p_achievement.m_nMaxProgress;
		}
	}
}
