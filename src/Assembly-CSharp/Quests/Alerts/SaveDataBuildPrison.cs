namespace Quests.Alerts;

public class SaveDataBuildPrison : SaveDataGameAlert
{
	public int currentStep;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		BuildPrison buildPrison = data as BuildPrison;
		currentStep = buildPrison.currentStep;
	}
}
