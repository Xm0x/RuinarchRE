namespace Locations.Settlements.Settlement_Events;

public abstract class SaveDataSettlementEvent : SaveData<SettlementEvent>
{
	public string settlementID;

	public override void Save(SettlementEvent data)
	{
		base.Save(data);
		settlementID = data.location.persistentID;
	}
}
