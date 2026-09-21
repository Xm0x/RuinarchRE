using System;

[Serializable]
public class SaveDataHeatWaveTileObject : SaveDataAOEPlayerSpellTileObject
{
	public int expiryInTicks;

	public bool isPlayerSource;

	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		HeatWaveTileObject heatWaveTileObject = tileObject as HeatWaveTileObject;
		expiryInTicks = heatWaveTileObject.expiryInTicks;
		isPlayerSource = heatWaveTileObject.isPlayerSource;
		expiryDate = heatWaveTileObject.expiryDate;
	}
}
