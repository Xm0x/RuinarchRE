using System;
using System.Collections.Generic;

namespace Traits;

public class Ensnared : Status, IElementalTrait
{
	public Character owner { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataEnsnared);

	public Ensnared()
	{
		name = "Ensnared";
		description = "Trapped and unable to move.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
		hindersMovement = true;
		hindersPerform = true;
		moodEffect = -5;
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.REMOVE_ENSNARED };
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataEnsnared saveDataEnsnared = saveDataTrait as SaveDataEnsnared;
		isPlayerSource = saveDataEnsnared.isPlayerSource;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		owner = addTo as Character;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		owner = addedTo as Character;
		if (addedTo is IPointOfInterest arg)
		{
			Messenger.Broadcast(CharacterSignals.REPROCESS_POI, arg);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		DisablePlayerSourceChaosOrb(owner);
		owner = null;
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public void SetIsPlayerSource(bool p_state)
	{
		if (isPlayerSource != p_state)
		{
			isPlayerSource = p_state;
			if (isPlayerSource)
			{
				EnablePlayerSourceChaosOrb(owner);
			}
			else
			{
				DisablePlayerSourceChaosOrb(owner);
			}
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
