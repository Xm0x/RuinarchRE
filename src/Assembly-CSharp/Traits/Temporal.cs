namespace Traits;

public class Temporal : Status
{
	private Character _traitOwner;

	public override bool shouldBeLoadedInMainThread => true;

	public Temporal()
	{
		name = "Temporal";
		description = "Is only temporary.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = true;
		stackLimit = 1;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character traitOwner)
		{
			_traitOwner = traitOwner;
			_traitOwner.SetDestroyMarkerOnDeath(state: true);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character traitOwner)
		{
			_traitOwner = traitOwner;
			_traitOwner.SetDestroyMarkerOnDeath(state: true);
			Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		_traitOwner = null;
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	private void OnCharacterGainedTrait(Character p_character, Trait p_trait)
	{
		if (_traitOwner == p_character && !p_character.isDead && (p_trait is Unconscious || p_trait is Paralyzed || p_trait is Restrained || p_trait is Ensnared))
		{
			_traitOwner.SetDestroyMarkerOnDeath(state: true);
			_traitOwner.Death();
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _traitOwner;
	}
}
