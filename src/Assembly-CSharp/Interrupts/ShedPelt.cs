using Inner_Maps;

namespace Interrupts;

public class ShedPelt : Interrupt
{
	public ShedPelt()
		: base(INTERRUPT.Shed_Pelt)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Work_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		LocationGridTile gridTileLocation = interruptHolder.actor.gridTileLocation;
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WEREWOLF_PELT);
		gridTileLocation.structure.AddPOI(tileObject, gridTileLocation);
		overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect", base.logTags);
		overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		overrideEffectLog.AddToFillers(tileObject, tileObject.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		return true;
	}
}
