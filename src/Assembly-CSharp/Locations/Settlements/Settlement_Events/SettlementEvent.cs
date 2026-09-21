namespace Locations.Settlements.Settlement_Events;

public abstract class SettlementEvent
{
	protected NPCSettlement _location;

	public abstract SETTLEMENT_EVENT eventType { get; }

	public NPCSettlement location => _location;

	public SettlementEvent(NPCSettlement location)
	{
		_location = location;
	}

	public SettlementEvent(SaveDataSettlementEvent data)
	{
		_location = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.settlementID) as NPCSettlement;
	}

	public abstract void ActivateEvent(NPCSettlement p_settlement);

	public abstract void DeactivateEvent(NPCSettlement p_settlement);

	public virtual void ProcessNewVillager(Character newVillager)
	{
	}

	public virtual void ProcessRemovedVillager(Character removedVillager)
	{
	}

	public abstract SaveDataSettlementEvent Save();

	public virtual void LoadAdditionalData(NPCSettlement p_settlement)
	{
	}

	public virtual string GetTestingInfo()
	{
		return eventType.ToStringEnumNoSpace();
	}
}
