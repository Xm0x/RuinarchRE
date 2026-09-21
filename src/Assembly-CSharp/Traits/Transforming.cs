using UnityEngine;

namespace Traits;

public class Transforming : Status
{
	private GameObject _transformRevertEffectGO;

	private Character _owner;

	public override bool shouldBeLoadedInMainThread => true;

	public Transforming()
	{
		name = "Transforming";
		description = "Transforming";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (_owner != null)
		{
			_transformRevertEffectGO = GameManager.Instance.CreateParticleEffectAt(_owner, PARTICLE_EFFECT.Transform_Revert, allowRotation: false);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			_transformRevertEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Transform_Revert, allowRotation: false);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character && (bool)_transformRevertEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_transformRevertEffectGO);
			_transformRevertEffectGO = null;
		}
		_owner = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is Character poi)
		{
			if ((bool)_transformRevertEffectGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_transformRevertEffectGO);
				_transformRevertEffectGO = null;
			}
			_transformRevertEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Transform_Revert, allowRotation: false);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_transformRevertEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_transformRevertEffectGO);
			_transformRevertEffectGO = null;
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
