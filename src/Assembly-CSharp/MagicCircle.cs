public class MagicCircle : TileObject
{
	public override bool canBeSeized => false;

	public MagicCircle()
	{
		Initialize(TILE_OBJECT_TYPE.MAGIC_CIRCLE, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DEVASTATION_RITUAL);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.traitContainer.AddTrait(this, "Indestructible");
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public MagicCircle(SaveDataTileObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Magic Circle " + base.id;
	}
}
