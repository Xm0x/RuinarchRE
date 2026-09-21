public class SaveDataTileObjectConstructionComponent : SaveData<TileObjectConstructionComponent>
{
	public int currentConstructionTick;

	public int totalNeededConstructionTicks;

	public GameDate expirationDate;

	public override void Save(TileObjectConstructionComponent data)
	{
		base.Save(data);
		currentConstructionTick = data.currentConstructionTick;
		totalNeededConstructionTicks = data.totalNeededConstructionTicks;
		expirationDate = data.expirationDate;
	}

	public override TileObjectConstructionComponent Load()
	{
		return new TileObjectConstructionComponent(this);
	}
}
