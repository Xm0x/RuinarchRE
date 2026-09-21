public class Trash : TileObject
{
	public Trash()
	{
		Initialize(TILE_OBJECT_TYPE.TRASH);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
	}

	public Trash(SaveDataTileObject data)
		: base(data)
	{
	}
}
