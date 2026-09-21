public class Iceberry : FoodPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Iceberry;

	public Iceberry()
		: base(TILE_OBJECT_TYPE.ICEBERRY)
	{
	}

	public Iceberry(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject)
	{
	}

	public override string ToString()
	{
		return "Iceberry " + base.id;
	}

	public override void ApplyFoodEffectsToConsumer(Character p_consumer)
	{
		p_consumer.traitContainer.AddTrait(p_consumer, "Iceberry Fed");
	}
}
