namespace Quests.Alerts;

public class SaveDataMummifiedReleaseAlert : SaveDataGameAlert
{
	public string characterID;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		if (data is MummifiedReleaseAlert { character: not null } mummifiedReleaseAlert)
		{
			characterID = mummifiedReleaseAlert.character.persistentID;
		}
	}
}
