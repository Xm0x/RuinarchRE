public class SaveDataVaporVent : SaveDataTileObject
{
	public int activityCycle;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		VaporVent vaporVent = tileObject as VaporVent;
		activityCycle = vaporVent.activityCycle;
	}
}
