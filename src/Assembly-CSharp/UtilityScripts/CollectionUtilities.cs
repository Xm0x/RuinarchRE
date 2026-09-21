using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityScripts;

public static class CollectionUtilities
{
	public static T GetNextElementCyclic<T>(List<T> collection, int index)
	{
		if (index > collection.Count)
		{
			throw new ArgumentOutOfRangeException("Trying to get next element cyclic, but provided index is greater than the size of the collection!");
		}
		if (index == collection.Count - 1)
		{
			return collection[0];
		}
		return collection[index + 1];
	}

	public static T GetNextElementCyclic<T>(T[] collection, int index)
	{
		if (index > collection.Length)
		{
			throw new ArgumentOutOfRangeException("Trying to get next element cyclic, but provided index is greater than the size of the collection!");
		}
		if (index == collection.Length - 1)
		{
			return collection[0];
		}
		return collection[index + 1];
	}

	public static T GetPreviousElementCyclic<T>(List<T> collection, int index)
	{
		if (index > collection.Count)
		{
			throw new ArgumentOutOfRangeException("Trying to get next element cyclic, but provided index is greater than the size of the collection!");
		}
		if (index <= 0)
		{
			return collection.Last();
		}
		return collection[index - 1];
	}

	public static T GetNextElementCyclic<T>(List<T> collection, T startAt)
	{
		int index = collection.IndexOf(startAt);
		return GetNextElementCyclic(collection, index);
	}

	public static T GetPreviousElementCyclic<T>(List<T> collection, T startAt)
	{
		int index = collection.IndexOf(startAt);
		return GetPreviousElementCyclic(collection, index);
	}

	public static bool ContainsRange<T>(List<T> sourceList, List<T> otherList)
	{
		for (int i = 0; i < otherList.Count; i++)
		{
			if (!sourceList.Contains(otherList[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static T[] GetEnumValues<T>() where T : struct
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("GetValues<T> can only be called for types derived from System.Enum", "T");
		}
		return (T[])Enum.GetValues(typeof(T));
	}

	public static void Shuffle<T>(List<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = Utilities.Rng.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static void Shuffle<T>(List<T> list, List<T> newList)
	{
		newList.AddRange(list);
		int num = newList.Count;
		while (num > 1)
		{
			num--;
			int index = Utilities.Rng.Next(num + 1);
			T value = newList[index];
			newList[index] = newList[num];
			newList[num] = value;
		}
	}

	public static void ListRemoveRange<T>(List<T> sourceList, List<T> itemsToRemove)
	{
		for (int i = 0; i < itemsToRemove.Count; i++)
		{
			T item = itemsToRemove[i];
			sourceList.Remove(item);
		}
	}

	public static void RemoveElements<T>(List<T> sourceList, T[] elementsToRemove)
	{
		for (int i = 0; i < sourceList.Count; i++)
		{
			T value = sourceList[i];
			if (elementsToRemove.Contains(value))
			{
				sourceList.RemoveAt(i);
				i--;
			}
		}
	}

	public static int GetRandomIndexInList<T>(List<T> list)
	{
		if (list == null || list.Count == 0)
		{
			return -1;
		}
		return Utilities.Rng.Next(0, list.Count);
	}

	public static T GetRandomElement<T>(List<T> list)
	{
		if (list == null || list.Count == 0)
		{
			return default(T);
		}
		return list[Utilities.Rng.Next(0, list.Count)];
	}

	public static T GetRandomElement<T>(T[] list)
	{
		if (list == null || list.Length == 0)
		{
			return default(T);
		}
		return list[Utilities.Rng.Next(0, list.Length)];
	}

	public static T GetRandomElement<T>(IEnumerable<T> list)
	{
		if (list == null)
		{
			return default(T);
		}
		int num = list.Count();
		if (num <= 0)
		{
			return default(T);
		}
		return list.ElementAt(Utilities.Rng.Next(0, num));
	}

	public static List<T> GetRandomElements<T>(List<T> list, int count)
	{
		if (list.Count < count)
		{
			return list;
		}
		List<T> list2 = new List<T>();
		List<T> list3 = new List<T>(list);
		for (int i = 0; i < count; i++)
		{
			T randomElement = GetRandomElement(list3);
			list3.Remove(randomElement);
			list2.Add(randomElement);
		}
		return list2;
	}

	public static bool IsLastIndex<T>(List<T> list, int index)
	{
		if (index + 1 == list.Count)
		{
			return true;
		}
		return false;
	}
}
