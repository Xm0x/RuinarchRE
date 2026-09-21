using UnityEngine;

public class RFX4_StartDelay : MonoBehaviour
{
	public GameObject ActivatedGameObject;

	public float Delay = 1f;

	private float currentTime;

	private bool isEnabled;

	private void OnEnable()
	{
		ActivatedGameObject.SetActive(value: false);
		isEnabled = false;
		currentTime = 0f;
	}

	private void Update()
	{
		currentTime += Time.deltaTime;
		if (!isEnabled && currentTime >= Delay)
		{
			isEnabled = true;
			ActivatedGameObject.SetActive(value: true);
		}
	}
}
