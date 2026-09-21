public class SaveDataPoisonVent : SaveDataTileObject
{
	public int activityCycle;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		PoisonVent poisonVent = tileObject as PoisonVent;
		activityCycle = poisonVent.activityCycle;
	}
}
