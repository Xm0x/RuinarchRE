using UnityEngine;

public class RFX4_DeactivateByTime : MonoBehaviour
{
	public GameObject DeactivatedGameObject;

	public float DeactivateTime = 3f;

	private bool isActiveState;

	private float currentTime;

	private void OnEnable()
	{
		currentTime = 0f;
		isActiveState = true;
	}

	private void Update()
	{
		currentTime += Time.deltaTime;
		if (isActiveState && currentTime >= DeactivateTime)
		{
			isActiveState = false;
			DeactivatedGameObject.SetActive(value: false);
		}
	}
}
