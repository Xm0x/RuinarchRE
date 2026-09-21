using Characters.Components;

namespace Traits;

public class Tending : Status, CharacterEventDispatcher.ICarryListener
{
	private Character _owner;

	private bool _hasTendedAtLeastOnce;

	public bool hasTendedAtLeastOnce => _hasTendedAtLeastOnce;

	public Tending()
	{
		name = "Tending";
		description = "This is Tending.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			_owner = character;
			Messenger.AddListener<Character, ActualGoapNode>(JobSignals.CHARACTER_DOING_ACTION, OnActionStarted);
			Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
			character.eventDispatcher.SubscribeToCharacterCarried(this);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			character.behaviourComponent.AddBehaviourComponent(typeof(TendFarmBehaviour));
			Messenger.AddListener<Character, ActualGoapNode>(JobSignals.CHARACTER_DOING_ACTION, OnActionStarted);
			Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
			character.eventDispatcher.SubscribeToCharacterCarried(this);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			if (_hasTendedAtLeastOnce)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Behaviour", "CharacterBehaviour_Table", "Tend_Farm_End", LOG_TAG.Work, null);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
			character.behaviourComponent.RemoveBehaviourComponent(typeof(TendFarmBehaviour));
			Messenger.RemoveListener<Character, ActualGoapNode>(JobSignals.CHARACTER_DOING_ACTION, OnActionStarted);
			Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
			character.eventDispatcher.UnsubscribeToCharacterCarried(this);
			_owner = null;
		}
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Tending tending)
		{
			_hasTendedAtLeastOnce = tending.hasTendedAtLeastOnce;
		}
	}

	private void OnActionStarted(Character character, ActualGoapNode goapNode)
	{
		if (_owner == character)
		{
			if (goapNode.action.goapType != INTERACTION_TYPE.TEND && goapNode.action.goapType != INTERACTION_TYPE.START_TEND)
			{
				_owner.traitContainer.RemoveTrait(_owner, this);
			}
			else if (goapNode.action.goapType == INTERACTION_TYPE.TEND)
			{
				_hasTendedAtLeastOnce = true;
			}
		}
	}

	private void OnCharacterStartedState(Character character, CharacterState state)
	{
		if (character == _owner)
		{
			_owner.traitContainer.RemoveTrait(_owner, this);
		}
	}

	public void OnCharacterCarried(Character p_character, Character p_carriedBy)
	{
		p_character.traitContainer.RemoveTrait(p_character, this);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
