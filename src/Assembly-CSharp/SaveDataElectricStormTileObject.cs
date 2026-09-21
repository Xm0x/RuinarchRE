using System;

[Serializable]
public class SaveDataElectricStormTileObject : SaveDataAOEPlayerSpellTileObject
{
	public bool isElectricStormCastedByPlayer;

	public int currentElectricStormDuration;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		ElectricStormTileObject electricStormTileObject = tileObject as ElectricStormTileObject;
		currentElectricStormDuration = electricStormTileObject.currentElectricStormDuration;
		isElectricStormCastedByPlayer = electricStormTileObject.isElectricStormCastedByPlayer;
	}
}
