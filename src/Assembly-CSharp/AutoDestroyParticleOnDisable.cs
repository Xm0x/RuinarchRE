using EZObjectPools;

public class AutoDestroyParticleOnDisable : PooledObject
{
	private void OnParticleSystemStopped()
	{
		ObjectPoolManager.Instance.DestroyObject(this);
	}
}
