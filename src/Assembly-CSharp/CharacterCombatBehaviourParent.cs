public class CharacterCombatBehaviourParent
{
	public CharacterCombatBehaviour currentCombatBehaviour { get; private set; }

	public bool canDoTankBehaviour { get; private set; }

	public GameDate tankBehaviourActiveDate { get; private set; }

	public CharacterCombatBehaviourParent()
	{
		canDoTankBehaviour = true;
	}

	public CharacterCombatBehaviourParent(SaveDataCharacterCombatBehaviourParent data)
	{
		canDoTankBehaviour = data.canDoTankBehaviour;
		tankBehaviourActiveDate = data.tankBehaviourActiveDate;
	}

	private void SetCombatBehaviour(CharacterCombatBehaviour p_combatBehaviour, Character owner)
	{
		if (currentCombatBehaviour != p_combatBehaviour)
		{
			CharacterCombatBehaviour characterCombatBehaviour = currentCombatBehaviour;
			currentCombatBehaviour = p_combatBehaviour;
			characterCombatBehaviour?.UnsetAsCombatBehaviourOf(owner);
			if (currentCombatBehaviour != null)
			{
				currentCombatBehaviour.SetAsCombatBehaviourOf(owner);
			}
		}
	}

	public void SetCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_behaviourType, Character owner)
	{
		CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(p_behaviourType);
		SetCombatBehaviour(combatBehaviour, owner);
	}

	public bool IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_behaviourType)
	{
		CharacterCombatBehaviour characterCombatBehaviour = currentCombatBehaviour;
		if (characterCombatBehaviour == null)
		{
			return false;
		}
		return characterCombatBehaviour.behaviourType == p_behaviourType;
	}

	public bool TryDoCombatBehaviour(Character p_character, CombatState p_combatState)
	{
		if (currentCombatBehaviour != null)
		{
			return currentCombatBehaviour.DetermineCombatBehaviour(p_character, p_combatState);
		}
		return false;
	}

	public void SetCanDoTankBehaviour(bool p_state)
	{
		if (canDoTankBehaviour == p_state)
		{
			return;
		}
		canDoTankBehaviour = p_state;
		if (!canDoTankBehaviour)
		{
			tankBehaviourActiveDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(1));
			SchedulingManager.Instance.AddEntry(tankBehaviourActiveDate, delegate
			{
				SetCanDoTankBehaviour(p_state: true);
			}, null);
		}
	}

	public void LoadReferences(SaveDataCharacterCombatBehaviourParent data)
	{
		currentCombatBehaviour = CombatManager.Instance.GetCombatBehaviour(data.currentCombatBehaviour);
		if (!canDoTankBehaviour)
		{
			SchedulingManager.Instance.AddEntry(tankBehaviourActiveDate, delegate
			{
				SetCanDoTankBehaviour(p_state: true);
			}, null);
		}
	}
}
