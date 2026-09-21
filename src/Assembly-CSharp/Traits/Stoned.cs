using UnityEngine;

namespace Traits;

public class Stoned : Status
{
	private GameObject _stonedGO;

	private Character _owner;

	public override bool shouldBeLoadedInMainThread => true;

	public Stoned()
	{
		name = "Stoned";
		description = "Has been turned into a stone.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		hindersMovement = true;
		hindersPerform = true;
		hindersWitness = true;
		hindersAttackTarget = true;
		hindersSocials = true;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (_owner != null && (bool)_owner.marker)
		{
			_owner.marker.PauseAnimation();
			if (_stonedGO == null)
			{
				_stonedGO = GameManager.Instance.CreateParticleEffectAt(_owner, PARTICLE_EFFECT.Stoned);
				_stonedGO.GetComponent<StonedEffect>().PlayEffect(_owner.marker.usedSprite);
			}
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			if (character.hasMarker)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", name + " effect", LOG_TAG.Life_Changes);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
				character.marker.UpdateAnimation();
				character.marker.ForceUpdateMarkerVisualsBasedOnAnimationName();
				_stonedGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Stoned);
				_stonedGO.GetComponent<StonedEffect>().PlayEffect(character.marker.usedSprite);
				character.marker.PauseAnimation();
			}
			UpdateCharacterAwarenessStateOnAddTrait(_owner);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if ((bool)_stonedGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_stonedGO);
			_stonedGO = null;
		}
		if (removedFrom is Character character && (bool)character.marker)
		{
			character.marker.UnpauseAnimation();
		}
		UpdateCharacterAwarenessStateOnRemoveTrait(_owner, removedBy);
		_owner = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is Character character)
		{
			if ((bool)_stonedGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_stonedGO);
				_stonedGO = null;
			}
			_stonedGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Stoned);
			_stonedGO.GetComponent<StonedEffect>().PlayEffect(character.marker.usedSprite);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_stonedGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_stonedGO);
			_stonedGO = null;
		}
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
