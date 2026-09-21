using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sobriquet;

internal class MarkovChain
{
	private readonly int _order;

	private readonly Dictionary<string, Dictionary<char, int>> _items = new Dictionary<string, Dictionary<char, int>>();

	private readonly Dictionary<string, int> _terminals = new Dictionary<string, int>();

	public MarkovChain(int order)
	{
		if (order < 0)
		{
			throw new ArgumentOutOfRangeException("order");
		}
		_order = order;
	}

	public void Add(string items, int weight)
	{
		int num = 0;
		int num2 = 0;
		string text = "";
		foreach (char next in items)
		{
			string state = text;
			Add(state, next, weight);
			num2++;
			if (num2 > _order)
			{
				num2--;
				num++;
			}
			text = SafeSubstring(items, num, num2);
		}
		string key = text;
		_terminals[key] = (_terminals.ContainsKey(key) ? (weight + _terminals[key]) : weight);
	}

	private void Add(string state, char next, int weight)
	{
		if (!_items.TryGetValue(state, out var value))
		{
			value = new Dictionary<char, int>();
			_items.Add(state, value);
		}
		value.TryGetValue(next, out var value2);
		value[next] = value2 + weight;
	}

	public string Chain()
	{
		return Chain("", new Random());
	}

	public string Chain(string previous)
	{
		return Chain(previous, new Random());
	}

	public string Chain(int seed)
	{
		return Chain("", new Random(seed));
	}

	public string Chain(string previous, int seed)
	{
		return Chain(previous, new Random(seed));
	}

	public string Chain(Random rand)
	{
		return Chain("", rand);
	}

	public string Chain(string previous, Random rand)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Queue<char> queue = new Queue<char>(previous);
		while (true)
		{
			if (queue.Count > _order)
			{
				queue.Dequeue();
				continue;
			}
			string key = new string(queue.ToArray());
			if (!_items.TryGetValue(key, out var value))
			{
				return stringBuilder.ToString();
			}
			_terminals.TryGetValue(key, out var value2);
			int num = value.Sum((KeyValuePair<char, int> w) => w.Value);
			int num2 = rand.Next(num + value2) + 1;
			if (num2 > num)
			{
				break;
			}
			int num3 = 0;
			foreach (KeyValuePair<char, int> item in value)
			{
				num3 += item.Value;
				if (num3 >= num2)
				{
					stringBuilder.Append(item.Key);
					queue.Enqueue(item.Key);
					break;
				}
			}
		}
		return stringBuilder.ToString();
	}

	internal IEnumerable<string> AllRaw(int maxlen)
	{
		return AllRaw("", maxlen);
	}

	internal IEnumerable<string> AllRaw(string prefix, int maxlen)
	{
		if (prefix.Length > _order)
		{
			throw new ArgumentException($"prefix should be fewer than {_order} chars long");
		}
		Queue<string> queue = new Queue<string>();
		queue.Enqueue(prefix);
		while (queue.Count > 0)
		{
			string current = queue.Dequeue();
			if (current.Length > maxlen)
			{
				continue;
			}
			string suffix = GetLast(current, _order);
			_terminals.TryGetValue(suffix, out var value);
			if (value > 0)
			{
				yield return current;
			}
			if (!_items.TryGetValue(suffix, out var value2))
			{
				continue;
			}
			foreach (KeyValuePair<char, int> item in value2)
			{
				char key = item.Key;
				if (item.Value != 0)
				{
					queue.Enqueue(current + key);
				}
			}
		}
	}

	private static string SafeSubstring(string text, int start, int length)
	{
		if (text.Length > start)
		{
			if (text.Length - start > length)
			{
				return text.Substring(start, length);
			}
			return text.Substring(start);
		}
		return "";
	}

	private string GetLast(string prefix, int length)
	{
		if (prefix.Length <= length)
		{
			return prefix;
		}
		return prefix.Substring(prefix.Length - length);
	}
}
