public class SaveDataTreeObject : SaveDataTileObject
{
	public TreeObject.Occupied_State occupiedState;

	public string occupyingEntID;

	public int count;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		TreeObject treeObject = tileObject as TreeObject;
		occupiedState = treeObject.occupiedState;
		count = treeObject.count;
		if (treeObject.ent != null)
		{
			occupyingEntID = treeObject.ent.persistentID;
		}
	}

	public override TileObject Load()
	{
		TileObject tileObject = base.Load();
		(tileObject as TreeObject).count = count;
		return tileObject;
	}
}
