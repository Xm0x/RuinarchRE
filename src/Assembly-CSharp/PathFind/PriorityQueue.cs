using System.Collections.Generic;
using System.Linq;

namespace PathFind;

internal class PriorityQueue<P, V>
{
	private readonly SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();

	public bool IsEmpty => !list.Any();

	public void Enqueue(P priority, V value)
	{
		if (!list.TryGetValue(priority, out var value2))
		{
			value2 = new Queue<V>();
			list.Add(priority, value2);
		}
		value2.Enqueue(value);
	}

	public V Dequeue()
	{
		KeyValuePair<P, Queue<V>> keyValuePair = list.First();
		V result = keyValuePair.Value.Dequeue();
		if (keyValuePair.Value.Count == 0)
		{
			list.Remove(keyValuePair.Key);
		}
		return result;
	}
}
