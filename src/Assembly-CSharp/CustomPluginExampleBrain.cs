using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CustomPluginExampleBrain : MonoBehaviour
{
	public Text txtCustomRange;

	private CustomRange customRange = new CustomRange(0f, 10f);

	private static CustomRangePlugin customRangePlugin = new CustomRangePlugin();

	private void Start()
	{
		DOTween.To(customRangePlugin, () => customRange, delegate(CustomRange x)
		{
			customRange = x;
		}, new CustomRange(20f, 100f), 4f);
	}

	private void Update()
	{
		txtCustomRange.text = customRange.min + "\n" + customRange.max;
	}
}
