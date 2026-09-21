using UnityEngine;
using UnityEngine.EventSystems;

namespace UtilityScripts;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class MaxRectTransform : UIBehaviour
{
	private RectTransform _rectTransform;

	[SerializeField]
	private bool _followMaxX;

	[SerializeField]
	private bool _followMaxY;

	[SerializeField]
	private float _maxX;

	[SerializeField]
	private float _maxY;

	public float maxX => _maxX;

	protected override void Awake()
	{
		_rectTransform = (RectTransform)base.transform;
	}

	protected override void OnEnable()
	{
		OnRectTransformDimensionsChange();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		if (_rectTransform != null)
		{
			float x = _rectTransform.sizeDelta.x;
			float num = _rectTransform.sizeDelta.y;
			if (x > _maxX && _followMaxX)
			{
				x = _maxX;
			}
			if (num > _maxY && _followMaxY)
			{
				num = _maxY;
			}
			_rectTransform.sizeDelta = new Vector2(x, num);
		}
	}
}
