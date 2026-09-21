using System;
using System.Collections.Generic;

[Serializable]
public struct UnsummonedMinionData
{
	public string minionName;

	public string className;

	public List<PLAYER_SKILL_TYPE> interventionAbilitiesToResearch;

	public override bool Equals(object obj)
	{
		if (obj is UnsummonedMinionData)
		{
			return Equals((UnsummonedMinionData)obj);
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private bool Equals(UnsummonedMinionData data)
	{
		if (minionName == data.minionName && className == data.className)
		{
			return interventionAbilitiesToResearch.Equals(data.interventionAbilitiesToResearch);
		}
		return false;
	}
}
