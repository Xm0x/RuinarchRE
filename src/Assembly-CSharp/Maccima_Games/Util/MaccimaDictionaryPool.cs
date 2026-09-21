using System.Collections.Generic;

namespace Maccima_Games.Util;

public static class MaccimaDictionaryPool<T, U>
{
	private static readonly Queue<Dictionary<T, U>> pool = new Queue<Dictionary<T, U>>(100);

	public static Dictionary<T, U> Claim()
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				return pool.Dequeue();
			}
			return new Dictionary<T, U>();
		}
	}

	public static Dictionary<T, U> Claim(int capacity)
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				return pool.Dequeue();
			}
			return new Dictionary<T, U>(capacity);
		}
	}

	public static void Release(Dictionary<T, U> dict)
	{
		lock (pool)
		{
			dict.Clear();
			pool.Enqueue(dict);
		}
	}
}
