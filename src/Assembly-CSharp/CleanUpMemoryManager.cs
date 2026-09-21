using UnityEngine;
using UnityEngine.SceneManagement;

public class CleanUpMemoryManager : MonoBehaviour
{
	public static CleanUpMemoryManager Instance;

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
		SceneManager.sceneUnloaded += OnSceneUnLoaded;
	}

	private void OnSceneUnLoaded(Scene p_scene)
	{
		BroadcastCleanupMemorySignal();
		CleanupMessenger();
	}

	private void BroadcastCleanupMemorySignal()
	{
		Messenger.Broadcast(Signals.CLEAN_UP_MEMORY);
	}

	private void CleanupMessenger()
	{
		Messenger.Cleanup();
	}
}
