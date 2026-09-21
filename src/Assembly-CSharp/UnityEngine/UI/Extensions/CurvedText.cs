namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(Text), typeof(RectTransform))]
[AddComponentMenu("UI/Effects/Extensions/Curved Text")]
public class CurvedText : BaseMeshEffect
{
	[SerializeField]
	private AnimationCurve _curveForText = AnimationCurve.Linear(0f, 0f, 1f, 10f);

	[SerializeField]
	private float _curveMultiplier = 1f;

	private RectTransform rectTrans;

	public AnimationCurve CurveForText
	{
		get
		{
			return _curveForText;
		}
		set
		{
			_curveForText = value;
			base.graphic.SetVerticesDirty();
		}
	}

	public float CurveMultiplier
	{
		get
		{
			return _curveMultiplier;
		}
		set
		{
			_curveMultiplier = value;
			base.graphic.SetVerticesDirty();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		rectTrans = GetComponent<RectTransform>();
		OnRectTransformDimensionsChange();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		rectTrans = GetComponent<RectTransform>();
		OnRectTransformDimensionsChange();
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		int currentVertCount = vh.currentVertCount;
		if (IsActive() && currentVertCount != 0)
		{
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				UIVertex vertex = default(UIVertex);
				vh.PopulateUIVertex(ref vertex, i);
				vertex.position.y += _curveForText.Evaluate(rectTrans.rect.width * rectTrans.pivot.x + vertex.position.x) * _curveMultiplier;
				vh.SetUIVertex(vertex, i);
			}
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		if ((bool)rectTrans)
		{
			Keyframe key = _curveForText[_curveForText.length - 1];
			key.time = rectTrans.rect.width;
			_curveForText.MoveKey(_curveForText.length - 1, key);
		}
	}
}
