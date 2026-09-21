using UnityEngine;
using UnityEngine.UI;

public class RaceStringButton : MonoBehaviour
{
	public Text buttonText;

	private string _text;

	private string _category;

	public string text => _text;

	public void SetCurrentlySelectedButton()
	{
		if (_category == "hpperlevel")
		{
			RacePanelUI.Instance.currentSelectedHPPerLevelButton = this;
		}
		else if (_category == "attackperlevel")
		{
			RacePanelUI.Instance.currentSelectedAttackPerLevelButton = this;
		}
		else if (_category == "trait")
		{
			RacePanelUI.Instance.currentSelectedTraitButton = this;
		}
	}

	public void SetText(string text, string category)
	{
		_text = text;
		_category = category;
		buttonText.text = _text;
	}
}
