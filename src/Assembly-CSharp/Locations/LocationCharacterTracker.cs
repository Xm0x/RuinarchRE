using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Locations;

public class LocationCharacterTracker
{
	public List<Character> charactersAtLocation { get; private set; }

	public List<Summon> animalsThatProducesMats { get; private set; }

	public List<Summon> shearables { get; private set; }

	public List<Summon> skinnables { get; private set; }

	public List<Summon> butcherables { get; private set; }

	public LocationCharacterTracker()
	{
		charactersAtLocation = new List<Character>(100);
		animalsThatProducesMats = new List<Summon>(100);
		shearables = new List<Summon>(50);
		skinnables = new List<Summon>(50);
		butcherables = new List<Summon>(50);
	}

	public void AddCharacterAtLocation(Character p_character, Area p_area)
	{
		charactersAtLocation.Add(p_character);
		if (p_character is Summon p_character2 && (p_character.race.IsShearable() || p_character.race.IsSkinnable() || p_character.race.IsButcherableWhenDead() || p_character.race.IsButcherableWhenDeadOrAlive()))
		{
			AddAnimalToSettlement(p_character2);
		}
	}

	public void RemoveCharacterFromLocation(Character p_character, Area p_area)
	{
		if (charactersAtLocation.Remove(p_character) && p_character is Summon p_character2 && (p_character.race.IsShearable() || p_character.race.IsSkinnable() || p_character.race.IsButcherableWhenDead() || p_character.race.IsButcherableWhenDeadOrAlive()))
		{
			RemoveAnimalFromSettlement(p_character2);
		}
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.REMOVE_STATUS, (IPointOfInterest)p_character);
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.APPREHEND, (IPointOfInterest)p_character);
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.KNOCKOUT, (IPointOfInterest)p_character);
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.APPREHEND_RESTRAINED, (IPointOfInterest)p_character);
	}

	private void AddAnimalToSettlement(Summon p_character)
	{
		if (!animalsThatProducesMats.Contains(p_character))
		{
			animalsThatProducesMats.Add(p_character);
			if ((p_character.race.IsButcherableWhenDead() || p_character.race.IsButcherableWhenDeadOrAlive()) && !butcherables.Contains(p_character))
			{
				butcherables.Add(p_character);
			}
			if (p_character.race.IsShearable())
			{
				shearables.Add(p_character);
			}
			if (p_character.race.IsSkinnable())
			{
				skinnables.Add(p_character);
			}
		}
	}

	private void RemoveAnimalFromSettlement(Summon p_character)
	{
		animalsThatProducesMats.Remove(p_character);
		shearables.Remove(p_character);
		skinnables.Remove(p_character);
		butcherables.Remove(p_character);
	}

	public void PopulateAllAnimalsForSkinnersLodgeSkinning(List<Character> allAvailableAnimals, LocationStructure p_currentWorkingStructure)
	{
		for (int i = 0; i < animalsThatProducesMats.Count; i++)
		{
			Summon summon = animalsThatProducesMats[i];
			if (!summon.isBeingSeized && !summon.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER))
			{
				LocationStructure currentStructure = summon.currentStructure;
				if (((currentStructure != null && currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) || summon.currentStructure == p_currentWorkingStructure) && summon.isDead && summon.race.IsSkinnable())
				{
					allAvailableAnimals.Add(summon);
				}
			}
		}
	}

	public void PopulateAllAnimalsForSkinnersLodgeShearing(List<Character> ableToShearTodayList)
	{
		for (int i = 0; i < animalsThatProducesMats.Count; i++)
		{
			Summon summon = animalsThatProducesMats[i];
			if (!summon.isBeingSeized && summon is ShearableAnimal { isAvailableForShearing: not false } shearableAnimal && !shearableAnimal.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER) && (summon.isDead || summon.combatComponent.combatMode == COMBAT_MODE.Passive))
			{
				ableToShearTodayList.Add(summon);
			}
		}
	}

	public Summon GetFirstButcherableAnimal()
	{
		for (int i = 0; i < butcherables.Count; i++)
		{
			Summon summon = butcherables[i];
			LocationStructure currentStructure = summon.currentStructure;
			if (!summon.isBeingSeized && summon.hasMarker && currentStructure != null && !summon.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER))
			{
				if (summon.isDead && summon.race.IsButcherableWhenDead())
				{
					return summon;
				}
				if (summon.race.IsButcherableWhenDeadOrAlive())
				{
					return summon;
				}
			}
		}
		return null;
	}

	public void PopulateCharacterListInsideHex(List<Character> p_characterList)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized)
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateAnimalsListInsideHex(List<Character> p_characterList, bool includeBeingSeized = false)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && (includeBeingSeized || !character.isBeingSeized) && character is Animal)
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateAnimalsListThatCharacterCanReachInsideHexThatIsNotTheSameRaceAs(Character p_character, List<Character> p_characterList, RACE p_race)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && p_character.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation) && character is Animal && character.race != p_race)
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexThatHasTrait(List<Character> p_characterList, string p_traitName)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && character.traitContainer.HasTrait(p_traitName))
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexThatHasTraitAndNotRace(List<Character> p_characterList, string p_traitName, RACE p_race)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && !character.isDead && character.traitContainer.HasTrait(p_traitName) && character.race != p_race)
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexThatIsAlive(List<Character> p_characterList)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && !character.isDead)
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexForInvadeBehaviour(List<Character> p_characterList, Character p_exception = null)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && (p_exception == null || p_exception != character) && character.isNormalCharacter && !character.isDead && !character.isAlliedWithPlayer && !character.traitContainer.HasTrait("Hibernating", "Indestructible") && !character.isInLimbo && character.carryComponent.IsNotBeingCarried())
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexForKoboldBehaviour(List<Character> p_characterList, Character p_abductor)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && (character.currentStructure == null || character.currentStructure != p_abductor.homeStructure) && !character.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER) && character.traitContainer.HasTrait("Frozen") && character.race != RACE.KOBOLD)
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexForCentaurBehaviour(List<Character> p_characterList, Character p_abductor)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && (character.currentStructure == null || character.currentStructure != p_abductor.homeStructure) && !character.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER) && character.traitContainer.HasTrait("Ensnared") && character.race != RACE.CENTAUR)
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexForPangatLooTargetForInvasion(List<Character> p_characterList)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && character.isNormalCharacter && !character.isDead && !character.isInLimbo && character.carryComponent.IsNotBeingCarried() && !character.traitContainer.HasTrait("Hibernating", "Indestructible"))
			{
				p_characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListInsideHexForVengefulGhostBehaviour(List<Character> p_characterList, Character p_invader)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && character != p_invader && p_invader.IsHostileWith(character) && !character.isDead && !character.traitContainer.HasTrait("Hibernating", "Indestructible") && !character.isInLimbo && character.carryComponent.IsNotBeingCarried())
			{
				p_characterList.Add(character);
			}
		}
	}

	public Character GetFirstCharacterInsideHexForBoneGolemBehaviour(Character p_boneGolem)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && CharacterManager.Instance.IsCharacterConsideredTargetOfBoneGolem(p_boneGolem, character))
			{
				return character;
			}
		}
		return null;
	}

	public Character GetFirstCharacterInsideHexThatIsAliveHostileThatHasPathTo(Character p_character)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && p_character != character && p_character.IsHostileWith(character) && !character.isDead && !character.isAlliedWithPlayer && (bool)character.marker && character.marker.isMainVisualActive && p_character.movementComponent.HasPathTo(character.gridTileLocation) && !character.isInLimbo && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried() && !character.traitContainer.HasTrait("Hibernating", "Indestructible"))
			{
				return character;
			}
		}
		return null;
	}

	public Character GetFirstCharacterInsideHexThatIsAliveHostileAndInCorruptedTileThatHasPathTo(Character p_character)
	{
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			LocationGridTile gridTileLocation = character.gridTileLocation;
			if (gridTileLocation != null && p_character != character && gridTileLocation.corruptionComponent.isCorrupted && p_character.IsHostileWith(character) && !character.isDead && !character.isAlliedWithPlayer && (bool)character.marker && character.marker.isMainVisualActive && p_character.movementComponent.HasPathTo(character.gridTileLocation) && !character.isInLimbo && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried() && !character.traitContainer.HasTrait("Hibernating", "Indestructible", "Dazed") && gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL)
			{
				return character;
			}
		}
		return null;
	}

	public Character GetRandomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory(Area p_area)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isDead && character.IsTerritory(p_area))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideAreaThatIsAliveVillager()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isDead && character.isNormalCharacter && character.faction != FactionManager.Instance.undeadFaction)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && !character.isDead && character.isNormalCharacter && character.faction != FactionManager.Instance.undeadFaction)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterForMothmanStealOrGiveTrait(Character mothman)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			LocationGridTile gridTileLocation = character.gridTileLocation;
			if (gridTileLocation != null && gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && character != mothman && !character.isDead && (character.isNormalCharacter || character is Summon || character.faction == FactionManager.Instance.undeadFaction) && mothman.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && !character.isDead && character.isNormalCharacter && character.faction != FactionManager.Instance.undeadFaction && character.traitContainer.HasTrait("Resting"))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isDead && character is Animal && character.faction != FactionManager.Instance.undeadFaction)
			{
				LocationStructure currentStructure = character.currentStructure;
				if (currentStructure == null || currentStructure.structureType != STRUCTURE_TYPE.KENNEL)
				{
					list.Add(character);
				}
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideHexThatIsHumanoidOrSapientAndIsDeadAndIsNotInPrisonOrKennel()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS)
			{
				RaceData raceData = RaceManager.Instance.GetRaceData(character.race);
				if (!(raceData == null) && character.isDead && (character.race.IsSapient() || raceData.category == CHARACTER_CATEGORY.Humanoid) && (character.hasMarker || character.grave != null))
				{
					list.Add(character);
				}
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideAreaThatIsAliveNonCombatantAndNotInPrisonOrKennel()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && !character.isDead && character.isNormalCharacter && character.faction != FactionManager.Instance.undeadFaction && !character.traitContainer.HasTrait("Combatant"))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideAreaThatIsAliveCombatantAndNotInPrisonOrKennel()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && character.gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && !character.isDead && character.isNormalCharacter && character.faction != FactionManager.Instance.undeadFaction && character.traitContainer.HasTrait("Combatant"))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterInsideHexThatIsDead()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && !character.isBeingSeized && character.isDead)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public int GetNumOfCharactersInsideHexThatHasRaceAndClassOf(RACE p_race, string p_className, Type p_behaviourTypeException = null)
	{
		int num = 0;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gridTileLocation != null && character.race == p_race && character.characterClass.className == p_className && (p_behaviourTypeException == null || !character.behaviourComponent.HasBehaviour(p_behaviourTypeException)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetCharacterCount()
	{
		return charactersAtLocation.Count;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		charactersAtLocation.Contains(p_character);
		animalsThatProducesMats.Contains(p_character);
		shearables.Contains(p_character);
		skinnables.Contains(p_character);
		butcherables.Contains(p_character);
	}
}
