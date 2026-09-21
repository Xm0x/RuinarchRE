using Steamworks;
using UnityEngine;

namespace Managers;

public class SteamworksManager : MonoBehaviour
{
	public static SteamworksManager Instance;

	[SerializeField]
	private bool _allowSteamworks;

	public bool allowSteamworks => _allowSteamworks;

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

	public string GetSteamName()
	{
		if (SteamManager.Initialized)
		{
			return SteamFriends.GetPersonaName();
		}
		return string.Empty;
	}
}
