using System;

public interface ISavable
{
	string persistentID { get; }

	OBJECT_TYPE objectType { get; }

	Type serializedData { get; }
}
