using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleTextColorizer : MonoBehaviour
{
	private Color onColor = new Color(0.28627452f, 31f / 85f, 0.41960785f, 1f);

	private Color offColor = new Color(0.96862745f, 14f / 15f, 0.83137256f, 1f);

	[SerializeField]
	private TextMeshProUGUI targetText;

	public void OnValueChange(bool isOn)
	{
		if (isOn)
		{
			targetText.color = onColor;
		}
		else
		{
			targetText.color = offColor;
		}
	}
}
