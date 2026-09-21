public class SaveDataTileObjectHiddenComponent : SaveData<TileObjectHiddenComponent>
{
	public bool isHidden;

	public bool affectAlpha;

	public override void Save(TileObjectHiddenComponent data)
	{
		base.Save(data);
		isHidden = data.isHidden;
		affectAlpha = data.affectAlpha;
	}

	public override TileObjectHiddenComponent Load()
	{
		return new TileObjectHiddenComponent(this);
	}
}
