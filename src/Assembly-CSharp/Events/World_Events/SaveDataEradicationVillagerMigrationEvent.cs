using UtilityScripts;

namespace Events.World_Events;

public class SaveDataEradicationVillagerMigrationEvent : SaveDataWorldEvent
{
	public GameDate nextMigrationDate;

	public RuinarchTimer migrationTimer;

	public override void Save(WorldEvent data)
	{
		base.Save(data);
		EradicationVillagerMigration eradicationVillagerMigration = data as EradicationVillagerMigration;
		nextMigrationDate = eradicationVillagerMigration.nextMigrationDate;
		migrationTimer = eradicationVillagerMigration.migrationTimer;
	}

	public override WorldEvent Load()
	{
		return new EradicationVillagerMigration(this);
	}
}
