using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class TarantulaBehaviour : BaseMonsterBehaviour
{
	public TarantulaBehaviour()
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
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			PopulateWebbedCharactersAtHome(list, character);
			int count = list.Count;
			RuinarchListPool<Character>.Release(list);
			if (count <= 1)
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
						List<Character> list3 = RuinarchListPool<Character>.Claim();
						Area areaLocation = character.areaLocation;
						if (areaLocation != null)
						{
							List<Area> list4 = RuinarchListPool<Area>.Claim();
							areaLocation.PopulateAreasInRange(list4, 6, includeCenterTile: true);
							for (int i = 0; i < list4.Count; i++)
							{
								Area area = list4[i];
								for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
								{
									Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
									LocationStructure currentStructure = character2.currentStructure;
									if (character2 != character && character2.gridTileLocation != null && !character2.isDead && character2.isNormalCharacter && !(currentStructure is DemonicStructure))
									{
										if (character2.traitContainer.HasTrait("Resting"))
										{
											list2.Add(character2);
										}
										else
										{
											list3.Add(character2);
										}
									}
								}
							}
							RuinarchListPool<Area>.Release(list4);
						}
						Character character3 = null;
						if (list2.Count > 0)
						{
							character3 = CollectionUtilities.GetRandomElement(list2);
						}
						else if (list3.Count > 0)
						{
							character3 = CollectionUtilities.GetRandomElement(list3);
						}
						if (character3 != null)
						{
							character.behaviourComponent.SetAbductionTarget(character3);
						}
						RuinarchListPool<Character>.Release(list3);
						RuinarchListPool<Character>.Release(list2);
					}
				}
				else if (character.behaviourComponent.currentAbductTarget != null)
				{
					character.behaviourComponent.SetAbductionTarget(null);
				}
				Character currentAbductTarget = character.behaviourComponent.currentAbductTarget;
				if (currentAbductTarget != null)
				{
					return character.jobComponent.TriggerMonsterAbduct(currentAbductTarget, out producedJob);
				}
			}
		}
		if (GameUtilities.RollChance(30))
		{
			Character character4 = null;
			List<Character> list5 = RuinarchListPool<Character>.Claim();
			PopulateWebbedCharactersAtHome(list5, character);
			if (list5.Count > 0)
			{
				character4 = CollectionUtilities.GetRandomElement(list5);
			}
			RuinarchListPool<Character>.Release(list5);
			if (character4 != null)
			{
				return character.jobComponent.TriggerEatAlive(character4, out producedJob);
			}
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
			List<Character> list2 = RuinarchListPool<Character>.Claim();
			List<Area> list3 = RuinarchListPool<Area>.Claim();
			p_character.areaLocation?.PopulateAreasInRange(list3, 6, includeCenterTile: true);
			for (int i = 0; i < list3.Count; i++)
			{
				Area area = list3[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character = area.locationCharacterTracker.charactersAtLocation[j];
					LocationStructure currentStructure = character.currentStructure;
					if (character != p_character && character.gridTileLocation != null && !character.isDead && character.isNormalCharacter && !(currentStructure is DemonicStructure) && character.faction != null && character.faction != p_character.faction && !p_character.faction.IsFriendlyWith(character.faction))
					{
						if (character.traitContainer.HasTrait("Resting"))
						{
							list.Add(character);
						}
						else
						{
							list2.Add(character);
						}
					}
				}
			}
			RuinarchListPool<Area>.Release(list3);
			Character character2 = null;
			if (list.Count > 0)
			{
				character2 = CollectionUtilities.GetRandomElement(list);
			}
			else if (list2.Count > 0)
			{
				character2 = CollectionUtilities.GetRandomElement(list2);
			}
			RuinarchListPool<Character>.Release(list);
			RuinarchListPool<Character>.Release(list2);
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
