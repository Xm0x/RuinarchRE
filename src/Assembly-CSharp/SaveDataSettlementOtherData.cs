public class SaveDataSettlementOtherData : SaveDataOtherData
{
	public string settlementID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		SettlementOtherData settlementOtherData = data as SettlementOtherData;
		settlementID = settlementOtherData.settlement.persistentID;
	}

	public override OtherData Load()
	{
		return new SettlementOtherData(this);
	}
}
