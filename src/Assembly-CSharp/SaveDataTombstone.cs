public class SaveDataTombstone : SaveDataTileObject
{
	public string characterID;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Tombstone tombstone = tileObject as Tombstone;
		if (tombstone.character != null)
		{
			characterID = tombstone.character.persistentID;
		}
	}
}
