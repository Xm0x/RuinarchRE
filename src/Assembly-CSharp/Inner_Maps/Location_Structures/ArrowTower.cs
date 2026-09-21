namespace Inner_Maps.Location_Structures;

public class ArrowTower : DefenseTower
{
	protected override int maxShots => 10;

	protected override int shotCooldown => 10;

	public ArrowTower(Region location)
		: base(STRUCTURE_TYPE.ARROW_TOWER, location)
	{
		SetMaxHPAndReset(8000);
	}

	public ArrowTower(Region location, SaveDataDefenseTower data)
		: base(location, data)
	{
		SetMaxHP(8000);
	}

	protected override void CreateProjectile(IDamageable target)
	{
		if (target != null && target.currentHP > 0 && target.gridTileLocation != null)
		{
			Projectile projectile = CombatManager.Instance.CreateNewProjectile(ELEMENTAL_TYPE.Normal, base.structureObj.objectsParent, base.structureObj.transform.position);
			projectile.SetTarget(target.GetProjectileTargetPosition(), target, null, null);
			projectile.onHitAction = OnProjectileHit;
			AkSoundEngine.PostEvent("Play_Arrow_Shoot", base.structureObj.gameObject);
		}
	}

	private void OnProjectileHit(Character actor, IDamageable target, CombatState fromState, Projectile projectile)
	{
		if (target.mapObjectVisual != null)
		{
			AkSoundEngine.PostEvent("Play_Arrow_Hit_Body", target.mapObjectVisual.gameObject);
		}
		target.AdjustHP(-100, ELEMENTAL_TYPE.Normal, triggerDeath: true, this, null, showHPBar: true);
	}
}
