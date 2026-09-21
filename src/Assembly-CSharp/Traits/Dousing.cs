namespace Traits;

public class Dousing : Status
{
	private Character _owner;

	public Dousing()
	{
		name = "Dousing";
		description = "This is Dousing fires.";
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
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			character.behaviourComponent.AddBehaviourComponent(typeof(DouseFireBehaviour));
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(DouseFireBehaviour));
			character.behaviourComponent.SetDouseFireSettlement(null);
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
			_owner = null;
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
		if (_owner == character && job.priority > JOB_TYPE.DOUSE_FIRE.GetJobTypePriority())
		{
			character.traitContainer.RemoveTrait(character, this);
		}
	}

	private void OnJobRemovedFromQueue(JobQueueItem job, Character character)
	{
		if (_owner != character || job.jobType != JOB_TYPE.DOUSE_FIRE || job.finishedSuccessfully || character.isBeingSeized)
		{
			return;
		}
		bool flag = false;
		if (character.hasMarker)
		{
			for (int i = 0; i < character.marker.inVisionPOIs.Count; i++)
			{
				if (character.marker.inVisionPOIs[i].traitContainer.HasTrait("Burning"))
				{
					flag = true;
					break;
				}
			}
		}
		character.traitContainer.RemoveTrait(character, this);
		if (flag)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Panicking, character, "", null, "Panicking_Fire");
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
