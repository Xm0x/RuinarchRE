public class SaveDataOreVein : SaveDataTileObject
{
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
		OreVein oreVein = tileObject as OreVein;
		leftBotImpassable = oreVein.leftBotImpassable;
		leftTopImpassable = oreVein.leftTopImpassable;
		topLeftImpassable = oreVein.topLeftImpassable;
		topRightImpassable = oreVein.topRightImpassable;
		rightTopImpassable = oreVein.rightTopImpassable;
		rightBotImpassable = oreVein.rightBotImpassable;
		botRightImpassable = oreVein.botRightImpassable;
		botLeftImpassable = oreVein.botLeftImpassable;
	}
}
