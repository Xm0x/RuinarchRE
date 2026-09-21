using UnityEngine;
using UnityEngine.EventSystems;

namespace UtilityScripts;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class OtherRectFollowsThisRect : UIBehaviour
{
	private RectTransform _rectTransform;

	[SerializeField]
	private RectTransform _follower;

	[SerializeField]
	private Vector2 _padding;

	[SerializeField]
	private bool followWidth = true;

	[SerializeField]
	private bool followHeight = true;

	[SerializeField]
	private Vector2 _minimumSize;

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
		if (_rectTransform != null && _follower != null)
		{
			Vector2 sizeDelta = _follower.sizeDelta;
			if (followWidth)
			{
				sizeDelta.x = _rectTransform.sizeDelta.x;
				sizeDelta.x += _padding.x;
			}
			if (followHeight)
			{
				sizeDelta.y = _rectTransform.sizeDelta.y;
				sizeDelta.y += _padding.y;
			}
			if (_minimumSize.x > 0f && sizeDelta.x < _minimumSize.x)
			{
				sizeDelta.x = _minimumSize.x;
			}
			if (_minimumSize.y > 0f && sizeDelta.y < _minimumSize.y)
			{
				sizeDelta.y = _minimumSize.y;
			}
			if (_follower != null)
			{
				_follower.sizeDelta = sizeDelta;
			}
		}
	}
}
