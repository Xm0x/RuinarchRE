using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

namespace Traits;

public class Freezing : Status, IElementalTrait
{
	private GameObject _freezingGO;

	public ITraitable traitable { get; private set; }

	public List<LocationStructure> excludedStructuresInSeekingShelter { get; private set; }

	public LocationStructure currentShelterStructure { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataFreezing);

	public override bool shouldBeLoadedInMainThread => true;

	public Freezing()
	{
		name = "Freezing";
		description = "May be completely Frozen soon.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
		isStacking = true;
		moodEffect = -5;
		stackLimit = 3;
		stackModifier = 1f;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.REMOVE_FREEZING,
			INTERACTION_TYPE.TAKE_SHELTER
		};
		excludedStructuresInSeekingShelter = new List<LocationStructure>();
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataFreezing saveDataFreezing = saveDataTrait as SaveDataFreezing;
		excludedStructuresInSeekingShelter = SaveUtilities.ConvertIDListToStructures(saveDataFreezing.excludedStructuresInSeekingShelter);
		if (!string.IsNullOrEmpty(saveDataFreezing.currentShelterStructure))
		{
			currentShelterStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataFreezing.currentShelterStructure);
		}
		isPlayerSource = saveDataFreezing.isPlayerSource;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		traitable = addTo;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (traitable is Character poi)
		{
			if (_freezingGO == null)
			{
				_freezingGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Freezing);
			}
			Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		}
		else if (traitable is IPointOfInterest poi2 && _freezingGO == null)
		{
			_freezingGO = GameManager.Instance.CreateParticleEffectAt(poi2, PARTICLE_EFFECT.Freezing_Object);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		traitable = addedTo;
		if (addedTo is Character character)
		{
			_freezingGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Freezing);
			character.needsComponent.AdjustTirednessDecreaseRate(1f);
			character.movementComponent.AdjustSpeedModifier(-0.15f);
			Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
			Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		}
		else if (addedTo is IPointOfInterest poi)
		{
			_freezingGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Freezing_Object);
		}
		if (addedTo.gridTileLocation != null && (addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand))
		{
			ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(3);
		}
	}

	public override void OnStackStatus(ITraitable addedTo)
	{
		base.OnStackStatus(addedTo);
		if (addedTo is Character)
		{
			Character character = addedTo as Character;
			character.movementComponent.AdjustSpeedModifier(-0.15f);
			int num = character.traitContainer.stacks[name];
			if (num >= 1 && num < stackLimit && !character.combatComponent.isInCombat)
			{
				character.jobComponent.TriggerSeekShelterJob();
			}
		}
	}

	public override void OnUnstackStatus(ITraitable addedTo, bool bySchedule)
	{
		base.OnUnstackStatus(addedTo, bySchedule);
		if (addedTo is Character)
		{
			(addedTo as Character).movementComponent.AdjustSpeedModifier(0.15f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if ((bool)_freezingGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_freezingGO);
			_freezingGO = null;
		}
		if (removedFrom is Character character)
		{
			character.needsComponent.AdjustTirednessDecreaseRate(-1f);
			character.movementComponent.AdjustSpeedModifier(0.15f);
			if (character.trapStructure.forcedStructure == currentShelterStructure)
			{
				character.trapStructure.SetForcedStructure(null);
			}
			Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
			Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		}
		traitable = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest pointOfInterest)
		{
			if ((bool)_freezingGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_freezingGO);
				_freezingGO = null;
			}
			PARTICLE_EFFECT particle = PARTICLE_EFFECT.Freezing_Object;
			if (pointOfInterest is Character)
			{
				particle = PARTICLE_EFFECT.Freezing;
			}
			_freezingGO = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, particle);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_freezingGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_freezingGO);
			_freezingGO = null;
		}
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Freezing freezing)
		{
			excludedStructuresInSeekingShelter.AddRange(freezing.excludedStructuresInSeekingShelter);
			currentShelterStructure = freezing.currentShelterStructure;
		}
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	private void AddExcludedStructureInSeekingShelter(LocationStructure structure)
	{
		excludedStructuresInSeekingShelter.Add(structure);
	}

	public bool IsStructureExcludedInSeekingShelter(LocationStructure structure)
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
		_ = traitable;
	}
}
