public class SaveDataRock : SaveDataTileObject
{
	public int yield;

	public int count;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Rock rock = tileObject as Rock;
		yield = rock.yield;
		count = rock.count;
	}

	public override TileObject Load()
	{
		Rock obj = base.Load() as Rock;
		obj.SetYield(yield);
		obj.count = count;
		return obj;
	}
}
