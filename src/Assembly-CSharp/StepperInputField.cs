using UnityEngine.UI;

public class StepperInputField : InputField
{
	public void OnStepperValueChanged(int value)
	{
		base.text = value.ToString();
	}
}
