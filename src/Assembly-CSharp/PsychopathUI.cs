using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class PsychopathUI : MonoBehaviour
{
	public PsychopathPicker psychopathPicker;

	public RuinarchButton confirmButton;

	public GameObject requirement1GO;

	public GameObject requirement2GO;

	public GameObject conjunctionGO;

	public GameObject andGO;

	public RuinarchButton victimType1Button;

	public RuinarchButton victimType2Button;

	public RuinarchButton victimDescription1Button;

	public RuinarchButton victimDescription2Button;

	public RuinarchButton clearButtonVictim1;

	public RuinarchButton clearButtonVictim2;

	public TextMeshProUGUI victimType1Text;

	public TextMeshProUGUI victimType2Text;

	public TextMeshProUGUI victimDescription1Text;

	public TextMeshProUGUI victimDescription2Text;

	public TextMeshProUGUI conjunctionText;

	private SERIAL_VICTIM_TYPE victimType1;

	private SERIAL_VICTIM_TYPE victimType2;

	private string victimDescription1;

	private string victimDescription2;

	private string localizedVictimDescription1;

	private string localizedVictimDescription2;

	private string conjunction;

	private List<string> serialVictimType1AsStrings = new List<string>();

	private List<string> serialVictimType2AsStrings = new List<string>();

	private List<string> localizedSerialVictimType1AsStrings = new List<string>();

	private List<string> localizedSerialVictimType2AsStrings = new List<string>();

	public Character character { get; private set; }

	public void ShowPsychopathUI(Character character)
	{
		this.character = character;
		UpdateUIBasedOnPsychopathyAfflictionLevel();
		ClearVictim1();
		ClearVictim2();
		ClearConjunction();
		base.gameObject.SetActive(value: true);
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
	}

	public void HidePsychopathUI()
	{
		character = null;
		base.gameObject.SetActive(value: false);
		if (!PlayerUI.Instance.TryShowPendingUI() && !UIManager.Instance.IsObjectPickerOpen())
		{
			UIManager.Instance.ResumeLastProgressionSpeed();
		}
	}

	private void UpdateUIBasedOnPsychopathyAfflictionLevel()
	{
		AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PSYCHOPATHY);
		requirement1GO.SetActive(value: true);
		requirement2GO.SetActive(afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Add_And_Selection));
		conjunctionGO.SetActive(afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Add_Or_Selection));
		andGO.SetActive(!conjunctionGO.activeSelf);
	}

	private void UpdateConfirmButtonState()
	{
		confirmButton.interactable = (victimType1 != SERIAL_VICTIM_TYPE.None && victimDescription1 != string.Empty) || (victimType2 != SERIAL_VICTIM_TYPE.None && victimDescription2 != string.Empty);
	}

	private void SetVictimType1(SERIAL_VICTIM_TYPE type, string localizedString)
	{
		victimType1 = type;
		if (type != SERIAL_VICTIM_TYPE.None)
		{
			victimType1Text.text = localizedString;
			clearButtonVictim1.gameObject.SetActive(value: true);
		}
		else
		{
			victimType1Text.text = "<i>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Type") + "</i>";
			clearButtonVictim1.gameObject.SetActive(value: false);
		}
		SetVictim1Description(string.Empty, string.Empty);
		UpdateVictimDescription1ButtonState();
		UpdateConfirmButtonState();
	}

	private void SetVictimType2(SERIAL_VICTIM_TYPE type, string localizedString)
	{
		victimType2 = type;
		if (type != SERIAL_VICTIM_TYPE.None)
		{
			victimType2Text.text = localizedString;
			clearButtonVictim2.gameObject.SetActive(value: true);
		}
		else
		{
			victimType2Text.text = "<i>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Type") + "</i>";
			clearButtonVictim2.gameObject.SetActive(value: false);
		}
		SetVictim2Description(string.Empty, string.Empty);
		UpdateVictimDescription2ButtonState();
		UpdateConfirmButtonState();
	}

	private void SetVictim1Description(string str, string localized)
	{
		victimDescription1 = str;
		localizedVictimDescription1 = localized;
		victimDescription1Text.text = (string.IsNullOrEmpty(str) ? ("<i>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Number_Criteria_Title") + "</i>") : localized);
		UpdateConfirmButtonState();
	}

	private void SetVictim2Description(string str, string localized)
	{
		victimDescription2 = str;
		localizedVictimDescription2 = localized;
		victimDescription2Text.text = (string.IsNullOrEmpty(str) ? ("<i>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Number_Criteria_Title") + "</i>") : localized);
		UpdateConfirmButtonState();
	}

	private void SetConjunction(string p_str, string localized)
	{
		conjunction = p_str;
		conjunctionText.text = localized;
	}

	private void UpdateVictimDescription1ButtonState()
	{
		victimDescription1Button.interactable = victimType1 != SERIAL_VICTIM_TYPE.None;
	}

	private void UpdateVictimDescription2ButtonState()
	{
		victimDescription2Button.interactable = victimType2 != SERIAL_VICTIM_TYPE.None;
	}

	private List<string> GetListOfVictimDescriptionsBasedOnType(SERIAL_VICTIM_TYPE type)
	{
		return type switch
		{
			SERIAL_VICTIM_TYPE.Gender => PsychopathyData.criteriaGenders, 
			SERIAL_VICTIM_TYPE.Class => PsychopathyData.criteriaClasses, 
			SERIAL_VICTIM_TYPE.Race => PsychopathyData.criteriaRaces, 
			SERIAL_VICTIM_TYPE.Trait => PsychopathyData.criteriaTraits, 
			_ => null, 
		};
	}

	private List<string> GetListOfLocalizedVictimDescriptionsBasedOnType(SERIAL_VICTIM_TYPE type)
	{
		return type switch
		{
			SERIAL_VICTIM_TYPE.Gender => PsychopathyData.localizedCriteriaGenders, 
			SERIAL_VICTIM_TYPE.Class => PsychopathyData.localizedCriteriaClasses, 
			SERIAL_VICTIM_TYPE.Race => PsychopathyData.localizedCriteriaRaces, 
			SERIAL_VICTIM_TYPE.Trait => PsychopathyData.localizedCriteriaTraits, 
			_ => null, 
		};
	}

	private void UpdateSerialVictimTypesAsStrings(List<string> list, List<string> localizedStrings)
	{
		list.Clear();
		localizedStrings.Clear();
		switch (PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PSYCHOPATHY).currentLevel)
		{
		case 0:
			list.Add("Trait");
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Trait"));
			break;
		case 1:
			list.Add("Trait");
			list.Add("Class");
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Trait"));
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Class"));
			break;
		case 2:
			list.Add("Trait");
			list.Add("Class");
			list.Add("Gender");
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Trait"));
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Class"));
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Gender"));
			break;
		case 3:
			list.Add("Trait");
			list.Add("Class");
			list.Add("Gender");
			list.Add("Race");
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Trait"));
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Class"));
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Gender"));
			localizedStrings.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Race"));
			break;
		}
	}

	public void OnClickVictimType1()
	{
		UpdateSerialVictimTypesAsStrings(serialVictimType1AsStrings, localizedSerialVictimType1AsStrings);
		for (int i = 0; i < serialVictimType1AsStrings.Count; i++)
		{
			if (serialVictimType1AsStrings[i] == victimType2Text.text || localizedSerialVictimType1AsStrings[i] == victimType2Text.text)
			{
				serialVictimType1AsStrings.RemoveAt(i);
				localizedSerialVictimType1AsStrings.RemoveAt(i);
				break;
			}
		}
		psychopathPicker.ShowPicker(serialVictimType1AsStrings, OnConfirmVictimType1, null, null, localizedSerialVictimType1AsStrings);
	}

	private void OnConfirmVictimType1(string str, string localized)
	{
		SERIAL_VICTIM_TYPE type = (SERIAL_VICTIM_TYPE)Enum.Parse(typeof(SERIAL_VICTIM_TYPE), str);
		SetVictimType1(type, localized);
	}

	public void OnClickVictimType2()
	{
		UpdateSerialVictimTypesAsStrings(serialVictimType2AsStrings, localizedSerialVictimType2AsStrings);
		for (int i = 0; i < serialVictimType2AsStrings.Count; i++)
		{
			if (serialVictimType2AsStrings[i] == victimType1Text.text || localizedSerialVictimType2AsStrings[i] == victimType1Text.text)
			{
				serialVictimType2AsStrings.RemoveAt(i);
				localizedSerialVictimType2AsStrings.RemoveAt(i);
				break;
			}
		}
		psychopathPicker.ShowPicker(serialVictimType2AsStrings, OnConfirmVictimType2, null, null, localizedSerialVictimType2AsStrings);
	}

	private void OnConfirmVictimType2(string str, string localized)
	{
		SERIAL_VICTIM_TYPE type = (SERIAL_VICTIM_TYPE)Enum.Parse(typeof(SERIAL_VICTIM_TYPE), str);
		SetVictimType2(type, localized);
	}

	public void OnClickVictimDescription1()
	{
		List<string> listOfVictimDescriptionsBasedOnType = GetListOfVictimDescriptionsBasedOnType(victimType1);
		List<string> listOfLocalizedVictimDescriptionsBasedOnType = GetListOfLocalizedVictimDescriptionsBasedOnType(victimType1);
		if (listOfVictimDescriptionsBasedOnType != null)
		{
			psychopathPicker.ShowPicker(listOfVictimDescriptionsBasedOnType, OnConfirmVictimDescription1, null, null, listOfLocalizedVictimDescriptionsBasedOnType);
		}
	}

	private void OnConfirmVictimDescription1(string str, string localized)
	{
		SetVictim1Description(str, localized);
	}

	public void OnClickVictimDescription2()
	{
		List<string> listOfVictimDescriptionsBasedOnType = GetListOfVictimDescriptionsBasedOnType(victimType2);
		List<string> listOfLocalizedVictimDescriptionsBasedOnType = GetListOfLocalizedVictimDescriptionsBasedOnType(victimType2);
		if (listOfVictimDescriptionsBasedOnType != null)
		{
			psychopathPicker.ShowPicker(listOfVictimDescriptionsBasedOnType, OnConfirmVictimDescription2, null, null, listOfLocalizedVictimDescriptionsBasedOnType);
		}
	}

	private void OnConfirmVictimDescription2(string str, string localized)
	{
		SetVictim2Description(str, localized);
	}

	public void OnClickConjunction()
	{
		psychopathPicker.ShowPicker(PsychopathyData.criteriaConjunctions, OnConfirmConjunction, null, null, PsychopathyData.localizedCriteriaConjunctions);
	}

	private void OnConfirmConjunction(string str, string localized)
	{
		SetConjunction(str, localized);
	}

	public void ClearVictim1()
	{
		SetVictimType1(SERIAL_VICTIM_TYPE.None, string.Empty);
	}

	public void ClearVictim2()
	{
		SetVictimType2(SERIAL_VICTIM_TYPE.None, string.Empty);
	}

	public void ClearConjunction()
	{
		SetConjunction(PsychopathyData.criteriaConjunctions[0], PsychopathyData.localizedCriteriaConjunctions[0]);
	}

	public void OnClickConfirm()
	{
		Psychopath psychopath = TraitManager.Instance.CreateNewInstancedTraitClass<Psychopath>("Psychopath");
		AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PSYCHOPATHY);
		character.traitContainer.AddTrait(character, psychopath);
		string value = ((!(LocalizationSettings.SelectedLocale.Identifier.Code == "tr-TR")) ? psychopath.localizedName : afflictionData.localizedName);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "player_afflicted", LOG_TAG.Life_Changes);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase(releaseLogAfter: true);
		if (victimType1 == SERIAL_VICTIM_TYPE.None)
		{
			victimDescription1 = string.Empty;
		}
		if (victimType2 == SERIAL_VICTIM_TYPE.None)
		{
			victimDescription2 = string.Empty;
		}
		if (victimDescription1 == string.Empty)
		{
			victimType1 = SERIAL_VICTIM_TYPE.None;
		}
		if (victimDescription2 == string.Empty)
		{
			victimType2 = SERIAL_VICTIM_TYPE.None;
		}
		psychopath.SetVictimRequirements(victimType1, victimDescription1, victimType2, victimDescription2, conjunction, localizedVictimDescription1, localizedVictimDescription2);
		HidePsychopathUI();
		afflictionData.OnExecutePlayerSkill();
		PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_CREATE_PSYCHO);
	}
}
