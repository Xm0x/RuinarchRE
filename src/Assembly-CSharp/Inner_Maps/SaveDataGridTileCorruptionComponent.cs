namespace Inner_Maps;

public class SaveDataGridTileCorruptionComponent : SaveData<GridTileCorruptionComponent>
{
	public bool isCurrentlyBeingCorrupted;

	public GameDate corruptDate;

	public bool wallIsBeingBuilt;

	public bool wallIsBeingDestroyed;

	public GameDate wallBuildOrDestroyDate;

	public bool willGenerateDemonicDecorOnCorruptionFinish;

	public override void Save(GridTileCorruptionComponent data)
	{
		base.Save(data);
		isCurrentlyBeingCorrupted = data.isCurrentlyBeingCorrupted;
		corruptDate = data.corruptDate;
		wallIsBeingBuilt = data.wallIsBeingBuilt;
		wallIsBeingDestroyed = data.wallIsBeingDestroyed;
		wallBuildOrDestroyDate = data.wallBuildOrDestroyDate;
		willGenerateDemonicDecorOnCorruptionFinish = data.willGenerateDemonicDecorOnCorruptionFinish;
	}

	public override GridTileCorruptionComponent Load()
	{
		return new GridTileCorruptionComponent(this);
	}
}
