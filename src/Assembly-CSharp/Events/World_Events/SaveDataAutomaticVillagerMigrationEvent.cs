namespace Events.World_Events;

public class SaveDataAutomaticVillagerMigrationEvent : SaveDataWorldEvent
{
	public override WorldEvent Load()
	{
		return new AutomaticVillagerMigrationEvent();
	}
}
