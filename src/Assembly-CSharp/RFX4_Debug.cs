using UnityEngine;

public class RFX4_Debug : MonoBehaviour
{
	private void Start()
	{
		RFX4_PhysicsMotion componentInChildren = GetComponentInChildren<RFX4_PhysicsMotion>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.CollisionEnter += CollisionEnter;
		}
		RFX4_RaycastCollision componentInChildren2 = GetComponentInChildren<RFX4_RaycastCollision>(includeInactive: true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.CollisionEnter += CollisionEnter;
		}
	}

	private void CollisionEnter(object sender, RFX4_PhysicsMotion.RFX4_CollisionInfo e)
	{
		Debug.Log(e.HitPoint);
		Debug.Log(e.HitGameObject.name);
		Debug.Log(e.HitCollider.name);
	}
}
