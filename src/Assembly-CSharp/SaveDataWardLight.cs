public class SaveDataWardLight : SaveDataTileObject
{
	public string settlementOwner;

	public string[] charactersInRange;

	public override void Save(TileObject data)
	{
		base.Save(data);
		WardLight wardLight = data as WardLight;
		settlementOwner = wardLight.settlementOwner?.persistentID;
		charactersInRange = SaveUtilities.ConvertSavableListToIDsArray(wardLight.charactersInRange);
	}
}
