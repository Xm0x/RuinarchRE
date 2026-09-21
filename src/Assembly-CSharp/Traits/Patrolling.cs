namespace Traits;

public class Patrolling : Status
{
	private Character _owner;

	public Patrolling()
	{
		name = "Patrolling";
		description = "This is Patrolling.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 160;
		isHidden = true;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERSONAL_PATROL, OnCharacterCanNoLongerPersonalPatrol);
			Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			character.behaviourComponent.AddBehaviourComponent(typeof(PatrolBehaviour));
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERSONAL_PATROL, OnCharacterCanNoLongerPersonalPatrol);
			Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
			_owner.behaviourComponent.SetCombatModeBeforePatrolling(_owner.combatComponent.combatMode);
			_owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			_owner = null;
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Behaviour", "CharacterBehaviour_Table", "Patrol_End", LOG_TAG.Work, null);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			character.behaviourComponent.RemoveBehaviourComponent(typeof(PatrolBehaviour));
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERSONAL_PATROL, OnCharacterCanNoLongerPersonalPatrol);
			Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
			character.combatComponent.SetCombatMode(character.behaviourComponent.combatModeBeforePatrolling);
		}
	}

	private void OnSeizePOI(IPointOfInterest poi)
	{
		if (poi == _owner)
		{
			_owner.traitContainer.RemoveTrait(_owner, this);
		}
	}

	private void OnCharacterCanNoLongerPersonalPatrol(Character character)
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

	private void OnJobAddedToQueue(JobQueueItem job, Character character)
	{
		if (_owner == character && job.priority > JOB_TYPE.PATROL.GetJobTypePriority())
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
