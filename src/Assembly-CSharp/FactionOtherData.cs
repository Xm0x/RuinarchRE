public class FactionOtherData : OtherData
{
	public Faction faction { get; private set; }

	public override object obj => faction;

	public FactionOtherData(Faction faction)
	{
		this.faction = faction;
	}

	public FactionOtherData(SaveDataFactionOtherData faction)
	{
		this.faction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(faction.factionID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataFactionOtherData saveDataFactionOtherData = new SaveDataFactionOtherData();
		saveDataFactionOtherData.Save(this);
		return saveDataFactionOtherData;
	}

	public override void CleanUp()
	{
		faction = null;
	}
}
