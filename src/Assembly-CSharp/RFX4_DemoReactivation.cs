using UnityEngine;

public class RFX4_DemoReactivation : MonoBehaviour
{
	public float ReactivationTime = 5f;

	public GameObject Effect;

	private void Start()
	{
		InvokeRepeating("Reactivate", 0f, ReactivationTime);
	}

	private void Reactivate()
	{
		Effect.SetActive(value: false);
		Effect.SetActive(value: true);
	}
}
