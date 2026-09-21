using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class DefaultOutsideHomeRegion : CharacterBehaviour
{
	public DefaultOutsideHomeRegion()
	{
		base.priority = 25;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!character.isAtHomeRegion)
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
			if (currentTimeInWordsOfTick == TIME_IN_WORDS.MORNING || currentTimeInWordsOfTick == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTERNOON || currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT)
			{
				if (Random.Range(0, 100) < 35)
				{
					return character.jobComponent.PlanIdleStrollOutside(out producedJob);
				}
				return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			}
			if (character.currentStructure != null && character.currentStructure.structureType == STRUCTURE_TYPE.TAVERN)
			{
				if (Random.Range(0, 100) < 35)
				{
					return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
				}
				Table randomTileObjectOfTypeThatHasTileLocation = character.currentStructure.GetRandomTileObjectOfTypeThatHasTileLocation<Table>();
				if (randomTileObjectOfTypeThatHasTileLocation != null)
				{
					return character.jobComponent.TriggerDrinkJob(JOB_TYPE.IDLE, randomTileObjectOfTypeThatHasTileLocation, out producedJob);
				}
				return character.jobComponent.PlanIdleStrollOutside(out producedJob);
			}
			List<LocationStructure> structuresAtLocation = character.currentRegion.GetStructuresAtLocation(STRUCTURE_TYPE.TAVERN);
			if (structuresAtLocation != null && structuresAtLocation.Count > 0)
			{
				LocationStructure locationStructure = null;
				for (int i = 0; i < structuresAtLocation.Count; i++)
				{
					LocationStructure locationStructure2 = structuresAtLocation[i];
					if (locationStructure2.settlementLocation == null || locationStructure2.settlementLocation.owner == null || character.faction == null || !locationStructure2.settlementLocation.owner.IsHostileWith(character.faction))
					{
						locationStructure = locationStructure2;
						break;
					}
				}
				if (locationStructure != null)
				{
					LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationStructure.passableTiles);
					return character.jobComponent.CreateGoToJob(randomElement, out producedJob);
				}
			}
			if (!character.currentStructure.isInterior)
			{
				List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
				LocationStructure locationStructure3 = null;
				Area area = character.gridTileLocation.area;
				for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
				{
					LocationGridTile centerGridTile = area.neighbourComponent.neighbours[j].gridTileComponent.centerGridTile;
					if (centerGridTile.structure.structureType.IsSpecialStructure() && centerGridTile.structure.isInterior && character.movementComponent.HasPathTo(centerGridTile))
					{
						list.Add(centerGridTile.structure);
					}
				}
				if (list.Count > 0)
				{
					locationStructure3 = CollectionUtilities.GetRandomElement(list);
				}
				RuinarchListPool<LocationStructure>.Release(list);
				if (locationStructure3 != null)
				{
					LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(locationStructure3.passableTiles);
					return character.jobComponent.CreateGoToJob(randomElement2, out producedJob);
				}
				if (character.currentSettlement != null)
				{
					Area aPlainAdjacentArea = character.currentSettlement.GetAPlainAdjacentArea();
					if (aPlainAdjacentArea != null)
					{
						LocationGridTile randomPassableTile = aPlainAdjacentArea.gridTileComponent.GetRandomPassableTile();
						return character.jobComponent.CreateGoToJob(randomPassableTile, out producedJob);
					}
					return character.jobComponent.PlanIdleStrollOutside(out producedJob);
				}
				Campfire campfire = null;
				List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
				area.tileObjectComponent.PopulateTileObjectsInArea<Campfire>(list2);
				if (list2 != null && list2.Count > 0)
				{
					for (int k = 0; k < list2.Count; k++)
					{
						TileObject tileObject = list2[k];
						if (tileObject.characterOwner == null || tileObject.IsOwnedBy(character) || (!character.IsHostileWith(tileObject.characterOwner) && !character.relationshipContainer.IsEnemiesWith(tileObject.characterOwner)))
						{
							campfire = tileObject as Campfire;
							break;
						}
					}
				}
				RuinarchListPool<TileObject>.Release(list2);
				if (campfire != null)
				{
					if (Random.Range(0, 100) < 25)
					{
						return character.jobComponent.TriggerRoamAroundTile(out producedJob);
					}
					return character.jobComponent.TriggerWarmUp(campfire, out producedJob);
				}
				return character.jobComponent.TriggerBuildCampfireJob(JOB_TYPE.IDLE, out producedJob);
			}
			if (Random.Range(0, 100) < 35)
			{
				return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
			}
			return character.jobComponent.TriggerStand(out producedJob);
		}
		return false;
	}
}
