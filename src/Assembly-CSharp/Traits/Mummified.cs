using System.Collections.Generic;
using UnityEngine;

namespace Traits;

public class Mummified : Status
{
	private GameObject _particleEffectGO;

	public IPointOfInterest owner { get; private set; }

	public override bool shouldBeLoadedInMainThread => true;

	public Mummified()
	{
		name = "Mummified";
		description = "Preserved. Strange glowing markings appear around the body.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.ADORE_MUMMIFIED_CORPSE,
			INTERACTION_TYPE.CLEAN_MUMMIFIED_CORPSE,
			INTERACTION_TYPE.CHECK_OUT
		};
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
			_particleEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Mummified, allowRotation: false);
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
			_particleEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Mummified, allowRotation: false);
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
		_particleEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Mummified, allowRotation: false);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
