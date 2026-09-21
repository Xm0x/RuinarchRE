using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;

public class CharacterStateComponent : CharacterComponent
{
	private StrollOutsideState _strollOutsideState;

	private CombatState _combatState;

	public CharacterState currentState { get; private set; }

	public CharacterStateComponent()
	{
		_strollOutsideState = new StrollOutsideState(this);
		_combatState = new CombatState(this);
	}

	public CharacterStateComponent(SaveDataCharacterStateComponent data)
	{
		_strollOutsideState = new StrollOutsideState(this);
		_combatState = new CombatState(this);
	}

	public CharacterState SwitchToState(CHARACTER_STATE state)
	{
		if (!base.owner.limiterComponent.canPerform)
		{
			return null;
		}
		if (base.owner.currentActionNode != null)
		{
			base.owner.StopCurrentActionNode();
		}
		if ((bool)base.owner.marker && base.owner.marker.isMoving)
		{
			base.owner.marker.StopMovement();
		}
		if (currentState != null)
		{
			ExitCurrentState();
		}
		CharacterState characterState = CreateNewState(state);
		characterState.EnterState();
		return characterState;
	}

	public void OnTickEnded()
	{
		PerTickCurrentState();
	}

	public void SetCurrentState(CharacterState state)
	{
		if (currentState != state)
		{
			currentState = state;
			if ((bool)base.owner.marker)
			{
				base.owner.marker.UpdateActionIcon();
			}
		}
	}

	public void ExitCurrentState()
	{
		if (currentState == null)
		{
			throw new Exception(base.owner.name + " is trying to exit his/her current state but it is null");
		}
		if ((bool)base.owner.marker && base.owner.marker.isMoving)
		{
			base.owner.marker.StopMovement();
		}
		CharacterStateJob job = currentState.job;
		CharacterState characterState = currentState;
		characterState.ExitState();
		SetCurrentState(null);
		characterState.AfterExitingState();
		if (job != null && job.originalOwner != null)
		{
			if (job == base.owner.currentJob)
			{
				base.owner.SetCurrentJob(null);
			}
			job.ForceCancelJob();
		}
		if (characterState.characterState == CHARACTER_STATE.COMBAT)
		{
			List<Trait> traitOverrideFunctions = base.owner.traitContainer.GetTraitOverrideFunctions("After_Exiting_Combat");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					traitOverrideFunctions[i].OnAfterExitingCombat(base.owner);
				}
			}
			if (base.owner.isInWerewolfForm)
			{
				if (!base.owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Werewolf))
				{
					base.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Werewolf, base.owner);
				}
				else
				{
					base.owner.crimeComponent.FleeToAllNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(base.owner, CRIME_TYPE.Werewolf);
				}
			}
		}
		characterState.Reset();
	}

	private void PerTickCurrentState()
	{
		if (currentState != null && !currentState.isPaused && !currentState.isDone)
		{
			if (!base.owner.limiterComponent.canPerform)
			{
				ExitCurrentState();
			}
			else if (currentState.duration > 0 && currentState.currentDuration >= currentState.duration)
			{
				ExitCurrentState();
			}
			else
			{
				currentState.PerTickInState();
			}
		}
	}

	private CharacterState CreateNewState(CHARACTER_STATE state)
	{
		CharacterState result = null;
		switch (state)
		{
		case CHARACTER_STATE.STROLL_OUTSIDE:
			result = _strollOutsideState;
			break;
		case CHARACTER_STATE.COMBAT:
			result = _combatState;
			break;
		}
		return result;
	}

	public void LoadReferences(SaveDataCharacterStateComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_combatState?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_combatState?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
