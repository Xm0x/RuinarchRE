public class SaveDataBlockWall : SaveDataTileObject
{
	public WALL_TYPE wallType;

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
		BlockWall blockWall = tileObject as BlockWall;
		wallType = blockWall.wallType;
		hasExpiry = !string.IsNullOrEmpty(blockWall.expiryScheduleKey);
		expiryDate = blockWall.expiryDate;
		leftBotImpassable = blockWall.leftBotImpassable;
		leftTopImpassable = blockWall.leftTopImpassable;
		topLeftImpassable = blockWall.topLeftImpassable;
		topRightImpassable = blockWall.topRightImpassable;
		rightTopImpassable = blockWall.rightTopImpassable;
		rightBotImpassable = blockWall.rightBotImpassable;
		botRightImpassable = blockWall.botRightImpassable;
		botLeftImpassable = blockWall.botLeftImpassable;
	}

	public override TileObject Load()
	{
		TileObject tileObject = base.Load();
		BlockWall blockWall = tileObject as BlockWall;
		blockWall.SetWallType(wallType);
		if (hasExpiry)
		{
			blockWall.SetExpiry(expiryDate);
		}
		return tileObject;
	}
}
