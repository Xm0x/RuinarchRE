using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[AddComponentMenu("UI/Effects/Extensions/Curly UI Image")]
public class CUIImage : CUIGraphic
{
	public static int SlicedImageCornerRefVertexIdx = 2;

	public static int FilledImageCornerRefVertexIdx = 0;

	[Tooltip("For changing the size of the corner for tiled or sliced Image")]
	[HideInInspector]
	[SerializeField]
	public Vector2 cornerPosRatio = Vector2.one * -1f;

	[HideInInspector]
	[SerializeField]
	protected Vector2 oriCornerPosRatio = Vector2.one * -1f;

	public Vector2 OriCornerPosRatio => oriCornerPosRatio;

	public Image UIImage => (Image)uiGraphic;

	public static int ImageTypeCornerRefVertexIdx(Image.Type _type)
	{
		if (_type == Image.Type.Sliced)
		{
			return SlicedImageCornerRefVertexIdx;
		}
		return FilledImageCornerRefVertexIdx;
	}

	public override void ReportSet()
	{
		if (uiGraphic == null)
		{
			uiGraphic = GetComponent<Image>();
		}
		base.ReportSet();
	}

	protected override void modifyVertices(List<UIVertex> _verts)
	{
		if (!IsActive())
		{
			return;
		}
		if (UIImage.type == Image.Type.Filled)
		{
			Debug.LogWarning("Might not work well Radial Filled at the moment!");
		}
		else if (UIImage.type == Image.Type.Sliced || UIImage.type == Image.Type.Tiled)
		{
			if (cornerPosRatio == Vector2.one * -1f)
			{
				cornerPosRatio = _verts[ImageTypeCornerRefVertexIdx(UIImage.type)].position;
				cornerPosRatio.x = (cornerPosRatio.x + rectTrans.pivot.x * rectTrans.rect.width) / rectTrans.rect.width;
				cornerPosRatio.y = (cornerPosRatio.y + rectTrans.pivot.y * rectTrans.rect.height) / rectTrans.rect.height;
				oriCornerPosRatio = cornerPosRatio;
			}
			if (cornerPosRatio.x < 0f)
			{
				cornerPosRatio.x = 0f;
			}
			if (cornerPosRatio.x >= 0.5f)
			{
				cornerPosRatio.x = 0.5f;
			}
			if (cornerPosRatio.y < 0f)
			{
				cornerPosRatio.y = 0f;
			}
			if (cornerPosRatio.y >= 0.5f)
			{
				cornerPosRatio.y = 0.5f;
			}
			for (int i = 0; i < _verts.Count; i++)
			{
				UIVertex value = _verts[i];
				float num = (value.position.x + rectTrans.rect.width * rectTrans.pivot.x) / rectTrans.rect.width;
				float num2 = (value.position.y + rectTrans.rect.height * rectTrans.pivot.y) / rectTrans.rect.height;
				num = ((!(num < oriCornerPosRatio.x)) ? ((!(num > 1f - oriCornerPosRatio.x)) ? Mathf.Lerp(cornerPosRatio.x, 1f - cornerPosRatio.x, (num - oriCornerPosRatio.x) / (1f - oriCornerPosRatio.x * 2f)) : Mathf.Lerp(1f - cornerPosRatio.x, 1f, (num - (1f - oriCornerPosRatio.x)) / oriCornerPosRatio.x)) : Mathf.Lerp(0f, cornerPosRatio.x, num / oriCornerPosRatio.x));
				num2 = ((!(num2 < oriCornerPosRatio.y)) ? ((!(num2 > 1f - oriCornerPosRatio.y)) ? Mathf.Lerp(cornerPosRatio.y, 1f - cornerPosRatio.y, (num2 - oriCornerPosRatio.y) / (1f - oriCornerPosRatio.y * 2f)) : Mathf.Lerp(1f - cornerPosRatio.y, 1f, (num2 - (1f - oriCornerPosRatio.y)) / oriCornerPosRatio.y)) : Mathf.Lerp(0f, cornerPosRatio.y, num2 / oriCornerPosRatio.y));
				value.position.x = num * rectTrans.rect.width - rectTrans.rect.width * rectTrans.pivot.x;
				value.position.y = num2 * rectTrans.rect.height - rectTrans.rect.height * rectTrans.pivot.y;
				_verts[i] = value;
			}
		}
		base.modifyVertices(_verts);
	}
}
