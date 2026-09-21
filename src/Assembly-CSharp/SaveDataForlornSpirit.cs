public class SaveDataForlornSpirit : SaveDataTileObject
{
	public int currentDuration;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		ForlornSpirit forlornSpirit = tileObject as ForlornSpirit;
		currentDuration = forlornSpirit.currentDuration;
	}
}
