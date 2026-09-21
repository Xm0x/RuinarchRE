using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DefaultProjectileReceiver : ProjectileReceiver
{
	public override void OnTriggerEnter2D(Collider2D collision)
	{
		if (base.owner != null)
		{
			Projectile andAddProjectileFromCache = CharacterManager.Instance.GetAndAddProjectileFromCache(collision);
			if (andAddProjectileFromCache != null && (andAddProjectileFromCache.targetObject == base.owner || andAddProjectileFromCache.targetObject == null))
			{
				andAddProjectileFromCache.OnProjectileHit(base.owner);
			}
		}
	}
}
