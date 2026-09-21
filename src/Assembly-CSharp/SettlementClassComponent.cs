using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine;
using UtilityScripts;

public class SettlementClassComponent : NPCSettlementComponent
{
	private static readonly string[] _characterClassOrder = new string[7] { "Civilian", "Combatant", "Civilian", "Combatant", "Combatant", "Noble", "Combatant" };

	private readonly List<string> _currentResidentClasses;

	private int _currentClassOrderIndex;

	private bool m_bypass;

	public GameDate morningScheduleDateForProcessingOfNeededClasses { get; private set; }

	public GameDate afternoonScheduleDateForProcessingOfNeededClasses { get; private set; }

	public int currentClassOrderIndex => _currentClassOrderIndex;

	public List<string> currentResidentClasses => _currentResidentClasses;

	public SettlementClassComponent()
	{
		_currentClassOrderIndex = 0;
		_currentResidentClasses = new List<string>();
	}

	public SettlementClassComponent(SaveDataSettlementClassComponent data)
	{
		_currentClassOrderIndex = data.currentClassOrderIndex;
		_currentResidentClasses = data.currentResidentClasses;
		morningScheduleDateForProcessingOfNeededClasses = data.morningScheduleDateForProcessingOfNeededClasses;
		afternoonScheduleDateForProcessingOfNeededClasses = data.afternoonScheduleDateForProcessingOfNeededClasses;
	}

	public string GetNextClassToCreateAndIncrementOrder([NotNull] Faction p_faction)
	{
		string currentClassInClassOrder = GetCurrentClassInClassOrder(p_faction);
		_currentClassOrderIndex = currentClassOrderIndex + 1;
		if (currentClassOrderIndex == _characterClassOrder.Length)
		{
			_currentClassOrderIndex = 0;
		}
		return currentClassInClassOrder;
	}

	private string GetCurrentClassInClassOrder([NotNull] Faction p_faction)
	{
		string text = _characterClassOrder[currentClassOrderIndex];
		if (text == "Combatant")
		{
			text = CollectionUtilities.GetRandomElement(p_faction.factionType.combatantClasses);
		}
		else if (text == "Civilian")
		{
			text = CollectionUtilities.GetRandomElement(p_faction.factionType.civilianClasses);
		}
		return text;
	}

	public void OnResidentAdded(Character p_newResident)
	{
		currentResidentClasses.Add(p_newResident.characterClass.className);
	}

	public void OnResidentRemoved(Character p_newResident)
	{
		currentResidentClasses.Remove(p_newResident.characterClass.className);
	}

	public void OnResidentChangedClass(string p_previousClass, Character p_character)
	{
		currentResidentClasses.Remove(p_previousClass);
		currentResidentClasses.Add(p_character.characterClass.className);
	}

	public int GetCurrentResidentClassAmount(string p_className)
	{
		int num = 0;
		for (int i = 0; i < _currentResidentClasses.Count; i++)
		{
			if (currentResidentClasses[i] == p_className)
			{
				num++;
			}
		}
		return num;
	}

	public void InitialMorningScheduleProcessingOfNeededClasses()
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE || base.owner.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			int ticksBasedOnHour = GameManager.Instance.GetTicksBasedOnHour(2);
			int ticksBasedOnHour2 = GameManager.Instance.GetTicksBasedOnHour(5);
			int ticks = GameUtilities.RandomBetweenTwoNumbers(ticksBasedOnHour, ticksBasedOnHour2);
			GameDate gameDate = GameManager.Instance.Today().AddDays(1);
			gameDate.SetTicks(ticks);
			morningScheduleDateForProcessingOfNeededClasses = gameDate;
			SchedulingManager.Instance.AddEntry(morningScheduleDateForProcessingOfNeededClasses, MorningProcessingOfNeededClasses, base.owner);
		}
	}

	public void InitialAfternoonScheduleProcessingOfNeededClasses()
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE || base.owner.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			int ticksBasedOnHour = GameManager.Instance.GetTicksBasedOnHour(12);
			int ticksBasedOnHour2 = GameManager.Instance.GetTicksBasedOnHour(15);
			int ticks = GameUtilities.RandomBetweenTwoNumbers(ticksBasedOnHour, ticksBasedOnHour2);
			GameDate gameDate = GameManager.Instance.Today().AddDays(1);
			gameDate.SetTicks(ticks);
			afternoonScheduleDateForProcessingOfNeededClasses = gameDate;
			SchedulingManager.Instance.AddEntry(afternoonScheduleDateForProcessingOfNeededClasses, AfternoonProcessingOfNeededClasses, base.owner);
		}
	}

	private void MorningProcessingOfNeededClasses()
	{
		if (base.owner.HasResidentThatIsNotDead())
		{
			ProcessNeededClasses();
		}
		morningScheduleDateForProcessingOfNeededClasses = GameManager.Instance.Today().AddDays(1);
		SchedulingManager.Instance.AddEntry(morningScheduleDateForProcessingOfNeededClasses, MorningProcessingOfNeededClasses, base.owner);
	}

	private void AfternoonProcessingOfNeededClasses()
	{
		if (base.owner.HasResidentThatIsNotDead())
		{
			ProcessNeededClasses();
		}
		afternoonScheduleDateForProcessingOfNeededClasses = GameManager.Instance.Today().AddDays(1);
		SchedulingManager.Instance.AddEntry(afternoonScheduleDateForProcessingOfNeededClasses, AfternoonProcessingOfNeededClasses, base.owner);
	}

	private void ProcessNeededClasses()
	{
		if (base.owner.owner != null && base.owner.owner.factionType.HasIdeology(FACTION_IDEOLOGY.Raiders))
		{
			ProcessNeededClassesForRaidersFaction();
		}
		else
		{
			ProcessNeededClassesForNormalFaction();
		}
	}

	private void ProcessNeededClassesForNormalFaction()
	{
		string log = string.Empty;
		m_bypass = false;
		base.owner.ForceCancelJobTypesImmediately(JOB_TYPE.CHANGE_CLASS);
		int numberOfResidentsThatIsAliveVillager = base.owner.GetNumberOfResidentsThatIsAliveVillager();
		int foodSupplyCapacity = base.owner.resourcesComponent.GetFoodSupplyCapacity();
		int resourceSupplyCapacity = base.owner.resourcesComponent.GetResourceSupplyCapacity();
		int numOfResidentsThatIsAliveCombatant = base.owner.GetNumOfResidentsThatIsAliveCombatant();
		int numberOfNeededCombatants = GetNumberOfNeededCombatants(numberOfResidentsThatIsAliveVillager);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num = 0;
		int num2 = 0;
		List<Character> list = RuinarchListPool<Character>.Claim();
		List<Character> list2 = RuinarchListPool<Character>.Claim();
		List<int> list3 = RuinarchListPool<int>.Claim();
		List<Character> list4 = RuinarchListPool<Character>.Claim();
		List<Character> list5 = RuinarchListPool<Character>.Claim();
		List<int> list6 = RuinarchListPool<int>.Claim();
		List<Character> list7 = RuinarchListPool<Character>.Claim();
		List<int> list8 = RuinarchListPool<int>.Claim();
		for (int i = 0; i < base.owner.residents.Count; i++)
		{
			Character character = base.owner.residents[i];
			if (character.isDead)
			{
				continue;
			}
			character.classComponent.SetShouldChangeClass(p_state: true);
			num2++;
			if ((!flag || !flag2 || !flag3 || !flag4) && !character.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && character.HasTalents())
			{
				if (!flag && character.classComponent.HasAbleClass("Fisher"))
				{
					flag = true;
				}
				if (!flag2 && character.classComponent.HasAbleClass("Butcher"))
				{
					flag2 = true;
				}
				if (!flag3 && character.classComponent.HasAbleClass("Skinner"))
				{
					flag3 = true;
				}
				if (!flag4 && character.classComponent.HasAbleClass("Merchant"))
				{
					flag4 = true;
				}
			}
			if (character.characterClass.IsCombatant())
			{
				int combatSupplyValue = character.classComponent.GetCombatSupplyValue();
				if (combatSupplyValue == 0)
				{
					list7.Add(character);
					list8.Add(combatSupplyValue);
					continue;
				}
				bool flag5 = false;
				for (int j = 0; j < list8.Count; j++)
				{
					int num3 = list8[j];
					if (combatSupplyValue > num3)
					{
						list7.Insert(j, character);
						list8.Insert(j, combatSupplyValue);
						flag5 = true;
						break;
					}
				}
				if (!flag5)
				{
					list7.Add(character);
					list8.Add(combatSupplyValue);
				}
			}
			else if (character.characterClass.className.IsFoodProducerClassName())
			{
				int foodSupplyCapacityValue = character.classComponent.GetFoodSupplyCapacityValue();
				if (foodSupplyCapacityValue == 0)
				{
					list2.Add(character);
					continue;
				}
				bool flag6 = false;
				for (int k = 0; k < list3.Count; k++)
				{
					int num4 = list3[k];
					if (foodSupplyCapacityValue > num4)
					{
						list.Insert(k, character);
						list3.Insert(k, foodSupplyCapacityValue);
						flag6 = true;
						break;
					}
				}
				if (!flag6)
				{
					list.Add(character);
					list3.Add(foodSupplyCapacityValue);
				}
			}
			else
			{
				if (!character.characterClass.className.IsResourceProducerClassName())
				{
					continue;
				}
				STRUCTURE_TYPE p_structureType = STRUCTURE_TYPE.NONE;
				if (character.structureComponent.HasWorkPlaceStructure())
				{
					p_structureType = character.structureComponent.workPlaceStructure.structureType;
				}
				int resourceSupplyCapacityValue = character.classComponent.GetResourceSupplyCapacityValue(p_structureType);
				if (resourceSupplyCapacityValue == 0)
				{
					list5.Add(character);
					continue;
				}
				bool flag7 = false;
				for (int l = 0; l < list6.Count; l++)
				{
					int num5 = list6[l];
					if (resourceSupplyCapacityValue > num5)
					{
						list4.Insert(l, character);
						list6.Insert(l, resourceSupplyCapacityValue);
						flag7 = true;
						break;
					}
				}
				if (!flag7)
				{
					list4.Add(character);
					list6.Add(resourceSupplyCapacityValue);
				}
			}
		}
		int num6 = 0;
		for (int m = 0; m < list.Count; m++)
		{
			Character character2 = list[m];
			if (num6 >= numberOfResidentsThatIsAliveVillager)
			{
				break;
			}
			num6 += list3[m];
			character2.classComponent.SetShouldChangeClass(p_state: false);
			num2--;
		}
		if (num6 < numberOfResidentsThatIsAliveVillager)
		{
			for (int n = 0; n < list2.Count; n++)
			{
				Character character3 = list2[n];
				int foodSupplyCapacityValueBase = character3.classComponent.GetFoodSupplyCapacityValueBase();
				if (num6 >= numberOfResidentsThatIsAliveVillager)
				{
					break;
				}
				if (foodSupplyCapacityValueBase != 0)
				{
					CharacterClass characterClass = character3.characterClass;
					if (base.owner.HasStructure(characterClass.workStructureType) || base.owner.HasBlueprintOnTileForStructure(characterClass.workStructureType))
					{
						num6 += foodSupplyCapacityValueBase;
						character3.classComponent.SetShouldChangeClass(p_state: false);
						num2--;
					}
				}
			}
		}
		int num7 = 0;
		for (int num8 = 0; num8 < list4.Count; num8++)
		{
			Character character4 = list4[num8];
			if (num7 >= numberOfResidentsThatIsAliveVillager)
			{
				break;
			}
			num7 += list6[num8];
			character4.classComponent.SetShouldChangeClass(p_state: false);
			num2--;
		}
		if (num7 < numberOfResidentsThatIsAliveVillager)
		{
			for (int num9 = 0; num9 < list5.Count; num9++)
			{
				Character character5 = list5[num9];
				int resourceSupplyCapacityValueBase = character5.classComponent.GetResourceSupplyCapacityValueBase();
				if (num6 >= numberOfResidentsThatIsAliveVillager)
				{
					break;
				}
				if (resourceSupplyCapacityValueBase != 0)
				{
					CharacterClass characterClass2 = character5.characterClass;
					if (base.owner.HasStructure(characterClass2.workStructureType) || base.owner.HasBlueprintOnTileForStructure(characterClass2.workStructureType))
					{
						num6 += resourceSupplyCapacityValueBase;
						character5.classComponent.SetShouldChangeClass(p_state: false);
					}
				}
			}
		}
		RuinarchListPool<Character>.Release(list2);
		RuinarchListPool<Character>.Release(list);
		RuinarchListPool<int>.Release(list3);
		RuinarchListPool<Character>.Release(list4);
		RuinarchListPool<Character>.Release(list5);
		RuinarchListPool<int>.Release(list6);
		ProcessNeededFoodProducerClasses(numberOfResidentsThatIsAliveVillager, foodSupplyCapacity, flag2, flag, ref log);
		ProcessNeededResourceClasses(numberOfResidentsThatIsAliveVillager, resourceSupplyCapacity, ref log);
		int numberOfJobsWith = base.owner.GetNumberOfJobsWith(JOB_TYPE.CHANGE_CLASS);
		int num10 = numberOfNeededCombatants - numberOfJobsWith;
		if (num10 > 0)
		{
			for (int num11 = 0; num11 < list7.Count; num11++)
			{
				Character character6 = list7[num11];
				if (num >= num10)
				{
					break;
				}
				character6.classComponent.SetShouldChangeClass(p_state: false);
				num++;
				num2--;
			}
		}
		bool hasCreatedChangeToCombatantClass = false;
		if (!m_bypass)
		{
			ProcessNeededCrafter(ref log);
			ProcessNeededCombatantClasses(numOfResidentsThatIsAliveCombatant, numberOfNeededCombatants, ref hasCreatedChangeToCombatantClass, ref log);
			ProcessNeededSpecialClasses(num2, flag3, flag4, ref log);
		}
		RuinarchListPool<Character>.Release(list7);
		RuinarchListPool<int>.Release(list8);
		ProcessNeededExtraClasses(hasCreatedChangeToCombatantClass, ref log);
	}

	private void ProcessNeededClassesForRaidersFaction()
	{
		string log = string.Empty;
		m_bypass = false;
		base.owner.ForceCancelJobTypesImmediately(JOB_TYPE.CHANGE_CLASS);
		int numberOfResidentsThatIsAliveVillager = base.owner.GetNumberOfResidentsThatIsAliveVillager();
		int numOfResidentsThatIsAliveCombatant = base.owner.GetNumOfResidentsThatIsAliveCombatant();
		int numberOfNeededCombatantsForBandits = GetNumberOfNeededCombatantsForBandits(numberOfResidentsThatIsAliveVillager);
		int num = 0;
		int num2 = 0;
		Character character = null;
		int num3 = 0;
		List<Character> list = RuinarchListPool<Character>.Claim();
		List<int> list2 = RuinarchListPool<int>.Claim();
		for (int i = 0; i < base.owner.residents.Count; i++)
		{
			Character character2 = base.owner.residents[i];
			if (character2.isDead)
			{
				continue;
			}
			character2.classComponent.SetShouldChangeClass(p_state: true);
			num2++;
			if (character2.characterClass.IsCombatant())
			{
				int combatSupplyValue = character2.classComponent.GetCombatSupplyValue();
				if (combatSupplyValue == 0)
				{
					list.Add(character2);
					list2.Add(combatSupplyValue);
					continue;
				}
				bool flag = false;
				for (int j = 0; j < list2.Count; j++)
				{
					int num4 = list2[j];
					if (combatSupplyValue > num4)
					{
						list.Insert(j, character2);
						list2.Insert(j, combatSupplyValue);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(character2);
					list2.Add(combatSupplyValue);
				}
			}
			else if (character2.characterClass.className == "Crafter")
			{
				int num5 = character2.TryGetTalentLevel(CHARACTER_TALENT.Crafting);
				if (character == null || num5 > num3)
				{
					character = character2;
					num3 = num5;
				}
			}
		}
		int num6 = 0;
		if (character != null)
		{
			if (list.Count >= 3)
			{
				character.classComponent.SetShouldChangeClass(p_state: false);
			}
		}
		else if (list.Count >= 3)
		{
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Crafter", null);
			num6++;
			m_bypass = true;
		}
		int num7 = numberOfNeededCombatantsForBandits - num6;
		if (num7 > 0)
		{
			for (int k = 0; k < list.Count; k++)
			{
				Character character3 = list[k];
				if (num >= num7)
				{
					break;
				}
				character3.classComponent.SetShouldChangeClass(p_state: false);
				num++;
				num2--;
			}
		}
		bool hasCreatedChangeToCombatantClass = false;
		if (!m_bypass)
		{
			ProcessNeededCombatantClasses(numOfResidentsThatIsAliveCombatant, numberOfNeededCombatantsForBandits, ref hasCreatedChangeToCombatantClass, ref log);
		}
		RuinarchListPool<Character>.Release(list);
		RuinarchListPool<int>.Release(list2);
	}

	private int GetNumberOfNeededCombatantsForBandits(int numOfActiveResidents)
	{
		return numOfActiveResidents - 1;
	}

	public static int GetNumberOfNeededCombatants(int numOfActiveResidents)
	{
		return numOfActiveResidents - Mathf.CeilToInt((float)numOfActiveResidents / 8f * 3f);
	}

	private void ProcessNeededFoodProducerClasses(int numOfActiveResidents, int foodSupplyCapacity, bool villagerCanBecomeButcher, bool villagerCanBecomeFisher, ref string log)
	{
		if (numOfActiveResidents <= foodSupplyCapacity)
		{
			return;
		}
		LocationStructure firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.BUTCHERS_SHOP);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null && villagerCanBecomeButcher)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Butcher", firstStructureOfTypeThatHasNoWorkerAndIsNotReserved);
			return;
		}
		log += "\nChecking Fishery...";
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.FISHERY);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null && villagerCanBecomeFisher)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Fisher", firstStructureOfTypeThatHasNoWorkerAndIsNotReserved);
			return;
		}
		log += "\nChecking Farm...";
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.FARM);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Farmer", firstStructureOfTypeThatHasNoWorkerAndIsNotReserved);
			return;
		}
		log += "\nChecking Butcher's Shop no limit...";
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstManmadeStructureOfTypeThatHasNotReachedMaxWorkers(STRUCTURE_TYPE.BUTCHERS_SHOP);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null && villagerCanBecomeButcher)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Butcher", null);
			return;
		}
		log += "\nChecking Fishery no limit...";
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstManmadeStructureOfTypeThatHasNotReachedMaxWorkers(STRUCTURE_TYPE.FISHERY);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null && villagerCanBecomeFisher)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Fisher", null);
			return;
		}
		log += "\nChecking Farm no limit...";
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstManmadeStructureOfTypeThatHasNotReachedMaxWorkers(STRUCTURE_TYPE.FARM);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Farmer", null);
		}
	}

	private void ProcessNeededResourceClasses(int numOfActiveResidents, int resourceSupplyCapacity, ref string log)
	{
		if (numOfActiveResidents > resourceSupplyCapacity)
		{
			Faction faction = base.owner.owner;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Elven_Kingdom)
			{
				ResourceProducersProcessing(STRUCTURE_TYPE.LUMBERYARD, "Logger", STRUCTURE_TYPE.MINE, "Miner", ref log);
			}
			else
			{
				ResourceProducersProcessing(STRUCTURE_TYPE.MINE, "Miner", STRUCTURE_TYPE.LUMBERYARD, "Logger", ref log);
			}
		}
	}

	private void ProcessNeededCrafter(ref string log)
	{
		LocationStructure firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.WORKSHOP);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null && base.owner.GetFirstStructureOfTypeThatHasWorkerOrIsReserved(STRUCTURE_TYPE.WORKSHOP) == null)
		{
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Crafter", firstStructureOfTypeThatHasNoWorkerAndIsNotReserved);
		}
	}

	private void ProcessNeededCombatantClasses(int numOfCombatants, int neededCombatants, ref bool hasCreatedChangeToCombatantClass, ref string log)
	{
		if (numOfCombatants < neededCombatants)
		{
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Combatant", null);
			hasCreatedChangeToCombatantClass = true;
		}
	}

	private void ProcessNeededSpecialClasses(int numberOfAvailableVillagers, bool villagerCanBecomeSkinner, bool villagerCanBecomeMerchant, ref string log)
	{
		if (base.owner.GetNumberOfJobsWith(JOB_TYPE.CHANGE_CLASS) >= numberOfAvailableVillagers)
		{
			return;
		}
		LocationStructure firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.WORKSHOP);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null)
		{
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Crafter", firstStructureOfTypeThatHasNoWorkerAndIsNotReserved);
			return;
		}
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.HUNTER_LODGE);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null && villagerCanBecomeSkinner)
		{
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Skinner", firstStructureOfTypeThatHasNoWorkerAndIsNotReserved);
			return;
		}
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE.TAVERN);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved != null && villagerCanBecomeMerchant)
		{
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Merchant", firstStructureOfTypeThatHasNoWorkerAndIsNotReserved);
		}
		else if (ChanceData.RollChance(CHANCE_TYPE.Create_Change_Class_Combatant, ref log))
		{
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob("Combatant", null);
		}
	}

	private bool ResourceProducersProcessing(STRUCTURE_TYPE primaryStructure, string primaryClass, STRUCTURE_TYPE secondaryStructure, string secondaryClass, ref string log)
	{
		LocationStructure firstStructureOfTypeThatHasNoWorkerAndIsNotReserved = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(primaryStructure);
		LocationStructure firstStructureOfTypeThatHasNoWorkerAndIsNotReserved2 = base.owner.GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(secondaryStructure);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved == null && firstStructureOfTypeThatHasNoWorkerAndIsNotReserved2 != null)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob(secondaryClass, firstStructureOfTypeThatHasNoWorkerAndIsNotReserved2);
			return true;
		}
		if (base.owner.GetFirstManmadeStructureOfTypeThatHasNotReachedMaxWorkers(primaryStructure) != null)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob(primaryClass, null);
			return true;
		}
		firstStructureOfTypeThatHasNoWorkerAndIsNotReserved2 = base.owner.GetFirstManmadeStructureOfTypeThatHasNotReachedMaxWorkers(secondaryStructure);
		if (firstStructureOfTypeThatHasNoWorkerAndIsNotReserved2 != null)
		{
			m_bypass = true;
			base.owner.settlementJobTriggerComponent.TriggerChangeClassJob(secondaryClass, null);
			return true;
		}
		return false;
	}

	private void ProcessNeededExtraClasses(bool hasCreatedChangeToCombatantClass, ref string log)
	{
		if (base.owner.GetNumberOfJobsWith(JOB_TYPE.CHANGE_CLASS) >= 2)
		{
			return;
		}
		LocationStructure firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved = base.owner.GetFirstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved();
		if (firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved != null)
		{
			StructureData structureData = LandmarkManager.Instance.GetStructureData(firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved.structureType);
			if (!string.IsNullOrEmpty(structureData.appropriateWorkerClassName))
			{
				base.owner.settlementJobTriggerComponent.TriggerExtraChangeClassJob(structureData.appropriateWorkerClassName, firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved);
				return;
			}
		}
		firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved = base.owner.GetFirstResourceProducingStructureThatCanAcceptWorkerAndIsNotReserved();
		if (firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved != null)
		{
			StructureData structureData2 = LandmarkManager.Instance.GetStructureData(firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved.structureType);
			if (!string.IsNullOrEmpty(structureData2.appropriateWorkerClassName))
			{
				base.owner.settlementJobTriggerComponent.TriggerExtraChangeClassJob(structureData2.appropriateWorkerClassName, firstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved);
			}
		}
		if (!hasCreatedChangeToCombatantClass)
		{
			base.owner.settlementJobTriggerComponent.TriggerExtraChangeClassJob("Combatant", null);
		}
	}

	public void LoadReferences(SaveDataSettlementClassComponent data)
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE || base.owner.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			SchedulingManager.Instance.AddEntry(morningScheduleDateForProcessingOfNeededClasses, MorningProcessingOfNeededClasses, base.owner);
			SchedulingManager.Instance.AddEntry(afternoonScheduleDateForProcessingOfNeededClasses, AfternoonProcessingOfNeededClasses, base.owner);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
