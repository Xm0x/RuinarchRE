public class WinterRose : TileObject
{
	private AutoDestroyParticle _particleEffect;

	public WinterRose()
	{
		Initialize(TILE_OBJECT_TYPE.WINTER_ROSE, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
	}

	public WinterRose(SaveDataTileObject data)
		: base(data)
	{
	}

	public void WinterRoseEffect()
	{
		if (gridTileLocation != null)
		{
			_particleEffect = GameManager.Instance.CreateParticleEffectAt(gridTileLocation.area.gridTileComponent.centerGridTile, PARTICLE_EFFECT.Winter_Rose).GetComponent<AutoDestroyParticle>();
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	private void OnDoneChangingBiome()
	{
		_particleEffect.StopEmission();
	}
}
