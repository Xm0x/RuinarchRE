using UnityEngine;

public class LockRotation : MonoBehaviour
{
	private Quaternion rotation;

	private void Start()
	{
		rotation = base.transform.rotation;
	}

	private void LateUpdate()
	{
		base.transform.rotation = rotation;
	}
}
