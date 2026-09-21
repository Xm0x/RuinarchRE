using UnityEngine;
using UnityEngine.UI;

public class ClassTraitButton : MonoBehaviour
{
	public Text buttonText;

	private string _traitName;

	public string traitName => _traitName;

	public void SetCurrentlySelectedButton()
	{
		ClassPanelUI.Instance.currentSelectedClassTraitButton = this;
	}

	public void SetTraitName(string traitName)
	{
		_traitName = traitName;
		buttonText.text = _traitName;
	}
}
