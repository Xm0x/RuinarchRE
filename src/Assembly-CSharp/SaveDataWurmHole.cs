using System;

[Serializable]
public class SaveDataWurmHole : SaveDataTileObject
{
	public string wurmHoleConnection;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		WurmHole wurmHole = tileObject as WurmHole;
		wurmHoleConnection = wurmHole.wurmHoleConnection.persistentID;
	}
}
