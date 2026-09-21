using System;

public class CharacterStateJob : JobQueueItem
{
	public CHARACTER_STATE targetState { get; protected set; }

	public CharacterState assignedState { get; protected set; }

	public IPointOfInterest targetPOI { get; protected set; }

	public override IPointOfInterest poiTarget => targetPOI;

	public override OBJECT_TYPE objectType => OBJECT_TYPE.Job;

	public override Type serializedData => typeof(SaveDataCharacterStateJob);

	public void Initialize(JOB_TYPE jobType, CHARACTER_STATE state, IPointOfInterest targetPOI, IJobOwner owner)
	{
		Initialize(jobType, owner);
		targetState = state;
		this.targetPOI = targetPOI;
	}

	public void Initialize(JOB_TYPE jobType, CHARACTER_STATE state, IJobOwner owner)
	{
		Initialize(jobType, owner);
		targetState = state;
	}

	public void Initialize(SaveDataCharacterStateJob data)
	{
		Initialize((SaveDataJobQueueItem)data);
		targetState = data.targetState;
	}

	public override bool ProcessJob()
	{
		if (base.hasBeenReset)
		{
			return true;
		}
		if (targetState == CHARACTER_STATE.COMBAT)
		{
			if (base.assignedCharacter.combatComponent.hostilesInRange.Count > 0)
			{
				for (int i = 0; i < base.assignedCharacter.combatComponent.hostilesInRange.Count; i++)
				{
					IPointOfInterest pointOfInterest = base.assignedCharacter.combatComponent.hostilesInRange[i];
					if (!pointOfInterest.IsValidCombatTargetFor(base.assignedCharacter) && base.assignedCharacter.combatComponent.RemoveHostileInRange(pointOfInterest, processCombatBehavior: false))
					{
						i--;
					}
				}
			}
			if (base.assignedCharacter.combatComponent.hostilesInRange.Count <= 0 && base.assignedCharacter.combatComponent.avoidInRange.Count <= 0)
			{
				CancelJob();
				return true;
			}
		}
		if (assignedState == null)
		{
			CharacterState characterState = base.assignedCharacter.stateComponent.SwitchToState(targetState);
			if (base.hasBeenReset)
			{
				return true;
			}
			if (characterState != null && base.assignedCharacter.stateComponent.currentState == characterState)
			{
				SetAssignedState(characterState);
				base.assignedCharacter.SetCurrentJob(this);
				return true;
			}
			return false;
		}
		if (assignedState.isDone)
		{
			CancelJob();
			return true;
		}
		if (assignedState.isPaused)
		{
			assignedState.ResumeState();
			if (assignedState != null)
			{
				if (assignedState.isDone && base.assignedCharacter.currentJob == this)
				{
					base.assignedCharacter.SetCurrentJob(null);
				}
				return true;
			}
		}
		return base.ProcessJob();
	}

	public override void PushedBack(JobQueueItem jobThatPushedBack, bool shouldIncreasePushBackCount = true)
	{
		if (!base.cannotBePushedBack || jobThatPushedBack.jobType == JOB_TYPE.DIG_THROUGH)
		{
			if (assignedState != null)
			{
				assignedState.PauseState();
			}
			if (jobThatPushedBack.jobType != JOB_TYPE.DIG_THROUGH && shouldIncreasePushBackCount)
			{
				base.numOfTimesPushedBack++;
				if (base.numOfTimesPushedBack >= 3)
				{
					SetCannotBePushedBack(state: true);
				}
			}
		}
		else
		{
			CancelJob();
		}
	}

	public override void StopJobNotDrop()
	{
		if (base.cannotBePushedBack)
		{
			CancelJob();
		}
		else if (assignedState != null)
		{
			assignedState.PauseState();
		}
	}

	public override void UnassignJob(string reason)
	{
		base.UnassignJob(reason);
		if (base.assignedCharacter != null && assignedState != null && base.assignedCharacter.stateComponent.currentState == assignedState)
		{
			Character character = base.assignedCharacter;
			SetAssignedCharacter(null);
			SetAssignedState(null);
			character.stateComponent.ExitCurrentState();
		}
	}

	public override bool CancelJob(string reason = "", bool shouldBlacklist = false)
	{
		if (assignedState != null && assignedState.characterState == CHARACTER_STATE.COMBAT && base.assignedCharacter != null)
		{
			if (assignedState.isPaused && !assignedState.isDone)
			{
				assignedState.ResumeState();
			}
			base.assignedCharacter?.combatComponent.ClearHostilesInRange();
			base.assignedCharacter?.combatComponent.ClearAvoidInRange();
		}
		return base.CancelJob(reason);
	}

	public override void Reset()
	{
		base.Reset();
		targetState = CHARACTER_STATE.NONE;
		assignedState = null;
		targetPOI = null;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		_ = string.Empty;
		if (targetPOI == p_character || p_character == base.originalOwner || base.assignedCharacter == p_character)
		{
			ForceCancelJob();
			if (!base.hasBeenReset)
			{
				Reset();
			}
		}
	}

	public void SetAssignedState(CharacterState state)
	{
		state?.SetJob(this);
		if (assignedState != null)
		{
			assignedState.SetJob(null);
		}
		assignedState = state;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = targetPOI;
	}
}
