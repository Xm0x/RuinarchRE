using Object_Pools;
using UnityEngine;

namespace Interrupts;

public class ReduceConflict : Interrupt
{
	public ReduceConflict()
		: base(INTERRUPT.Reduce_Conflict)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character character = interruptHolder.target as Character;
		Character randomAliveEnemyCharacterThatIsNot = character.relationshipContainer.GetRandomAliveEnemyCharacterThatIsNot(interruptHolder.actor);
		if (randomAliveEnemyCharacterThatIsNot != null)
		{
			string text = "reduce_conflict";
			if (Random.Range(0, 2) == 0 && randomAliveEnemyCharacterThatIsNot.traitContainer.HasTrait("Hothead"))
			{
				text = "reduce_conflict_rebuffed";
			}
			else
			{
				character.relationshipContainer.AdjustOpinion(character, randomAliveEnemyCharacterThatIsNot, "Base", 15);
			}
			if (overrideEffectLog != null)
			{
				LogPool.Release(overrideEffectLog);
			}
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " " + text, base.logTags);
			overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			overrideEffectLog.AddToFillers(randomAliveEnemyCharacterThatIsNot, randomAliveEnemyCharacterThatIsNot.name, LOG_IDENTIFIER.CHARACTER_3);
		}
		return true;
	}
}
