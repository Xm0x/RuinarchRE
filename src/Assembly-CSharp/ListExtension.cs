using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public static class ListExtension
{
	public static string ComafyList<T>(this List<T> p_list)
	{
		string text = string.Empty;
		for (int i = 0; i < p_list.Count; i++)
		{
			text += Utilities.NotNormalizedConversionEnumToString(p_list[i].ToString());
			if (p_list.IsSecondToTheLastIndex(i))
			{
				text = text + " " + LocalizationManager.And + " ";
			}
			else if (!p_list.IsLastIndex(i))
			{
				text += ", ";
			}
		}
		return text;
	}

	public static string ComafyListNoAnd<T>(this List<T> p_list)
	{
		string text = string.Empty;
		for (int i = 0; i < p_list.Count; i++)
		{
			text += Utilities.NotNormalizedConversionEnumToString(p_list[i].ToString());
			if (!p_list.IsLastIndex(i))
			{
				text += ", ";
			}
		}
		return text;
	}

	public static string LocalizedComafyListNoAnd<T>(this List<T> p_list)
	{
		string text = string.Empty;
		for (int i = 0; i < p_list.Count; i++)
		{
			string text2 = Utilities.NotNormalizedConversionEnumToString(p_list[i].ToString());
			string text3 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", text2);
			if (string.IsNullOrEmpty(text3))
			{
				text3 = text2;
			}
			text += text3;
			if (!p_list.IsLastIndex(i))
			{
				text += ", ";
			}
		}
		return text;
	}

	public static bool IsLastIndex<T>(this List<T> p_list, int p_index)
	{
		return p_list.Count - 1 == p_index;
	}

	public static bool IsSecondToTheLastIndex<T>(this List<T> p_list, int p_index)
	{
		return p_list.Count - 2 == p_index;
	}

	public static bool IsIndexInList<T>(this List<T> p_list, int p_index)
	{
		if (p_index >= 0)
		{
			return p_index < p_list.Count;
		}
		return false;
	}

	public static void Shuffle<T>(this IList<T> ts)
	{
		int count = ts.Count;
		int num = count - 1;
		for (int i = 0; i < num; i++)
		{
			int index = Random.Range(i, count);
			T value = ts[i];
			ts[i] = ts[index];
			ts[index] = value;
		}
	}

	public static bool HasTileWithFeature(this List<Area> p_list, string p_featureName)
	{
		for (int i = 0; i < p_list.Count; i++)
		{
			if (p_list[i].featureComponent.HasFeature(p_featureName))
			{
				return true;
			}
		}
		return false;
	}

	public static void ListRemoveRange<T>(this List<T> sourceList, List<T> itemsToRemove)
	{
		for (int i = 0; i < itemsToRemove.Count; i++)
		{
			T item = itemsToRemove[i];
			sourceList.Remove(item);
		}
	}

	public static bool HasValueInListUntilIndex<T>(this List<T> sourceList, int p_index, T value)
	{
		int num = p_index + 1;
		for (int i = 0; i < num; i++)
		{
			if (sourceList[i].Equals(value))
			{
				return true;
			}
		}
		return false;
	}

	public static void GetAreasThatHaveNeededBiomeOfSpecialStructure(this List<Area> p_sourceList, List<Area> p_outList, STRUCTURE_TYPE p_structure)
	{
		BIOMES neededBiomeForSpecialStructure = p_structure.GetNeededBiomeForSpecialStructure();
		if (neededBiomeForSpecialStructure == BIOMES.NONE)
		{
			p_outList.AddRange(p_sourceList);
			return;
		}
		for (int i = 0; i < p_sourceList.Count; i++)
		{
			Area area = p_sourceList[i];
			if (area.biomeComponent.biomeType == neededBiomeForSpecialStructure || (neededBiomeForSpecialStructure == BIOMES.GRASSLAND && area.biomeComponent.biomeType == BIOMES.FOREST))
			{
				p_outList.Add(area);
			}
		}
	}
}
