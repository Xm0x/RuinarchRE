using Inner_Maps;
using Inner_Maps.Location_Structures;
using Interrupts;
using Object_Pools;

public class InterruptComponent : CharacterComponent
{
	public InterruptHolder currentInterrupt { get; private set; }

	public int currentDuration { get; private set; }

	public InterruptHolder triggeredSimultaneousInterrupt { get; private set; }

	public int currentSimultaneousInterruptDuration { get; private set; }

	public Log thoughtBubbleLog { get; private set; }

	public bool isInterrupted => currentInterrupt != null;

	public bool hasTriggeredSimultaneousInterrupt => triggeredSimultaneousInterrupt != null;

	public InterruptComponent()
	{
	}

	public InterruptComponent(SaveDataInterruptComponent data)
	{
		currentDuration = data.currentDuration;
		currentSimultaneousInterruptDuration = data.currentSimultaneousInterruptDuration;
	}

	public bool TriggerInterrupt(INTERRUPT interrupt, IPointOfInterest targetPOI, string identifier = "", ActualGoapNode actionThatTriggered = null, string reason = "")
	{
		Interrupt interruptData = InteractionManager.Instance.GetInterruptData(interrupt);
		if (!interruptData.isSimulateneous)
		{
			if (isInterrupted)
			{
				return false;
			}
			InterruptHolder interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
			interruptHolder.Initialize(interruptData, base.owner, targetPOI, identifier, reason);
			SetNonSimultaneousInterrupt(interruptHolder);
			CreateThoughtBubbleLog(interruptData);
			if ((object)base.owner.marker != null && base.owner.marker.isMoving && interruptData.shouldStopMovement)
			{
				base.owner.marker.StopMovement();
			}
			if (base.owner.hasMarker)
			{
				base.owner.marker.SetHasFleePath(state: false);
			}
			if (currentInterrupt.interrupt.doesDropCurrentJob)
			{
				base.owner.currentJob?.CancelJob();
			}
			if (currentInterrupt.interrupt.doesStopCurrentAction)
			{
				base.owner.currentJob?.StopJobNotDrop();
			}
			ExecuteStartInterrupt(currentInterrupt, actionThatTriggered);
			Messenger.Broadcast(InterruptSignals.INTERRUPT_STARTED, currentInterrupt);
			Messenger.Broadcast(UISignals.UPDATE_THOUGHT_BUBBLE, base.owner);
			if (currentInterrupt.interrupt.duration <= 0)
			{
				AddEffectLog(currentInterrupt);
				currentInterrupt.interrupt.ExecuteInterruptEndEffect(currentInterrupt);
				EndInterrupt();
			}
		}
		else
		{
			TriggeredSimultaneousInterrupt(interruptData, targetPOI, identifier, actionThatTriggered, reason);
		}
		return true;
	}

	private void TriggeredSimultaneousInterrupt(Interrupt interrupt, IPointOfInterest targetPOI, string identifier, ActualGoapNode actionThatTriggered, string reason)
	{
		InterruptHolder interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
		interruptHolder.Initialize(interrupt, base.owner, targetPOI, identifier, reason);
		ExecuteStartInterrupt(interruptHolder, actionThatTriggered);
		AddEffectLog(interruptHolder);
		interrupt.ExecuteInterruptEndEffect(interruptHolder);
		bool num = hasTriggeredSimultaneousInterrupt;
		SetSimultaneousInterrupt(interruptHolder);
		currentSimultaneousInterruptDuration = 0;
		if (!num)
		{
			Messenger.AddListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
		}
	}

	private void ExecuteStartInterrupt(InterruptHolder interruptHolder, ActualGoapNode actionThatTriggered)
	{
		Log overrideEffectLog = null;
		_ = interruptHolder.interrupt.type;
		interruptHolder.interrupt.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, actionThatTriggered);
		interruptHolder.SetCrimeType();
		if (overrideEffectLog == null || !overrideEffectLog.hasValue)
		{
			overrideEffectLog = interruptHolder.interrupt.CreateEffectLog(base.owner, interruptHolder.target);
		}
		if (overrideEffectLog != null && interruptHolder.interrupt.isIntel)
		{
			overrideEffectLog.AddTag(LOG_TAG.Intel);
		}
		interruptHolder.SetEffectLog(overrideEffectLog);
		InnerMapManager.Instance.FaceTarget(base.owner, interruptHolder.target);
	}

	public void OnTickEnded()
	{
		if (isInterrupted)
		{
			currentDuration++;
			if (currentDuration >= currentInterrupt.interrupt.duration)
			{
				AddEffectLog(currentInterrupt);
				currentInterrupt.interrupt.ExecuteInterruptEndEffect(currentInterrupt);
				EndInterrupt();
			}
			else
			{
				currentInterrupt.interrupt.PerTickInterrupt(currentInterrupt);
			}
		}
	}

	private void PerTickSimultaneousInterrupt()
	{
		if (hasTriggeredSimultaneousInterrupt)
		{
			currentSimultaneousInterruptDuration++;
			if (currentSimultaneousInterruptDuration > 2)
			{
				Messenger.RemoveListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
				SetSimultaneousInterrupt(null);
			}
		}
	}

	public void ForceEndNonSimultaneousInterrupt()
	{
		if (isInterrupted)
		{
			currentInterrupt.interrupt.OnForceEndInterrupt(currentInterrupt);
			EndInterrupt();
		}
	}

	public void ForceEndSimultaneousInterrupt()
	{
		if (hasTriggeredSimultaneousInterrupt)
		{
			currentSimultaneousInterruptDuration = 0;
			triggeredSimultaneousInterrupt.interrupt.OnForceEndInterrupt(currentInterrupt);
			Messenger.RemoveListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
			SetSimultaneousInterrupt(null);
		}
	}

	private void EndInterrupt()
	{
		if (currentInterrupt == null || currentInterrupt.interrupt == null)
		{
			return;
		}
		bool flag = currentInterrupt.interrupt.duration > 0;
		Interrupt interrupt = currentInterrupt.interrupt;
		SetNonSimultaneousInterrupt(null);
		currentDuration = 0;
		if (!base.owner.isDead && base.owner.limiterComponent.canPerform)
		{
			if (base.owner.combatComponent.isInCombat)
			{
				Messenger.Broadcast(CharacterSignals.DETERMINE_COMBAT_REACTION, base.owner);
			}
			else if (base.owner.combatComponent.hostilesInRange.Count > 0 || base.owner.combatComponent.avoidInRange.Count > 0)
			{
				if (!base.owner.jobQueue.HasJob(JOB_TYPE.COMBAT))
				{
					CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, base.owner);
					base.owner.jobQueue.AddJobInQueue(job);
				}
			}
			else if (flag)
			{
				if ((bool)base.owner.marker)
				{
					for (int i = 0; i < base.owner.marker.inVisionCharacters.Count; i++)
					{
						Character poi = base.owner.marker.inVisionCharacters[i];
						base.owner.marker.AddUnprocessedPOI(poi);
					}
				}
				base.owner.needsComponent.CheckExtremeNeeds(interrupt);
			}
		}
		if (thoughtBubbleLog != null)
		{
			LogPool.Release(thoughtBubbleLog);
		}
		thoughtBubbleLog = null;
		Messenger.Broadcast(CharacterSignals.INTERRUPT_FINISHED, interrupt.type, base.owner);
	}

	private void CreateThoughtBubbleLog(Interrupt interrupt)
	{
		if (LocalizationManager.Instance.HasLocalizedValue("Interrupts_Table", currentInterrupt.name + " thought_bubble"))
		{
			thoughtBubbleLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", currentInterrupt.name + " thought_bubble", interrupt.logTags);
			thoughtBubbleLog.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			thoughtBubbleLog.AddToFillers(currentInterrupt.target, currentInterrupt.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			interrupt.AddAdditionalFillersToThoughtLog(thoughtBubbleLog, base.owner);
		}
	}

	private void AddEffectLog(InterruptHolder interruptHolder)
	{
		if (interruptHolder.effectLog == null)
		{
			return;
		}
		if (interruptHolder.interrupt.ShouldAddLogs(interruptHolder) || interruptHolder.interrupt.shouldShowNotif)
		{
			interruptHolder.effectLog.AddLogToDatabase();
		}
		if (!interruptHolder.interrupt.shouldShowNotif)
		{
			return;
		}
		if (interruptHolder.interrupt.isIntel)
		{
			if (PlayerManager.Instance.player.ShouldShowNotificationFrom(base.owner))
			{
				PlayerManager.Instance.player.ShowNotificationFrom(base.owner, InteractionManager.Instance.CreateNewIntel(interruptHolder));
			}
		}
		else
		{
			PlayerManager.Instance.player.ShowNotificationFrom(base.owner, interruptHolder.effectLog);
		}
	}

	private void SetNonSimultaneousInterrupt(InterruptHolder interrupt)
	{
		if (currentInterrupt != interrupt)
		{
			if (currentInterrupt != null)
			{
				ObjectPoolManager.Instance.TryReturnInterruptToPool(currentInterrupt);
			}
			currentInterrupt = interrupt;
			if ((bool)base.owner.marker)
			{
				base.owner.marker.UpdateActionIcon();
			}
		}
	}

	private void SetSimultaneousInterrupt(InterruptHolder interrupt)
	{
		if (triggeredSimultaneousInterrupt != interrupt)
		{
			if (triggeredSimultaneousInterrupt != null)
			{
				ObjectPoolManager.Instance.TryReturnInterruptToPool(triggeredSimultaneousInterrupt);
			}
			triggeredSimultaneousInterrupt = interrupt;
			if (base.owner != null && base.owner.hasMarker)
			{
				base.owner.marker.UpdateActionIcon();
			}
		}
	}

	public void OnSeizedOwner()
	{
		if (isInterrupted && currentInterrupt.interrupt.shouldEndOnSeize)
		{
			ForceEndNonSimultaneousInterrupt();
		}
	}

	public bool NecromanticTransform()
	{
		if (CanNecromanticTransform() && base.owner.HasItem("Necronomicon"))
		{
			return base.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Necromantic_Transformation, base.owner);
		}
		return false;
	}

	private bool CanNecromanticTransform()
	{
		if (CharacterManager.Instance.necromancerInTheWorld == null && base.owner.characterClass.className != "Necromancer")
		{
			return base.owner.traitContainer.HasTrait("Evil", "Treacherous", "Demon Cultist");
		}
		return false;
	}

	public void LoadReferences(SaveDataInterruptComponent data)
	{
		if (!string.IsNullOrEmpty(data.currentInterruptID))
		{
			currentInterrupt = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.currentInterruptID);
		}
		if (!string.IsNullOrEmpty(data.triggeredSimultaneousInterruptID))
		{
			currentInterrupt = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.triggeredSimultaneousInterruptID);
		}
	}

	public void LoadReferencesInMainThread(SaveDataInterruptComponent data)
	{
		if (currentInterrupt != null)
		{
			CreateThoughtBubbleLog(currentInterrupt.interrupt);
		}
		if (triggeredSimultaneousInterrupt != null)
		{
			Messenger.AddListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (currentInterrupt != null)
		{
			currentInterrupt.DisconnectFromCharacter(p_character);
			if (currentInterrupt.IsCharacterReferenced(p_character) || currentInterrupt.IsImportantDataNull())
			{
				ForceEndNonSimultaneousInterrupt();
				ForceEndSimultaneousInterrupt();
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		currentInterrupt?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
