using Inner_Maps.Location_Structures;

namespace Interrupts;

public class SetLair : Interrupt
{
	public SetLair()
		: base(INTERRUPT.Set_Lair)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Region currentRegion = interruptHolder.actor.currentRegion;
		LocationStructure lairInRegionFor = GetLairInRegionFor(currentRegion, interruptHolder.actor);
		interruptHolder.actor.necromancerTrait.SetLairStructure(lairInRegionFor);
		if (lairInRegionFor != null)
		{
			interruptHolder.actor.MigrateHomeStructureTo(interruptHolder.actor.necromancerTrait.lairStructure);
			interruptHolder.actor.ClearTerritory();
			return true;
		}
		return false;
	}

	private LocationStructure GetLairInRegionFor(Region region, Character character)
	{
		LocationStructure firstUnoccupiedOrOccupiedByFactionStructureOfType = region.GetFirstUnoccupiedOrOccupiedByFactionStructureOfType(STRUCTURE_TYPE.NECROMANCER_LAIR, character.faction);
		if (firstUnoccupiedOrOccupiedByFactionStructureOfType == null)
		{
			firstUnoccupiedOrOccupiedByFactionStructureOfType = region.GetFirstUnoccupiedOrOccupiedByFactionStructureOfType(STRUCTURE_TYPE.MAGE_TOWER, character.faction);
		}
		if (firstUnoccupiedOrOccupiedByFactionStructureOfType == null)
		{
			firstUnoccupiedOrOccupiedByFactionStructureOfType = region.GetFirstUnoccupiedOrOccupiedByFactionStructureOfType(STRUCTURE_TYPE.TEMPLE, character.faction);
		}
		return firstUnoccupiedOrOccupiedByFactionStructureOfType;
	}
}
