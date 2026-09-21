public class Mushroom : FoodPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Vegetables;

	public Mushroom()
		: base(TILE_OBJECT_TYPE.MUSHROOM)
	{
	}

	public Mushroom(SaveDataTileObject data)
		: base(data)
	{
	}

	protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true)
	{
		base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
		if (ChanceData.RollChance(CHANCE_TYPE.Mushroom_Abomination_Germ))
		{
			base.traitContainer.AddTrait(this, "Abomination Germ");
		}
	}

	public override string ToString()
	{
		return "Mushroom " + base.id;
	}
}
