using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class RatmanBehaviour : CharacterBehaviour
{
	public RatmanBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
		bool flag = character.IsAtHome();
		if (flag && character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance(5) && character.GetAliveResidentsCountInHome() >= 6)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home_Ratman, character);
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Ratman_Find_Home) && character.homeSettlement == null && (character.homeStructure == null || character.homeStructure.hasBeenDestroyed))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home_Ratman, character);
		}
		if (character.HasHome())
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
			if (currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT)
			{
				int num = 10;
				if (HasResidentFromSameHomeThatIsNotDeadAndEnslaved(character))
				{
					num -= 7;
				}
				if (HasFoodPileInHomeStorage(character))
				{
					num -= 4;
				}
				if (GameUtilities.RollChance(num) && flag && GetFirstPrisonerAtHome(character) == null && !HasResidentFromSameHomeThatHasMonsterAbductJob(character))
				{
					character.behaviourComponent.SetAbductionTarget(null);
					if (character.behaviourComponent.currentAbductTarget == null && character.currentRegion != null)
					{
						List<Character> list = RuinarchListPool<Character>.Claim();
						Area areaLocation = character.areaLocation;
						if (areaLocation != null)
						{
							List<Area> list2 = RuinarchListPool<Area>.Claim();
							areaLocation.PopulateAreasInRange(list2, 6, includeCenterTile: true);
							for (int i = 0; i < list2.Count; i++)
							{
								Area area = list2[i];
								for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
								{
									Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
									if (CanBeTargetedForAbduction(character, character2))
									{
										list.Add(character2);
									}
								}
							}
							RuinarchListPool<Area>.Release(list2);
						}
						if (list.Count > 0)
						{
							Character randomElement = CollectionUtilities.GetRandomElement(list);
							character.behaviourComponent.SetAbductionTarget(randomElement);
						}
						RuinarchListPool<Character>.Release(list);
					}
					Character currentAbductTarget = character.behaviourComponent.currentAbductTarget;
					if (currentAbductTarget != null)
					{
						LocationGridTile locationGridTile = null;
						if (character.homeStructure != null)
						{
							if (!(character.homeStructure is ThePortal))
							{
								locationGridTile = character.homeStructure.GetRandomPassableTile();
							}
						}
						else if (character.homeSettlement != null && character.homeSettlement.mainStorage != null)
						{
							locationGridTile = character.homeSettlement.mainStorage.GetRandomPassableTile();
						}
						if (locationGridTile != null && character.jobComponent.TriggerMonsterAbduct(currentAbductTarget, out producedJob, locationGridTile))
						{
							character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
							return true;
						}
					}
				}
			}
			else if (flag)
			{
				Character firstPrisonerAtHome = GetFirstPrisonerAtHome(character);
				if (firstPrisonerAtHome != null)
				{
					if (GameUtilities.RollChance(30) && firstPrisonerAtHome.race == RACE.RATMAN)
					{
						return character.jobComponent.TriggerRecruitJob(firstPrisonerAtHome, out producedJob);
					}
					if (GameUtilities.RollChance(20) && CanProduceFood(firstPrisonerAtHome) && !HasResidentFromSameHomeThatHasTortureOrMonsterButcherJob(character))
					{
						return character.jobComponent.TriggerTorture(firstPrisonerAtHome, out producedJob);
					}
					if (GameUtilities.RollChance(30) && CanBeButchered(firstPrisonerAtHome) && !HasResidentFromSameHomeThatHasTortureOrMonsterButcherJob(character) && !HasFoodPileInHomeStorage(character))
					{
						return character.jobComponent.CreateButcherJob(firstPrisonerAtHome, JOB_TYPE.MONSTER_BUTCHER, out producedJob);
					}
				}
			}
		}
		if (GameUtilities.RollChance(1) && flag && character.GetAliveResidentsCountInHome() < 6)
		{
			return character.jobComponent.TriggerBirthRatman(out producedJob);
		}
		if (!flag && character.HasHome())
		{
			return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundTile(out producedJob);
	}

	private bool HasResidentFromSameHomeThatIsNotDeadAndEnslaved(Character character)
	{
		List<Character> residents;
		bool flag = PopulateResidentsFromSameHome(out residents, character);
		bool result = true;
		if (residents != null)
		{
			result = false;
			for (int i = 0; i < residents.Count; i++)
			{
				Character character2 = residents[i];
				if (character2 != character && !character2.isDead && character2.traitContainer.HasTrait("Enslaved"))
				{
					result = true;
					break;
				}
			}
		}
		if (flag)
		{
			RuinarchListPool<Character>.Release(residents);
		}
		return result;
	}

	private bool HasResidentFromSameHomeThatHasMonsterAbductJob(Character character)
	{
		List<Character> residents;
		bool flag = PopulateResidentsFromSameHome(out residents, character);
		bool result = true;
		if (residents != null)
		{
			result = false;
			for (int i = 0; i < residents.Count; i++)
			{
				Character character2 = residents[i];
				if (character2 != character && character2.jobQueue.HasJob(JOB_TYPE.MONSTER_ABDUCT))
				{
					result = true;
					break;
				}
			}
		}
		if (flag)
		{
			RuinarchListPool<Character>.Release(residents);
		}
		return result;
	}

	private bool HasResidentFromSameHomeThatHasTortureOrMonsterButcherJob(Character character)
	{
		List<Character> residents;
		bool flag = PopulateResidentsFromSameHome(out residents, character);
		bool result = true;
		if (residents != null)
		{
			result = false;
			for (int i = 0; i < residents.Count; i++)
			{
				Character character2 = residents[i];
				if (character2 != character && character2.jobQueue.HasJob(JOB_TYPE.TORTURE, JOB_TYPE.MONSTER_BUTCHER))
				{
					result = true;
					break;
				}
			}
		}
		if (flag)
		{
			RuinarchListPool<Character>.Release(residents);
		}
		return result;
	}

	private bool PopulateResidentsFromSameHome(out List<Character> residents, Character character)
	{
		bool result = false;
		residents = null;
		if (character.homeSettlement != null)
		{
			residents = character.homeSettlement.residents;
		}
		else if (character.homeStructure != null)
		{
			residents = character.homeStructure.residents;
		}
		else if (character.HasTerritory())
		{
			result = true;
			residents = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
			{
				Character character2 = CharacterManager.Instance.allCharacters[i];
				if (!character2.isDead && character2.IsTerritory(character.territory))
				{
					residents.Add(character2);
				}
			}
		}
		return result;
	}

	private Character GetFirstPrisonerAtHome(Character character)
	{
		if (character.homeSettlement != null)
		{
			for (int i = 0; i < character.homeSettlement.region.charactersAtLocation.Count; i++)
			{
				Character character2 = character.homeSettlement.region.charactersAtLocation[i];
				if (!character2.isDead && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(character.homeSettlement) && character2.traitContainer.HasTrait("Prisoner") && character2.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
				{
					return character2;
				}
			}
		}
		else if (character.homeStructure != null)
		{
			for (int j = 0; j < character.homeStructure.charactersHere.Count; j++)
			{
				Character character3 = character.homeStructure.charactersHere[j];
				if (!character3.isDead && character3.traitContainer.HasTrait("Prisoner") && character3.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
				{
					return character3;
				}
			}
		}
		return null;
	}

	private bool CanProduceFood(Character character)
	{
		if (character.race == RACE.HUMANS || character.race == RACE.ELVES || character is Incubus || character is Succubus || character.race == RACE.NYMPH || character.race == RACE.KOBOLD || character.race == RACE.TROLL || character.race == RACE.RATMAN || character.race == RACE.ABOMINATION || character.race == RACE.GOLEM || (character.race == RACE.ENT && character is Ent { isTree: false }) || character is GiantSpider || character is Tarantula)
		{
			return true;
		}
		return false;
	}

	private bool CanBeButchered(Character character)
	{
		if ((character is Animal && !(character is Rat)) || character.race == RACE.ELVES || character.race == RACE.HUMANS || character is GiantSpider || character is SmallSpider || character is Wolf || character is Tarantula)
		{
			return true;
		}
		return false;
	}

	private bool HasFoodPileInHomeStorage(Character character)
	{
		LocationStructure locationStructure = null;
		if (character.homeSettlement != null)
		{
			locationStructure = character.homeSettlement.mainStorage;
		}
		else if (character.homeStructure != null)
		{
			locationStructure = character.homeStructure;
		}
		else if (character.HasTerritory())
		{
			return character.territory.tileObjectComponent.HasBuiltFoodPileInArea();
		}
		return locationStructure?.HasTileObjectThatIsBuiltFoodPile() ?? false;
	}

	private bool CanBeTargetedForAbduction(Character p_actor, Character p_target)
	{
		LocationStructure currentStructure = p_target.currentStructure;
		if (!p_target.isDead && p_target.gridTileLocation != null && p_target != p_actor && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.KENNEL && currentStructure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && !p_target.traitContainer.HasTrait("Enslaved", "Hibernating") && (CanProduceFood(p_target) || CanBeButchered(p_target)) && (p_actor.faction == null || !p_actor.faction.IsFriendlyWith(p_target.faction)) && !CharacterManager.Instance.IsCultistOfSameReligion(p_actor, p_target))
		{
			Prisoner traitOrStatus = p_target.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus == null || !traitOrStatus.IsConsideredPrisonerOf(p_actor))
			{
				return true;
			}
		}
		return false;
	}
}
