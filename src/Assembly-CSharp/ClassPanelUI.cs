using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class ClassPanelUI : MonoBehaviour
{
	public static ClassPanelUI Instance;

	public InputField classNameInput;

	public InputField identifierInput;

	public InputField tamedTraitInput;

	public InputField baseAttackPowerInput;

	public InputField baseSpeedInput;

	public InputField baseHPInput;

	public InputField baseAttackSpeedInput;

	public InputField attackRangeInput;

	public InputField inventoryCapacityInput;

	public InputField interestedItemNamesInput;

	public InputField staminaReductionInput;

	public Dropdown traitOptions;

	public Dropdown ableJobsOptions;

	public Dropdown elementalTypeOptions;

	public Dropdown attackTypeOptions;

	public Dropdown rangeTypeOptions;

	public GameObject traitsGO;

	public GameObject relatedStructuresGO;

	public GameObject priorityJobsGO;

	public GameObject secondaryJobsGO;

	public GameObject ableJobsGO;

	public GameObject traitBtnGO;

	public GameObject relatedStructuresBtnGO;

	public GameObject priorityJobBtnPrefab;

	public ScrollRect traitsScrollRect;

	public ScrollRect relatedStructuresScrollRect;

	public ScrollRect priorityJobsScrollRect;

	public ScrollRect secondaryJobsScrollRect;

	public ScrollRect ableJobsScrollRect;

	[NonSerialized]
	public ClassTraitButton currentSelectedClassTraitButton;

	[NonSerialized]
	public StructureTypeButton currentSelectedRelatedStructuresButton;

	[NonSerialized]
	public PriorityJobButton currentSelectedPriorityJobButton;

	[NonSerialized]
	public PriorityJobButton currentSelectedSecondaryJobButton;

	[NonSerialized]
	public PriorityJobButton currentSelectedAbleJobButton;

	[NonSerialized]
	public List<string> allClasses;

	private List<string> _traitNames;

	private List<JOB_TYPE> _ableJobs;

	public List<string> traitNames => _traitNames;

	public List<JOB_TYPE> ableJobs => _ableJobs;

	private void Awake()
	{
		Instance = this;
	}

	private void UpdateClassList()
	{
		allClasses.Clear();
		string[] files = Directory.GetFiles(Utilities.coreStreamingDataPath + "/CharacterClasses/", "*.json");
		foreach (string path in files)
		{
			allClasses.Add(Path.GetFileNameWithoutExtension(path));
		}
		CharacterPanelUI.Instance.UpdateClassOptions();
	}

	public void UpdateTraitOptions()
	{
		traitOptions.ClearOptions();
		traitOptions.AddOptions(TraitPanelUI.Instance.allTraits);
		traitOptions.AddOptions(TraitManager.instancedTraitsAndStatuses.ToList());
	}

	public void LoadAllData()
	{
		allClasses = new List<string>();
		_traitNames = new List<string>();
		_ableJobs = new List<JOB_TYPE>();
		elementalTypeOptions.ClearOptions();
		attackTypeOptions.ClearOptions();
		rangeTypeOptions.ClearOptions();
		ableJobsOptions.ClearOptions();
		string[] names = Enum.GetNames(typeof(ELEMENTAL_TYPE));
		string[] names2 = Enum.GetNames(typeof(ATTACK_TYPE));
		string[] names3 = Enum.GetNames(typeof(RANGE_TYPE));
		Enum.GetNames(typeof(STRUCTURE_TYPE));
		string[] names4 = Enum.GetNames(typeof(JOB_TYPE));
		elementalTypeOptions.AddOptions(names.ToList());
		attackTypeOptions.AddOptions(names2.ToList());
		rangeTypeOptions.AddOptions(names3.ToList());
		ableJobsOptions.AddOptions(names4.ToList());
		UpdateClassList();
	}

	private void ClearData()
	{
		currentSelectedClassTraitButton = null;
		classNameInput.text = string.Empty;
		identifierInput.text = string.Empty;
		interestedItemNamesInput.text = string.Empty;
		tamedTraitInput.text = string.Empty;
		baseAttackPowerInput.text = "0";
		baseSpeedInput.text = "0";
		baseHPInput.text = "0";
		baseAttackSpeedInput.text = "1";
		attackRangeInput.text = "1";
		inventoryCapacityInput.text = "0";
		staminaReductionInput.text = "0";
		traitOptions.value = 0;
		elementalTypeOptions.value = 0;
		attackTypeOptions.value = 0;
		rangeTypeOptions.value = 0;
		_traitNames.Clear();
		_ableJobs.Clear();
		Utilities.DestroyChildren(traitsScrollRect.content);
		Utilities.DestroyChildren(relatedStructuresScrollRect.content);
		Utilities.DestroyChildren(priorityJobsScrollRect.content);
		Utilities.DestroyChildren(secondaryJobsScrollRect.content);
		Utilities.DestroyChildren(ableJobsScrollRect.content);
	}

	private void SaveClass()
	{
		string.IsNullOrEmpty(classNameInput.text);
		string.IsNullOrEmpty(identifierInput.text);
		string path = Utilities.coreStreamingDataPath + "/CharacterClasses/" + classNameInput.text + ".json";
		if (!Utilities.DoesFileExist(path))
		{
			SaveClassJson(path);
		}
	}

	private void SaveClassJson(string path)
	{
		string value = JsonUtility.ToJson(new CharacterClass());
		StreamWriter streamWriter = new StreamWriter(path, append: false);
		streamWriter.WriteLine(value);
		streamWriter.Close();
		Debug.Log("Successfully saved class at " + path);
		UpdateClassList();
	}

	private void LoadClass()
	{
	}

	private void LoadClassDataToUI(CharacterClass characterClass)
	{
		classNameInput.text = characterClass.className;
		identifierInput.text = characterClass.identifier;
		baseAttackPowerInput.text = characterClass.baseAttackPower.ToString();
		baseHPInput.text = characterClass.baseHP.ToString();
		baseAttackSpeedInput.text = characterClass.baseAttackSpeed.ToString();
		attackRangeInput.text = characterClass.attackRange.ToString();
		staminaReductionInput.text = characterClass.staminaReduction.ToString();
		inventoryCapacityInput.text = characterClass.inventoryCapacity.ToString();
		interestedItemNamesInput.text = Utilities.ConvertArrayToString(characterClass.interestedItemNames, ',');
		elementalTypeOptions.value = GetDropdownIndex(elementalTypeOptions, characterClass.elementalType.ToString());
		attackTypeOptions.value = GetDropdownIndex(attackTypeOptions, characterClass.attackType.ToString());
		rangeTypeOptions.value = GetDropdownIndex(rangeTypeOptions, characterClass.rangeType.ToString());
		for (int i = 0; i < characterClass.traitNames.Length; i++)
		{
			string text = characterClass.traitNames[i];
			_traitNames.Add(text);
			UnityEngine.Object.Instantiate(traitBtnGO, traitsScrollRect.content).GetComponent<ClassTraitButton>().SetTraitName(text);
		}
		if (characterClass.ableJobs != null)
		{
			for (int j = 0; j < characterClass.ableJobs.Length; j++)
			{
				JOB_TYPE jobType = characterClass.ableJobs[j];
				OnAddJob(jobType, "able");
			}
		}
	}

	private int GetDropdownIndex(Dropdown options, string name)
	{
		for (int i = 0; i < options.options.Count; i++)
		{
			if (options.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	public void OnAddNewClass()
	{
		ClearData();
	}

	public void OnSaveClass()
	{
		SaveClass();
	}

	public void OnEditClass()
	{
		LoadClass();
	}

	public void OnAddTrait()
	{
		string text = traitOptions.options[traitOptions.value].text;
		if (!_traitNames.Contains(text))
		{
			_traitNames.Add(text);
			UnityEngine.Object.Instantiate(traitBtnGO, traitsScrollRect.content).GetComponent<ClassTraitButton>().SetTraitName(text);
		}
	}

	public void OnRemoveTrait()
	{
		if (currentSelectedClassTraitButton != null)
		{
			string text = currentSelectedClassTraitButton.buttonText.text;
			if (_traitNames.Remove(text))
			{
				UnityEngine.Object.Destroy(currentSelectedClassTraitButton.gameObject);
				currentSelectedClassTraitButton = null;
			}
		}
	}

	public void OnAddAbleJob()
	{
		string text = ableJobsOptions.options[ableJobsOptions.value].text;
		JOB_TYPE jobType = (JOB_TYPE)Enum.Parse(typeof(JOB_TYPE), text);
		OnAddJob(jobType, "able");
	}

	public void OnRemovePriorityJob()
	{
		OnRemoveJob("priority");
	}

	public void OnRemoveSecondaryJob()
	{
		OnRemoveJob("secondary");
	}

	public void OnRemoveAbleJob()
	{
		OnRemoveJob("able");
	}

	private void OnAddJob(JOB_TYPE jobType, string identifier)
	{
		bool flag = false;
		ScrollRect scrollRect = null;
		if (identifier == "able")
		{
			scrollRect = ableJobsScrollRect;
			if (!_ableJobs.Contains(jobType))
			{
				_ableJobs.Add(jobType);
				flag = true;
			}
		}
		if (flag)
		{
			UnityEngine.Object.Instantiate(priorityJobBtnPrefab, scrollRect.content).GetComponent<PriorityJobButton>().SetJobType(jobType, identifier);
		}
	}

	public void OnRemoveJob(string identifier)
	{
		if (identifier == "able" && currentSelectedAbleJobButton != null)
		{
			JOB_TYPE jobType = currentSelectedAbleJobButton.jobType;
			if (_ableJobs.Remove(jobType))
			{
				UnityEngine.Object.Destroy(currentSelectedAbleJobButton.gameObject);
				currentSelectedAbleJobButton = null;
			}
		}
	}

	public void OnClickTraitsTab()
	{
		relatedStructuresGO.SetActive(value: false);
		traitsGO.SetActive(value: true);
		priorityJobsGO.SetActive(value: false);
		secondaryJobsGO.SetActive(value: false);
		ableJobsGO.SetActive(value: false);
	}

	public void OnClickRelatedStructuresTab()
	{
		relatedStructuresGO.SetActive(value: true);
		traitsGO.SetActive(value: false);
		priorityJobsGO.SetActive(value: false);
		secondaryJobsGO.SetActive(value: false);
		ableJobsGO.SetActive(value: false);
	}

	public void OnClickPriorityJobsTab()
	{
		relatedStructuresGO.SetActive(value: false);
		traitsGO.SetActive(value: false);
		priorityJobsGO.SetActive(value: true);
		secondaryJobsGO.SetActive(value: false);
		ableJobsGO.SetActive(value: false);
	}

	public void OnClickSecondaryJobsTab()
	{
		relatedStructuresGO.SetActive(value: false);
		traitsGO.SetActive(value: false);
		priorityJobsGO.SetActive(value: false);
		secondaryJobsGO.SetActive(value: true);
		ableJobsGO.SetActive(value: false);
	}

	public void OnClickAbleJobsTab()
	{
		relatedStructuresGO.SetActive(value: false);
		traitsGO.SetActive(value: false);
		priorityJobsGO.SetActive(value: false);
		secondaryJobsGO.SetActive(value: false);
		ableJobsGO.SetActive(value: true);
	}
}
