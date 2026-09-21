namespace UnityEngine.UI.Extensions.Examples;

public class UpdateRadialValue : MonoBehaviour
{
	public InputField input;

	public RadialSlider slider;

	private void Start()
	{
	}

	public void UpdateSliderValue()
	{
		float.TryParse(input.text, out var result);
		slider.Value = result;
	}

	public void UpdateSliderAndle()
	{
		int.TryParse(input.text, out var result);
		slider.Angle = result;
	}
}
