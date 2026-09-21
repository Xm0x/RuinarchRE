using Inner_Maps.Location_Structures;

public interface ICrimeable : IReactable
{
	string persistentID { get; }

	OBJECT_TYPE objectType { get; }

	CRIMABLE_TYPE crimableType { get; }

	bool IsImportantDataNull();

	bool IsCharacterReferenced(Character p_character);

	bool IsStructureReferenced(LocationStructure p_structure);
}
