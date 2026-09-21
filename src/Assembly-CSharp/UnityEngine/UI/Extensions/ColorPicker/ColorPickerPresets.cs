namespace UnityEngine.UI.Extensions.ColorPicker;

public class ColorPickerPresets : MonoBehaviour
{
	public ColorPickerControl picker;

	public GameObject[] presets;

	public Image createPresetImage;

	private void Awake()
	{
		picker.onValueChanged.AddListener(ColorChanged);
	}

	public void CreatePresetButton()
	{
		for (int i = 0; i < presets.Length; i++)
		{
			if (!presets[i].activeSelf)
			{
				presets[i].SetActive(value: true);
				presets[i].GetComponent<Image>().color = picker.CurrentColor;
				break;
			}
		}
	}

	public void PresetSelect(Image sender)
	{
		picker.CurrentColor = sender.color;
	}

	private void ColorChanged(Color color)
	{
		createPresetImage.color = color;
	}
}
