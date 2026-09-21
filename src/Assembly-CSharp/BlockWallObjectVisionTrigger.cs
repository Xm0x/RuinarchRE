using UnityEngine;

public class BlockWallObjectVisionTrigger : TileObjectVisionTrigger
{
	[SerializeField]
	protected ProjectileReceiver _projectileReceiver;

	public override ProjectileReceiver projectileReceiver => _projectileReceiver;

	public override void Initialize(IDamageable damageable)
	{
		_projectileReceiver.gameObject.SetActive(value: true);
		_projectileReceiver.Initialize(damageable);
		base.Initialize(damageable);
	}
}
