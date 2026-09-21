public class ProfessionPedestal : TileObject
{
	public ProfessionPedestal()
	{
		Initialize(TILE_OBJECT_TYPE.PROFESSION_PEDESTAL, shouldAddCommonAdvertisements: false);
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public ProfessionPedestal(SaveDataTileObject data)
		: base(data)
	{
	}
}
