using UnityEngine;

public class RFX4_CopyPosition : MonoBehaviour
{
	public Transform CopiedTransform;

	private Transform t;

	private void Start()
	{
		t = base.transform;
	}

	private void Update()
	{
		t.position = CopiedTransform.position;
	}
}
