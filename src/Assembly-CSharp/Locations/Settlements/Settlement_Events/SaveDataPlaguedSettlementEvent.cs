namespace Locations.Settlements.Settlement_Events;

public class SaveDataPlaguedSettlementEvent : SaveDataSettlementEvent
{
	public GameDate endDate;

	public PLAGUE_EVENT_RESPONSE rulerDecision;

	public bool hasLeaderMadeADecision;

	public override void Save(SettlementEvent data)
	{
		base.Save(data);
		PlaguedEvent plaguedEvent = data as PlaguedEvent;
		endDate = plaguedEvent.endDate;
		rulerDecision = plaguedEvent.rulerDecision;
		hasLeaderMadeADecision = plaguedEvent.hasLeaderMadeADecision;
	}

	public override SettlementEvent Load()
	{
		return new PlaguedEvent(this);
	}
}
