using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;
using UnityEngine;

namespace Traits;

public class Paralyzed : Status
{
	private WeightedDictionary<INTERACTION_TYPE> _happinessRecoveryWeightSystem;

	public Character owner { get; private set; }

	public Paralyzed()
	{
		name = "Paralyzed";
		description = "Unable to move.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.FEED };
		ticksDuration = 0;
		hindersMovement = true;
		hindersPerform = true;
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
		_happinessRecoveryWeightSystem = new WeightedDictionary<INTERACTION_TYPE>(3);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
			Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			owner = character;
			Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
			if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Paralyzed) && character.homeSettlement != null && PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(character.homeSettlement) && !character.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && character.homeSettlement.eventManager.CanHaveEvents())
			{
				character.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
			}
			PlayerManager.Instance?.player?.retaliationComponent.OnCharacterDisabled(owner);
			UpdateCharacterAwarenessStateOnAddTrait(owner);
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (owner != null)
		{
			Messenger.RemoveListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
			UpdateCharacterAwarenessStateOnRemoveTrait(owner, removedBy);
			owner = null;
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		CheckParalyzedTrait();
	}

	private void CheckParalyzedTrait()
	{
		if (!owner.marker || owner.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER))
		{
			return;
		}
		if (owner.jobQueue.jobsInQueue.Count > 0)
		{
			if (owner.CanPerformEndTickJobs())
			{
				JobQueueItem jobQueueItem = owner.jobQueue.jobsInQueue[0];
				if (jobQueueItem != null)
				{
					owner.PerformJob(jobQueueItem);
				}
			}
		}
		else if (!PlanTirednessRecovery())
		{
			PlanHappinessRecovery();
		}
	}

	private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status)
	{
		if (p_type == INTERACTION_TYPE.DROP && p_target == owner)
		{
			if (owner.gridTileLocation.tileObjectComponent.objHere != null && owner.gridTileLocation.tileObjectComponent.objHere is Bed)
			{
				CreateSleepOnBedJob(owner.gridTileLocation.tileObjectComponent.objHere as Bed);
			}
			else if (owner.gridTileLocation.structure == owner.homeStructure)
			{
				CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
			}
			else
			{
				CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
			}
		}
	}

	private bool PlanHappinessRecovery()
	{
		if ((owner.needsComponent.isSulking || owner.needsComponent.isBored) && !owner.HasJobTargetingThis(JOB_TYPE.HAPPINESS_RECOVERY) && !owner.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY) && !owner.traitContainer.HasTrait("Abstain Happiness"))
		{
			return CreateDaydreamOrPrayOrSingJob();
		}
		return false;
	}

	private bool CreateDaydreamOrPrayOrSingJob()
	{
		_happinessRecoveryWeightSystem.Clear();
		if (owner.currentRegion.IsResident(owner))
		{
			if (owner.homeStructure != null && owner.currentRegion.HasStructure(STRUCTURE_TYPE.WILDERNESS))
			{
				if (owner.currentStructure == owner.homeStructure)
				{
					_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.PRAY, 50);
					_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.SING, 50);
				}
				else if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS)
				{
					_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.DAYDREAM, 50);
					_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.SING, 50);
				}
				else
				{
					_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.DAYDREAM, 50);
					_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.SING, 50);
					_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.PRAY, 50);
				}
			}
			else
			{
				_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.DAYDREAM, 50);
				_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.SING, 50);
				_happinessRecoveryWeightSystem.AddElement(INTERACTION_TYPE.PRAY, 50);
			}
			if (owner.traitContainer.HasTrait("Devout") && _happinessRecoveryWeightSystem.HasElement(INTERACTION_TYPE.PRAY))
			{
				_happinessRecoveryWeightSystem.SetElementWeight(INTERACTION_TYPE.PRAY, 150);
			}
			if (owner.traitContainer.HasTrait("Music Lover"))
			{
				_happinessRecoveryWeightSystem.SetElementWeight(INTERACTION_TYPE.SING, 150);
			}
			else if (owner.traitContainer.HasTrait("Music Hater"))
			{
				_happinessRecoveryWeightSystem.SetElementWeight(INTERACTION_TYPE.SING, 0);
			}
			INTERACTION_TYPE iNTERACTION_TYPE = _happinessRecoveryWeightSystem.PickRandomElementGivenWeights();
			if (iNTERACTION_TYPE == INTERACTION_TYPE.SING || iNTERACTION_TYPE == INTERACTION_TYPE.DAYDREAM || iNTERACTION_TYPE == INTERACTION_TYPE.PRAY)
			{
				CreateActualHappinessRecoveryJob(iNTERACTION_TYPE);
				return true;
			}
		}
		return false;
	}

	private bool CreatePrayJob()
	{
		if (owner.homeStructure == null || owner.currentStructure == owner.homeStructure)
		{
			CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
			return true;
		}
		return false;
	}

	private bool CreateDaydreamJob()
	{
		if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || !owner.currentRegion.HasStructure(STRUCTURE_TYPE.WILDERNESS))
		{
			CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
			return true;
		}
		return false;
	}

	private void CreateActualHappinessRecoveryJob(INTERACTION_TYPE actionType)
	{
		bool flag = false;
		Heartbroken traitOrStatus = owner.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
		if (traitOrStatus != null)
		{
			flag = Random.Range(0, 100) < 25 * owner.traitContainer.stacks[traitOrStatus.name];
		}
		if (!flag)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, actionType, owner, owner);
			goapPlanJob.SetDoNotRecalculate(state: true);
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		else
		{
			traitOrStatus.TriggerBrokenhearted();
		}
	}

	private bool PlanTirednessRecovery()
	{
		if ((owner.needsComponent.isExhausted || owner.needsComponent.isTired) && !owner.HasJobTargetingThis(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT) && !owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT) && !owner.traitContainer.HasTrait("Abstain Tiredness"))
		{
			return CreateSleepJob();
		}
		return false;
	}

	private bool CreateSleepJob()
	{
		bool flag = false;
		if (owner.homeStructure != null)
		{
			if (owner.gridTileLocation.tileObjectComponent.objHere != null && owner.gridTileLocation.tileObjectComponent.objHere is Bed)
			{
				if (CreateSleepOnBedJob(owner.gridTileLocation.tileObjectComponent.objHere as Bed))
				{
					return true;
				}
			}
			else
			{
				flag = true;
			}
		}
		if (owner.needsComponent.isExhausted && CreateSleepOutsideJob())
		{
			return true;
		}
		if (flag && owner.needsComponent.isTired)
		{
			owner.traitContainer.AddTrait(owner, "Abstain Tiredness");
		}
		return false;
	}

	private bool CreateSleepOnBedJob(Bed bed)
	{
		JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
		if (owner.needsComponent.isExhausted)
		{
			jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
		}
		Spooked traitOrStatus = owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
		if (traitOrStatus == null || !traitOrStatus.TryTriggerFeelingSpooked(owner))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	private bool CreateSleepOutsideJob()
	{
		JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
		if (owner.needsComponent.isExhausted)
		{
			jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
		}
		Spooked traitOrStatus = owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
		if (traitOrStatus == null || !traitOrStatus.TryTriggerFeelingSpooked(owner))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP_OUTSIDE, owner, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
