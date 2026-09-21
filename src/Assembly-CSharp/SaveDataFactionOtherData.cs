public class SaveDataFactionOtherData : SaveDataOtherData
{
	public string factionID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		FactionOtherData factionOtherData = data as FactionOtherData;
		factionID = factionOtherData.faction.persistentID;
	}

	public override OtherData Load()
	{
		return new FactionOtherData(this);
	}
}
