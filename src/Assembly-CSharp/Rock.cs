using System;
using Inner_Maps;
using UnityEngine;

public class Rock : TileObject
{
	public int yield { get; private set; }

	public int count { get; set; }

	public override Type serializedData => typeof(SaveDataRock);

	public Rock()
	{
		Initialize(TILE_OBJECT_TYPE.ROCK, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.MINE_STONE);
		base.traitContainer.RemoveTrait(this, "Flammable");
		count = 100;
		SetYield(50);
	}

	public Rock(SaveDataTileObject data)
		: base(data)
	{
	}

	public void AdjustYield(int amount)
	{
		yield += amount;
		yield = Mathf.Max(0, yield);
		if (yield == 0)
		{
			LocationGridTile locationGridTile = gridTileLocation;
			base.structureLocation.RemovePOI(this);
			SetGridTileLocation(locationGridTile);
		}
	}

	public void SetYield(int amount)
	{
		yield = amount;
	}

	public override string GetAdditionalTestingData()
	{
		return base.GetAdditionalTestingData() + " <b>Count:</b> " + count;
	}
}
