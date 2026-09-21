namespace Quests.Alerts;

public class SaveDataDevastationRitualAlert : SaveDataGameAlert
{
	public string actorID;

	public bool hasRitualEndedSuccessfully;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		DevastationRitualAlert devastationRitualAlert = data as DevastationRitualAlert;
		actorID = devastationRitualAlert.actor.persistentID;
		hasRitualEndedSuccessfully = devastationRitualAlert.hasRitualEndedSuccessfully;
	}
}
