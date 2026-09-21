public class CultAltar : TileObject
{
	public override bool canBeSeized => false;

	public CultAltar()
	{
		Initialize(TILE_OBJECT_TYPE.CULT_ALTAR);
		AddAdvertisedAction(INTERACTION_TYPE.SUMMON_BONE_GOLEM);
		base.traitContainer.AddTrait(this, "Indestructible");
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public CultAltar(SaveDataTileObject data)
		: base(data)
	{
	}

	public override bool CanBeDamaged()
	{
		return false;
	}
}
