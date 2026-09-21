public class LightningParticleEffect : BaseParticleEffect
{
	private void OnLightningStrike()
	{
		ObjectPoolManager.Instance.DestroyObject(base.gameObject);
	}
}
