public class GoddessStatue : TileObject
{
	public GoddessStatue()
	{
		Initialize(TILE_OBJECT_TYPE.GODDESS_STATUE);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public GoddessStatue(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void SetPOIState(POI_STATE state)
	{
		base.SetPOIState(state);
		if (gridTileLocation != null && mapVisual != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	public override string ToString()
	{
		return "Goddess Statue " + base.id;
	}
}
