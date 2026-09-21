public class SaveDataArcheryTarget : SaveDataTileObject
{
	public string currentUserID;

	public override void Save(TileObject data)
	{
		base.Save(data);
		ArcheryTarget archeryTarget = data as ArcheryTarget;
		if (archeryTarget.currentUser != null)
		{
			currentUserID = archeryTarget.currentUser.persistentID;
		}
	}
}
