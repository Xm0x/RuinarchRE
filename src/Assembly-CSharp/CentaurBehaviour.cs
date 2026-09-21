using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class CentaurBehaviour : BaseMonsterBehaviour
{
	public CentaurBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		PopulateEnsnaredCharactersSurroundingHome(list, character);
		if (list.Count > 0)
		{
			Character randomElement = CollectionUtilities.GetRandomElement(list);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CAPTURE_CHARACTER, INTERACTION_TYPE.DROP_RESTRAINED, randomElement, character);
			if (character.homeSettlement?.mainStorage != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { character.homeSettlement.mainStorage });
			}
			else if (character.homeStructure != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { character.homeStructure });
			}
			else if (character.HasTerritory())
			{
				LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(character.territory.gridTileComponent.gridTiles);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { randomElement2.structure, randomElement2 });
			}
			RuinarchListPool<Character>.Release(list);
			producedJob = goapPlanJob;
			return true;
		}
		PopulateRestrainedCharactersInHome(list, character);
		if (list.Count > 0)
		{
			Character randomElement3 = CollectionUtilities.GetRandomElement(list);
			if (randomElement3.race.IsSapient())
			{
				if (GameUtilities.RollChance(3, ref log))
				{
					character.jobComponent.TriggerTransformCentaur(JOB_TYPE.RAISE_CORPSE, randomElement3, out producedJob);
					if (producedJob != null)
					{
						return true;
					}
				}
			}
			else if (RaceManager.Instance.GetRaceData(randomElement3.race).category == CHARACTER_CATEGORY.Beast && GameUtilities.RollChance(3, ref log))
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER, randomElement3, character);
				goapPlanJob2.SetCancelOnDeath(state: false);
				goapPlanJob2.SetDoNotRecalculate(state: true);
				RuinarchListPool<Character>.Release(list);
				producedJob = goapPlanJob2;
				return true;
			}
		}
		RuinarchListPool<Character>.Release(list);
		Character randomCharacterInsideHomeThatIsDead = GetRandomCharacterInsideHomeThatIsDead(character);
		if (randomCharacterInsideHomeThatIsDead != null)
		{
			RaceData raceData = RaceManager.Instance.GetRaceData(randomCharacterInsideHomeThatIsDead.race);
			if ((raceData.category == CHARACTER_CATEGORY.Humanoid || raceData.category == CHARACTER_CATEGORY.Beast || randomCharacterInsideHomeThatIsDead.race.IsSapient()) && GameUtilities.RollChance(3, ref log))
			{
				GoapPlanJob goapPlanJob3 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER, randomCharacterInsideHomeThatIsDead, character);
				goapPlanJob3.SetDoNotRecalculate(state: true);
				goapPlanJob3.SetCancelOnDeath(state: false);
				producedJob = goapPlanJob3;
				return true;
			}
		}
		if (character.homeStructure != null && GameUtilities.RollChance(10, ref log))
		{
			List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
			PopulateListOfFoodPilesOfSameTypeForCombine(list2, character.homeStructure);
			if (list2.Count > 1)
			{
				character.jobComponent.TryCreateCombineStockpile(list2[1] as ResourcePile, list2[0] as ResourcePile, out producedJob);
				if (producedJob != null)
				{
					RuinarchListPool<TileObject>.Release(list2);
					return true;
				}
			}
			RuinarchListPool<TileObject>.Release(list2);
		}
		if (GameUtilities.RollChance(3, ref log))
		{
			List<TileObject> list3 = RuinarchListPool<TileObject>.Claim();
			PopulateFoodPilesAtHome(list3, character);
			FoodPile foodPile = null;
			if (list3.Count > 0)
			{
				foodPile = CollectionUtilities.GetRandomElement(list3) as FoodPile;
			}
			RuinarchListPool<TileObject>.Release(list3);
			if (foodPile != null)
			{
				GoapPlanJob goapPlanJob4 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT, foodPile, character);
				producedJob = goapPlanJob4;
				return true;
			}
		}
		if (GameUtilities.RollChance(20))
		{
			return character.jobComponent.TriggerStand(out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
	}

	private void PopulateEnsnaredCharactersSurroundingHome(List<Character> p_characterList, Character p_abductor)
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		PopulateAreasSurroundingHome(list, p_abductor);
		if (p_abductor.homeSettlement != null)
		{
			for (int i = 0; i < p_abductor.homeSettlement.areas.Count; i++)
			{
				Area item = p_abductor.homeSettlement.areas[i];
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].locationCharacterTracker.PopulateCharacterListInsideHexForCentaurBehaviour(p_characterList, p_abductor);
		}
		RuinarchListPool<Area>.Release(list);
	}

	private void PopulateRestrainedCharactersInHome(List<Character> p_characterList, Character character)
	{
		if (character.homeSettlement?.mainStorage != null)
		{
			character.homeSettlement.mainStorage.PopulateCharacterListThatHasTraitAndIsNotRace(p_characterList, "Restrained", RACE.CENTAUR);
		}
		else if (character.homeStructure != null)
		{
			character.homeStructure.PopulateCharacterListThatHasTraitAndIsNotRace(p_characterList, "Restrained", RACE.CENTAUR);
		}
		else if (character.HasTerritory())
		{
			character.territory.locationCharacterTracker.PopulateCharacterListInsideHexThatHasTraitAndNotRace(p_characterList, "Restrained", RACE.CENTAUR);
		}
	}

	private Character GetRandomCharacterInsideHomeThatIsDead(Character character)
	{
		if (character.homeSettlement != null)
		{
			return character.homeSettlement.GetRandomCharacterThatIsDead();
		}
		if (character.homeStructure != null)
		{
			return character.homeStructure.GetRandomCharacterThatIsDead();
		}
		if (character.HasTerritory())
		{
			return character.territory.locationCharacterTracker.GetRandomCharacterInsideHexThatIsDead();
		}
		return null;
	}

	private void PopulateListOfFoodPilesOfSameTypeForCombine(List<TileObject> p_list, LocationStructure structure)
	{
		for (int i = 0; i < InnerMapManager.Instance.foodPileTypes.Length; i++)
		{
			TILE_OBJECT_TYPE p_type = InnerMapManager.Instance.foodPileTypes[i];
			PopulateListOfFoodPilesOfTypeForCombine(p_list, p_type, structure);
			if (p_list.Count <= 1)
			{
				p_list.Clear();
				continue;
			}
			break;
		}
	}

	private void PopulateListOfFoodPilesOfTypeForCombine(List<TileObject> p_list, TILE_OBJECT_TYPE p_type, LocationStructure structure)
	{
		List<TileObject> tileObjectsOfType = structure.GetTileObjectsOfType(p_type);
		if (tileObjectsOfType == null || tileObjectsOfType.Count <= 1)
		{
			return;
		}
		for (int i = 0; i < tileObjectsOfType.Count; i++)
		{
			TileObject tileObject = tileObjectsOfType[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && !tileObject.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				p_list.Add(tileObject);
			}
		}
	}
}
