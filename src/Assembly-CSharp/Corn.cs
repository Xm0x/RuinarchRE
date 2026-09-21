public class Corn : FoodPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Corn;

	public Corn()
		: base(TILE_OBJECT_TYPE.CORN)
	{
	}

	public Corn(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject)
	{
	}

	public override string ToString()
	{
		return "Corn " + base.id;
	}

	public override void ApplyFoodEffectsToConsumer(Character p_consumer)
	{
		p_consumer.traitContainer.AddTrait(p_consumer, "Corn Fed");
	}
}
