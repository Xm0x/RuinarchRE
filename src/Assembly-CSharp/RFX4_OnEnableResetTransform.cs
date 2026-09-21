using UnityEngine;

public class RFX4_OnEnableResetTransform : MonoBehaviour
{
	private Transform t;

	private Vector3 startPosition;

	private Quaternion startRotation;

	private Vector3 startScale;

	private bool isInitialized;

	private void OnEnable()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			t = base.transform;
			startPosition = t.position;
			startRotation = t.rotation;
			startScale = t.localScale;
		}
		else
		{
			t.position = startPosition;
			t.rotation = startRotation;
			t.localScale = startScale;
		}
	}

	private void OnDisable()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			t = base.transform;
			startPosition = t.position;
			startRotation = t.rotation;
			startScale = t.localScale;
		}
		else
		{
			t.position = startPosition;
			t.rotation = startRotation;
			t.localScale = startScale;
		}
	}
}
