using Inner_Maps.Location_Structures;
using Object_Pools;

namespace Interrupts;

public class ClaimWorkStructure : Interrupt
{
	public ClaimWorkStructure()
		: base(INTERRUPT.Claim_Work_Structure)
	{
		base.duration = 0;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.isSimulateneous = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		ManMadeStructure workPlaceStructure = actor.structureComponent.workPlaceStructure;
		if (workPlaceStructure != null)
		{
			if (overrideEffectLog != null)
			{
				LogPool.Release(overrideEffectLog);
			}
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " set_work_structure", base.logTags);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(workPlaceStructure, workPlaceStructure.name, LOG_IDENTIFIER.LANDMARK_1);
		}
		return true;
	}
}
