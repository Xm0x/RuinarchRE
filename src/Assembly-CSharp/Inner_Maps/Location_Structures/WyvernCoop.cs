namespace Inner_Maps.Location_Structures;

public class WyvernCoop : ManMadeStructure
{
	public WyvernCoop(Region location)
		: base(STRUCTURE_TYPE.WYVERN_COOP, location)
	{
		SetMaxHPAndReset(3000);
	}

	public WyvernCoop(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(3000);
	}

	public void TendCoopBy(Character p_character)
	{
		ProduceWyvernEgg(p_character);
	}

	private void ProduceWyvernEgg(Character p_producer)
	{
		LocationGridTile locationGridTile = GetRandomPassableTileThatIsNotOccupied();
		if (locationGridTile == null)
		{
			locationGridTile = GetRandomPassableTile();
		}
		if (locationGridTile == null)
		{
			locationGridTile = GetRandomTile();
		}
		if (locationGridTile != null)
		{
			TileObject objHere = locationGridTile.tileObjectComponent.objHere;
			if (objHere != null)
			{
				locationGridTile.structure.RemovePOI(objHere);
			}
			if (locationGridTile.IsPassable() && locationGridTile.HasWalkableNode())
			{
				WyvernEgg wyvernEgg = InnerMapManager.Instance.CreateNewTileObject<WyvernEgg>(TILE_OBJECT_TYPE.WYVERN_EGG);
				wyvernEgg.SetCharacterThatLay(p_producer);
				AddPOI(wyvernEgg, locationGridTile);
			}
		}
	}
}
