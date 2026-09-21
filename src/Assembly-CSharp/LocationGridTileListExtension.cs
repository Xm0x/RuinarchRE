using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public static class LocationGridTileListExtension
{
	public static List<LocationGridTile> GetTilesCharacterCanGoTo(this List<LocationGridTile> p_tiles, Character p_character)
	{
		List<LocationGridTile> list = null;
		for (int i = 0; i < p_tiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_tiles[i];
			if (p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				if (list == null)
				{
					list = new List<LocationGridTile>();
				}
				list.Add(locationGridTile);
			}
		}
		return list;
	}

	public static void PopulateListWithTilesCharacterCanGoTo(this List<LocationGridTile> p_tiles, Character p_character, List<LocationGridTile> p_outList)
	{
		for (int i = 0; i < p_tiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_tiles[i];
			if (p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				p_outList.Add(locationGridTile);
			}
		}
	}

	public static LocationGridTile GetFirstTileCharacterCanGoTo(this List<LocationGridTile> p_tiles, Character p_character)
	{
		for (int i = 0; i < p_tiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_tiles[i];
			if (p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public static LocationGridTile GetRandomPassableTile(this List<LocationGridTile> p_tiles)
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		CollectionUtilities.Shuffle(p_tiles, list);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.IsPassable())
			{
				result = locationGridTile;
				break;
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}
}
