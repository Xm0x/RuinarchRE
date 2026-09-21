using System;
using UnityEngine.Localization.Settings;
using UtilityScripts;

namespace Traits;

[Serializable]
public class SerialVictim
{
	public SERIAL_VICTIM_TYPE victimFirstType;

	public SERIAL_VICTIM_TYPE victimSecondType;

	public string victimFirstDescription;

	public string victimSecondDescription;

	public string localizedVictimFirstDescription;

	public string localizedVictimSecondDescription;

	public string text { get; private set; }

	public bool isEmpty
	{
		get
		{
			if (victimFirstType == SERIAL_VICTIM_TYPE.None)
			{
				return victimSecondType == SERIAL_VICTIM_TYPE.None;
			}
			return false;
		}
	}

	public SerialVictim(SERIAL_VICTIM_TYPE victimFirstType, string victimFirstDesc, SERIAL_VICTIM_TYPE victimSecondType, string victimSecondDesc, string p_localizedVictimFirstDescription, string p_localizedVictimSecondDescription)
	{
		this.victimFirstType = victimFirstType;
		this.victimSecondType = victimSecondType;
		victimFirstDescription = victimFirstDesc;
		victimSecondDescription = victimSecondDesc;
		localizedVictimFirstDescription = p_localizedVictimFirstDescription;
		localizedVictimSecondDescription = p_localizedVictimSecondDescription;
		GenerateText();
	}

	private void GenerateText()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		bool fromSecondType = true;
		if (victimFirstType == SERIAL_VICTIM_TYPE.Trait)
		{
			fromSecondType = true;
			text = Utilities.NormalizeStringUpperCaseFirstLetters(localizedVictimFirstDescription);
		}
		else if (victimSecondType == SERIAL_VICTIM_TYPE.Trait)
		{
			fromSecondType = false;
			text = Utilities.NormalizeStringUpperCaseFirstLetters(localizedVictimSecondDescription);
		}
		if (text == string.Empty)
		{
			if (victimFirstType == SERIAL_VICTIM_TYPE.Gender)
			{
				fromSecondType = true;
				text = Utilities.NormalizeStringUpperCaseFirstLetters(localizedVictimFirstDescription);
			}
			else if (victimSecondType == SERIAL_VICTIM_TYPE.Gender)
			{
				fromSecondType = false;
				text = Utilities.NormalizeStringUpperCaseFirstLetters(localizedVictimSecondDescription);
			}
			if (text == string.Empty)
			{
				if (victimFirstType == SERIAL_VICTIM_TYPE.Race)
				{
					fromSecondType = true;
					text = GameUtilities.GetNormalizedRaceAdjective(localizedVictimFirstDescription);
				}
				else if (victimSecondType == SERIAL_VICTIM_TYPE.Race)
				{
					fromSecondType = false;
					text = GameUtilities.GetNormalizedRaceAdjective(localizedVictimSecondDescription);
				}
				if (text == string.Empty)
				{
					if (victimFirstDescription != string.Empty)
					{
						text = Utilities.NormalizeStringUpperCaseFirstLetters(localizedVictimFirstDescription);
					}
					else if (victimSecondDescription != string.Empty)
					{
						text = Utilities.NormalizeStringUpperCaseFirstLetters(localizedVictimSecondDescription);
					}
				}
				else
				{
					text2 = GetDescriptionText(fromSecondType);
				}
			}
			else
			{
				text2 = GetDescriptionText(fromSecondType);
			}
		}
		else
		{
			text2 = GetDescriptionText(fromSecondType);
		}
		if (text != string.Empty && text2 != string.Empty)
		{
			if (LocalizationSettings.SelectedLocale.Identifier.Code == "es")
			{
				if ((victimFirstType == SERIAL_VICTIM_TYPE.Race && victimSecondType == SERIAL_VICTIM_TYPE.Class) || (victimFirstType == SERIAL_VICTIM_TYPE.Class && victimSecondType == SERIAL_VICTIM_TYPE.Race))
				{
					this.text = text2 + " " + text;
				}
				else
				{
					this.text = text + " " + text2;
				}
			}
			else
			{
				this.text = text + " " + text2;
			}
		}
		else if (victimFirstType == SERIAL_VICTIM_TYPE.Race || victimSecondType == SERIAL_VICTIM_TYPE.Race || victimFirstType == SERIAL_VICTIM_TYPE.Gender || victimSecondType == SERIAL_VICTIM_TYPE.Gender)
		{
			if (text != string.Empty)
			{
				this.text = Utilities.PluralizeString(text) ?? "";
			}
			else
			{
				this.text = Utilities.PluralizeString(text2) ?? "";
			}
		}
		else if (LocalizationSettings.SelectedLocale.Identifier.Code != "pl")
		{
			if (text != string.Empty)
			{
				this.text = text + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "People");
			}
			else
			{
				this.text = text2 + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "People");
			}
		}
		this.text = this.text.Trim();
	}

	private string GetDescriptionText(bool fromSecondType)
	{
		string text = localizedVictimSecondDescription;
		if (!fromSecondType)
		{
			text = localizedVictimFirstDescription;
		}
		string text2 = string.Empty;
		if (text != string.Empty)
		{
			text2 += Utilities.PluralizeString(Utilities.NormalizeStringUpperCaseFirstLetters(text));
		}
		return text2;
	}

	public bool DoesCharacterFitVictimRequirements(Character character)
	{
		if (DoesCharacterFitVictimTypeDescription(victimFirstType, victimFirstDescription, character))
		{
			return DoesCharacterFitVictimTypeDescription(victimSecondType, victimSecondDescription, character);
		}
		return false;
	}

	private bool DoesCharacterFitVictimTypeDescription(SERIAL_VICTIM_TYPE victimType, string victimDesc, Character character)
	{
		if (victimType == SERIAL_VICTIM_TYPE.None)
		{
			return true;
		}
		if (!character.isNormalCharacter)
		{
			return false;
		}
		string text = string.Empty;
		switch (victimType)
		{
		case SERIAL_VICTIM_TYPE.Gender:
			text = character.gender.ToStringEnum();
			break;
		case SERIAL_VICTIM_TYPE.Race:
			text = character.race.ToStringEnum();
			break;
		case SERIAL_VICTIM_TYPE.Class:
			text = character.characterClass.className;
			break;
		case SERIAL_VICTIM_TYPE.Trait:
			if (character.traitContainer.HasTrait(victimDesc))
			{
				return true;
			}
			break;
		}
		if (text != string.Empty && victimDesc.Equals(text, StringComparison.CurrentCultureIgnoreCase))
		{
			return true;
		}
		return false;
	}
}
