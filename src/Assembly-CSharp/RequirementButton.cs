using UnityEngine;
using UnityEngine.UI;

public class RequirementButton : MonoBehaviour
{
	public Text buttonText;

	private string _requirement;

	public string requirement => _requirement;

	public void SetCurrentlySelectedButton()
	{
		TraitPanelUI.Instance.currentSelectedRequirementButton = this;
	}

	public void SetRequirement(string requirement)
	{
		_requirement = requirement;
		buttonText.text = _requirement;
	}
}
