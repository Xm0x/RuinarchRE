using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;

public class SaveDataLocationEventManager : SaveData<LocationEventManager>
{
	public List<SaveDataSettlementEvent> settlementEvents;

	public override void Save(LocationEventManager data)
	{
		base.Save(data);
		settlementEvents = new List<SaveDataSettlementEvent>();
		for (int i = 0; i < data.activeEvents.Count; i++)
		{
			SaveDataSettlementEvent item = data.activeEvents[i].Save();
			settlementEvents.Add(item);
		}
	}
}
