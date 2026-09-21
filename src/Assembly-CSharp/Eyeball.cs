using Inner_Maps.Location_Structures;

public class Eyeball : TileObject
{
	public Eyeball()
	{
		Initialize(TILE_OBJECT_TYPE.EYEBALL);
	}

	public Eyeball(SaveDataTileObject data)
		: base(data)
	{
	}

	public override bool CanBeSelected()
	{
		if (gridTileLocation?.structure is DemonicStructure)
		{
			return false;
		}
		return true;
	}
}
