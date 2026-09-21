using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Graphic))]
[DisallowMultipleComponent]
[AddComponentMenu("UI/Effects/Extensions/Curly UI Graphic")]
public class CUIGraphic : BaseMeshEffect
{
	public static readonly int bottomCurveIdx = 0;

	public static readonly int topCurveIdx = 1;

	[Tooltip("Set true to make the curve/morph to work. Set false to quickly see the original UI.")]
	[SerializeField]
	protected bool isCurved = true;

	[Tooltip("Set true to dynamically change the curve according to the dynamic change of the UI layout")]
	[SerializeField]
	protected bool isLockWithRatio = true;

	[Tooltip("Pick a higher resolution to improve the quality of the curved graphic.")]
	[SerializeField]
	[Range(0.01f, 30f)]
	protected float resolution = 5f;

	protected RectTransform rectTrans;

	[Tooltip("Put in the Graphic you want to curve/morph here.")]
	[SerializeField]
	protected Graphic uiGraphic;

	[Tooltip("Put in the reference Graphic that will be used to tune the bezier curves. Think button image and text.")]
	[SerializeField]
	protected CUIGraphic refCUIGraphic;

	[Tooltip("Do not touch this unless you are sure what you are doing. The curves are (re)generated automatically.")]
	[SerializeField]
	protected CUIBezierCurve[] refCurves;

	[HideInInspector]
	[SerializeField]
	protected Vector3_Array2D[] refCurvesControlRatioPoints;

	protected List<UIVertex> reuse_quads = new List<UIVertex>();

	public bool IsCurved => isCurved;

	public bool IsLockWithRatio => isLockWithRatio;

	public RectTransform RectTrans => rectTrans;

	public Graphic UIGraphic => uiGraphic;

	public CUIGraphic RefCUIGraphic => refCUIGraphic;

	public CUIBezierCurve[] RefCurves => refCurves;

	public Vector3_Array2D[] RefCurvesControlRatioPoints => refCurvesControlRatioPoints;

	protected void solveDoubleEquationWithVector(float _x_1, float _y_1, float _x_2, float _y_2, Vector3 _constant_1, Vector3 _contant_2, out Vector3 _x, out Vector3 _y)
	{
		if (Mathf.Abs(_x_1) > Mathf.Abs(_x_2))
		{
			Vector3 vector = _constant_1 * _x_2 / _x_1;
			float num = _y_1 * _x_2 / _x_1;
			_y = (_contant_2 - vector) / (_y_2 - num);
			if (_x_2 != 0f)
			{
				_x = (vector - num * _y) / _x_2;
			}
			else
			{
				_x = (_constant_1 - _y_1 * _y) / _x_1;
			}
		}
		else
		{
			Vector3 vector = _contant_2 * _x_1 / _x_2;
			float num = _y_2 * _x_1 / _x_2;
			_x = (_constant_1 - vector) / (_y_1 - num);
			if (_x_1 != 0f)
			{
				_y = (vector - num * _x) / _x_1;
			}
			else
			{
				_y = (_contant_2 - _y_2 * _x) / _x_2;
			}
		}
	}

	protected UIVertex uiVertexLerp(UIVertex _a, UIVertex _b, float _time)
	{
		return new UIVertex
		{
			position = Vector3.Lerp(_a.position, _b.position, _time),
			normal = Vector3.Lerp(_a.normal, _b.normal, _time),
			tangent = Vector3.Lerp(_a.tangent, _b.tangent, _time),
			uv0 = Vector2.Lerp(_a.uv0, _b.uv0, _time),
			uv1 = Vector2.Lerp(_a.uv1, _b.uv1, _time),
			color = Color.Lerp(_a.color, _b.color, _time)
		};
	}

	protected UIVertex uiVertexBerp(UIVertex v_bottomLeft, UIVertex v_topLeft, UIVertex v_topRight, UIVertex v_bottomRight, float _xTime, float _yTime)
	{
		UIVertex b = uiVertexLerp(v_topLeft, v_topRight, _xTime);
		UIVertex a = uiVertexLerp(v_bottomLeft, v_bottomRight, _xTime);
		return uiVertexLerp(a, b, _yTime);
	}

	protected void tessellateQuad(List<UIVertex> _quads, int _thisQuadIdx)
	{
		UIVertex v_bottomLeft = _quads[_thisQuadIdx];
		UIVertex v_topLeft = _quads[_thisQuadIdx + 1];
		UIVertex v_topRight = _quads[_thisQuadIdx + 2];
		UIVertex v_bottomRight = _quads[_thisQuadIdx + 3];
		float num = 100f / resolution;
		int num2 = Mathf.Max(1, Mathf.CeilToInt((v_topLeft.position - v_bottomLeft.position).magnitude / num));
		int num3 = Mathf.Max(1, Mathf.CeilToInt((v_topRight.position - v_topLeft.position).magnitude / num));
		int num4 = 0;
		for (int i = 0; i < num3; i++)
		{
			int num5 = 0;
			while (num5 < num2)
			{
				_quads.Add(default(UIVertex));
				_quads.Add(default(UIVertex));
				_quads.Add(default(UIVertex));
				_quads.Add(default(UIVertex));
				float xTime = (float)i / (float)num3;
				float yTime = (float)num5 / (float)num2;
				float xTime2 = (float)(i + 1) / (float)num3;
				float yTime2 = (float)(num5 + 1) / (float)num2;
				_quads[_quads.Count - 4] = uiVertexBerp(v_bottomLeft, v_topLeft, v_topRight, v_bottomRight, xTime, yTime);
				_quads[_quads.Count - 3] = uiVertexBerp(v_bottomLeft, v_topLeft, v_topRight, v_bottomRight, xTime, yTime2);
				_quads[_quads.Count - 2] = uiVertexBerp(v_bottomLeft, v_topLeft, v_topRight, v_bottomRight, xTime2, yTime2);
				_quads[_quads.Count - 1] = uiVertexBerp(v_bottomLeft, v_topLeft, v_topRight, v_bottomRight, xTime2, yTime);
				num5++;
				num4++;
			}
		}
	}

	protected void tessellateGraphic(List<UIVertex> _verts)
	{
		for (int i = 0; i < _verts.Count; i += 6)
		{
			reuse_quads.Add(_verts[i]);
			reuse_quads.Add(_verts[i + 1]);
			reuse_quads.Add(_verts[i + 2]);
			reuse_quads.Add(_verts[i + 4]);
		}
		int num = reuse_quads.Count / 4;
		for (int j = 0; j < num; j++)
		{
			tessellateQuad(reuse_quads, j * 4);
		}
		reuse_quads.RemoveRange(0, num * 4);
		_verts.Clear();
		for (int k = 0; k < reuse_quads.Count; k += 4)
		{
			_verts.Add(reuse_quads[k]);
			_verts.Add(reuse_quads[k + 1]);
			_verts.Add(reuse_quads[k + 2]);
			_verts.Add(reuse_quads[k + 2]);
			_verts.Add(reuse_quads[k + 3]);
			_verts.Add(reuse_quads[k]);
		}
		reuse_quads.Clear();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		if (isLockWithRatio)
		{
			UpdateCurveControlPointPositions();
		}
	}

	public void Refresh()
	{
		ReportSet();
		for (int i = 0; i < refCurves.Length; i++)
		{
			CUIBezierCurve cUIBezierCurve = refCurves[i];
			if (cUIBezierCurve.ControlPoints != null)
			{
				Vector3[] controlPoints = cUIBezierCurve.ControlPoints;
				for (int j = 0; j < CUIBezierCurve.CubicBezierCurvePtNum; j++)
				{
					Vector3 value = controlPoints[j];
					value.x = (value.x + rectTrans.rect.width * rectTrans.pivot.x) / rectTrans.rect.width;
					value.y = (value.y + rectTrans.rect.height * rectTrans.pivot.y) / rectTrans.rect.height;
					refCurvesControlRatioPoints[i][j] = value;
				}
			}
		}
		if (uiGraphic != null)
		{
			uiGraphic.enabled = false;
			uiGraphic.enabled = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		OnRectTransformDimensionsChange();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		OnRectTransformDimensionsChange();
	}

	public virtual void ReportSet()
	{
		if (rectTrans == null)
		{
			rectTrans = GetComponent<RectTransform>();
		}
		if (refCurves == null)
		{
			refCurves = new CUIBezierCurve[2];
		}
		bool flag = true;
		for (int i = 0; i < 2; i++)
		{
			flag &= refCurves[i] != null;
		}
		if (!(flag & (refCurves.Length == 2)))
		{
			CUIBezierCurve[] array = refCurves;
			for (int j = 0; j < 2; j++)
			{
				if (refCurves[j] == null)
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.SetParent(base.transform);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localEulerAngles = Vector3.zero;
					if (j == 0)
					{
						gameObject.name = "BottomRefCurve";
					}
					else
					{
						gameObject.name = "TopRefCurve";
					}
					array[j] = gameObject.AddComponent<CUIBezierCurve>();
				}
				else
				{
					array[j] = refCurves[j];
				}
				array[j].ReportSet();
			}
			refCurves = array;
		}
		if (refCurvesControlRatioPoints == null)
		{
			refCurvesControlRatioPoints = new Vector3_Array2D[refCurves.Length];
			for (int k = 0; k < refCurves.Length; k++)
			{
				refCurvesControlRatioPoints[k].array = new Vector3[refCurves[k].ControlPoints.Length];
			}
			FixTextToRectTrans();
			Refresh();
		}
		for (int l = 0; l < 2; l++)
		{
			refCurves[l].OnRefresh = Refresh;
		}
	}

	public void FixTextToRectTrans()
	{
		for (int i = 0; i < refCurves.Length; i++)
		{
			CUIBezierCurve cUIBezierCurve = refCurves[i];
			for (int j = 0; j < CUIBezierCurve.CubicBezierCurvePtNum; j++)
			{
				if (cUIBezierCurve.ControlPoints != null)
				{
					Vector3[] controlPoints = cUIBezierCurve.ControlPoints;
					if (i == 0)
					{
						controlPoints[j].y = (0f - rectTrans.rect.height) * rectTrans.pivot.y;
					}
					else
					{
						controlPoints[j].y = rectTrans.rect.height - rectTrans.rect.height * rectTrans.pivot.y;
					}
					controlPoints[j].x = rectTrans.rect.width * (float)j / (float)(CUIBezierCurve.CubicBezierCurvePtNum - 1);
					controlPoints[j].x -= rectTrans.rect.width * rectTrans.pivot.x;
					controlPoints[j].z = 0f;
				}
			}
		}
	}

	public void ReferenceCUIForBCurves()
	{
		Vector3 localPosition = rectTrans.localPosition;
		localPosition.x += (0f - rectTrans.rect.width) * rectTrans.pivot.x + refCUIGraphic.rectTrans.rect.width * refCUIGraphic.rectTrans.pivot.x;
		localPosition.y += (0f - rectTrans.rect.height) * rectTrans.pivot.y + refCUIGraphic.rectTrans.rect.height * refCUIGraphic.rectTrans.pivot.y;
		Vector3 vector = new Vector3(localPosition.x / refCUIGraphic.RectTrans.rect.width, localPosition.y / refCUIGraphic.RectTrans.rect.height, localPosition.z);
		Vector3 vector2 = new Vector3((localPosition.x + rectTrans.rect.width) / refCUIGraphic.RectTrans.rect.width, (localPosition.y + rectTrans.rect.height) / refCUIGraphic.RectTrans.rect.height, localPosition.z);
		refCurves[0].ControlPoints[0] = refCUIGraphic.GetBCurveSandwichSpacePoint(vector.x, vector.y) - rectTrans.localPosition;
		refCurves[0].ControlPoints[3] = refCUIGraphic.GetBCurveSandwichSpacePoint(vector2.x, vector.y) - rectTrans.localPosition;
		refCurves[1].ControlPoints[0] = refCUIGraphic.GetBCurveSandwichSpacePoint(vector.x, vector2.y) - rectTrans.localPosition;
		refCurves[1].ControlPoints[3] = refCUIGraphic.GetBCurveSandwichSpacePoint(vector2.x, vector2.y) - rectTrans.localPosition;
		for (int i = 0; i < refCurves.Length; i++)
		{
			CUIBezierCurve obj = refCurves[i];
			float yTime = ((i == 0) ? vector.y : vector2.y);
			Vector3 bCurveSandwichSpacePoint = refCUIGraphic.GetBCurveSandwichSpacePoint(vector.x, yTime);
			Vector3 bCurveSandwichSpacePoint2 = refCUIGraphic.GetBCurveSandwichSpacePoint(vector2.x, yTime);
			float num = 0.25f;
			float num2 = 0.75f;
			Vector3 bCurveSandwichSpacePoint3 = refCUIGraphic.GetBCurveSandwichSpacePoint((vector.x * 0.75f + vector2.x * 0.25f) / 1f, yTime);
			Vector3 bCurveSandwichSpacePoint4 = refCUIGraphic.GetBCurveSandwichSpacePoint((vector.x * 0.25f + vector2.x * 0.75f) / 1f, yTime);
			float x_ = 3f * num2 * num2 * num;
			float y_ = 3f * num2 * num * num;
			float x_2 = 3f * num * num * num2;
			float y_2 = 3f * num * num2 * num2;
			Vector3 constant_ = bCurveSandwichSpacePoint3 - Mathf.Pow(num2, 3f) * bCurveSandwichSpacePoint - Mathf.Pow(num, 3f) * bCurveSandwichSpacePoint2;
			Vector3 contant_ = bCurveSandwichSpacePoint4 - Mathf.Pow(num, 3f) * bCurveSandwichSpacePoint - Mathf.Pow(num2, 3f) * bCurveSandwichSpacePoint2;
			solveDoubleEquationWithVector(x_, y_, x_2, y_2, constant_, contant_, out var _x, out var _y);
			obj.ControlPoints[1] = _x - rectTrans.localPosition;
			obj.ControlPoints[2] = _y - rectTrans.localPosition;
		}
	}

	public override void ModifyMesh(Mesh _mesh)
	{
		if (!IsActive())
		{
			return;
		}
		using VertexHelper vertexHelper = new VertexHelper(_mesh);
		ModifyMesh(vertexHelper);
		vertexHelper.FillMesh(_mesh);
	}

	public override void ModifyMesh(VertexHelper _vh)
	{
		if (IsActive())
		{
			List<UIVertex> list = new List<UIVertex>();
			_vh.GetUIVertexStream(list);
			modifyVertices(list);
			_vh.Clear();
			_vh.AddUIVertexTriangleStream(list);
		}
	}

	protected virtual void modifyVertices(List<UIVertex> _verts)
	{
		if (!IsActive())
		{
			return;
		}
		tessellateGraphic(_verts);
		if (isCurved)
		{
			for (int i = 0; i < _verts.Count; i++)
			{
				UIVertex value = _verts[i];
				float xTime = (value.position.x + rectTrans.rect.width * rectTrans.pivot.x) / rectTrans.rect.width;
				float yTime = (value.position.y + rectTrans.rect.height * rectTrans.pivot.y) / rectTrans.rect.height;
				Vector3 bCurveSandwichSpacePoint = GetBCurveSandwichSpacePoint(xTime, yTime);
				value.position.x = bCurveSandwichSpacePoint.x;
				value.position.y = bCurveSandwichSpacePoint.y;
				value.position.z = bCurveSandwichSpacePoint.z;
				_verts[i] = value;
			}
		}
	}

	public void UpdateCurveControlPointPositions()
	{
		ReportSet();
		for (int i = 0; i < refCurves.Length; i++)
		{
			CUIBezierCurve cUIBezierCurve = refCurves[i];
			for (int j = 0; j < refCurves[i].ControlPoints.Length; j++)
			{
				Vector3 vector = refCurvesControlRatioPoints[i][j];
				vector.x = vector.x * rectTrans.rect.width - rectTrans.rect.width * rectTrans.pivot.x;
				vector.y = vector.y * rectTrans.rect.height - rectTrans.rect.height * rectTrans.pivot.y;
				cUIBezierCurve.ControlPoints[j] = vector;
			}
		}
	}

	public Vector3 GetBCurveSandwichSpacePoint(float _xTime, float _yTime)
	{
		return refCurves[0].GetPoint(_xTime) * (1f - _yTime) + refCurves[1].GetPoint(_xTime) * _yTime;
	}

	public Vector3 GetBCurveSandwichSpaceTangent(float _xTime, float _yTime)
	{
		return refCurves[0].GetTangent(_xTime) * (1f - _yTime) + refCurves[1].GetTangent(_xTime) * _yTime;
	}
}
