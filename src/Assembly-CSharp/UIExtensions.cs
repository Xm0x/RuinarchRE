using System;
using TMPro;
using UtilityScripts;

public static class UIExtensions
{
	public static int GetDropdownOptionIndex(this TMP_Dropdown p_dropDown, string p_optionName, bool ignoreCase = false)
	{
		for (int i = 0; i < p_dropDown.options.Count; i++)
		{
			if (string.Equals(p_dropDown.options[i].text, p_optionName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return i;
			}
		}
		return -1;
	}

	public static T ConvertCurrentSelectedOption<T>(this TMP_Dropdown p_dropDown) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		string text = p_dropDown.options[p_dropDown.value].text;
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			if (Utilities.NotNormalizedConversionEnumToString(value.ToString()).ToLower().Equals(text.Trim().ToLower()))
			{
				return value;
			}
		}
		return default(T);
	}

	public static T ConvertOption<T>(this TMP_Dropdown p_dropDown, int p_optionIndex) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		string text = p_dropDown.options[p_optionIndex].text;
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			if (Utilities.NotNormalizedConversionEnumToString(value.ToString()).ToLower().Equals(text.Trim().ToLower()))
			{
				return value;
			}
		}
		return default(T);
	}
}
