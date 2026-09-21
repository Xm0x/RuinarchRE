using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Effects/Extensions/Gradient")]
public class Gradient : BaseMeshEffect
{
	[SerializeField]
	private GradientMode _gradientMode;

	[SerializeField]
	private GradientDir _gradientDir;

	[SerializeField]
	private bool _overwriteAllColor;

	[SerializeField]
	private Color _vertex1 = Color.white;

	[SerializeField]
	private Color _vertex2 = Color.black;

	private Graphic targetGraphic;

	public GradientMode GradientMode
	{
		get
		{
			return _gradientMode;
		}
		set
		{
			_gradientMode = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public GradientDir GradientDir
	{
		get
		{
			return _gradientDir;
		}
		set
		{
			_gradientDir = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public bool OverwriteAllColor
	{
		get
		{
			return _overwriteAllColor;
		}
		set
		{
			_overwriteAllColor = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public Color Vertex1
	{
		get
		{
			return _vertex1;
		}
		set
		{
			_vertex1 = value;
			base.graphic.SetAllDirty();
		}
	}

	public Color Vertex2
	{
		get
		{
			return _vertex2;
		}
		set
		{
			_vertex2 = value;
			base.graphic.SetAllDirty();
		}
	}

	protected override void Awake()
	{
		targetGraphic = GetComponent<Graphic>();
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		int currentVertCount = vh.currentVertCount;
		if (!IsActive() || currentVertCount == 0)
		{
			return;
		}
		List<UIVertex> list = new List<UIVertex>();
		vh.GetUIVertexStream(list);
		UIVertex vertex = default(UIVertex);
		if (_gradientMode == GradientMode.Global)
		{
			if (_gradientDir == GradientDir.DiagonalLeftToRight || _gradientDir == GradientDir.DiagonalRightToLeft)
			{
				_gradientDir = GradientDir.Vertical;
			}
			float num = ((_gradientDir == GradientDir.Vertical) ? list[list.Count - 1].position.y : list[list.Count - 1].position.x);
			float num2 = ((_gradientDir == GradientDir.Vertical) ? list[0].position.y : list[0].position.x) - num;
			for (int i = 0; i < currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref vertex, i);
				if (_overwriteAllColor || !(vertex.color != targetGraphic.color))
				{
					ref Color32 color = ref vertex.color;
					color *= Color.Lerp(_vertex2, _vertex1, (((_gradientDir == GradientDir.Vertical) ? vertex.position.y : vertex.position.x) - num) / num2);
					vh.SetUIVertex(vertex, i);
				}
			}
			return;
		}
		for (int j = 0; j < currentVertCount; j++)
		{
			vh.PopulateUIVertex(ref vertex, j);
			if (_overwriteAllColor || CompareCarefully(vertex.color, targetGraphic.color))
			{
				switch (_gradientDir)
				{
				case GradientDir.Vertical:
				{
					ref Color32 color4 = ref vertex.color;
					color4 *= ((j % 4 == 0 || (j - 1) % 4 == 0) ? _vertex1 : _vertex2);
					break;
				}
				case GradientDir.Horizontal:
				{
					ref Color32 color5 = ref vertex.color;
					color5 *= ((j % 4 == 0 || (j - 3) % 4 == 0) ? _vertex1 : _vertex2);
					break;
				}
				case GradientDir.DiagonalLeftToRight:
				{
					ref Color32 color3 = ref vertex.color;
					color3 *= ((j % 4 == 0) ? _vertex1 : (((j - 2) % 4 == 0) ? _vertex2 : Color.Lerp(_vertex2, _vertex1, 0.5f)));
					break;
				}
				case GradientDir.DiagonalRightToLeft:
				{
					ref Color32 color2 = ref vertex.color;
					color2 *= (((j - 1) % 4 == 0) ? _vertex1 : (((j - 3) % 4 == 0) ? _vertex2 : Color.Lerp(_vertex2, _vertex1, 0.5f)));
					break;
				}
				}
				vh.SetUIVertex(vertex, j);
			}
		}
	}

	private bool CompareCarefully(Color col1, Color col2)
	{
		if (Mathf.Abs(col1.r - col2.r) < 0.003f && Mathf.Abs(col1.g - col2.g) < 0.003f && Mathf.Abs(col1.b - col2.b) < 0.003f && Mathf.Abs(col1.a - col2.a) < 0.003f)
		{
			return true;
		}
		return false;
	}
}
