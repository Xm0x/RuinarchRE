public class Pineapple : FoodPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Pineapple;

	public Pineapple()
		: base(TILE_OBJECT_TYPE.PINEAPPLE)
	{
	}

	public Pineapple(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject)
	{
	}

	public override string ToString()
	{
		return "Pineapple " + base.id;
	}

	public override void ApplyFoodEffectsToConsumer(Character p_consumer)
	{
		p_consumer.traitContainer.AddTrait(p_consumer, "Pineapple Fed");
	}
}
