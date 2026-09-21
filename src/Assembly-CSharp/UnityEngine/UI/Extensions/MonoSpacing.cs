using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Effects/Extensions/Mono Spacing")]
[RequireComponent(typeof(Text))]
[RequireComponent(typeof(RectTransform))]
public class MonoSpacing : BaseMeshEffect
{
	[SerializeField]
	private float m_spacing;

	public float HalfCharWidth = 1f;

	public bool UseHalfCharWidth;

	private RectTransform rectTransform;

	private Text text;

	public float Spacing
	{
		get
		{
			return m_spacing;
		}
		set
		{
			if (m_spacing != value)
			{
				m_spacing = value;
				if (base.graphic != null)
				{
					base.graphic.SetVerticesDirty();
				}
			}
		}
	}

	protected MonoSpacing()
	{
	}

	protected override void Awake()
	{
		text = GetComponent<Text>();
		if (text == null)
		{
			Debug.LogWarning("MonoSpacing: Missing Text component");
		}
		else
		{
			rectTransform = text.GetComponent<RectTransform>();
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive())
		{
			return;
		}
		List<UIVertex> list = new List<UIVertex>();
		vh.GetUIVertexStream(list);
		string[] array = this.text.text.Split('\n');
		float num = Spacing * (float)this.text.fontSize / 100f;
		float num2 = 0f;
		int i = 0;
		switch (this.text.alignment)
		{
		case TextAnchor.UpperLeft:
		case TextAnchor.MiddleLeft:
		case TextAnchor.LowerLeft:
			num2 = 0f;
			break;
		case TextAnchor.UpperCenter:
		case TextAnchor.MiddleCenter:
		case TextAnchor.LowerCenter:
			num2 = 0.5f;
			break;
		case TextAnchor.UpperRight:
		case TextAnchor.MiddleRight:
		case TextAnchor.LowerRight:
			num2 = 1f;
			break;
		}
		foreach (string text in array)
		{
			float num3 = 0f - ((float)(text.Length - 1) * num * num2 - (num2 - 0.5f) * rectTransform.rect.width) + num / 2f * (1f - num2 * 2f);
			int index;
			int index2;
			int index3;
			int index4;
			int index5;
			int num4;
			UIVertex value;
			UIVertex value2;
			UIVertex value3;
			UIVertex value4;
			UIVertex value5;
			UIVertex value6;
			float x;
			int num5;
			float num6;
			float num7;
			for (int k = 0; k < text.Length; num7 = num6, value.position += new Vector3(0f - value.position.x + num3 + -0.5f * x + num7, 0f, 0f), value2.position += new Vector3(0f - value2.position.x + num3 + 0.5f * x + num7, 0f, 0f), value3.position += new Vector3(0f - value3.position.x + num3 + 0.5f * x + num7, 0f, 0f), value4.position += new Vector3(0f - value4.position.x + num3 + 0.5f * x + num7, 0f, 0f), value5.position += new Vector3(0f - value5.position.x + num3 + -0.5f * x + num7, 0f, 0f), value6.position += new Vector3(0f - value6.position.x + num3 + -0.5f * x + num7, 0f, 0f), num3 = ((num5 == 0) ? (num3 + num) : (num3 + num / 2f)), list[index] = value, list[index2] = value2, list[index3] = value3, list[index4] = value4, list[index5] = value5, list[num4] = value6, i++, k++)
			{
				index = i * 6;
				index2 = i * 6 + 1;
				index3 = i * 6 + 2;
				index4 = i * 6 + 3;
				index5 = i * 6 + 4;
				num4 = i * 6 + 5;
				if (num4 > list.Count - 1)
				{
					return;
				}
				value = list[index];
				value2 = list[index2];
				value3 = list[index3];
				value4 = list[index4];
				value5 = list[index5];
				value6 = list[num4];
				x = (value2.position - value.position).x;
				if (UseHalfCharWidth)
				{
					num5 = ((x < HalfCharWidth) ? 1 : 0);
					if (num5 != 0)
					{
						num6 = (0f - num) / 4f;
						continue;
					}
				}
				else
				{
					num5 = 0;
				}
				num6 = 0f;
			}
			i++;
		}
		vh.Clear();
		vh.AddUIVertexTriangleStream(list);
	}
}
