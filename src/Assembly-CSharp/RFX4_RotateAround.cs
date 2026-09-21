using UnityEngine;

public class RFX4_RotateAround : MonoBehaviour
{
	public Vector3 Offset = Vector3.forward;

	public Vector3 RotateVector = Vector3.forward;

	public float LifeTime = 1f;

	private Transform t;

	private float currentTime;

	private Quaternion rotation;

	private void Start()
	{
		t = base.transform;
		rotation = t.rotation;
	}

	private void OnEnable()
	{
		currentTime = 0f;
		if (t != null)
		{
			t.rotation = rotation;
		}
	}

	private void Update()
	{
		if (!(currentTime >= LifeTime) || !(LifeTime > 0.0001f))
		{
			currentTime += Time.deltaTime;
			t.Rotate(RotateVector * Time.deltaTime);
		}
	}
}
