using System;
using UnityEngine;

public class Ore : TileObject
{
	public int yield { get; private set; }

	public int count { get; set; }

	public override Type serializedData => typeof(SaveDataOre);

	public CONCRETE_RESOURCES providedMetal { get; private set; }

	public Ore()
	{
		Initialize(TILE_OBJECT_TYPE.ORE, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.MINE_ORE);
		base.traitContainer.RemoveTrait(this, "Flammable");
		count = 120;
		SetYield(50);
	}

	public Ore(SaveDataOre data)
		: base(data)
	{
		yield = data.yield;
		providedMetal = data.providedMetal;
	}

	public override string ToString()
	{
		return "Ore " + base.id;
	}

	public override void SetPOIState(POI_STATE state)
	{
		base.SetPOIState(state);
		if (gridTileLocation != null && mapVisual != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	public void SetProvidedMetal(CONCRETE_RESOURCES p_providedMetal)
	{
		providedMetal = p_providedMetal;
	}

	public void AdjustYield(int amount)
	{
		yield += amount;
		yield = Mathf.Max(0, yield);
		if (yield == 0)
		{
			_ = gridTileLocation;
			base.structureLocation.RemovePOI(this);
		}
	}

	private void SetYield(int amount)
	{
		yield = amount;
	}

	public override string GetAdditionalTestingData()
	{
		return string.Concat(base.GetAdditionalTestingData() + " <b>Count:</b> " + count, "\n\tYield: ", yield.ToString());
	}
}
