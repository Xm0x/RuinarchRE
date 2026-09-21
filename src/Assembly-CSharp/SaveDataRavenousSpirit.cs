public class SaveDataRavenousSpirit : SaveDataTileObject
{
	public int currentDuration;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		RavenousSpirit ravenousSpirit = tileObject as RavenousSpirit;
		currentDuration = ravenousSpirit.currentDuration;
	}
}
