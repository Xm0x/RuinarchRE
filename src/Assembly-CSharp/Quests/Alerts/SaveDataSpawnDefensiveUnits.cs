namespace Quests.Alerts;

public class SaveDataSpawnDefensiveUnits : SaveDataGameAlert
{
	public int currentStep;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		SpawnDefensiveUnits spawnDefensiveUnits = data as SpawnDefensiveUnits;
		currentStep = spawnDefensiveUnits.currentStep;
	}
}
