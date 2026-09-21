using System.Collections.Generic;

namespace UtilityScripts;

public class RuinarchHashSetPool<T>
{
	private static readonly Queue<HashSet<T>> pool = new Queue<HashSet<T>>();

	public static HashSet<T> Claim()
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				return pool.Dequeue();
			}
			return new HashSet<T>();
		}
	}

	public static void Release(HashSet<T> list)
	{
		lock (pool)
		{
			list.Clear();
			pool.Enqueue(list);
		}
	}
}
