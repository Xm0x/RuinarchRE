using EZObjectPools;
using UnityEngine;

[AddComponentMenu("EZ Object Pools/Pooled Objects/Timed Disable")]
public class TimedDisable : PooledObject
{
	private float timer;

	public float DisableTime;

	public bool paused;

	private void OnEnable()
	{
		timer = 0f;
	}

	private void Update()
	{
		if (!paused)
		{
			timer += Time.deltaTime;
			if (timer > DisableTime)
			{
				SendObjectBackToPool();
			}
		}
	}
}
