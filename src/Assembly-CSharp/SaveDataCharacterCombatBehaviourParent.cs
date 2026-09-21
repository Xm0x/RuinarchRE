public class SaveDataCharacterCombatBehaviourParent : SaveData<CharacterCombatBehaviourParent>
{
	public bool canDoTankBehaviour;

	public GameDate tankBehaviourActiveDate;

	public CHARACTER_COMBAT_BEHAVIOUR currentCombatBehaviour { get; private set; }

	public override void Save(CharacterCombatBehaviourParent data)
	{
		base.Save(data);
		canDoTankBehaviour = data.canDoTankBehaviour;
		tankBehaviourActiveDate = data.tankBehaviourActiveDate;
		currentCombatBehaviour = CHARACTER_COMBAT_BEHAVIOUR.None;
		if (data.currentCombatBehaviour != null)
		{
			currentCombatBehaviour = data.currentCombatBehaviour.behaviourType;
		}
	}

	public override CharacterCombatBehaviourParent Load()
	{
		return new CharacterCombatBehaviourParent(this);
	}
}
