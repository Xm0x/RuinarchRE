using System;
using System.Collections.Generic;
using UnityEngine;

namespace Traits;

public class Catatonic : Status
{
	private float _chanceToRemove;

	private const int MaxDays = 4;

	public Character owner { get; private set; }

	public float chanceToRemove => _chanceToRemove;

	public override Type serializedData => typeof(SaveDataCatatonic);

	public Catatonic()
	{
		name = "Catatonic";
		description = "In an unresponsive stupor.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.FEED };
		hindersMovement = true;
		hindersWitness = true;
		hindersPerform = true;
		hindersSocials = true;
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataCatatonic saveDataCatatonic = saveDataTrait as SaveDataCatatonic;
		_chanceToRemove = saveDataCatatonic.chanceToRemove;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
			Messenger.AddListener(Signals.HOUR_STARTED, CheckRemovalChance);
			Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			owner = character;
			Messenger.AddListener(Signals.HOUR_STARTED, CheckRemovalChance);
			Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (sourceCharacter is Character)
		{
			Messenger.RemoveListener(Signals.HOUR_STARTED, CheckRemovalChance);
			Messenger.RemoveListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
			owner = null;
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		if (traitable is Character character)
		{
			CheckTrait(character);
		}
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Catatonic catatonic)
		{
			_chanceToRemove = catatonic.chanceToRemove;
		}
	}

	private void CheckTrait(Character owner)
	{
		if (!owner.CanPlanGoap() || !owner.carryComponent.IsNotBeingCarried() || owner.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER))
		{
			return;
		}
		if (owner.jobQueue.jobsInQueue.Count > 0)
		{
			JobQueueItem jobQueueItem = owner.jobQueue.jobsInQueue[0];
			if (jobQueueItem != null)
			{
				owner.PerformJob(jobQueueItem);
			}
		}
		else
		{
			PlanTirednessRecovery();
		}
	}

	private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status)
	{
		if (p_type == INTERACTION_TYPE.DROP && p_target == owner && owner.gridTileLocation.tileObjectComponent.objHere != null && owner.gridTileLocation.tileObjectComponent.objHere is Bed)
		{
			CreateActualSleepJob(owner.gridTileLocation.tileObjectComponent.objHere as Bed);
		}
	}

	private bool PlanTirednessRecovery()
	{
		if ((owner.needsComponent.isExhausted || owner.needsComponent.isTired) && !owner.HasJobTargetingThis(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT))
		{
			return CreateSleepJob();
		}
		return false;
	}

	private bool CreateSleepJob()
	{
		if (owner.homeStructure != null && owner.gridTileLocation.tileObjectComponent.objHere != null && owner.gridTileLocation.tileObjectComponent.objHere is Bed)
		{
			CreateActualSleepJob(owner.gridTileLocation.tileObjectComponent.objHere as Bed);
			return true;
		}
		return false;
	}

	private void CreateActualSleepJob(Bed bed)
	{
		JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
		if (owner.needsComponent.isExhausted)
		{
			jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
		}
		Spooked traitOrStatus = owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
		if (traitOrStatus == null || !traitOrStatus.TryTriggerFeelingSpooked(owner))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.SLEEP, new object[1] { ACTION_LOCATION_TYPE.IN_PLACE });
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	private void CheckRemovalChance()
	{
		_chanceToRemove = chanceToRemove + GetChanceIncreasePerHour();
		if (UnityEngine.Random.Range(0f, 100f) <= chanceToRemove)
		{
			owner.traitContainer.RemoveTrait(owner, this);
		}
	}

	private float GetChanceIncreasePerHour()
	{
		return 1.0416666f;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
