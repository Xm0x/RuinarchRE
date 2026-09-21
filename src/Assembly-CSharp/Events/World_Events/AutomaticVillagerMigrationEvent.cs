namespace Events.World_Events;

public class AutomaticVillagerMigrationEvent : VillagerMigrationEvent
{
	public override void InitializeEvent()
	{
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
	}

	public override string ToString()
	{
		return "Automatic Villager Migration Event";
	}

	public override SaveDataWorldEvent Save()
	{
		SaveDataAutomaticVillagerMigrationEvent saveDataAutomaticVillagerMigrationEvent = new SaveDataAutomaticVillagerMigrationEvent();
		saveDataAutomaticVillagerMigrationEvent.Save(this);
		return saveDataAutomaticVillagerMigrationEvent;
	}

	private void OnDayStarted()
	{
		if (GameManager.Instance.continuousDays > 1)
		{
			int num = ChanceData.GetChance(CHANCE_TYPE.Automatic_Villager_Migration);
			if (WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed == MIGRATION_SPEED.Slow)
			{
				num /= 2;
			}
			int num2 = ChanceData.GetChance(CHANCE_TYPE.Automatic_Villager_Migration_No_Hostile);
			if (WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed == MIGRATION_SPEED.Slow)
			{
				num2 /= 2;
			}
			TryTriggerGenericMigrationEvent(num, num2);
		}
	}
}
