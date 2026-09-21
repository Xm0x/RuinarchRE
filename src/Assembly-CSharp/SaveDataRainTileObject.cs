using System;

[Serializable]
public class SaveDataRainTileObject : SaveDataAOEPlayerSpellTileObject
{
	public int expiryInTicks;

	public bool isPlayerSource;

	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		RainTileObject rainTileObject = tileObject as RainTileObject;
		expiryInTicks = rainTileObject.expiryInTicks;
		isPlayerSource = rainTileObject.isPlayerSource;
		expiryDate = rainTileObject.expiryDate;
	}
}
