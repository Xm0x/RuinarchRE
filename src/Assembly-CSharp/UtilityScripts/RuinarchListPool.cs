using System.Collections.Generic;

namespace UtilityScripts;

public static class RuinarchListPool<T>
{
	private static readonly Queue<List<T>> pool = new Queue<List<T>>(200);

	public static List<T> Claim()
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				return pool.Dequeue();
			}
			return new List<T>(100);
		}
	}

	public static List<T> Claim(int p_capacity)
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				return pool.Dequeue();
			}
			return new List<T>(p_capacity);
		}
	}

	public static void Release(List<T> list)
	{
		lock (pool)
		{
			list.Clear();
			pool.Enqueue(list);
		}
	}
}
