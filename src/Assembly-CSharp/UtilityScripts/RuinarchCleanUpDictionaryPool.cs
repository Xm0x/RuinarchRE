using System;
using System.Collections.Generic;

namespace UtilityScripts;

public static class RuinarchCleanUpDictionaryPool
{
	private static readonly List<Dictionary<string, WeakReference>> pool = new List<Dictionary<string, WeakReference>>();

	public static Dictionary<string, WeakReference> Claim()
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				Dictionary<string, WeakReference> result = pool[0];
				pool.RemoveAt(0);
				return result;
			}
			return new Dictionary<string, WeakReference>();
		}
	}

	public static void Release(Dictionary<string, WeakReference> dict)
	{
		lock (pool)
		{
			dict.Clear();
			pool.Add(dict);
		}
	}
}
