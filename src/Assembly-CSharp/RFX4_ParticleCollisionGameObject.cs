using System.Collections.Generic;
using UnityEngine;

public class RFX4_ParticleCollisionGameObject : MonoBehaviour
{
	public GameObject InstancedGO;

	public float DestroyDelay = 5f;

	public GameObject RotationParent;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private ParticleSystem initiatorPS;

	private void OnEnable()
	{
		collisionEvents.Clear();
		initiatorPS = GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject other)
	{
		int num = initiatorPS.GetCollisionEvents(other, collisionEvents);
		for (int i = 0; i < num; i++)
		{
			GameObject obj = ((!(RotationParent != null)) ? Object.Instantiate(InstancedGO, collisionEvents[i].intersection, default(Quaternion)) : Object.Instantiate(InstancedGO, collisionEvents[i].intersection, RotationParent.transform.rotation));
			Object.Destroy(obj, DestroyDelay);
		}
	}
}
