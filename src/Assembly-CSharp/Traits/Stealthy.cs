namespace Traits;

public class Stealthy : Trait
{
	private Character _owner;

	public Stealthy()
	{
		name = "Stealthy";
		description = "Usually invisible outside of combat.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			if (character.limiterComponent.canMove && character.limiterComponent.canPerform && !character.isDead && !(character.stateComponent.currentState is CombatState))
			{
				character.traitContainer.AddTrait(character, "Invisible");
			}
			Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
			Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character)
		{
			_owner = null;
			Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		}
	}

	private void OnCharacterEndedState(Character character, CharacterState state)
	{
		if (character == _owner && state is CombatState)
		{
			character.traitContainer.AddTrait(character, "Invisible");
		}
	}

	private void OnCharacterCanMoveAgain(Character character)
	{
		if (character == _owner && !(character.stateComponent.currentState is CombatState))
		{
			character.traitContainer.AddTrait(character, "Invisible");
		}
	}

	private void OnCharacterCanPerformAgain(Character character)
	{
		if (character == _owner && !(character.stateComponent.currentState is CombatState))
		{
			character.traitContainer.AddTrait(character, "Invisible");
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
