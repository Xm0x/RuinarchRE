namespace Inner_Maps.Location_Structures;

public class LightningTower : DefenseTower
{
	protected override int maxShots => 5;

	protected override int shotCooldown => 20;

	public LightningTower(Region location)
		: base(STRUCTURE_TYPE.LIGHTNING_TOWER, location)
	{
		SetMaxHPAndReset(8000);
	}

	public LightningTower(Region location, SaveDataDefenseTower data)
		: base(location, data)
	{
		SetMaxHP(8000);
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		if (structureObj is LightningTowerStructureObject lightningTowerStructureObject)
		{
			lightningTowerStructureObject.SetLightningParticleParentState(p_state: true);
		}
	}

	protected override void CreateProjectile(IDamageable target)
	{
		LocationGridTile gridTileLocation = target.gridTileLocation;
		GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Lightning_Strike);
		AkSoundEngine.PostEvent("Play_Lightning_Tower_Attack", base.structureObj.gameObject);
		target.AdjustHP(-300, ELEMENTAL_TYPE.Electric, triggerDeath: true, this, null, showHPBar: true);
	}
}
