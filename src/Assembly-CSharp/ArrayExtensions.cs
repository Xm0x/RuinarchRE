using UtilityScripts;

public static class ArrayExtensions
{
	public static string ComafyList<T>(this T[] p_list)
	{
		string text = string.Empty;
		for (int i = 0; i < p_list.Length; i++)
		{
			T val = p_list[i];
			text = ((val != null) ? (text + Utilities.NotNormalizedConversionEnumToString(val.ToString())) : (text + "null"));
			if (p_list.IsSecondToTheLastIndex(i))
			{
				text += " and ";
			}
			else if (!p_list.IsLastIndex(i))
			{
				text += ", ";
			}
		}
		return text;
	}

	public static string ComafyAndLocalizeList<T>(this T[] p_list, string p_tableName)
	{
		string text = string.Empty;
		for (int i = 0; i < p_list.Length; i++)
		{
			T val = p_list[i];
			if (val == null)
			{
				text += "null";
			}
			else
			{
				string key = Utilities.NotNormalizedConversionEnumToString(val.ToString());
				text += LocalizationManager.Instance.GetLocalizedValue(p_tableName, key);
			}
			if (!p_list.IsLastIndex(i))
			{
				text += ", ";
			}
		}
		return text;
	}

	public static string ComafyLocalizeAndAddLinksList<T>(this T[] p_list, string p_tableName)
	{
		string text = string.Empty;
		for (int i = 0; i < p_list.Length; i++)
		{
			T val = p_list[i];
			if (val == null)
			{
				text += "null";
			}
			else
			{
				string key = Utilities.NotNormalizedConversionEnumToString(val.ToString());
				text = $"{text}<link={i}>{LocalizationManager.Instance.GetLocalizedValue(p_tableName, key)}</link>";
			}
			if (!p_list.IsLastIndex(i))
			{
				text += ", ";
			}
		}
		return text;
	}

	public static bool IsSecondToTheLastIndex<T>(this T[] p_list, int p_index)
	{
		return p_list.Length - 2 == p_index;
	}

	public static bool IsLastIndex<T>(this T[] p_list, int p_index)
	{
		return p_list.Length - 1 == p_index;
	}

	public static bool IsIndexInArray<T>(this T[] p_list, int p_index)
	{
		if (p_index >= 0)
		{
			return p_index < p_list.Length;
		}
		return false;
	}
}
