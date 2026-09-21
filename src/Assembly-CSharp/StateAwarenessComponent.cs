using Inner_Maps.Location_Structures;

public class StateAwarenessComponent : CharacterComponent
{
	public StateAwarenessComponent()
	{
	}

	public StateAwarenessComponent(SaveDataStateAwarenessComponent data)
	{
	}

	public void OnCharacterPresumedDeadBy(Faction p_faction)
	{
		if (base.owner.faction == p_faction)
		{
			if (base.owner.structureComponent.HasWorkPlaceStructure() && base.owner.structureComponent.workPlaceStructure.DoesCharacterWorkHere(base.owner))
			{
				base.owner.structureComponent.workPlaceStructure.RemoveAssignedWorker(base.owner);
			}
			base.owner.faction.successionComponent.UpdateSuccessors();
		}
	}

	public void LoadReferences(SaveDataStateAwarenessComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
