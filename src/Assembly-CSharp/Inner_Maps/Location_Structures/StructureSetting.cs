using System;

namespace Inner_Maps.Location_Structures;

[Serializable]
public struct StructureSetting
{
	public RESOURCE resource;

	public STRUCTURE_TYPE structureType;

	public bool hasValue;

	public StructureSetting(STRUCTURE_TYPE structureType, RESOURCE resource)
	{
		this.structureType = structureType;
		this.resource = resource;
		hasValue = true;
	}

	public override string ToString()
	{
		return resource.ToStringEnum() + " " + structureType.ToStringEnum();
	}

	public bool Equals(StructureSetting other)
	{
		if (resource == other.resource)
		{
			return structureType == other.structureType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is StructureSetting other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((int)resource * 397) ^ (int)structureType;
	}
}
