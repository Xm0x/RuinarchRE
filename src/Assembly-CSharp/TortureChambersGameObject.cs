public class TortureChambersGameObject : TileObjectGameObject
{
	public override void Initialize(TileObject tileObject)
	{
		base.Initialize(tileObject);
		base.selectable = tileObject.gridTileLocation.structure;
	}
}
