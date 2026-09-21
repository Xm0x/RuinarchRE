using System;

[Serializable]
public class SaveDataIceteroidsTileObject : SaveDataAOEPlayerSpellTileObject
{
	public int remainingIceteroidsDuration;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		IceteroidsTileObject iceteroidsTileObject = tileObject as IceteroidsTileObject;
		remainingIceteroidsDuration = iceteroidsTileObject.currentIceteroidsDuration;
	}
}
