public class SaveDataFeebleSpirit : SaveDataTileObject
{
	public int currentDuration;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		FeebleSpirit feebleSpirit = tileObject as FeebleSpirit;
		currentDuration = feebleSpirit.currentDuration;
	}
}
