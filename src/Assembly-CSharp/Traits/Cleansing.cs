namespace Traits;

public class Cleansing : Status
{
	private Character _owner;

	public Cleansing()
	{
		name = "Cleansing";
		description = "This is Cleansing tiles.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
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
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			character.behaviourComponent.AddBehaviourComponent(typeof(CleanseTileBehaviour));
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			_owner = null;
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Behaviour", "CharacterBehaviour_Table", "Cleanse_Tile_End", LOG_TAG.Work, null);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			character.behaviourComponent.RemoveBehaviourComponent(typeof(CleanseTileBehaviour));
			character.behaviourComponent.SetCleansingTilesForSettlement(null);
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
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
		if (_owner == character && job.priority > JOB_TYPE.CLEANSE_TILES.GetJobTypePriority())
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
