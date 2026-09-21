using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits;

public class Frozen : Status, IElementalTrait
{
	private GameObject _frozenEffect;

	public ITraitable traitable { get; private set; }

	public bool isPlayerSource { get; private set; }

	public GameDate lastFrostbiteStackDate { get; private set; }

	public override Type serializedData => typeof(SaveDataFrozen);

	public override bool shouldBeLoadedInMainThread => true;

	public Frozen()
	{
		name = "Frozen";
		description = "Encased in ice.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
		isStacking = true;
		moodEffect = -5;
		stackLimit = 1;
		stackModifier = 1f;
		hindersMovement = true;
		hindersPerform = true;
		hindersWitness = true;
		hindersSocials = true;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.EXTRACT_ITEM,
			INTERACTION_TYPE.REMOVE_FREEZING
		};
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataFrozen saveDataFrozen = saveDataTrait as SaveDataFrozen;
		isPlayerSource = saveDataFrozen.isPlayerSource;
		lastFrostbiteStackDate = saveDataFrozen.lastFrostbiteStackDate;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		traitable = addTo;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (traitable.gridTileLocation != null && traitable is IPointOfInterest pointOfInterest && !(pointOfInterest is GenericTileObject) && _frozenEffect == null)
		{
			_frozenEffect = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Frozen, allowRotation: false);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		traitable = addedTo;
		if (addedTo.gridTileLocation != null)
		{
			if (addedTo is IPointOfInterest pointOfInterest && !(pointOfInterest is GenericTileObject))
			{
				_frozenEffect = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Frozen, allowRotation: false);
			}
			if (addedTo is Character)
			{
				Character obj = addedTo as Character;
				obj.needsComponent.AdjustDoNotGetBored(1);
				obj.needsComponent.AdjustDoNotGetHungry(1);
				obj.needsComponent.AdjustDoNotGetTired(1);
				obj.needsComponent.AdjustDoNotGetDrained(1);
				Messenger.Broadcast(CharacterSignals.REPROCESS_POI, addedTo as IPointOfInterest);
			}
			if (addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand)
			{
				ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(3);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		traitable = null;
		if ((bool)_frozenEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
			_frozenEffect = null;
		}
		if (removedFrom is Character)
		{
			Character character = removedFrom as Character;
			character.needsComponent.AdjustDoNotGetBored(-1);
			character.needsComponent.AdjustDoNotGetHungry(-1);
			character.needsComponent.AdjustDoNotGetTired(-1);
			character.needsComponent.AdjustDoNotGetDrained(-1);
			DisablePlayerSourceChaosOrb(character);
		}
	}

	public override void OnRemoveStatusBySchedule(ITraitable removedFrom)
	{
		base.OnRemoveStatusBySchedule(removedFrom);
		if (ticksDuration < GameManager.Instance.GetTicksBasedOnHour(4))
		{
			return;
		}
		if (!lastFrostbiteStackDate.hasValue)
		{
			removedFrom.traitContainer.AddTrait(removedFrom, "Frostbite");
			lastFrostbiteStackDate = GameManager.Instance.Today();
			return;
		}
		GameDate gameDate = GameManager.Instance.Today();
		if (gameDate.GetHourDifference(lastFrostbiteStackDate) >= 4)
		{
			removedFrom.traitContainer.AddTrait(removedFrom, "Frostbite");
			lastFrostbiteStackDate = gameDate;
		}
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest pointOfInterest)
		{
			if ((bool)_frozenEffect)
			{
				ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
				_frozenEffect = null;
			}
			if (!(pointOfInterest is GenericTileObject))
			{
				_frozenEffect = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Frozen, allowRotation: false);
			}
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_frozenEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
			_frozenEffect = null;
		}
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public void SetIsPlayerSource(bool p_state)
	{
		if (isPlayerSource == p_state)
		{
			return;
		}
		isPlayerSource = p_state;
		if (traitable is Character p_owner)
		{
			if (isPlayerSource)
			{
				EnablePlayerSourceChaosOrb(p_owner);
			}
			else
			{
				DisablePlayerSourceChaosOrb(p_owner);
			}
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = traitable;
	}
}
