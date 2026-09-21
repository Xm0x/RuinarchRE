namespace Inner_Maps;

public class SaveDataGridTileMouseEventsComponent : SaveData<GridTileMouseEventsComponent>
{
	public override GridTileMouseEventsComponent Load()
	{
		return new GridTileMouseEventsComponent(this);
	}
}
