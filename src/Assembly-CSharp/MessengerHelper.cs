using UnityEngine;

public sealed class MessengerHelper : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnDestroy()
	{
	}
}
