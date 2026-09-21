public class SaveDataIceBlockWall : SaveDataTileObject
{
	public bool hasExpiry;

	public GameDate expiryDate;

	public bool leftBotImpassable;

	public bool leftTopImpassable;

	public bool topLeftImpassable;

	public bool topRightImpassable;

	public bool rightTopImpassable;

	public bool rightBotImpassable;

	public bool botRightImpassable;

	public bool botLeftImpassable;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		IceBlockWall iceBlockWall = tileObject as IceBlockWall;
		hasExpiry = !string.IsNullOrEmpty(iceBlockWall.expiryScheduleKey);
		expiryDate = iceBlockWall.expiryDate;
		leftBotImpassable = iceBlockWall.leftBotImpassable;
		leftTopImpassable = iceBlockWall.leftTopImpassable;
		topLeftImpassable = iceBlockWall.topLeftImpassable;
		topRightImpassable = iceBlockWall.topRightImpassable;
		rightTopImpassable = iceBlockWall.rightTopImpassable;
		rightBotImpassable = iceBlockWall.rightBotImpassable;
		botRightImpassable = iceBlockWall.botRightImpassable;
		botLeftImpassable = iceBlockWall.botLeftImpassable;
	}

	public override TileObject Load()
	{
		TileObject tileObject = base.Load();
		IceBlockWall iceBlockWall = tileObject as IceBlockWall;
		if (hasExpiry)
		{
			iceBlockWall.SetExpiry(expiryDate);
		}
		return tileObject;
	}
}
