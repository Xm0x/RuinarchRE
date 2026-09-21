namespace Quests.Alerts;

public class SaveDataBuildWatcher : SaveDataGameAlert
{
	public int currentStep;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		BuildWatcher buildWatcher = data as BuildWatcher;
		currentStep = buildWatcher.currentStep;
	}
}
