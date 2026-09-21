using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PsychopathRequirementsUI : MonoBehaviour
{
	public TMP_Dropdown reqTypeDropdown;

	public TMP_Dropdown reqDescriptionDropdown;

	public TextMeshProUGUI reqDescriptionsLabel;

	public Button addRequirementButton;

	public Button removeRequirementButton;

	private List<string> criteriaRaces = new List<string> { "HUMANS", "ELVES" };

	private List<string> criteriaClasses = new List<string>
	{
		"Farmer", "Miner", "Crafter", "Fisher", "Logger", "Merchant", "Butcher", "Skinner", "Archer", "Stalker",
		"Hunter", "Druid", "Shaman", "Mage", "Knight", "Barbarian", "Marauder", "Noble", "Ratman"
	};

	private List<string> criteriaTraits = new List<string>
	{
		"Accident Prone", "Agoraphobic", "Alcoholic", "Ambitious", "Authoritative", "Cannibal", "Chaste", "Coward", "Diplomatic", "Evil",
		"Fast", "Fire Resistant", "Glutton", "Hothead", "Inspiring", "Kleptomaniac", "Lazy", "Lustful", "Lycanthrope", "Music Hater",
		"Music Lover", "Narcoleptic", "Nocturnal", "Optimist", "Pessimist", "Psychopath", "Pyrophobic", "Robust", "Suspicious", "Treacherous",
		"Unattractive", "Unfaithful", "Vampire", "Vigilant"
	};

	public SERIAL_VICTIM_TYPE victimType { get; private set; }

	public List<string> victimDescriptions { get; private set; }

	public void ShowRequirementsUI()
	{
		if (victimDescriptions == null)
		{
			victimDescriptions = new List<string>();
		}
		PopulateRequirementsType();
		HideAddRemoveButtons();
		reqDescriptionsLabel.text = "None";
		PopulateRequirementsDescriptions();
	}

	private void PopulateRequirementsType()
	{
		reqTypeDropdown.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		Utilities.GetEnumChoices(list, includeNone: true, Array.Empty<SERIAL_VICTIM_TYPE>());
		reqTypeDropdown.AddOptions(list);
		reqTypeDropdown.value = 0;
		RuinarchListPool<string>.Release(list);
	}

	private void PopulateRequirementsDescriptions()
	{
		reqDescriptionDropdown.ClearOptions();
		victimDescriptions.Clear();
		if (victimType == SERIAL_VICTIM_TYPE.None)
		{
			reqDescriptionDropdown.options.Add(new TMP_Dropdown.OptionData("NONE"));
			HideAddRemoveButtons();
		}
		else if (victimType == SERIAL_VICTIM_TYPE.Gender)
		{
			List<string> list = RuinarchListPool<string>.Claim();
			Utilities.PopulateEnumChoices<GENDER>(list);
			reqDescriptionDropdown.AddOptions(list);
			RuinarchListPool<string>.Release(list);
			HideAddRemoveButtons();
		}
		else if (victimType == SERIAL_VICTIM_TYPE.Race)
		{
			reqDescriptionDropdown.AddOptions(criteriaRaces);
			HideAddRemoveButtons();
		}
		else if (victimType == SERIAL_VICTIM_TYPE.Class)
		{
			reqDescriptionDropdown.AddOptions(criteriaClasses);
			ShowAddRemoveButtons();
		}
		else if (victimType == SERIAL_VICTIM_TYPE.Trait)
		{
			reqDescriptionDropdown.AddOptions(criteriaTraits);
			ShowAddRemoveButtons();
		}
		reqDescriptionDropdown.value = 0;
		reqDescriptionDropdown.RefreshShownValue();
		UpdateRequirementsLabel();
	}

	private void ShowAddRemoveButtons()
	{
		addRequirementButton.gameObject.SetActive(value: true);
		removeRequirementButton.gameObject.SetActive(value: true);
	}

	private void HideAddRemoveButtons()
	{
		addRequirementButton.gameObject.SetActive(value: false);
		removeRequirementButton.gameObject.SetActive(value: false);
	}

	private void UpdateRequirementsLabel()
	{
		string text = string.Empty;
		if (victimType == SERIAL_VICTIM_TYPE.Gender || victimType == SERIAL_VICTIM_TYPE.Race)
		{
			text = reqDescriptionDropdown.options[reqDescriptionDropdown.value].text;
		}
		else if (victimDescriptions.Count > 0)
		{
			for (int i = 0; i < victimDescriptions.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				text += victimDescriptions[i];
			}
		}
		else
		{
			text = "None";
		}
		reqDescriptionsLabel.text = $"{victimType}: {text}";
	}

	public void OnChangedReqType(int index)
	{
		victimType = (SERIAL_VICTIM_TYPE)reqTypeDropdown.value;
		PopulateRequirementsDescriptions();
	}

	public void OnChangedReqDescription(int index)
	{
		UpdateRequirementsLabel();
	}

	public void OnClickAddReq()
	{
		string text = reqDescriptionDropdown.options[reqDescriptionDropdown.value].text;
		if (!victimDescriptions.Contains(text))
		{
			victimDescriptions.Add(text);
			UpdateRequirementsLabel();
		}
	}

	public void OnClickRemoveReq()
	{
		victimDescriptions.Remove(reqDescriptionDropdown.options[reqDescriptionDropdown.value].text);
		UpdateRequirementsLabel();
	}
}
