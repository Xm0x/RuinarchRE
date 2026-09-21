using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class GoblinBehaviour : BaseMonsterBehaviour
{
	public GoblinBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (TryAbduction(character, ref log, out producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance((float)ChanceData.GetChance(CHANCE_TYPE.Goblin_Impregnate) / 100f, ref log) && CanStillImpregnate(character, 6))
		{
			Character firstPrisonerAtHomeToImpregnate = GetFirstPrisonerAtHomeToImpregnate(character);
			if (firstPrisonerAtHomeToImpregnate != null)
			{
				return character.jobComponent.TriggerImpregnate(firstPrisonerAtHomeToImpregnate, out producedJob);
			}
		}
		Character firstPrisonerAtHome = GetFirstPrisonerAtHome(character);
		if (firstPrisonerAtHome != null)
		{
			if (GameUtilities.RollChance((float)ChanceData.GetChance(CHANCE_TYPE.Goblin_Kill) / 100f, ref log))
			{
				return character.jobComponent.CreateSlayTargetJob(firstPrisonerAtHome, out producedJob);
			}
			if (GameUtilities.RollChance(3, ref log))
			{
				character.interruptComponent.TriggerInterrupt(GameUtilities.RollChance(50) ? INTERRUPT.Mock : INTERRUPT.Laugh_At, firstPrisonerAtHome);
				producedJob = null;
				return true;
			}
		}
		Character firstDeadCharacterAtHomeForButcher = GetFirstDeadCharacterAtHomeForButcher(character);
		if (firstDeadCharacterAtHomeForButcher != null && GameUtilities.RollChance(8, ref log))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER, firstDeadCharacterAtHomeForButcher, character);
			goapPlanJob.SetCancelOnDeath(state: false);
			producedJob = goapPlanJob;
			return true;
		}
		if (GameUtilities.RollChance(3))
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			PopulateFoodPilesAtHome(list, character);
			FoodPile foodPile = null;
			if (list.Count > 0)
			{
				foodPile = CollectionUtilities.GetRandomElement(list) as FoodPile;
			}
			RuinarchListPool<TileObject>.Release(list);
			if (foodPile != null)
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT, foodPile, character);
				producedJob = goapPlanJob2;
				return true;
			}
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
	}

	private bool TryAbduction(Character p_actor, ref string log, out JobQueueItem p_producedJob)
	{
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
		{
			Area areaLocation = p_actor.areaLocation;
			if (GameUtilities.RollChance(1, ref log) && areaLocation != null && !AreThereEnoughPrisoners(p_actor))
			{
				Character character = null;
				List<Character> list = RuinarchListPool<Character>.Claim();
				for (int i = 0; i < p_actor.currentRegion.charactersAtLocation.Count; i++)
				{
					Character character2 = p_actor.currentRegion.charactersAtLocation[i];
					if (character2 != p_actor && character2.gridTileLocation != null && !character2.isDead && (character2 is Animal || (character2.isNormalCharacter && character2.traitContainer.HasTrait("Resting"))) && !(character2.currentStructure is DemonicStructure))
					{
						Area areaLocation2 = character2.areaLocation;
						if (areaLocation2 != null && areaLocation2.IsNearbyTo(areaLocation))
						{
							list.Add(character2);
						}
					}
				}
				if (list.Count > 0)
				{
					character = CollectionUtilities.GetRandomElement(list);
				}
				RuinarchListPool<Character>.Release(list);
				if (character != null)
				{
					return p_actor.jobComponent.TriggerMonsterAbduct(character, out p_producedJob);
				}
			}
		}
		p_producedJob = null;
		return false;
	}

	private bool AreThereEnoughPrisoners(Character character)
	{
		if (character.homeSettlement != null && character.homeSettlement.locationType != LOCATION_TYPE.DUNGEON)
		{
			int num = 0;
			for (int i = 0; i < character.homeSettlement.areas.Count; i++)
			{
				Area area = character.homeSettlement.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
					if (character2.isDead || character2.gridTileLocation == null || !character2.gridTileLocation.IsPartOfSettlement(character.homeSettlement) || !character2.traitContainer.HasTrait("Prisoner"))
					{
						continue;
					}
					Prisoner traitOrStatus = character2.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
					if (traitOrStatus.prisonerOfCharacter != null && traitOrStatus.prisonerOfCharacter.homeSettlement == character.homeSettlement)
					{
						num++;
						if (num >= 2)
						{
							return true;
						}
					}
				}
			}
		}
		else if (character.homeStructure != null)
		{
			int num2 = 0;
			for (int k = 0; k < character.homeStructure.charactersHere.Count; k++)
			{
				Character character3 = character.homeStructure.charactersHere[k];
				if (character3.isDead || !character3.traitContainer.HasTrait("Prisoner"))
				{
					continue;
				}
				Prisoner traitOrStatus2 = character3.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
				if (traitOrStatus2.prisonerOfCharacter != null && traitOrStatus2.prisonerOfCharacter.homeSettlement == character.homeSettlement)
				{
					num2++;
					if (num2 >= 2)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private Character GetFirstPrisonerAtHome(Character character)
	{
		if (character.homeSettlement != null && character.homeSettlement.locationType != LOCATION_TYPE.DUNGEON)
		{
			for (int i = 0; i < character.homeSettlement.areas.Count; i++)
			{
				Area area = character.homeSettlement.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
					if (!character2.isDead && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(character.homeSettlement) && character2.traitContainer.HasTrait("Prisoner") && character2.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
					{
						return character2;
					}
				}
			}
		}
		else if (character.homeStructure != null)
		{
			for (int k = 0; k < character.homeStructure.charactersHere.Count; k++)
			{
				Character character3 = character.homeStructure.charactersHere[k];
				if (!character3.isDead && character3.traitContainer.HasTrait("Prisoner") && character3.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
				{
					return character3;
				}
			}
		}
		return null;
	}

	private Character GetFirstPrisonerAtHomeToImpregnate(Character character)
	{
		if (character.homeSettlement != null && character.homeSettlement.locationType != LOCATION_TYPE.DUNGEON)
		{
			for (int i = 0; i < character.homeSettlement.areas.Count; i++)
			{
				Area area = character.homeSettlement.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
					if (!character2.isDead && !character2.traitContainer.HasTrait("Impregnated") && !character2.HasJobTargetingThis(JOB_TYPE.IMPREGNATE) && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(character.homeSettlement) && character2.traitContainer.HasTrait("Prisoner") && character2.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
					{
						return character2;
					}
				}
			}
		}
		else if (character.homeStructure != null)
		{
			for (int k = 0; k < character.homeStructure.charactersHere.Count; k++)
			{
				Character character3 = character.homeStructure.charactersHere[k];
				if (!character3.isDead && !character3.traitContainer.HasTrait("Impregnated") && !character3.HasJobTargetingThis(JOB_TYPE.IMPREGNATE) && character3.traitContainer.HasTrait("Prisoner") && character3.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
				{
					return character3;
				}
			}
		}
		return null;
	}

	private Character GetFirstDeadCharacterAtHomeForButcher(Character character)
	{
		if (character.homeSettlement != null && character.homeSettlement.locationType != LOCATION_TYPE.DUNGEON)
		{
			for (int i = 0; i < character.homeSettlement.areas.Count; i++)
			{
				Area area = character.homeSettlement.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
					if (character2.isDead && !character2.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER) && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(character.homeSettlement))
					{
						return character2;
					}
				}
			}
		}
		else if (character.homeStructure != null)
		{
			for (int k = 0; k < character.homeStructure.charactersHere.Count; k++)
			{
				Character character3 = character.homeStructure.charactersHere[k];
				if (character3.isDead && !character3.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER))
				{
					return character3;
				}
			}
		}
		return null;
	}

	private bool CanStillImpregnate(Character p_actor, int p_maxResidentCount)
	{
		int num = 0;
		if (p_actor.homeSettlement != null)
		{
			for (int i = 0; i < p_actor.homeSettlement.residents.Count; i++)
			{
				Character character = p_actor.homeSettlement.residents[i];
				if (!character.isDead && character.race == p_actor.race)
				{
					num++;
				}
			}
		}
		else if (p_actor.homeStructure != null)
		{
			for (int j = 0; j < p_actor.homeStructure.residents.Count; j++)
			{
				Character character2 = p_actor.homeStructure.residents[j];
				if (!character2.isDead && character2.race == p_actor.race)
				{
					num++;
				}
			}
		}
		else if (p_actor.HasTerritory() && p_actor.homeRegion != null)
		{
			for (int k = 0; k < p_actor.homeRegion.residents.Count; k++)
			{
				Character character3 = p_actor.homeRegion.residents[k];
				if (!character3.isDead && character3.HasTerritory() && p_actor.race == character3.race && character3.IsTerritory(p_actor.territory))
				{
					num++;
				}
			}
		}
		return num < p_maxResidentCount;
	}
}
