using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class LocationClassManager
{
	private int startLoopIndex;

	private int numberOfRotations;

	private Dictionary<string, LocationClassNumberGuide> characterClassGuide;

	public string[] characterClassOrder { get; private set; }

	public int currentIndex { get; private set; }

	public Dictionary<string, int> combatantClasses { get; }

	public Dictionary<string, int> civilianClasses { get; }

	public LocationClassManager()
	{
		currentIndex = 0;
		startLoopIndex = 3;
		numberOfRotations = 0;
		combatantClasses = new Dictionary<string, int>();
		civilianClasses = new Dictionary<string, int>
		{
			{ "Peasant", 1 },
			{ "Crafter", 1 },
			{ "Miner", 1 }
		};
		CreateCharacterClassOrderAndGuide();
	}

	public string GetCurrentClassToCreate()
	{
		return GetClassToCreate(currentIndex);
	}

	public void AddCombatantClass(string className)
	{
		if (!combatantClasses.ContainsKey(className))
		{
			combatantClasses.Add(className, 0);
		}
		combatantClasses[className]++;
	}

	private string GetClassToCreate(int index)
	{
		string text = characterClassOrder[index];
		if (text == "Combatant")
		{
			text = CollectionUtilities.GetRandomElement(combatantClasses.Keys);
		}
		else if (text == "Civilian")
		{
			text = CollectionUtilities.GetRandomElement(civilianClasses.Keys);
		}
		return text;
	}

	private void CreateCharacterClassOrderAndGuide()
	{
		characterClassOrder = new string[8] { "Crafter", "Peasant", "Combatant", "Civilian", "Combatant", "Combatant", "Noble", "Combatant" };
		characterClassGuide = new Dictionary<string, LocationClassNumberGuide>
		{
			{
				"Peasant",
				new LocationClassNumberGuide
				{
					supposedNumber = 0,
					currentNumber = 0
				}
			},
			{
				"Combatant",
				new LocationClassNumberGuide
				{
					supposedNumber = 0,
					currentNumber = 0
				}
			},
			{
				"Crafter",
				new LocationClassNumberGuide
				{
					supposedNumber = 0,
					currentNumber = 0
				}
			},
			{
				"Civilian",
				new LocationClassNumberGuide
				{
					supposedNumber = 0,
					currentNumber = 0
				}
			},
			{
				"Noble",
				new LocationClassNumberGuide
				{
					supposedNumber = 0,
					currentNumber = 0
				}
			}
		};
	}

	public void OnAddResident(Character residentAdded)
	{
		string key = characterClassOrder[currentIndex];
		LocationClassNumberGuide value = characterClassGuide[key];
		value.supposedNumber++;
		value.currentNumber++;
		characterClassGuide[key] = value;
		currentIndex++;
		if (currentIndex >= characterClassOrder.Length)
		{
			currentIndex = startLoopIndex;
			numberOfRotations++;
		}
	}

	public void OnRemoveResident(Character residentRemoved)
	{
	}

	public void OnResidentChangeClass(Character resident, CharacterClass previousClass, CharacterClass currentClass)
	{
	}

	private void RevertCharacterClassOrderByOne()
	{
		string classIdentifier = characterClassOrder[currentIndex];
		if (currentIndex >= startLoopIndex)
		{
			currentIndex--;
			if (numberOfRotations > 0 && currentIndex < startLoopIndex)
			{
				currentIndex = characterClassOrder.Length - 1;
				numberOfRotations--;
			}
		}
		else
		{
			currentIndex--;
			if (currentIndex < 0)
			{
				currentIndex = 0;
				Debug.LogWarning("Wrong data! Current index cannot be less than zero");
			}
		}
		AdjustSupposedNumberOfClass(classIdentifier, -1);
	}

	private void AdjustCurrentNumberOfClass(string classIdentifier, int amount)
	{
		LocationClassNumberGuide value = characterClassGuide[classIdentifier];
		value.currentNumber += amount;
		characterClassGuide[classIdentifier] = value;
	}

	private void AdjustSupposedNumberOfClass(string classIdentifier, int amount)
	{
		LocationClassNumberGuide value = characterClassGuide[classIdentifier];
		value.supposedNumber += amount;
		characterClassGuide[classIdentifier] = value;
	}
}
