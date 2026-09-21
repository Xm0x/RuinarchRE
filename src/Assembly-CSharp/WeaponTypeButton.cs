using UnityEngine;
using UnityEngine.UI;

public class WeaponTypeButton : MonoBehaviour
{
	public Text buttonText;

	public string panelName;

	public string categoryName;

	public void SetCurrentlySelectedButton()
	{
		if (panelName == "skill")
		{
			SkillPanelUI.Instance.currentSelectedWeaponTypeButton = this;
		}
		else
		{
			_ = panelName == "class";
		}
	}
}
