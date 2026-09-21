public class Ice : TileObject
{
	public Ice()
	{
		Initialize(TILE_OBJECT_TYPE.ICE, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
	}

	public Ice(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		base.traitContainer.RemoveTrait(this, "Melting");
		if (base.previousTile != null)
		{
			base.previousTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(base.previousTile.tileObjectComponent.genericTileObject, "Wet", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation.mainBiomeType != BIOMES.SNOW)
		{
			base.traitContainer.AddTrait(this, "Melting");
		}
		else
		{
			base.traitContainer.RemoveTrait(this, "Melting");
		}
	}

	public override void OnLoadPlacePOI()
	{
		DefaultProcessOnPlacePOI();
	}
}
