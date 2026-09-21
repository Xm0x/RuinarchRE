using UnityEngine;

namespace Traits;

public class BurningAtStake : Status
{
	private GameObject burningEffect;

	private Character owner { get; set; }

	public override bool shouldBeLoadedInMainThread => true;

	public BurningAtStake()
	{
		name = "Burning At Stake";
		description = "Burning to death!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		isHidden = true;
		hindersSocials = true;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (burningEffect == null)
		{
			burningEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Burning, allowRotation: false);
		}
		if (owner.hasMarker)
		{
			owner.marker.ShowAdditionalEffect(CharacterManager.Instance.stakeEffect);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		if (addedTo is Character character)
		{
			owner = character;
			burningEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Burning, allowRotation: false);
			if ((bool)owner.marker)
			{
				owner.marker.ShowAdditionalEffect(CharacterManager.Instance.stakeEffect);
			}
		}
		base.OnAddTrait(addedTo);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if ((bool)burningEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(burningEffect);
			burningEffect = null;
		}
		if ((bool)owner.marker && owner.marker.IsShowingAdditionEffectImage(CharacterManager.Instance.stakeEffect))
		{
			owner.marker.HideAdditionalEffect();
		}
		owner = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if ((bool)burningEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(burningEffect);
			burningEffect = null;
		}
		if (traitable is Character character)
		{
			burningEffect = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Burning, allowRotation: false);
			if ((bool)character.marker)
			{
				character.marker.ShowAdditionalEffect(CharacterManager.Instance.stakeEffect);
			}
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)burningEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(burningEffect);
			burningEffect = null;
		}
		if (traitable is Character character && (bool)character.marker && character.marker.IsShowingAdditionEffectImage(CharacterManager.Instance.stakeEffect))
		{
			character.marker.HideAdditionalEffect();
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
