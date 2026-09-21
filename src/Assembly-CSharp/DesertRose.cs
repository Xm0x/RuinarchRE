public class DesertRose : TileObject
{
	private AutoDestroyParticle _particleEffect;

	public DesertRose()
	{
		Initialize(TILE_OBJECT_TYPE.DESERT_ROSE, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
	}

	public DesertRose(SaveDataTileObject data)
		: base(data)
	{
	}
}
