using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TraitPanelUI : MonoBehaviour
{
	public static TraitPanelUI Instance;

	public InputField nameInput;

	public InputField descriptionInput;

	public InputField thoughtInput;

	public InputField moodInput;

	public InputField durationInput;

	public Dropdown traitTypeOptions;

	public Dropdown traitEffectOptions;

	public Dropdown traitTriggerOptions;

	public Dropdown advertisedInteractionOptions;

	public Dropdown crimeSeverityOptions;

	public Dropdown elementOptions;

	public TextMeshProUGUI advertisedInteractionsText;

	public InputField amountInput;

	public InputField traitEffectDescriptionInput;

	public Dropdown statOptions;

	public Dropdown requirementCheckerOptions;

	public Dropdown requirementTargetOptions;

	public Dropdown requirementDamageIdentifierOptions;

	public Dropdown requirementTypeOptions;

	public Dropdown requirementSeparatorOptions;

	public Dropdown requirementOptions;

	public Toggle percentageToggle;

	public Toggle hasRequirementToggle;

	public Toggle isNotToggle;

	public Toggle isHiddenToggle;

	public Toggle isTangibleToggle;

	public ScrollRect requirementsScrollRect;

	public ScrollRect effectsScrollRect;

	public GameObject traitEffectBtnGO;

	public GameObject requirementBtnGO;

	public GameObject requirementsParentGO;

	public InputField mutuallyExclusiveInput;

	[NonSerialized]
	public TraitEffectButton currentSelectedTraitEffectButton;

	[NonSerialized]
	public RequirementButton currentSelectedRequirementButton;

	public Toggle hindersSocialsToggle;

	public Toggle isStackingToggle;

	public GameObject stackGroupGO;

	public InputField stackLimitInput;

	public InputField stackModInput;

	private List<string> _allTraits;

	private List<TraitEffect> _effects;

	private List<string> _requirements;

	private List<INTERACTION_TYPE> _advertisedInteractions;

	public List<string> allTraits => _allTraits;

	private void Awake()
	{
		Instance = this;
	}

	private void UpdateTraits()
	{
		_allTraits.Clear();
		string[] files = Directory.GetFiles(Utilities.coreStreamingDataPath + "/Traits/", "*.json");
		foreach (string path in files)
		{
			_allTraits.Add(Path.GetFileNameWithoutExtension(path));
		}
		CharacterPanelUI.Instance.UpdateTraitOptions();
		ClassPanelUI.Instance.UpdateTraitOptions();
		RacePanelUI.Instance.UpdateTraitOptions();
	}

	public void LoadAllData()
	{
		_allTraits = new List<string>();
		_requirements = new List<string>();
		_effects = new List<TraitEffect>();
		_advertisedInteractions = new List<INTERACTION_TYPE>();
		UpdateAdvertisedInteractionsText();
		statOptions.ClearOptions();
		traitTypeOptions.ClearOptions();
		traitEffectOptions.ClearOptions();
		elementOptions.ClearOptions();
		traitTriggerOptions.ClearOptions();
		advertisedInteractionOptions.ClearOptions();
		crimeSeverityOptions.ClearOptions();
		requirementTypeOptions.ClearOptions();
		requirementOptions.ClearOptions();
		requirementTargetOptions.ClearOptions();
		requirementCheckerOptions.ClearOptions();
		requirementDamageIdentifierOptions.ClearOptions();
		requirementSeparatorOptions.ClearOptions();
		string[] names = Enum.GetNames(typeof(STAT));
		string[] names2 = Enum.GetNames(typeof(TRAIT_TYPE));
		string[] names3 = Enum.GetNames(typeof(TRAIT_EFFECT));
		string[] names4 = Enum.GetNames(typeof(ELEMENTAL_TYPE));
		string[] names5 = Enum.GetNames(typeof(TRAIT_TRIGGER));
		string[] names6 = Enum.GetNames(typeof(INTERACTION_TYPE));
		string[] names7 = Enum.GetNames(typeof(CRIME_SEVERITY));
		string[] names8 = Enum.GetNames(typeof(TRAIT_REQUIREMENT));
		string[] names9 = Enum.GetNames(typeof(TRAIT_REQUIREMENT_TARGET));
		string[] names10 = Enum.GetNames(typeof(TRAIT_REQUIREMENT_CHECKER));
		string[] names11 = Enum.GetNames(typeof(DAMAGE_IDENTIFIER));
		string[] names12 = Enum.GetNames(typeof(TRAIT_REQUIREMENT_SEPARATOR));
		statOptions.AddOptions(names.ToList());
		traitTypeOptions.AddOptions(names2.ToList());
		traitEffectOptions.AddOptions(names3.ToList());
		elementOptions.AddOptions(names4.ToList());
		traitTriggerOptions.AddOptions(names5.ToList());
		advertisedInteractionOptions.AddOptions(names6.ToList());
		crimeSeverityOptions.AddOptions(names7.ToList());
		requirementTypeOptions.AddOptions(names8.ToList());
		requirementTargetOptions.AddOptions(names9.ToList());
		requirementCheckerOptions.AddOptions(names10.ToList());
		requirementDamageIdentifierOptions.AddOptions(names11.ToList());
		requirementSeparatorOptions.AddOptions(names12.ToList());
		OnRequirementTypeChange(requirementTypeOptions.value);
		UpdateTraits();
	}

	private void ClearData()
	{
		currentSelectedTraitEffectButton = null;
		currentSelectedRequirementButton = null;
		statOptions.value = 0;
		traitTypeOptions.value = 0;
		traitEffectOptions.value = 0;
		elementOptions.value = 0;
		traitTriggerOptions.value = 0;
		advertisedInteractionOptions.value = 0;
		crimeSeverityOptions.value = 0;
		requirementTypeOptions.value = 0;
		requirementOptions.value = 0;
		requirementTargetOptions.value = 0;
		requirementCheckerOptions.value = 0;
		requirementDamageIdentifierOptions.value = 0;
		requirementSeparatorOptions.value = 0;
		nameInput.text = string.Empty;
		descriptionInput.text = string.Empty;
		thoughtInput.text = string.Empty;
		traitEffectDescriptionInput.text = string.Empty;
		mutuallyExclusiveInput.text = string.Empty;
		amountInput.text = "0";
		durationInput.text = "0";
		stackLimitInput.text = "0";
		stackModInput.text = "0";
		percentageToggle.isOn = false;
		hasRequirementToggle.isOn = false;
		isNotToggle.isOn = false;
		isHiddenToggle.isOn = false;
		isTangibleToggle.isOn = false;
		isStackingToggle.isOn = false;
		hindersSocialsToggle.isOn = false;
		_effects.Clear();
		_requirements.Clear();
		_advertisedInteractions.Clear();
		UpdateAdvertisedInteractionsText();
		Utilities.DestroyChildren(effectsScrollRect.content);
		Utilities.DestroyChildren(requirementsScrollRect.content);
	}

	private void UpdateAdvertisedInteractionsText()
	{
		if (_advertisedInteractions.Count > 0)
		{
			string[] array = new string[_advertisedInteractions.Count];
			for (int i = 0; i < _advertisedInteractions.Count; i++)
			{
				array[i] = _advertisedInteractions[i].ToString();
			}
			advertisedInteractionsText.text = Utilities.ConvertArrayToString(array, ',');
		}
		else
		{
			advertisedInteractionsText.text = string.Empty;
		}
	}

	private void SaveTrait()
	{
	}

	private void SaveTraitJson(string path)
	{
		string empty = string.Empty;
		TRAIT_TYPE tRAIT_TYPE = (TRAIT_TYPE)Enum.Parse(typeof(TRAIT_TYPE), traitTypeOptions.options[traitTypeOptions.value].text);
		empty = ((tRAIT_TYPE != TRAIT_TYPE.STATUS) ? JsonUtility.ToJson(new Trait
		{
			name = nameInput.text,
			description = descriptionInput.text,
			thoughtText = thoughtInput.text,
			type = tRAIT_TYPE,
			effect = (TRAIT_EFFECT)Enum.Parse(typeof(TRAIT_EFFECT), traitEffectOptions.options[traitEffectOptions.value].text),
			elementalType = (ELEMENTAL_TYPE)Enum.Parse(typeof(ELEMENTAL_TYPE), elementOptions.options[elementOptions.value].text),
			ticksDuration = int.Parse(durationInput.text),
			isHidden = isHiddenToggle.isOn,
			mutuallyExclusive = GetMutuallyExclusiveTraits(),
			advertisedInteractions = _advertisedInteractions,
			moodEffect = int.Parse(moodInput.text)
		}) : JsonUtility.ToJson(new Status
		{
			name = nameInput.text,
			description = descriptionInput.text,
			thoughtText = thoughtInput.text,
			type = tRAIT_TYPE,
			effect = (TRAIT_EFFECT)Enum.Parse(typeof(TRAIT_EFFECT), traitEffectOptions.options[traitEffectOptions.value].text),
			elementalType = (ELEMENTAL_TYPE)Enum.Parse(typeof(ELEMENTAL_TYPE), elementOptions.options[elementOptions.value].text),
			ticksDuration = int.Parse(durationInput.text),
			isHidden = isHiddenToggle.isOn,
			isTangible = isTangibleToggle.isOn,
			hindersSocials = hindersSocialsToggle.isOn,
			mutuallyExclusive = GetMutuallyExclusiveTraits(),
			advertisedInteractions = _advertisedInteractions,
			moodEffect = int.Parse(moodInput.text),
			isStacking = isStackingToggle.isOn,
			stackLimit = int.Parse(stackLimitInput.text),
			stackModifier = float.Parse(stackModInput.text)
		}));
		StreamWriter streamWriter = new StreamWriter(path, append: false);
		streamWriter.WriteLine(empty);
		streamWriter.Close();
		Debug.Log("Successfully saved trait at " + path);
		UpdateTraits();
	}

	private void LoadTrait()
	{
	}

	private void LoadTraitToUI(Trait trait)
	{
		nameInput.text = trait.name;
		descriptionInput.text = trait.description;
		thoughtInput.text = trait.thoughtText;
		traitTypeOptions.value = GetOptionIndex(trait.type.ToString(), traitTypeOptions);
		traitEffectOptions.value = GetOptionIndex(trait.effect.ToString(), traitEffectOptions);
		elementOptions.value = GetOptionIndex(trait.elementalType.ToString(), elementOptions);
		durationInput.text = trait.ticksDuration.ToString();
		mutuallyExclusiveInput.text = ConvertMutuallyExclusiveTraitsToText(trait);
		_advertisedInteractions = trait.advertisedInteractions;
		moodInput.text = trait.moodEffect.ToString();
		isHiddenToggle.isOn = trait.isHidden;
		UpdateAdvertisedInteractionsText();
	}

	private void LoadStatusToUI(Status status)
	{
		nameInput.text = status.name;
		descriptionInput.text = status.description;
		thoughtInput.text = status.thoughtText;
		traitTypeOptions.value = GetOptionIndex(status.type.ToString(), traitTypeOptions);
		traitEffectOptions.value = GetOptionIndex(status.effect.ToString(), traitEffectOptions);
		elementOptions.value = GetOptionIndex(status.elementalType.ToString(), elementOptions);
		durationInput.text = status.ticksDuration.ToString();
		mutuallyExclusiveInput.text = ConvertMutuallyExclusiveTraitsToText(status);
		_advertisedInteractions = status.advertisedInteractions;
		moodInput.text = status.moodEffect.ToString();
		isHiddenToggle.isOn = status.isHidden;
		hindersSocialsToggle.isOn = status.hindersSocials;
		isTangibleToggle.isOn = status.isTangible;
		isStackingToggle.isOn = status.isStacking;
		stackLimitInput.text = status.stackLimit.ToString();
		stackModInput.text = status.stackModifier.ToString();
		UpdateAdvertisedInteractionsText();
	}

	private void PopulateRequirements(List<string> requirements)
	{
		if (requirements != null)
		{
			requirementOptions.ClearOptions();
			requirementOptions.AddOptions(requirements);
		}
	}

	private int GetOptionIndex(string identifier, Dropdown options)
	{
		for (int i = 0; i < options.options.Count; i++)
		{
			if (options.options[i].text.Equals(identifier, StringComparison.CurrentCultureIgnoreCase))
			{
				return i;
			}
		}
		return 0;
	}

	private List<string> GetCombatRequirementsByType(TRAIT_REQUIREMENT requirementType)
	{
		return requirementType switch
		{
			TRAIT_REQUIREMENT.RACE => Enum.GetNames(typeof(RACE)).ToList(), 
			TRAIT_REQUIREMENT.TRAIT => _allTraits, 
			TRAIT_REQUIREMENT.ROLE => Enum.GetNames(typeof(CHARACTER_ROLE)).ToList(), 
			_ => null, 
		};
	}

	public void OnRequirementTypeChange(int index)
	{
		TRAIT_REQUIREMENT requirementType = (TRAIT_REQUIREMENT)Enum.Parse(typeof(TRAIT_REQUIREMENT), requirementTypeOptions.options[index].text);
		List<string> combatRequirementsByType = GetCombatRequirementsByType(requirementType);
		PopulateRequirements(combatRequirementsByType);
	}

	public void OnToggleRequirement(bool state)
	{
		RequirementOptionsActivation(state);
	}

	private void RequirementOptionsActivation(bool state)
	{
		requirementsParentGO.SetActive(state);
	}

	public void OnClickAddNewAttribute()
	{
		ClearData();
	}

	public void OnClickEditAttribute()
	{
		LoadTrait();
	}

	public void OnClickSaveAttribute()
	{
		SaveTrait();
	}

	public void OnClickAddRequirement()
	{
		string text = requirementOptions.options[requirementOptions.value].text;
		if (!_requirements.Contains(text))
		{
			_requirements.Add(text);
			UnityEngine.Object.Instantiate(requirementBtnGO, requirementsScrollRect.content).GetComponent<RequirementButton>().SetRequirement(text);
		}
	}

	public void OnClickRemoveRequirement()
	{
		if (currentSelectedRequirementButton != null)
		{
			string requirement = currentSelectedRequirementButton.requirement;
			if (_requirements.Remove(requirement))
			{
				UnityEngine.Object.Destroy(currentSelectedRequirementButton.gameObject);
				currentSelectedRequirementButton = null;
			}
		}
	}

	public void OnClickAddTraitEffect()
	{
		TraitEffect traitEffect = new TraitEffect
		{
			stat = (STAT)Enum.Parse(typeof(STAT), statOptions.options[statOptions.value].text),
			amount = float.Parse(amountInput.text),
			isPercentage = percentageToggle.isOn,
			target = (TRAIT_REQUIREMENT_TARGET)Enum.Parse(typeof(TRAIT_REQUIREMENT_TARGET), requirementTargetOptions.options[requirementTargetOptions.value].text),
			checker = (TRAIT_REQUIREMENT_CHECKER)Enum.Parse(typeof(TRAIT_REQUIREMENT_CHECKER), requirementCheckerOptions.options[requirementCheckerOptions.value].text),
			damageIdentifier = (DAMAGE_IDENTIFIER)Enum.Parse(typeof(DAMAGE_IDENTIFIER), requirementDamageIdentifierOptions.options[requirementDamageIdentifierOptions.value].text),
			description = traitEffectDescriptionInput.text,
			hasRequirement = hasRequirementToggle.isOn,
			isNot = isNotToggle.isOn,
			requirementType = (TRAIT_REQUIREMENT)Enum.Parse(typeof(TRAIT_REQUIREMENT), requirementTypeOptions.options[requirementTypeOptions.value].text),
			requirementSeparator = (TRAIT_REQUIREMENT_SEPARATOR)Enum.Parse(typeof(TRAIT_REQUIREMENT_SEPARATOR), requirementSeparatorOptions.options[requirementSeparatorOptions.value].text),
			requirements = new List<string>(_requirements)
		};
		_effects.Add(traitEffect);
		UnityEngine.Object.Instantiate(traitEffectBtnGO, effectsScrollRect.content).GetComponent<TraitEffectButton>().SetTraitEffect(traitEffect);
	}

	public void OnClickRemoveTraitEffect()
	{
		if (currentSelectedTraitEffectButton != null)
		{
			TraitEffect traitEffect = currentSelectedTraitEffectButton.traitEffect;
			if (_effects.Remove(traitEffect))
			{
				UnityEngine.Object.Destroy(currentSelectedTraitEffectButton.gameObject);
				currentSelectedTraitEffectButton = null;
			}
		}
	}

	public void OnClickAddAdvertisedInteraction()
	{
		string text = advertisedInteractionOptions.options[advertisedInteractionOptions.value].text;
		INTERACTION_TYPE item = (INTERACTION_TYPE)Enum.Parse(typeof(INTERACTION_TYPE), text);
		if (!_advertisedInteractions.Contains(item))
		{
			_advertisedInteractions.Add(item);
			UpdateAdvertisedInteractionsText();
		}
	}

	public void OnClickRemoveAdvertisedInteraction()
	{
		string text = advertisedInteractionOptions.options[advertisedInteractionOptions.value].text;
		INTERACTION_TYPE item = (INTERACTION_TYPE)Enum.Parse(typeof(INTERACTION_TYPE), text);
		if (_advertisedInteractions.Remove(item))
		{
			UpdateAdvertisedInteractionsText();
		}
	}

	private string[] GetMutuallyExclusiveTraits()
	{
		return Utilities.ConvertStringToArray(mutuallyExclusiveInput.text, ',');
	}

	private string ConvertMutuallyExclusiveTraitsToText(Trait trait)
	{
		return Utilities.ConvertArrayToString(trait.mutuallyExclusive, ',');
	}

	public void OnToggleIsStacking(bool state)
	{
		stackGroupGO.SetActive(state);
	}
}
