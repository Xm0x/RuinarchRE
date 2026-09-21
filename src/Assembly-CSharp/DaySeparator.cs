using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class DaySeparator : PooledObject
{
	[SerializeField]
	private TextMeshProUGUI mainLbl;

	public void SetDay(int day)
	{
		if (LocalizationSettings.SelectedLocale.Identifier.Code == "ja")
		{
			mainLbl.text = day + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capitalized_Day").ToUpperInvariant() + " ";
		}
		else
		{
			mainLbl.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capitalized_Day").ToUpperInvariant() + " " + day;
		}
	}
}
