using UnityEngine;

public class WallProjectileReceiver : ProjectileReceiver
{
	private void Awake()
	{
		base.gameObject.tag = "Structure_Wall";
		if (_collider == null)
		{
			_collider = GetComponent<Collider2D>();
			_collider.isTrigger = true;
		}
	}

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
