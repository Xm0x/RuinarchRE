using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public static class FactionEmblemRandomizer
{
	public static Sprite wildMonsterFactionEmblem;

	public static Sprite vagrantFactionEmblem;

	public static Sprite disguisedFactionEmblem;

	public static Sprite undeadFactionEmblem;

	public static Sprite playerFactionEmblem;

	public static Sprite cultFactionEmblem;

	public static Sprite ratmenFactionEmblem;

	public static Sprite banditFactionEmblem;

	public static Sprite retaliatorFactionEmblem;

	private static List<Sprite> _unusedEmblems;

	private static List<Sprite> _usedEmblems;

	private static bool hasBeenInitialized;

	public static List<Sprite> allEmblems;

	public static void Initialize(List<Sprite> p_emblems)
	{
		if (allEmblems == null)
		{
			allEmblems = new List<Sprite>(p_emblems);
		}
		if (_unusedEmblems == null)
		{
			_unusedEmblems = new List<Sprite>(p_emblems);
		}
		else
		{
			_unusedEmblems.Clear();
			_unusedEmblems.AddRange(p_emblems);
		}
		if (_usedEmblems == null)
		{
			_usedEmblems = new List<Sprite>();
		}
		else
		{
			_usedEmblems.Clear();
		}
		hasBeenInitialized = true;
	}

	public static Sprite GetUnusedFactionEmblem()
	{
		if (_unusedEmblems.Count > 0)
		{
			return CollectionUtilities.GetRandomElement(_unusedEmblems);
		}
		Reset();
		return CollectionUtilities.GetRandomElement(_unusedEmblems);
	}

	public static void SetEmblemAsUsed(Sprite p_emblem)
	{
		_unusedEmblems.Remove(p_emblem);
		if (!_usedEmblems.Contains(p_emblem))
		{
			_usedEmblems.Add(p_emblem);
		}
	}

	public static void SetEmblemAsUnUsed(Sprite p_emblem)
	{
		if (!_unusedEmblems.Contains(p_emblem))
		{
			_unusedEmblems.Add(p_emblem);
		}
		_usedEmblems.Remove(p_emblem);
	}

	public static void Reset()
	{
		_unusedEmblems.AddRange(_usedEmblems);
		_usedEmblems.Clear();
	}
}
