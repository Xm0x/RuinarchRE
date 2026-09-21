using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UtilityScripts;

public class WeightedDictionary<T>
{
	private Dictionary<T, int> _dictionary;

	public Dictionary<T, int> dictionary => _dictionary;

	public int Count => _dictionary.Count;

	public WeightedDictionary()
	{
		_dictionary = new Dictionary<T, int>();
	}

	public WeightedDictionary(int p_capacity)
	{
		_dictionary = new Dictionary<T, int>(p_capacity);
	}

	public WeightedDictionary(Dictionary<T, int> dictionary)
	{
		_dictionary = new Dictionary<T, int>();
		foreach (KeyValuePair<T, int> item in dictionary)
		{
			_dictionary.Add(item.Key, item.Value);
		}
	}

	internal void AddElement(T newElement, int weight = 0)
	{
		if (!_dictionary.ContainsKey(newElement))
		{
			weight = Mathf.Max(0, weight);
			_dictionary.Add(newElement, weight);
		}
		else
		{
			AddWeightToElement(newElement, weight);
		}
	}

	internal void AddElements(Dictionary<T, int> otherDictionary)
	{
		foreach (KeyValuePair<T, int> item in otherDictionary)
		{
			T key = item.Key;
			int value = item.Value;
			AddElement(key, value);
		}
	}

	internal void AddElements(WeightedDictionary<T> otherDictionary)
	{
		AddElements(otherDictionary._dictionary);
	}

	internal void SetElementWeight(T element, int newWeight)
	{
		if (_dictionary.ContainsKey(element))
		{
			_dictionary[element] = newWeight;
		}
		else
		{
			_dictionary.Add(element, newWeight);
		}
	}

	internal void ReplaceElement(T element, T newElement)
	{
		if (_dictionary.ContainsKey(element))
		{
			AddElement(newElement, _dictionary[element]);
			RemoveElement(element);
		}
	}

	internal void RemoveElement(T element)
	{
		if (_dictionary.ContainsKey(element))
		{
			_dictionary.Remove(element);
		}
	}

	internal void AddWeightToElement(T key, int weight)
	{
		if (_dictionary.ContainsKey(key))
		{
			int b = _dictionary[key] + weight;
			b = Mathf.Max(0, b);
			_dictionary[key] = b;
		}
		else
		{
			weight = Mathf.Max(0, weight);
			_dictionary.Add(key, weight);
		}
	}

	internal void SubtractWeightFromElement(T key, int weight)
	{
		if (_dictionary.ContainsKey(key))
		{
			_dictionary[key] -= weight;
		}
	}

	public bool HasElement(T key)
	{
		return _dictionary.ContainsKey(key);
	}

	public int GetElementWeight(T key)
	{
		if (_dictionary.ContainsKey(key))
		{
			return _dictionary[key];
		}
		return 0;
	}

	internal T PickRandomElementGivenWeights()
	{
		return Utilities.PickRandomElementWithWeights(_dictionary);
	}

	[Conditional("DEBUG_LOG")]
	internal void LogDictionaryValues(string title)
	{
	}

	internal string GetWeightsSummary(string title)
	{
		return Utilities.GetWeightsSummary(_dictionary, title);
	}

	internal int GetTotalOfWeights()
	{
		return Utilities.GetTotalOfWeights(_dictionary);
	}

	internal void Clear()
	{
		_dictionary.Clear();
	}
}
