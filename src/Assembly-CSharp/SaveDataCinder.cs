public class SaveDataCinder : SaveDataTileObject
{
	public int level;

	public bool hasExpiry;

	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Cinder cinder = tileObject as Cinder;
		hasExpiry = !string.IsNullOrEmpty(cinder.expiryScheduleKey);
		expiryDate = cinder.expiryDate;
		level = cinder.level;
	}

	public override TileObject Load()
	{
		TileObject tileObject = base.Load();
		Cinder cinder = tileObject as Cinder;
		if (hasExpiry)
		{
			cinder.SetExpiry(expiryDate);
		}
		return tileObject;
	}
}
