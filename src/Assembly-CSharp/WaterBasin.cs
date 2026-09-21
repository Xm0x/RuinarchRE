public class WaterBasin : TileObject
{
	public WaterBasin()
	{
		Initialize(TILE_OBJECT_TYPE.WATER_BASIN);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public WaterBasin(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.MAGIC_ACADEMY)
		{
			AddAdvertisedAction(INTERACTION_TYPE.TRAIN_HEALING_MAGIC);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_HEALING_MAGIC);
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_HEALING_MAGIC);
	}
}
