using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Characters.Behaviour;

public class SnatcherBehaviour : CharacterBehaviour
{
	public SnatcherBehaviour()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		Area areaLocation = character.areaLocation;
		if (areaLocation != null)
		{
			if (areaLocation.gridTileComponent.HasCorruption())
			{
				List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
				LocationGridTile locationGridTile = null;
				for (int i = 0; i < areaLocation.gridTileComponent.gridTiles.Count; i++)
				{
					LocationGridTile locationGridTile2 = areaLocation.gridTileComponent.gridTiles[i];
					if (!locationGridTile2.structure.IsTilePartOfARoom(locationGridTile2, out var _) && locationGridTile2.IsPassable() && PathfindingManager.Instance.HasPath(character.gridTileLocation, locationGridTile2))
					{
						list.Add(locationGridTile2);
					}
				}
				if (list.Count > 0)
				{
					locationGridTile = CollectionUtilities.GetRandomElement(list);
				}
				RuinarchListPool<LocationGridTile>.Release(list);
				if (locationGridTile != null)
				{
					return character.jobComponent.TriggerRoamAroundTile(out producedJob, locationGridTile);
				}
				character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
				return true;
			}
			Area nearestDemonicStructure = GetNearestDemonicStructure(character);
			if (nearestDemonicStructure != null)
			{
				StructureRoom room2;
				List<LocationGridTile> list2 = nearestDemonicStructure.gridTileComponent.gridTiles.Where((LocationGridTile t) => !t.structure.IsTilePartOfARoom(t, out room2) && t.IsPassable() && PathfindingManager.Instance.HasPathEvenDiffRegion(character.gridTileLocation, t)).ToList();
				if (list2.Count > 0)
				{
					LocationGridTile randomElement = CollectionUtilities.GetRandomElement(list2);
					return character.jobComponent.CreateGoToJob(randomElement, out producedJob);
				}
				character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.movementComponent.SetEnableDigging(state: true);
		character.behaviourComponent.OnBecomeSnatcher();
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.movementComponent.SetEnableDigging(state: false);
		character.behaviourComponent.OnNoLongerSnatcher();
	}

	public override void OnLoadBehaviourToCharacter(Character character)
	{
		base.OnLoadBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeSnatcher();
	}

	private Area GetNearestDemonicStructure(Character character)
	{
		Area areaLocation = character.areaLocation;
		if (areaLocation != null)
		{
			Area result = null;
			float num = 0f;
			for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.areas.Count; i++)
			{
				Area area = PlayerManager.Instance.player.playerSettlement.areas[i];
				float num2 = Vector2.Distance(area.gridTileComponent.centerGridTile.centeredWorldLocation, areaLocation.gridTileComponent.centerGridTile.centeredWorldLocation);
				if (num2 < num)
				{
					result = area;
					num = num2;
				}
			}
			return result;
		}
		return null;
	}
}
