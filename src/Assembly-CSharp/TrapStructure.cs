using Inner_Maps.Location_Structures;

public class TrapStructure
{
	public LocationStructure structure { get; private set; }

	public int duration { get; private set; }

	public int currentDuration { get; private set; }

	public LocationStructure forcedStructure { get; private set; }

	public Area forcedArea { get; private set; }

	public TrapStructure()
	{
	}

	public TrapStructure(SaveDataTrapStructure data)
	{
		duration = data.duration;
		currentDuration = data.currentDuration;
	}

	public void SetStructureAndDuration(LocationStructure structure, int duration)
	{
		this.structure = structure;
		this.duration = duration;
		currentDuration = 0;
	}

	public void IncrementCurrentDuration(int amount)
	{
		if (duration > 0 && structure != null)
		{
			currentDuration += amount;
			if (currentDuration >= duration)
			{
				SetStructureAndDuration(null, 0);
			}
		}
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (structure == p_structure)
		{
			SetStructureAndDuration(null, 0);
		}
		if (forcedStructure == p_structure)
		{
			SetForcedStructure(null);
		}
	}

	public void SetForcedStructure(LocationStructure structure)
	{
		forcedStructure = structure;
	}

	public bool SatisfiesForcedStructure(IPointOfInterest target)
	{
		if (forcedStructure == null)
		{
			return true;
		}
		if (target.gridTileLocation != null)
		{
			return target.gridTileLocation.structure == forcedStructure;
		}
		return false;
	}

	public void SetForcedArea(Area p_area)
	{
		forcedArea = p_area;
	}

	public bool SatisfiesForcedArea(IPointOfInterest target)
	{
		if (forcedArea == null)
		{
			return true;
		}
		if (target.gridTileLocation != null)
		{
			return target.gridTileLocation.area == forcedArea;
		}
		return false;
	}

	public void ResetAllTrapStructures()
	{
		SetStructureAndDuration(null, 0);
		SetForcedStructure(null);
	}

	public void ResetTrapArea()
	{
		SetForcedArea(null);
	}

	public void ResetAllTrappedValues()
	{
		if (IsTrapped())
		{
			ResetAllTrapStructures();
		}
		if (IsTrappedInArea())
		{
			ResetTrapArea();
		}
	}

	public bool IsTrapped()
	{
		if (forcedStructure == null)
		{
			return structure != null;
		}
		return true;
	}

	public bool IsTrapStructure(LocationStructure structure)
	{
		if (structure != null)
		{
			if (structure != this.structure)
			{
				return structure == forcedStructure;
			}
			return true;
		}
		return false;
	}

	public bool IsTrappedAndTrapStructureIs(LocationStructure structure)
	{
		if (IsTrapped())
		{
			return IsTrapStructure(structure);
		}
		return false;
	}

	public bool IsTrappedAndTrapStructureIsNot(LocationStructure structure)
	{
		if (IsTrapped())
		{
			return !IsTrapStructure(structure);
		}
		return false;
	}

	public bool IsTrappedInArea()
	{
		return forcedArea != null;
	}

	public bool IsTrapArea(Area p_area)
	{
		if (forcedArea != null)
		{
			return forcedArea == p_area;
		}
		return false;
	}

	public bool IsTrappedAndTrapAreaIs(Area p_area)
	{
		if (IsTrappedInArea())
		{
			return IsTrapArea(p_area);
		}
		return false;
	}

	public bool IsTrappedAndTrapAreaIsNot(Area p_area)
	{
		if (IsTrappedInArea())
		{
			return !IsTrapArea(p_area);
		}
		return false;
	}

	public void LoadReferences(SaveDataTrapStructure data)
	{
		if (!string.IsNullOrEmpty(data.structure))
		{
			structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.structure);
		}
		if (!string.IsNullOrEmpty(data.forcedStructure))
		{
			forcedStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.forcedStructure);
		}
		if (!string.IsNullOrEmpty(data.forcedHex))
		{
			forcedArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.forcedHex);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = structure;
		_ = forcedStructure;
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
