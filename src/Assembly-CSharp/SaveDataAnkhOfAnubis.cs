public class SaveDataAnkhOfAnubis : SaveDataArtifact
{
	public bool isActivated;

	public int ghostLimit;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		AnkhOfAnubis ankhOfAnubis = tileObject as AnkhOfAnubis;
		isActivated = ankhOfAnubis.isActivated;
		ghostLimit = ankhOfAnubis.ghostLimit;
	}

	public override TileObject Load()
	{
		return base.Load();
	}
}
