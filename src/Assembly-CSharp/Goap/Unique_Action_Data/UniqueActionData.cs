using Inner_Maps.Location_Structures;

namespace Goap.Unique_Action_Data;

public abstract class UniqueActionData
{
	public abstract SaveDataUniqueActionData Save();

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}

	public virtual bool IsCharacterReferenced(Character p_character)
	{
		return false;
	}

	public virtual bool IsStructureReferenced(LocationStructure p_structure)
	{
		return false;
	}
}
