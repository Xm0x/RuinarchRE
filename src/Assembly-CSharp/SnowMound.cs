using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class SnowMound : TileObject
{
	public SnowMound()
	{
		Initialize(TILE_OBJECT_TYPE.SNOW_MOUND, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.EXTRACT_ITEM);
	}

	public SnowMound(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		base.traitContainer.RemoveTrait(this, "Melting");
		if (base.previousTile != null)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			base.previousTile.PopulateTilesInRadius(list, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].tileObjectComponent.genericTileObject.traitContainer.AddTrait(list[i].tileObjectComponent.genericTileObject, "Wet", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
			}
			RuinarchListPool<LocationGridTile>.Release(list);
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
}
