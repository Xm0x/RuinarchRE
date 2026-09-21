using UnityEngine;

public class RFX4_PhysXSetImpulse : MonoBehaviour
{
	public float Force = 1f;

	public ForceMode ForceMode;

	private Rigidbody rig;

	private Transform t;

	private void Start()
	{
		rig = GetComponent<Rigidbody>();
		t = base.transform;
	}

	private void FixedUpdate()
	{
		if (rig != null)
		{
			rig.AddForce(t.forward * Force, ForceMode);
		}
	}

	private void OnDisable()
	{
		if (rig != null)
		{
			rig.velocity = Vector3.zero;
		}
	}
}
