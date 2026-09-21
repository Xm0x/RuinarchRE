using UnityEngine;

public class RFX4_CollisionPropertyDeactiavtion : MonoBehaviour
{
	public float DeactivateTimeDelay = 1f;

	private float startTime;

	private WindZone windZone;

	private ParticleSystem ps;

	private ParticleSystem.CollisionModule collisionModule;

	private void Awake()
	{
		ps = GetComponent<ParticleSystem>();
		collisionModule = ps.collision;
	}

	private void OnEnable()
	{
		startTime = Time.time;
		collisionModule.enabled = true;
	}

	private void Update()
	{
		if (Time.time - startTime >= DeactivateTimeDelay)
		{
			collisionModule.enabled = false;
		}
	}
}
