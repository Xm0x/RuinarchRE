using System.Collections.Generic;

public class SaveDataSettlementFactionIdeologyComponent : SaveData<SettlementFactionIdeologyComponent>
{
	public GameDate scheduleDateForProcessingOfEvents;

	public FACTION_IDEOLOGY[] ideologies;

	public int[] daysPassed;

	public override void Save(SettlementFactionIdeologyComponent data)
	{
		scheduleDateForProcessingOfEvents = data.scheduleDateForProcessingOfEvents;
		if (data.daysPassedPerIdeology.Count <= 0)
		{
			return;
		}
		ideologies = new FACTION_IDEOLOGY[data.daysPassedPerIdeology.Count];
		daysPassed = new int[data.daysPassedPerIdeology.Count];
		int num = 0;
		foreach (KeyValuePair<FactionIdeology, int> item in data.daysPassedPerIdeology)
		{
			ideologies[num] = item.Key.ideologyType;
			daysPassed[num] = item.Value;
			num++;
		}
	}

	public override SettlementFactionIdeologyComponent Load()
	{
		return new SettlementFactionIdeologyComponent(this);
	}
}
