using System.Collections.Generic;

public class SaveDataPowerCrystal : SaveDataTileObject
{
	public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();

	public float bonusResistance;

	public float bonusPiercing;

	public bool hasScheduleDestruction;

	public GameDate destroyDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		PowerCrystal powerCrystal = tileObject as PowerCrystal;
		bonusResistance = powerCrystal.amountBonusResistance;
		bonusPiercing = powerCrystal.amountBonusPiercing;
		powerCrystal.resistanceBonuses.ForEach(delegate(RESISTANCE eachRes)
		{
			resistanceBonuses.Add(eachRes);
		});
		hasScheduleDestruction = powerCrystal.hasScheduleDestruction;
		destroyDate = powerCrystal.destroyDate;
	}
}
