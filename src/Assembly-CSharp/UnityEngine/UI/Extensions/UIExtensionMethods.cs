namespace UnityEngine.UI.Extensions;

public static class UIExtensionMethods
{
	public static Canvas GetParentCanvas(this RectTransform rt)
	{
		RectTransform rectTransform = rt;
		Canvas canvas = rt.GetComponent<Canvas>();
		int num = 0;
		while (canvas == null || num > 50)
		{
			canvas = rt.GetComponentInParent<Canvas>();
			if (canvas == null)
			{
				rectTransform = rectTransform.parent.GetComponent<RectTransform>();
				num++;
			}
		}
		return canvas;
	}
}
