namespace Quests.Alerts;

public class SaveDataFactionAwareAlert : SaveDataGameAlert
{
	public string factionName;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		FactionAwareAlert factionAwareAlert = data as FactionAwareAlert;
		factionName = factionAwareAlert.factionName;
	}
}
