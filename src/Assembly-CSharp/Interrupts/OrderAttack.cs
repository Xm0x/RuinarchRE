namespace Interrupts;

public class OrderAttack : Interrupt
{
	public OrderAttack()
		: base(INTERRUPT.Order_Attack)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Hostile_Icon;
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
			if (character.race == RACE.SKELETON && !character.isDead && !character.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)))
			{
				character.behaviourComponent.SetAttackVillageTarget(interruptHolder.actor.necromancerTrait.attackVillageTarget);
				character.behaviourComponent.AddBehaviourComponent(typeof(AttackVillageBehaviour));
			}
		}
		return true;
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		if (LocalizationManager.Instance.HasLocalizedValue("Interrupts_Table", base.name + " effect"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect", base.logTags);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(actor.necromancerTrait.attackVillageTarget, actor.necromancerTrait.attackVillageTarget.name, LOG_IDENTIFIER.LANDMARK_1);
			return log;
		}
		return null;
	}
}
