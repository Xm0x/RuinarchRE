using UnityEngine;

public class SecretManager : MonoBehaviour
{
	public static SecretManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	public void Initialize()
	{
	}
}
