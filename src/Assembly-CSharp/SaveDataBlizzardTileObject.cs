using System;

[Serializable]
public class SaveDataBlizzardTileObject : SaveDataAOEPlayerSpellTileObject
{
	public int expiryInTicks;

	public bool isPlayerSource;

	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		BlizzardTileObject blizzardTileObject = tileObject as BlizzardTileObject;
		expiryInTicks = blizzardTileObject.expiryInTicks;
		isPlayerSource = blizzardTileObject.isPlayerSource;
		expiryDate = blizzardTileObject.expiryDate;
	}
}
