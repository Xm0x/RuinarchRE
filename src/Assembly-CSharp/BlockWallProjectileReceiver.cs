using UnityEngine;

public class BlockWallProjectileReceiver : ProjectileReceiver
{
	public override void OnTriggerEnter2D(Collider2D collision)
	{
		if (base.owner != null)
		{
			Projectile andAddProjectileFromCache = CharacterManager.Instance.GetAndAddProjectileFromCache(collision);
			if (andAddProjectileFromCache != null && (andAddProjectileFromCache.source == null || base.owner.gridTileLocation == null || andAddProjectileFromCache.source.currentStructure != base.owner.gridTileLocation.structure))
			{
				andAddProjectileFromCache.OnProjectileHit(base.owner);
			}
		}
	}
}
