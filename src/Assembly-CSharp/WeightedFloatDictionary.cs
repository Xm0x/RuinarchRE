using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class WeightedFloatDictionary<T>
{
	private Dictionary<T, float> _dictionary;

	public Dictionary<T, float> dictionary => _dictionary;

	public int Count => _dictionary.Count;

	public WeightedFloatDictionary()
	{
		_dictionary = new Dictionary<T, float>();
	}

	public WeightedFloatDictionary(Dictionary<T, float> dictionary)
	{
		_dictionary = new Dictionary<T, float>();
		foreach (KeyValuePair<T, float> item in dictionary)
		{
			_dictionary.Add(item.Key, item.Value);
		}
	}

	internal void AddElement(T newElement, float weight = 0f)
	{
		if (!_dictionary.ContainsKey(newElement))
		{
			_dictionary.Add(newElement, weight);
		}
		else
		{
			AddWeightToElement(newElement, weight);
		}
	}

	internal void AddElements(Dictionary<T, float> otherDictionary)
	{
		foreach (KeyValuePair<T, float> item in otherDictionary)
		{
			T key = item.Key;
			float value = item.Value;
			AddElement(key, value);
		}
	}

	internal void AddElements(WeightedFloatDictionary<T> otherDictionary)
	{
		AddElements(otherDictionary._dictionary);
	}

	internal void ChangeElement(T element, float newWeight)
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

	internal void RemoveElement(T element)
	{
		if (_dictionary.ContainsKey(element))
		{
			_dictionary.Remove(element);
		}
	}

	internal void AddWeightToElement(T key, float weight)
	{
		if (_dictionary.ContainsKey(key))
		{
			_dictionary[key] += weight;
		}
		else
		{
			_dictionary.Add(key, weight);
		}
	}

	internal void SubtractWeightFromElement(T key, float weight)
	{
		if (_dictionary.ContainsKey(key))
		{
			_dictionary[key] -= weight;
		}
	}

	internal T PickRandomElementGivenWeights()
	{
		return Utilities.PickRandomElementWithWeights(_dictionary);
	}

	internal void LogDictionaryValues(string title)
	{
		Debug.Log(Utilities.GetWeightsSummary(_dictionary, title));
	}

	internal string GetWeightsSummary(string title)
	{
		return Utilities.GetWeightsSummary(_dictionary, title);
	}

	internal float GetTotalOfWeights()
	{
		return Utilities.GetTotalOfWeights(_dictionary);
	}

	internal void Clear()
	{
		_dictionary.Clear();
	}
}
