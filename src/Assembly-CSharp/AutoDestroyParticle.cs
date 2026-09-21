using EZObjectPools;
using UnityEngine;

public class AutoDestroyParticle : PooledObject
{
	[SerializeField]
	private ParticleSystem[] particleSystems;

	private void OnEnable()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem obj = particleSystems[i];
			obj.Stop();
			obj.Clear();
			obj.Play();
		}
	}

	private void LateUpdate()
	{
		bool flag = true;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			if (particleSystems[i].IsAlive())
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			ObjectPoolManager.Instance.DestroyObject(base.gameObject);
		}
	}

	public void StopEmission()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem obj = particleSystems[i];
			obj.Stop();
			obj.Clear();
		}
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem obj = particleSystems[i];
			obj.Stop();
			obj.Clear();
		}
	}
}
