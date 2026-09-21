using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class DesiresIsolationBehaviour : CharacterBehaviour
{
	public DesiresIsolationBehaviour()
	{
		base.priority = 5;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.currentStructure == character.homeStructure)
		{
			int num = Random.Range(0, 100);
			int num2 = 25;
			if (num < num2)
			{
				TileObject unoccupiedBuiltTileObject = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
				if (unoccupiedBuiltTileObject != null)
				{
					character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, unoccupiedBuiltTileObject, out producedJob);
					return true;
				}
			}
			character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character);
		}
		else if (!(character.jobTriggerComponent as CharacterJobTriggerComponent).CreateHideAtHomeJob())
		{
			LocationGridTile tile = GetRandomTileOutsideSettlement(character.currentRegion, character) ?? character.gridTileLocation;
			character.jobComponent.TriggerRoamAroundTile(out producedJob, tile);
		}
		return true;
	}

	private LocationGridTile GetRandomTileOutsideSettlement(Region p_region, Character p_character)
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		Area area = null;
		if (p_region.areas != null)
		{
			for (int i = 0; i < p_region.areas.Count; i++)
			{
				Area area2 = p_region.areas[i];
				if (area2.elevationType == ELEVATION.PLAIN && !area2.HasSettlementOnArea())
				{
					list.Add(area2);
				}
			}
		}
		if (list.Count > 0)
		{
			area = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Area>.Release(list);
		return area?.gridTileComponent.GetRandomTileThatCharacterCanReach(p_character);
	}
}
