using System;
using Factions.Faction_Types;
using Object_Pools;

[Serializable]
public class Exclusive : FactionIdeology
{
	public EXCLUSIVE_IDEOLOGY_CATEGORIES category { get; private set; }

	public RACE raceRequirement { get; private set; }

	public GENDER genderRequirement { get; private set; }

	public string traitRequirement { get; private set; }

	public RELIGION religionRequirement { get; private set; }

	public Exclusive()
		: base(FACTION_IDEOLOGY.Exclusive)
	{
	}

	public override void LoadReferencesInMainThread(SaveDataFactionIdeology p_type)
	{
		ConstructLocalizedTexts();
	}

	public void LoadRequirement(RACE race)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE;
		raceRequirement = race;
	}

	public void LoadRequirement(GENDER gender)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER;
		genderRequirement = gender;
	}

	public void LoadRequirement(string trait)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT;
		traitRequirement = trait;
	}

	public void LoadRequirement(RELIGION religion)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.RELIGION;
		religionRequirement = religion;
	}

	public override bool DoesCharacterFitIdeology(Character character)
	{
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER)
		{
			return character.gender == genderRequirement;
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE)
		{
			return character.race == raceRequirement;
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT)
		{
			return character.traitContainer.HasTrait(traitRequirement);
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RELIGION)
		{
			return character.religionComponent.religion == religionRequirement;
		}
		return true;
	}

	public override bool DoesCharacterFitIdeology(PreCharacterData character)
	{
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER)
		{
			return character.gender == genderRequirement;
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE)
		{
			return character.race == raceRequirement;
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT)
		{
			return false;
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RELIGION)
		{
			return ReligionComponent.GetDefaultReligionForRace(character.race) == religionRequirement;
		}
		return true;
	}

	protected override void OnAddIdeology(FactionType factionType, Faction p_faction)
	{
		factionType.RemoveIdeology(FACTION_IDEOLOGY.Inclusive, p_faction);
	}

	public override bool IsReligionType()
	{
		return category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RELIGION;
	}

	private void ConstructLocalizedTexts()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "FactionIdeologies_Table", "Exclusive_Description");
		log.AddToFillers(null, GetRequirementAsString(), LOG_IDENTIFIER.STRING_1);
		_localizedDescription = log.logText;
		LogPool.Release(log);
		Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "FactionIdeologies_Table", "Exclusive");
		log2.AddToFillers(null, GetRequirementAsString(), LOG_IDENTIFIER.STRING_1);
		_localizedName = log2.logText;
		LogPool.Release(log2);
	}

	public void SetRequirement(RACE race)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE;
		raceRequirement = race;
		ConstructLocalizedTexts();
	}

	public void SetRequirement(GENDER gender)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER;
		genderRequirement = gender;
		ConstructLocalizedTexts();
	}

	public void SetRequirement(string trait)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT;
		traitRequirement = trait;
		ConstructLocalizedTexts();
	}

	public void SetRequirement(RELIGION religion)
	{
		category = EXCLUSIVE_IDEOLOGY_CATEGORIES.RELIGION;
		religionRequirement = religion;
		ConstructLocalizedTexts();
	}

	private string GetRequirementAsString()
	{
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER)
		{
			return genderRequirement switch
			{
				GENDER.MALE => LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Male"), 
				GENDER.FEMALE => LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Female"), 
				_ => genderRequirement.ToStringEnum(), 
			};
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE)
		{
			return LocalizationManager.Instance.GetLocalizedValue("CharacterGeneric_Table", raceRequirement.ToStringEnum() + "_Adjective");
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT)
		{
			return TraitManager.Instance.GetLocalizedNameOfTrait(traitRequirement);
		}
		if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RELIGION)
		{
			return LocalizationManager.Instance.GetLocalizedValue("FactionIdeologies_Table", religionRequirement.ToStringEnumWithSpace());
		}
		return string.Empty;
	}
}
