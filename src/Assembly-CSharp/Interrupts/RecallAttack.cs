namespace Interrupts;

public class RecallAttack : Interrupt
{
	public RecallAttack()
		: base(INTERRUPT.Recall_Attack)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Combat,
			LOG_TAG.Work
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		for (int i = 0; i < interruptHolder.actor.faction.characters.Count; i++)
		{
			Character character = interruptHolder.actor.faction.characters[i];
			if (character.race == RACE.SKELETON && !character.isDead)
			{
				character.behaviourComponent.SetAttackVillageTarget(null);
				character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
			}
		}
		return true;
	}
}
