using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Effects/Extensions/Gradient2")]
public class Gradient2 : BaseMeshEffect
{
	public enum Type
	{
		Horizontal,
		Vertical,
		Radial,
		Diamond
	}

	public enum Blend
	{
		Override,
		Add,
		Multiply
	}

	[SerializeField]
	private Type _gradientType;

	[SerializeField]
	private Blend _blendMode = Blend.Multiply;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _offset;

	[SerializeField]
	private UnityEngine.Gradient _effectGradient = new UnityEngine.Gradient
	{
		colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(Color.black, 0f),
			new GradientColorKey(Color.white, 1f)
		}
	};

	public Blend BlendMode
	{
		get
		{
			return _blendMode;
		}
		set
		{
			_blendMode = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public UnityEngine.Gradient EffectGradient
	{
		get
		{
			return _effectGradient;
		}
		set
		{
			_effectGradient = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public Type GradientType
	{
		get
		{
			return _gradientType;
		}
		set
		{
			_gradientType = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public float Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public override void ModifyMesh(VertexHelper helper)
	{
		if (!IsActive() || helper.currentVertCount == 0)
		{
			return;
		}
		List<UIVertex> list = new List<UIVertex>();
		helper.GetUIVertexStream(list);
		int count = list.Count;
		switch (GradientType)
		{
		case Type.Horizontal:
		{
			float num30 = list[0].position.x;
			float num31 = list[0].position.x;
			float num32 = 0f;
			for (int num33 = count - 1; num33 >= 1; num33--)
			{
				num32 = list[num33].position.x;
				if (num32 > num31)
				{
					num31 = num32;
				}
				else if (num32 < num30)
				{
					num30 = num32;
				}
			}
			float num34 = 1f / (num31 - num30);
			UIVertex vertex4 = default(UIVertex);
			for (int num35 = 0; num35 < helper.currentVertCount; num35++)
			{
				helper.PopulateUIVertex(ref vertex4, num35);
				vertex4.color = BlendColor(vertex4.color, EffectGradient.Evaluate((vertex4.position.x - num30) * num34 - Offset));
				helper.SetUIVertex(vertex4, num35);
			}
			break;
		}
		case Type.Vertical:
		{
			float num18 = list[0].position.y;
			float num19 = list[0].position.y;
			float num20 = 0f;
			for (int num21 = count - 1; num21 >= 1; num21--)
			{
				num20 = list[num21].position.y;
				if (num20 > num19)
				{
					num19 = num20;
				}
				else if (num20 < num18)
				{
					num18 = num20;
				}
			}
			float num22 = 1f / (num19 - num18);
			UIVertex vertex2 = default(UIVertex);
			for (int l = 0; l < helper.currentVertCount; l++)
			{
				helper.PopulateUIVertex(ref vertex2, l);
				vertex2.color = BlendColor(vertex2.color, EffectGradient.Evaluate((vertex2.position.y - num18) * num22 - Offset));
				helper.SetUIVertex(vertex2, l);
			}
			break;
		}
		case Type.Diamond:
		{
			float num23 = list[0].position.y;
			float num24 = list[0].position.y;
			float num25 = 0f;
			for (int num26 = count - 1; num26 >= 1; num26--)
			{
				num25 = list[num26].position.y;
				if (num25 > num24)
				{
					num24 = num25;
				}
				else if (num25 < num23)
				{
					num23 = num25;
				}
			}
			float num27 = 1f / (num24 - num23);
			helper.Clear();
			for (int m = 0; m < count; m++)
			{
				helper.AddVert(list[m]);
			}
			float num28 = (num23 + num24) / 2f;
			UIVertex v3 = new UIVertex
			{
				position = (Vector3.right + Vector3.up) * num28 + Vector3.forward * list[0].position.z,
				normal = list[0].normal,
				color = Color.white
			};
			helper.AddVert(v3);
			for (int n = 1; n < count; n++)
			{
				helper.AddTriangle(n - 1, n, count);
			}
			helper.AddTriangle(0, count - 1, count);
			UIVertex vertex3 = default(UIVertex);
			for (int num29 = 0; num29 < helper.currentVertCount; num29++)
			{
				helper.PopulateUIVertex(ref vertex3, num29);
				vertex3.color = BlendColor(vertex3.color, EffectGradient.Evaluate(Vector3.Distance(vertex3.position, v3.position) * num27 - Offset));
				helper.SetUIVertex(vertex3, num29);
			}
			break;
		}
		case Type.Radial:
		{
			float num = list[0].position.x;
			float num2 = list[0].position.x;
			float num3 = list[0].position.y;
			float num4 = list[0].position.y;
			float num5 = 0f;
			float num6 = 0f;
			for (int num7 = count - 1; num7 >= 1; num7--)
			{
				num5 = list[num7].position.x;
				if (num5 > num2)
				{
					num2 = num5;
				}
				else if (num5 < num)
				{
					num = num5;
				}
				num6 = list[num7].position.y;
				if (num6 > num4)
				{
					num4 = num6;
				}
				else if (num6 < num3)
				{
					num3 = num6;
				}
			}
			float num8 = 1f / (num2 - num);
			float num9 = 1f / (num4 - num3);
			helper.Clear();
			float num10 = (num2 + num) / 2f;
			float num11 = (num3 + num4) / 2f;
			float num12 = (num2 - num) / 2f;
			float num13 = (num4 - num3) / 2f;
			UIVertex v = new UIVertex
			{
				position = Vector3.right * num10 + Vector3.up * num11 + Vector3.forward * list[0].position.z,
				normal = list[0].normal,
				color = Color.white
			};
			int num14 = 64;
			for (int i = 0; i < num14; i++)
			{
				UIVertex v2 = default(UIVertex);
				float num15 = (float)i * 360f / (float)num14;
				float num16 = Mathf.Cos((float)Math.PI / 180f * num15) * num12;
				float num17 = Mathf.Sin((float)Math.PI / 180f * num15) * num13;
				v2.position = Vector3.right * num16 + Vector3.up * num17 + Vector3.forward * list[0].position.z;
				v2.normal = list[0].normal;
				v2.color = Color.white;
				helper.AddVert(v2);
			}
			helper.AddVert(v);
			for (int j = 1; j < num14; j++)
			{
				helper.AddTriangle(j - 1, j, num14);
			}
			helper.AddTriangle(0, num14 - 1, num14);
			UIVertex vertex = default(UIVertex);
			for (int k = 0; k < helper.currentVertCount; k++)
			{
				helper.PopulateUIVertex(ref vertex, k);
				vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate(Mathf.Sqrt(Mathf.Pow(Mathf.Abs(vertex.position.x - num10) * num8, 2f) + Mathf.Pow(Mathf.Abs(vertex.position.y - num11) * num9, 2f)) * 2f - Offset));
				helper.SetUIVertex(vertex, k);
			}
			break;
		}
		}
	}

	private Color BlendColor(Color colorA, Color colorB)
	{
		return BlendMode switch
		{
			Blend.Add => colorA + colorB, 
			Blend.Multiply => colorA * colorB, 
			_ => colorB, 
		};
	}
}
