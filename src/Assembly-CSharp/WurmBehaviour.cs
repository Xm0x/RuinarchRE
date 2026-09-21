using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class WurmBehaviour : BaseMonsterBehaviour
{
	public WurmBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		return false;
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (GameUtilities.RollChance(15, ref p_log))
		{
			LocationGridTile burrowTargetTile = GetBurrowTargetTile(p_character);
			if (burrowTargetTile != null && p_character.jobComponent.TriggerIdleBurrow(burrowTargetTile, out p_producedJob))
			{
				return true;
			}
		}
		return p_character.jobComponent.PlanIdleLongStandStill(out p_producedJob);
	}

	private LocationGridTile GetBurrowTargetTile(Character p_character)
	{
		if (p_character.homeSettlement != null)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			for (int i = 0; i < p_character.homeSettlement.areas.Count; i++)
			{
				Area area = p_character.homeSettlement.areas[i];
				for (int j = 0; j < area.gridTileComponent.gridTiles.Count; j++)
				{
					LocationGridTile locationGridTile = area.gridTileComponent.gridTiles[j];
					if (locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS || locationGridTile.structure.structureType == STRUCTURE_TYPE.CAVE)
					{
						list.Add(locationGridTile);
					}
				}
			}
			LocationGridTile result = null;
			if (list.Count > 0)
			{
				result = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<LocationGridTile>.Release(list);
			return result;
		}
		if (p_character.homeStructure != null)
		{
			if (p_character.homeStructure.tiles.Count > 0)
			{
				return CollectionUtilities.GetRandomElement(p_character.homeStructure.tiles);
			}
		}
		else if (p_character.HasTerritory())
		{
			List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
			for (int k = 0; k < p_character.territory.gridTileComponent.gridTiles.Count; k++)
			{
				LocationGridTile locationGridTile2 = p_character.territory.gridTileComponent.gridTiles[k];
				if (locationGridTile2.structure.structureType == STRUCTURE_TYPE.WILDERNESS || locationGridTile2.structure.structureType == STRUCTURE_TYPE.CAVE)
				{
					list2.Add(locationGridTile2);
				}
			}
			LocationGridTile result2 = null;
			if (list2.Count > 0)
			{
				result2 = CollectionUtilities.GetRandomElement(list2);
			}
			RuinarchListPool<LocationGridTile>.Release(list2);
			return result2;
		}
		return null;
	}
}
