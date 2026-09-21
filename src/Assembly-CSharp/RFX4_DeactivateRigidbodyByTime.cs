using UnityEngine;

public class RFX4_DeactivateRigidbodyByTime : MonoBehaviour
{
	public float TimeDelayToDeactivate = 6f;

	private void OnEnable()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		component.isKinematic = false;
		component.detectCollisions = true;
		Invoke("Deactivate", TimeDelayToDeactivate);
	}

	private void Deactivate()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		component.isKinematic = true;
		component.detectCollisions = false;
	}
}
