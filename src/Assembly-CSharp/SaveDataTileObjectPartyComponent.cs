public class SaveDataTileObjectPartyComponent : SaveData<TileObjectPartyComponent>
{
	public string currentSnatchObjectParty;

	public override void Save(TileObjectPartyComponent data)
	{
		base.Save(data);
		if (data.currentSnatchObjectParty != null)
		{
			currentSnatchObjectParty = data.currentSnatchObjectParty.persistentID;
		}
	}

	public override TileObjectPartyComponent Load()
	{
		return new TileObjectPartyComponent(this);
	}
}
