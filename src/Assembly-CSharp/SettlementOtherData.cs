using Locations.Settlements;

public class SettlementOtherData : OtherData
{
	public BaseSettlement settlement { get; private set; }

	public override object obj => settlement;

	public SettlementOtherData(BaseSettlement settlement)
	{
		this.settlement = settlement;
	}

	public SettlementOtherData(SaveDataSettlementOtherData saveData)
	{
		settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveData.settlementID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataSettlementOtherData saveDataSettlementOtherData = new SaveDataSettlementOtherData();
		saveDataSettlementOtherData.Save(this);
		return saveDataSettlementOtherData;
	}

	public override void CleanUp()
	{
		settlement = null;
	}
}
