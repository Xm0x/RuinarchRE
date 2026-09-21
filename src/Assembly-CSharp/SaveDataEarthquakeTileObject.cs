using System;

[Serializable]
public class SaveDataEarthquakeTileObject : SaveDataAOEPlayerSpellTileObject
{
	public int remainingEarthquakeDuration;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		EarthquakeTileObject earthquakeTileObject = tileObject as EarthquakeTileObject;
		remainingEarthquakeDuration = earthquakeTileObject.currentEarthquakeDuration;
	}
}
