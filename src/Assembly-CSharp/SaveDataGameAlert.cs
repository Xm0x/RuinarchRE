using Quests.Alerts;
using Tutorial;

public class SaveDataGameAlert : SaveData<GameAlert>, ISavableCounterpart
{
	public string displayName;

	public bool isActive;

	public Game_Alert alertType;

	public GameDate expirationDate;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Game_Alert;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		persistentID = data.persistentID;
		displayName = data.displayName;
		alertType = data.alertType;
		isActive = data.isActive;
		expirationDate = data.expirationDate;
	}

	public override GameAlert Load()
	{
		return TutorialManager.Instance.LoadGameAlert<GameAlert>(this);
	}

	public override void CleanUp()
	{
	}
}
