using Inner_Maps;

public class SaveDataArtifact : SaveDataTileObject
{
	public ARTIFACT_TYPE artifactType;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Artifact artifact = tileObject as Artifact;
		artifactType = artifact.data.type;
	}

	public override TileObject Load()
	{
		TileObject tileObject = InnerMapManager.Instance.LoadTileObject<TileObject>(this);
		tileObject.Initialize(this);
		return tileObject;
	}
}
