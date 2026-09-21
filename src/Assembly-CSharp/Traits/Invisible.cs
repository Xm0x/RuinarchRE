using System;

namespace Traits;

public class Invisible : Status
{
	private IPointOfInterest _owner;

	public COMBAT_MODE originalCombatMode { get; private set; }

	public override Type serializedData => typeof(SaveDataInvisible);

	public Invisible()
	{
		name = "Invisible";
		description = "You can't see me.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataInvisible saveDataInvisible = saveDataTrait as SaveDataInvisible;
		originalCombatMode = saveDataInvisible.originalCombatMode;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is IPointOfInterest pointOfInterest)
		{
			_owner = pointOfInterest;
			if (pointOfInterest.mapObjectVisual != null)
			{
				pointOfInterest.mapObjectVisual.visionTrigger?.SetVisionTriggerCollidersState(state: false);
				pointOfInterest.mapObjectVisual.SetVisualAlpha(0.45f);
			}
			if (pointOfInterest is Character)
			{
				Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
				Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
				Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
				Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			}
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is IPointOfInterest pointOfInterest)
		{
			_owner = pointOfInterest;
			if (pointOfInterest.mapObjectVisual != null)
			{
				pointOfInterest.mapObjectVisual.visionTrigger?.SetVisionTriggerCollidersState(state: false);
				pointOfInterest.mapObjectVisual.SetVisualAlpha(0.45f);
			}
			if (pointOfInterest is Character character)
			{
				originalCombatMode = character.combatComponent.combatMode;
				character.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
				Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
				Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
				Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
				Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is IPointOfInterest pointOfInterest)
		{
			_owner = null;
			if (pointOfInterest.mapObjectVisual != null)
			{
				pointOfInterest.mapObjectVisual.visionTrigger?.SetVisionTriggerCollidersState(state: true);
				pointOfInterest.mapObjectVisual.SetVisualAlpha(1f);
			}
			if (pointOfInterest is Character character)
			{
				character.combatComponent.SetCombatMode(originalCombatMode);
				Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
				Messenger.RemoveListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
				Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
				Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			}
		}
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest pointOfInterest && pointOfInterest.mapObjectVisual != null)
		{
			pointOfInterest.mapObjectVisual.visionTrigger?.SetVisionTriggerCollidersState(state: false);
			pointOfInterest.mapObjectVisual.SetVisualAlpha(0.45f);
		}
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Invisible invisible)
		{
			originalCombatMode = invisible.originalCombatMode;
		}
	}

	private void OnCharacterStartedState(Character character, CharacterState state)
	{
		if (character == _owner && state is CombatState)
		{
			character.traitContainer.RemoveTrait(character, this);
		}
	}

	private void OnCharacterAdjustedHP(Character character, int amount, object source)
	{
		if (character == _owner && amount < 0 && source != character)
		{
			character.traitContainer.RemoveTrait(character, this);
		}
	}

	private void OnCharacterCanNoLongerMove(Character character)
	{
		if (character == _owner)
		{
			character.traitContainer.RemoveTrait(character, this);
		}
	}

	private void OnCharacterCanNoLongerPerform(Character character)
	{
		if (character == _owner)
		{
			character.traitContainer.RemoveTrait(character, this);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
