using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class KoboldBehaviour : BaseMonsterBehaviour
{
	public KoboldBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		PopulateFrozenCharactersSurroundingHome(list, character);
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
			if (GameUtilities.RollChance(8))
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER, randomElement3, character);
				goapPlanJob2.SetCancelOnDeath(state: false);
				RuinarchListPool<Character>.Release(list);
				producedJob = goapPlanJob2;
				return true;
			}
			if (GameUtilities.RollChance(30) && character.marker.IsPOIInVision(randomElement3))
			{
				character.interruptComponent.TriggerInterrupt(GameUtilities.RollChance(50) ? INTERRUPT.Mock : INTERRUPT.Laugh_At, randomElement3);
				RuinarchListPool<Character>.Release(list);
				producedJob = null;
				return true;
			}
		}
		RuinarchListPool<Character>.Release(list);
		if (GameUtilities.RollChance(15))
		{
			List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
			PopulateFoodPilesAtHome(list2, character);
			FoodPile foodPile = null;
			if (list2.Count > 0)
			{
				foodPile = CollectionUtilities.GetRandomElement(list2) as FoodPile;
			}
			RuinarchListPool<TileObject>.Release(list2);
			if (foodPile != null)
			{
				GoapPlanJob goapPlanJob3 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT, foodPile, character);
				producedJob = goapPlanJob3;
				return true;
			}
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
	}

	private void PopulateFrozenCharactersSurroundingHome(List<Character> p_characterList, Character p_abductor)
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
			list[j].locationCharacterTracker.PopulateCharacterListInsideHexForKoboldBehaviour(p_characterList, p_abductor);
		}
		RuinarchListPool<Area>.Release(list);
	}

	private void PopulateRestrainedCharactersInHome(List<Character> p_characterList, Character character)
	{
		if (character.homeSettlement?.mainStorage != null)
		{
			character.homeSettlement.mainStorage.PopulateCharacterListThatHasTraitAndIsNotRace(p_characterList, "Restrained", RACE.KOBOLD);
		}
		else if (character.homeStructure != null)
		{
			character.homeStructure.PopulateCharacterListThatHasTraitAndIsNotRace(p_characterList, "Restrained", RACE.KOBOLD);
		}
		else if (character.HasTerritory())
		{
			character.territory.locationCharacterTracker.PopulateCharacterListInsideHexThatHasTraitAndNotRace(p_characterList, "Restrained", RACE.KOBOLD);
		}
	}
}
