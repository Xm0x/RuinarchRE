using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Overheating : Status, IElementalTrait
{
	private GameObject _overheatingEffectGO;

	private readonly WeightedDictionary<string> weights;

	private IPointOfInterest _owner;

	public ITraitable traitable { get; private set; }

	public List<LocationStructure> excludedStructuresInSeekingShelter { get; private set; }

	public LocationStructure currentShelterStructure { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataOverheating);

	public override bool shouldBeLoadedInMainThread => true;

	public Overheating()
	{
		name = "Overheating";
		description = "Its temperature is burning up.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2);
		isStacking = true;
		moodEffect = -6;
		stackLimit = 3;
		stackModifier = 1.5f;
		excludedStructuresInSeekingShelter = new List<LocationStructure>();
		weights = new WeightedDictionary<string>();
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataOverheating saveDataOverheating = saveDataTrait as SaveDataOverheating;
		excludedStructuresInSeekingShelter = SaveUtilities.ConvertIDListToStructures(saveDataOverheating.excludedStructuresInSeekingShelter);
		if (!string.IsNullOrEmpty(saveDataOverheating.currentShelterStructure))
		{
			currentShelterStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataOverheating.currentShelterStructure);
		}
		isPlayerSource = saveDataOverheating.isPlayerSource;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		traitable = addTo;
		if (addTo is IPointOfInterest owner)
		{
			_owner = owner;
		}
		if (addTo is Character)
		{
			Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (_overheatingEffectGO == null)
		{
			_overheatingEffectGO = GameManager.Instance.CreateParticleEffectAt(_owner, PARTICLE_EFFECT.Overheating);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		traitable = addedTo;
		if (addedTo is IPointOfInterest owner)
		{
			_owner = owner;
		}
		if (addedTo is Character poi)
		{
			Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
			Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
			_overheatingEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Overheating);
		}
	}

	public override void OnStackStatus(ITraitable addedTo)
	{
		base.OnStackStatus(addedTo);
		if (addedTo is Character character)
		{
			int num = character.traitContainer.stacks[name];
			if (num >= 1 && num < stackLimit && !character.combatComponent.isInCombat)
			{
				character.jobComponent.TriggerSeekShelterJob();
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			if ((bool)_overheatingEffectGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_overheatingEffectGO);
				_overheatingEffectGO = null;
			}
			if (character.trapStructure.forcedStructure == currentShelterStructure)
			{
				character.trapStructure.SetForcedStructure(null);
			}
			Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
			Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		}
		_owner = null;
		traitable = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is Character poi)
		{
			if ((bool)_overheatingEffectGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_overheatingEffectGO);
				_overheatingEffectGO = null;
			}
			_overheatingEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Overheating);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_overheatingEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_overheatingEffectGO);
			_overheatingEffectGO = null;
		}
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (GameUtilities.RollChance(0.35f * (float)traitable.traitContainer.GetStacks(name)))
		{
			return OverheatingEffects();
		}
		return false;
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Overheating overheating)
		{
			excludedStructuresInSeekingShelter.AddRange(overheating.excludedStructuresInSeekingShelter);
			currentShelterStructure = overheating.currentShelterStructure;
		}
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public void AddExcludedStructureInSeekingShelter(LocationStructure structure)
	{
		excludedStructuresInSeekingShelter.Add(structure);
	}

	public bool IsStructureExludedInSeekingShelter(LocationStructure structure)
	{
		return excludedStructuresInSeekingShelter.Contains(structure);
	}

	public void SetCurrentShelterStructure(LocationStructure structure)
	{
		currentShelterStructure = structure;
	}

	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (traitable is Character character2 && character2 == character && currentShelterStructure != null && currentShelterStructure != structure)
		{
			AddExcludedStructureInSeekingShelter(currentShelterStructure);
			SetCurrentShelterStructure(null);
			character2.trapStructure.SetForcedStructure(null);
		}
	}

	private bool OverheatingEffects()
	{
		if (traitable is Character { isDead: false } character)
		{
			weights.Clear();
			if (!character.traitContainer.HasTrait("Unconscious"))
			{
				weights.AddElement("unconscious", 20);
			}
			weights.AddElement("heatstroke", 20);
			weights.AddElement("seizure", 20);
			switch (weights.PickRandomElementGivenWeights())
			{
			case "unconscious":
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "overheat_unconscious", LOG_TAG.Needs);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
				character.traitContainer.AddTrait(character, "Unconscious");
				return true;
			}
			case "heatstroke":
				return character.interruptComponent.TriggerInterrupt(INTERRUPT.Heatstroke_Death, character);
			case "seizure":
				return character.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, character);
			}
		}
		return false;
	}

	private void DisconnectFromStructure(LocationStructure p_structure)
	{
		excludedStructuresInSeekingShelter.Remove(p_structure);
		if (currentShelterStructure == p_structure)
		{
			SetCurrentShelterStructure(null);
		}
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isPlayerSource = p_state;
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		excludedStructuresInSeekingShelter.Contains(p_structure);
		_ = currentShelterStructure;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
		_ = traitable;
	}
}
