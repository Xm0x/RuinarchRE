using Inner_Maps.Location_Structures;

public class LimiterComponent : CharacterComponent
{
	public int canWitnessValue { get; private set; }

	public int canMoveValue { get; private set; }

	public int canBeAttackedValue { get; private set; }

	public int canPerformValue { get; private set; }

	public int canTakeJobsValue { get; private set; }

	public int sociableValue { get; private set; }

	public int canDoFullnessRecoveryValue { get; private set; }

	public int canDoHappinessRecoveryValue { get; private set; }

	public int canDoTirednessRecoveryValue { get; private set; }

	public int targetedByDemonicSnatchValue { get; private set; }

	public int hinderAfflictionVotes { get; private set; }

	public bool canWitness => canWitnessValue >= 0;

	public bool canMove => canMoveValue >= 0;

	public bool canBeAttacked => canBeAttackedValue >= 0;

	public bool canPerform => canPerformValue >= 0;

	public bool canTakeJobs => canTakeJobsValue >= 0;

	public bool isSociable => sociableValue >= 0;

	public bool canDoFullnessRecovery => canDoFullnessRecoveryValue >= 0;

	public bool canDoHappinessRecovery => canDoHappinessRecoveryValue >= 0;

	public bool canDoTirednessRecovery => canDoTirednessRecoveryValue >= 0;

	public bool isTargetedByDemonicSnatch => targetedByDemonicSnatchValue > 0;

	public bool canBeAfflicted => hinderAfflictionVotes <= 0;

	public LimiterComponent()
	{
	}

	public LimiterComponent(SaveDataLimiterComponent data)
	{
		ApplyDataFromSave(data);
	}

	public bool IsIncapacitated()
	{
		if (canPerform)
		{
			return !canMove;
		}
		return true;
	}

	public void IncreaseCanWitness()
	{
		canWitnessValue++;
	}

	public void DecreaseCanWitness()
	{
		canWitnessValue--;
	}

	public void IncreaseCanMove()
	{
		bool num = !canMove;
		canMoveValue++;
		if (num && canMove)
		{
			OnCharacterCanMoveAgain();
			Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, base.owner);
		}
	}

	public void DecreaseCanMove()
	{
		bool num = canMove;
		canMoveValue--;
		if (num && !canMove)
		{
			base.owner.partyComponent.UnfollowBeacon();
			OnCharacterCanNoLongerMove();
			Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, base.owner);
		}
	}

	public void IncreaseCanBeAttacked()
	{
		canBeAttackedValue++;
	}

	public void DecreaseCanBeAttacked()
	{
		canBeAttackedValue--;
	}

	public void IncreaseCanPerform()
	{
		bool num = !canPerform;
		canPerformValue++;
		if (num && canPerform)
		{
			OnCharacterCanPerformAgain();
			Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, base.owner);
		}
	}

	public void DecreaseCanPerform()
	{
		bool num = canPerform;
		canPerformValue--;
		if (num && !canPerform)
		{
			base.owner.partyComponent.UnfollowBeacon();
			OnCharacterCanNoLongerPerform();
			Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, base.owner);
		}
	}

	public void IncreaseCanTakeJobs()
	{
		canTakeJobsValue++;
	}

	public void DecreaseCanTakeJobs()
	{
		canTakeJobsValue--;
	}

	public void IncreaseSociable()
	{
		sociableValue++;
	}

	public void DecreaseSociable()
	{
		sociableValue--;
	}

	public void IncreaseCanDoFullnessRecovery()
	{
		canDoFullnessRecoveryValue++;
	}

	public void DecreaseCanDoFullnessRecovery()
	{
		canDoFullnessRecoveryValue--;
	}

	public void IncreaseCanDoHappinessRecovery()
	{
		canDoHappinessRecoveryValue++;
	}

	public void DecreaseCanDoHappinessRecovery()
	{
		canDoHappinessRecoveryValue--;
	}

	public void IncreaseCanDoTirednessRecovery()
	{
		canDoTirednessRecoveryValue++;
	}

	public void DecreaseCanDoTirednessRecovery()
	{
		canDoTirednessRecoveryValue--;
	}

	public void IncreaseTargetedByDemonicSnatch()
	{
		targetedByDemonicSnatchValue++;
	}

	public void DecreaseTargetedByDemonicSnatch()
	{
		targetedByDemonicSnatchValue--;
	}

	public void VoteToHinderAfflictions()
	{
		hinderAfflictionVotes++;
	}

	public void VoteToAllowAfflictions()
	{
		hinderAfflictionVotes--;
	}

	private void OnCharacterCanPerformAgain()
	{
		base.owner.needsComponent.CheckExtremeNeeds();
		if (base.owner.hasMarker)
		{
			for (int i = 0; i < base.owner.marker.inVisionPOIs.Count; i++)
			{
				IPointOfInterest poi = base.owner.marker.inVisionPOIs[i];
				base.owner.marker.AddUnprocessedPOI(poi);
			}
		}
		base.owner.homeStructure?.partyStructureComponent?.OnResidentCanPerformAgain(base.owner);
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.FEED, (IPointOfInterest)base.owner);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)base.owner);
	}

	private void OnCharacterCanNoLongerPerform()
	{
		if (base.owner.isDead)
		{
			return;
		}
		if (!base.owner.interruptComponent.isInterrupted || (base.owner.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap && base.owner.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap_Short && base.owner.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap_Medium && base.owner.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap_Long))
		{
			if (base.owner.currentActionNode != null && base.owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && base.owner.currentActionNode.action.goapType.IsRestingAction())
			{
				base.owner.CancelAllJobsExceptForCurrent();
			}
			else
			{
				base.owner.jobQueue.CancelAllJobs();
			}
		}
		if (base.owner.hasMarker)
		{
			base.owner.marker.StopMovement();
			base.owner.marker.pathfindingAI.ClearAllCurrentPathData();
		}
		base.owner.reactionComponent.SetIsHidden(state: false);
		base.owner.UncarryPOI();
		if (base.owner.traitContainer.HasTrait("Unconscious"))
		{
			base.owner.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.KNOCKOUT);
		}
		base.owner.behaviourComponent.ClearAttackVillageData();
		base.owner.traitContainer.RemoveTrait(base.owner, "Polymorphed");
		if (base.owner.behaviourComponent.HasBehaviour(typeof(DazedBehaviour)))
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Dazed");
		}
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)base.owner);
	}

	private void OnCharacterCanNoLongerMove()
	{
		base.owner.reactionComponent.SetIsHidden(state: false);
		base.owner.jobComponent.TryStartScreamCheck();
	}

	private void OnCharacterCanMoveAgain()
	{
		base.owner.jobComponent.TryStopScreamCheck();
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.FEED, (IPointOfInterest)base.owner);
	}

	public void ApplyDataFromSave(SaveDataLimiterComponent data)
	{
		canWitnessValue = data.canWitnessValue;
		canMoveValue = data.canMoveValue;
		canBeAttackedValue = data.canBeAttackedValue;
		canPerformValue = data.canPerformValue;
		canTakeJobsValue = data.canTakeJobsValue;
		sociableValue = data.sociableValue;
		canDoFullnessRecoveryValue = data.canDoFullnessRecoveryValue;
		canDoHappinessRecoveryValue = data.canDoHappinessRecoveryValue;
		canDoTirednessRecoveryValue = data.canDoTirednessRecoveryValue;
		targetedByDemonicSnatchValue = data.targetedByDemonicSnatchValue;
		hinderAfflictionVotes = data.hinderAfflictionVotes;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
