namespace Quests.Alerts;

public class SaveDataBuildSpire : SaveDataGameAlert
{
	public int currentStep;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		BuildSpire buildSpire = data as BuildSpire;
		currentStep = buildSpire.currentStep;
	}
}
