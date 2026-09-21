public interface ISavableCounterpart
{
	string persistentID { get; }

	OBJECT_TYPE objectType { get; }
}
