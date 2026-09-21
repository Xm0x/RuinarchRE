public class SaveDataDestroyVillageStructures : SaveDataGoalTask
{
	public int neededDestroyStructureCount;

	public int currentDestroyStructureCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		DestroyVillageStructures destroyVillageStructures = data as DestroyVillageStructures;
		neededDestroyStructureCount = destroyVillageStructures.neededDestroyStructureCount;
		currentDestroyStructureCount = destroyVillageStructures.currentDestroyStructureCount;
	}

	public override GoalTask Load()
	{
		return new DestroyVillageStructures(this);
	}
}
