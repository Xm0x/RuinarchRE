using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class PsychopathyData : AfflictData
{
	public static readonly List<string> criteriaGenders = new List<string> { "Male", "Female" };

	public static readonly List<string> localizedCriteriaGenders = new List<string>();

	public static readonly List<string> criteriaRaces = new List<string> { "Humans", "Elves" };

	public static readonly List<string> localizedCriteriaRaces = new List<string>();

	public static readonly List<string> criteriaClasses = new List<string>
	{
		"Farmer", "Miner", "Crafter", "Fisher", "Logger", "Merchant", "Butcher", "Skinner", "Archer", "Stalker",
		"Hunter", "Druid", "Shaman", "Mage", "Knight", "Barbarian", "Marauder", "Noble", "Ratman"
	};

	public static readonly List<string> localizedCriteriaClasses = new List<string>();

	public static readonly List<string> criteriaTraits = new List<string>
	{
		"Accident Prone", "Agoraphobic", "Alcoholic", "Ambitious", "Authoritative", "Blessed", "Cannibal", "Chaste", "Coward", "Diplomatic",
		"Evil", "Fast", "Fire Resistant", "Glutton", "Hothead", "Inspiring", "Kleptomaniac", "Lazy", "Lustful", "Lycanthrope",
		"Music Hater", "Music Lover", "Narcoleptic", "Nocturnal", "Optimist", "Pessimist", "Psychopath", "Pyrophobic", "Robust", "Suspicious",
		"Treacherous", "Unattractive", "Unfaithful", "Vampire", "Vigilant", "Devout", "Heavy", "Slick", "Nullchild", "Grounded",
		"Negative Nancy", "Jinxed"
	};

	public static readonly List<string> localizedCriteriaTraits = new List<string>();

	public static readonly List<string> criteriaConjunctions = new List<string> { "And", "Or" };

	public static readonly List<string> localizedCriteriaConjunctions = new List<string>();

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PSYCHOPATHY;

	public override string name => "Psychopathy";

	public override string description => "This Affliction will turn a Villager into a Serial Killer. Serial Killers have a specific type of victim that they would target for abduction and killing. You may set your desired Victim Profile.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Psychopath";

	public PsychopathyData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		UIManager.Instance.psychopathUI.ShowPsychopathUI(targetPOI as Character);
	}

	public override void ApplyAfflictionEffects(IPointOfInterest target, int overridenDuration = 0)
	{
		Psychopath psychopath = TraitManager.Instance.CreateNewInstancedTraitClass<Psychopath>("Psychopath");
		target.traitContainer.AddTrait(target, psychopath);
		string randomElement = CollectionUtilities.GetRandomElement(criteriaTraits);
		psychopath.SetVictimRequirements(SERIAL_VICTIM_TYPE.Trait, randomElement, SERIAL_VICTIM_TYPE.None, string.Empty, string.Empty, TraitManager.Instance.GetLocalizedNameOfTrait(randomElement), string.Empty);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Beast"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public static void ConstructLocalizedTexts()
	{
		localizedCriteriaGenders.Clear();
		for (int i = 0; i < criteriaGenders.Count; i++)
		{
			string text = criteriaGenders[i];
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_" + text);
			localizedCriteriaGenders.Add(localizedValue);
		}
		localizedCriteriaRaces.Clear();
		for (int j = 0; j < criteriaRaces.Count; j++)
		{
			string key = criteriaRaces[j];
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", key);
			localizedCriteriaRaces.Add(localizedValue2);
		}
		localizedCriteriaClasses.Clear();
		for (int k = 0; k < criteriaClasses.Count; k++)
		{
			string key2 = criteriaClasses[k];
			string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("CharacterClasses_Table", key2);
			localizedCriteriaClasses.Add(localizedValue3);
		}
		localizedCriteriaTraits.Clear();
		for (int l = 0; l < criteriaTraits.Count; l++)
		{
			string key3 = criteriaTraits[l];
			string localizedValue4 = LocalizationManager.Instance.GetLocalizedValue("Traits_Table", key3);
			localizedCriteriaTraits.Add(localizedValue4);
		}
		localizedCriteriaConjunctions.Clear();
		for (int m = 0; m < criteriaConjunctions.Count; m++)
		{
			string text2 = criteriaConjunctions[m];
			string localizedValue5 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_" + text2);
			localizedCriteriaConjunctions.Add(localizedValue5);
		}
	}
}
