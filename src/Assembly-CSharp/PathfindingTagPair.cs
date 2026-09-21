using System;

[Serializable]
public struct PathfindingTagPair
{
	public readonly uint groundTag;

	public readonly uint doorsTag;

	public PathfindingTagPair(uint p_groundTag, uint p_doorsTag)
	{
		groundTag = p_groundTag;
		doorsTag = p_doorsTag;
	}

	public override bool Equals(object obj)
	{
		if (obj is PathfindingTagPair other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(PathfindingTagPair other)
	{
		if (groundTag == other.groundTag)
		{
			return doorsTag == other.doorsTag;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)((groundTag * 397) ^ doorsTag);
	}

	public override string ToString()
	{
		return "Ground Tag: " + groundTag + ". Door Tag: " + doorsTag;
	}
}
