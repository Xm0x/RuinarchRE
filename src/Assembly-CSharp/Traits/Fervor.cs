using UnityEngine;

namespace Traits;

public class Fervor : Status
{
	private GameObject _fervorGO;

	private IPointOfInterest _owner;

	public override bool shouldBeLoadedInMainThread => true;

	public Fervor()
	{
		name = "Fervor";
		description = "Intensely focused on something. All Needs reduction are temporarily paused.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		moodEffect = 20;
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is IPointOfInterest owner)
		{
			_owner = owner;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (_fervorGO == null)
		{
			_fervorGO = GameManager.Instance.CreateParticleEffectAt(_owner, PARTICLE_EFFECT.Fervor);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is IPointOfInterest pointOfInterest)
		{
			_owner = pointOfInterest;
			_fervorGO = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Fervor);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if ((bool)_fervorGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_fervorGO);
			_fervorGO = null;
		}
		_owner = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest poi)
		{
			if ((bool)_fervorGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_fervorGO);
				_fervorGO = null;
			}
			_fervorGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Fervor);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_fervorGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_fervorGO);
			_fervorGO = null;
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
