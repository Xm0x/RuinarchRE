using Object_Pools;

public class CharacterState
{
	public CharacterStateComponent stateComponent { get; protected set; }

	public string stateName { get; protected set; }

	public CHARACTER_STATE characterState { get; protected set; }

	public int duration { get; protected set; }

	public int currentDuration { get; protected set; }

	public bool isDone { get; protected set; }

	public bool hasStarted { get; protected set; }

	public bool isPaused { get; protected set; }

	public Log thoughtBubbleLog { get; protected set; }

	public CharacterStateJob job { get; protected set; }

	public string actionIconString { get; protected set; }

	protected CharacterState(CharacterStateComponent characterComp)
	{
		stateComponent = characterComp;
		actionIconString = GoapActionStateDB.No_Icon;
	}

	protected virtual void StartState()
	{
		hasStarted = true;
		currentDuration = 0;
		stateComponent.SetCurrentState(this);
		CreateStartStateLog();
		CreateThoughtBubbleLog();
		DoMovementBehavior();
		Messenger.Broadcast(CharacterSignals.CHARACTER_STARTED_STATE, stateComponent.owner, this);
		if (characterState.IsCombatState() && (bool)stateComponent.owner.marker)
		{
			stateComponent.owner.marker.visionColliderComponent.TransferAllDifferentStructureCharacters();
		}
		ProcessInVisionPOIsOnStartState();
	}

	protected virtual void EndState()
	{
		isDone = true;
	}

	public virtual void PerTickInState()
	{
		currentDuration++;
	}

	protected virtual void DoMovementBehavior()
	{
	}

	protected virtual bool ProcessInVisionPOIsOnStartState()
	{
		if (stateComponent.owner.marker.inVisionPOIs.Count > 0)
		{
			return true;
		}
		return false;
	}

	public virtual void AfterExitingState()
	{
		Messenger.Broadcast(CharacterSignals.CHARACTER_ENDED_STATE, stateComponent.owner, this);
	}

	public virtual void PauseState()
	{
		if (!isPaused)
		{
			isPaused = true;
			if (stateComponent.currentState == this)
			{
				stateComponent.SetCurrentState(null);
			}
			if (stateComponent.owner.currentJob == job)
			{
				stateComponent.owner.SetCurrentJob(null);
			}
			Messenger.Broadcast(CharacterSignals.CHARACTER_PAUSED_STATE, stateComponent.owner, this);
		}
	}

	public virtual void ResumeState()
	{
		if (!isDone && isPaused)
		{
			isPaused = false;
			if (stateComponent.currentState != this)
			{
				stateComponent.SetCurrentState(this);
			}
			if (stateComponent.owner.currentJob != job)
			{
				stateComponent.owner.SetCurrentJob(job);
			}
			DoMovementBehavior();
		}
	}

	private void CreateThoughtBubbleLog()
	{
		if (LocalizationManager.Instance.HasLocalizedValue("CharacterStates_Table", stateName + " thought_bubble"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "CharacterState", "CharacterStates_Table", stateName + " thought_bubble", (characterState == CHARACTER_STATE.COMBAT) ? LOG_TAG.Combat : LOG_TAG.Work);
			log.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			SetThoughtBubbleLog(log);
		}
	}

	protected void SetThoughtBubbleLog(Log p_log)
	{
		if (thoughtBubbleLog != null)
		{
			LogPool.Release(thoughtBubbleLog);
		}
		thoughtBubbleLog = p_log;
	}

	public virtual void Reset()
	{
		currentDuration = 0;
		isDone = false;
		hasStarted = false;
		isPaused = false;
		job = null;
	}

	public void EnterState()
	{
		if (!isDone)
		{
			StartState();
		}
	}

	public void ExitState()
	{
		EndState();
	}

	public void SetJob(CharacterStateJob job)
	{
		this.job = job;
	}

	private void CreateStartStateLog()
	{
		if (LocalizationManager.Instance.HasLocalizedValue("CharacterStates_Table", stateName + " start"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "CharacterState", "CharacterStates_Table", stateName + " start", (characterState == CHARACTER_STATE.COMBAT) ? LOG_TAG.Combat : LOG_TAG.Work);
			log.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
	}

	public override string ToString()
	{
		return stateName + " by " + stateComponent.owner.name + " with job : " + (job?.name ?? "None");
	}
}
