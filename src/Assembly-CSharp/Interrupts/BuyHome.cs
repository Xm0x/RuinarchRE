using Inner_Maps.Location_Structures;
using Object_Pools;

namespace Interrupts;

public class BuyHome : Interrupt
{
	public BuyHome()
		: base(INTERRUPT.Buy_Home)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		LocationStructure homeStructure = actor.homeStructure;
		GenericTileObject genericTileObject = interruptHolder.target as GenericTileObject;
		actor.MigrateHomeStructureTo(genericTileObject.gridTileLocation.structure);
		actor.moneyComponent.AdjustCoins(-50);
		if (actor.homeStructure != null && actor.homeStructure != actor.previousCharacterDataComponent.previousHomeStructure && actor.homeStructure != homeStructure)
		{
			if (overrideEffectLog != null)
			{
				LogPool.Release(overrideEffectLog);
			}
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " buy_new_home_structure", base.logTags);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(actor.homeStructure, actor.homeStructure.name, LOG_IDENTIFIER.LANDMARK_1);
		}
		return true;
	}
}
