using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Cursed : Status
{
	private GameObject _particleEffectGO;

	public IPointOfInterest owner { get; private set; }

	public override bool shouldBeLoadedInMainThread => true;

	public Cursed()
	{
		name = "Cursed";
		description = "Is slowly dying but is unaware of it!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		hindersTirednessRecovery = true;
		AddTraitOverrideFunctionIdentifier("Tick_Ended_Trait");
		AddTraitOverrideFunctionIdentifier("Death_Trait");
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is IPointOfInterest pointOfInterest)
		{
			owner = pointOfInterest;
			if ((bool)_particleEffectGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_particleEffectGO);
				_particleEffectGO = null;
			}
			_particleEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Cursed, allowRotation: false);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if ((bool)_particleEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_particleEffectGO);
			_particleEffectGO = null;
		}
		owner = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest poi)
		{
			if ((bool)_particleEffectGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_particleEffectGO);
				_particleEffectGO = null;
			}
			_particleEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Cursed, allowRotation: false);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_particleEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_particleEffectGO);
			_particleEffectGO = null;
		}
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override void OnTickEnded(ITraitable traitable)
	{
		base.OnTickEnded(traitable);
		if (traitable is Character p_character)
		{
			CursedPerTick(p_character);
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is IPointOfInterest pointOfInterest)
		{
			owner = pointOfInterest;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if ((bool)_particleEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_particleEffectGO);
			_particleEffectGO = null;
		}
		_particleEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Cursed, allowRotation: false);
	}

	private void CursedPerTick(Character p_character)
	{
		if (p_character.isDead)
		{
			return;
		}
		int num = 10;
		p_character.AdjustHP(-num, ELEMENTAL_TYPE.Normal, triggerDeath: true, null, null, showHPBar: true, 0f, isPlayerSource: false, isTrueDamage: true);
		if (!p_character.isDead || !p_character.hasMarker || p_character.gridTileLocation == null)
		{
			return;
		}
		GameManager.Instance.CreateParticleEffectAt(p_character.gridTileLocation, PARTICLE_EFFECT.Landmine_Explosion);
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		p_character.gridTileLocation.PopulateTilesInRadius(list, 2, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (!character.isDead && character != p_character)
				{
					character.traitContainer.AddTrait(character, "Cursed");
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
