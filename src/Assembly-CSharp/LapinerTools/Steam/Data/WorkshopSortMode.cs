using System;
using Steamworks;
using UnityEngine;

namespace LapinerTools.Steam.Data;

[Serializable]
public class WorkshopSortMode
{
	[SerializeField]
	public EUGCQuery MODE;

	[SerializeField]
	public EWorkshopSource SOURCE;

	public WorkshopSortMode()
	{
	}

	public WorkshopSortMode(EUGCQuery p_mode)
	{
		MODE = p_mode;
	}

	public WorkshopSortMode(EWorkshopSource p_source)
	{
		SOURCE = p_source;
	}

	public WorkshopSortMode(EUGCQuery p_mode, EWorkshopSource p_source)
	{
		MODE = p_mode;
		SOURCE = p_source;
	}

	public override bool Equals(object p_other)
	{
		if (p_other != null && p_other is WorkshopSortMode)
		{
			return p_other.GetHashCode() == GetHashCode();
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((int)(MODE + 100 * (int)SOURCE)).GetHashCode();
	}
}
