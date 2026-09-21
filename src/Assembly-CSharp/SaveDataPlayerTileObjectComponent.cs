public class SaveDataPlayerTileObjectComponent : SaveData<PlayerTileObjectComponent>
{
	public override PlayerTileObjectComponent Load()
	{
		return new PlayerTileObjectComponent(this);
	}
}
