using UnityEngine;

public static class RendererExtensions
{
	private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera)
	{
		Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 point = camera.WorldToScreenPoint(array[i]);
			if (rect.Contains(point))
			{
				num++;
			}
		}
		return num;
	}

	public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera)
	{
		return rectTransform.CountCornersVisibleFrom(camera) == 4;
	}

	public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera)
	{
		return rectTransform.CountCornersVisibleFrom(camera) > 0;
	}
}
