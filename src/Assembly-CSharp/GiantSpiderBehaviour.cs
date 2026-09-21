using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class GiantSpiderBehaviour : BaseMonsterBehaviour
{
	public GiantSpiderBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.currentStructure is Kennel)
		{
			return false;
		}
		if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) >= 0 && GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) <= 3)
		{
			Character character2 = null;
			List<Character> list = RuinarchListPool<Character>.Claim();
			PopulateWebbedCharactersAtHome(list, character);
			if (list.Count <= 1)
			{
				if (character.behaviourComponent.currentAbductTarget != null && (character.behaviourComponent.currentAbductTarget.isDead || character.behaviourComponent.currentAbductTarget.traitContainer.HasTrait("Restrained")))
				{
					character.behaviourComponent.SetAbductionTarget(null);
				}
				if (character.homeStructure != null)
				{
					if (character.behaviourComponent.currentAbductTarget == null && GameUtilities.RollChance(1))
					{
						List<Character> list2 = RuinarchListPool<Character>.Claim();
						Area areaLocation = character.areaLocation;
						if (areaLocation != null)
						{
							List<Area> list3 = RuinarchListPool<Area>.Claim();
							areaLocation.PopulateAreasInRange(list3, 6, includeCenterTile: true);
							for (int i = 0; i < list3.Count; i++)
							{
								Area area = list3[i];
								for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
								{
									Character character3 = area.locationCharacterTracker.charactersAtLocation[j];
									LocationStructure currentStructure = character3.currentStructure;
									if (character3 != character && character3.gridTileLocation != null && !character3.isDead && (character3 is Animal || (character3.isNormalCharacter && character3.traitContainer.HasTrait("Resting"))) && currentStructure != null && !(currentStructure is DemonicStructure))
									{
										list2.Add(character3);
									}
								}
							}
							RuinarchListPool<Area>.Release(list3);
						}
						if (list2.Count > 0)
						{
							Character randomElement = CollectionUtilities.GetRandomElement(list2);
							character.behaviourComponent.SetAbductionTarget(randomElement);
						}
						RuinarchListPool<Character>.Release(list2);
					}
				}
				else if (character.behaviourComponent.currentAbductTarget != null)
				{
					character.behaviourComponent.SetAbductionTarget(null);
				}
				character2 = character.behaviourComponent.currentAbductTarget;
			}
			RuinarchListPool<Character>.Release(list);
			if (character2 != null)
			{
				return character.jobComponent.TriggerMonsterAbduct(character2, out producedJob);
			}
		}
		if (GameUtilities.RollChance(1) && (character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory()) && character.jobComponent.TryTriggerLayEgg(character, 4, TILE_OBJECT_TYPE.SPIDER_EGG, out producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance(30))
		{
			List<Character> list4 = RuinarchListPool<Character>.Claim();
			PopulateWebbedCharactersAtHome(list4, character);
			if (list4.Count > 0)
			{
				Character randomElement2 = CollectionUtilities.GetRandomElement(list4);
				RuinarchListPool<Character>.Release(list4);
				return character.jobComponent.TriggerEatAlive(randomElement2, out producedJob);
			}
			RuinarchListPool<Character>.Release(list4);
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, checkIfPathPossibleWithoutDigging: true);
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if ((currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT) && GameUtilities.RollChance(5, ref p_log))
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			List<Area> list2 = RuinarchListPool<Area>.Claim();
			p_character.areaLocation?.PopulateAreasInRange(list2, 6, includeCenterTile: true);
			for (int i = 0; i < list2.Count; i++)
			{
				Area area = list2[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character = area.locationCharacterTracker.charactersAtLocation[j];
					LocationStructure currentStructure = character.currentStructure;
					if (character != p_character && character.gridTileLocation != null && !character.isDead && character is Animal && !(currentStructure is DemonicStructure))
					{
						list.Add(character);
					}
				}
			}
			Character character2 = null;
			if (list.Count > 0)
			{
				character2 = CollectionUtilities.GetRandomElement(list);
			}
			else
			{
				for (int k = 0; k < list2.Count; k++)
				{
					Area area2 = list2[k];
					for (int l = 0; l < area2.locationCharacterTracker.charactersAtLocation.Count; l++)
					{
						Character character3 = area2.locationCharacterTracker.charactersAtLocation[l];
						LocationStructure currentStructure2 = character3.currentStructure;
						if (character3 != p_character && character3.gridTileLocation != null && !character3.isDead && character3.isNormalCharacter && !(currentStructure2 is DemonicStructure) && character3.traitContainer.HasTrait("Resting") && character3.faction != null && character3.faction != p_character.faction && !p_character.faction.IsFriendlyWith(character3.faction))
						{
							list.Add(character3);
						}
					}
				}
				if (list.Count > 0)
				{
					character2 = CollectionUtilities.GetRandomElement(list);
				}
			}
			RuinarchListPool<Area>.Release(list2);
			RuinarchListPool<Character>.Release(list);
			if (character2 != null)
			{
				LocationGridTile targetTile = null;
				if (p_character.homeSettlement?.mainStorage != null)
				{
					targetTile = CollectionUtilities.GetRandomElement(p_character.homeSettlement.mainStorage.tiles);
				}
				if (p_character.jobComponent.TriggerMonsterAbduct(character2, out p_producedJob, targetTile))
				{
					return true;
				}
			}
		}
		if (TryTakePersonalPatrolJob(p_character, 15, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance(3, ref p_log) && p_character.jobComponent.TryTriggerLayEgg(p_character, 5, TILE_OBJECT_TYPE.SPIDER_EGG, out p_producedJob))
		{
			return true;
		}
		return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
	}

	private void PopulateWebbedCharactersAtHome(List<Character> p_characterList, Character character)
	{
		if (character.homeStructure != null)
		{
			character.homeStructure.PopulateCharacterListThatIsWebbed(p_characterList);
		}
		else if (character.HasTerritory())
		{
			character.territory.locationCharacterTracker.PopulateCharacterListInsideHexThatHasTrait(p_characterList, "Webbed");
		}
	}
}
