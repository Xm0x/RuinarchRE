using UnityEngine;

public class CharacterPointerUI : MonoBehaviour
{
	public static CharacterPointerUI Instance;

	[SerializeField]
	private RectTransform _parentRectTransform;

	[SerializeField]
	private RectTransform _arrowPointerTransform;

	[SerializeField]
	private float _yOffsetCharacterArrowPosition;

	private Transform _targetTransform;

	private Vector3 _arrowWorldPosition;

	private Vector2 _pivotSize;

	private float _xMinPosition => _parentRectTransform.offsetMin.x;

	private float _yMinPosition => _parentRectTransform.offsetMin.y;

	private float _xMaxPosition => UIManager.Instance.canvasScaler.referenceResolution.x + _parentRectTransform.offsetMax.x;

	private float _yMaxPosition => UIManager.Instance.canvasScaler.referenceResolution.y + _parentRectTransform.offsetMax.y;

	private float _xMinPositionWithPivot => _pivotSize.x + _xMinPosition;

	private float _yMinPositionWithPivot => _pivotSize.y + _yMinPosition;

	private float _xMaxPositionWithPivot => _xMaxPosition - _pivotSize.x;

	private float _yMaxPositionWithPivot => _yMaxPosition - _pivotSize.y;

	private Vector2 _maxPositionWithPivotAndScaling => new Vector2(_xMaxPositionWithPivot * scaleFactorX, _yMaxPositionWithPivot * scaleFactorY);

	private Vector2 _minPositionWithPivotAndScaling => new Vector2(_xMinPositionWithPivot * scaleFactorX, _yMinPositionWithPivot * scaleFactorY);

	private Vector2 _maxPositionWithScalingOnly => new Vector2(_xMaxPosition * scaleFactorX, _yMaxPosition * scaleFactorY);

	private Vector2 _minPositionWithScalingOnly => new Vector2(_xMinPosition * scaleFactorX, _yMinPosition * scaleFactorY);

	private float scaleFactorX => (float)Screen.width / UIManager.Instance.canvasScaler.referenceResolution.x;

	private float scaleFactorY => (float)Screen.height / UIManager.Instance.canvasScaler.referenceResolution.y;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_pivotSize = new Vector2(_arrowPointerTransform.sizeDelta.x * _arrowPointerTransform.pivot.x, _arrowPointerTransform.sizeDelta.y * _arrowPointerTransform.pivot.y);
	}

	private void Update()
	{
		if (!(_targetTransform == null))
		{
			_arrowWorldPosition = GridMap.Instance.mainRegion.innerMap.GetWorldPositionFromScreenPosition(_arrowPointerTransform.position);
			Vector3 screenPositionFromWorldPosition = GridMap.Instance.mainRegion.innerMap.GetScreenPositionFromWorldPosition(_targetTransform.position);
			screenPositionFromWorldPosition.z = 0f;
			if (IsPositionInsideCoordinates(screenPositionFromWorldPosition, _minPositionWithScalingOnly, _maxPositionWithScalingOnly))
			{
				PositionArrowOnCharacter(screenPositionFromWorldPosition);
				return;
			}
			MoveTo(screenPositionFromWorldPosition);
			LookAt(_targetTransform.position);
		}
	}

	public void SetTargetTransform(Transform p_target)
	{
		_targetTransform = p_target;
		_arrowPointerTransform.gameObject.SetActive(_targetTransform != null);
	}

	private void LookAt(Vector3 p_targetWorldPosition)
	{
		Vector3 vector = p_targetWorldPosition - _arrowWorldPosition;
		vector.Normalize();
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		_arrowPointerTransform.localRotation = Quaternion.Euler(0f, 0f, num - 90f);
	}

	private void MoveTo(Vector3 p_targetScreenPosition)
	{
		KeepArrowPositionFullyInRect(p_targetScreenPosition);
	}

	private void PositionArrowOnCharacter(Vector3 p_targetPosition)
	{
		p_targetPosition.y += _yOffsetCharacterArrowPosition * UIManager.Instance.canvas.scaleFactor;
		_arrowPointerTransform.position = p_targetPosition;
		_arrowPointerTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Vector3 vector = p_targetPosition;
		Vector3 vector2 = vector;
		vector2 = GetArrowPositionOutsideOtherUI(vector, DIRECTION.RIGHT);
		if (vector2.x >= _maxPositionWithPivotAndScaling.x)
		{
			vector2 = GetArrowPositionOutsideOtherUI(vector, DIRECTION.LEFT);
			if (vector2.x <= _minPositionWithPivotAndScaling.x)
			{
				vector2 = GetArrowPositionOutsideOtherUI(vector, DIRECTION.UP);
				if (vector2.y >= _maxPositionWithPivotAndScaling.y)
				{
					vector2 = GetArrowPositionOutsideOtherUI(vector, DIRECTION.DOWN);
				}
			}
		}
		if (vector2 == vector)
		{
			vector.y += _yOffsetCharacterArrowPosition * UIManager.Instance.canvas.scaleFactor;
			_arrowPointerTransform.position = vector;
			_arrowPointerTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else
		{
			_arrowPointerTransform.position = vector2;
			LookAt(_targetTransform.position);
		}
	}

	private void KeepArrowPositionFullyInRect(Vector3 p_targetScreenPosition)
	{
		Vector3 p_supposedPosition = p_targetScreenPosition;
		DIRECTION p_direction = DIRECTION.RIGHT;
		if (p_supposedPosition.x > _maxPositionWithPivotAndScaling.x)
		{
			p_supposedPosition.x = _maxPositionWithPivotAndScaling.x;
			p_direction = DIRECTION.LEFT;
		}
		if (p_supposedPosition.y > _maxPositionWithPivotAndScaling.y)
		{
			p_supposedPosition.y = _maxPositionWithPivotAndScaling.y;
			p_direction = DIRECTION.DOWN;
		}
		if (p_supposedPosition.x < _minPositionWithPivotAndScaling.x)
		{
			p_supposedPosition.x = _minPositionWithPivotAndScaling.x;
			p_direction = DIRECTION.RIGHT;
		}
		if (p_supposedPosition.y < _minPositionWithPivotAndScaling.y)
		{
			p_supposedPosition.y = _minPositionWithPivotAndScaling.y;
			p_direction = DIRECTION.UP;
		}
		p_supposedPosition = GetArrowPositionOutsideOtherUI(p_supposedPosition, p_direction);
		_arrowPointerTransform.position = p_supposedPosition;
	}

	private Vector3 GetArrowPositionOutsideOtherUI(Vector3 p_supposedPosition, DIRECTION p_direction)
	{
		Vector3 result = p_supposedPosition;
		Vector2 vector = new Vector2(_pivotSize.x * scaleFactorX, _pivotSize.y * scaleFactorY);
		for (int i = 0; i < UIManager.Instance.disallowOverlapsWithCP.Count; i++)
		{
			DisallowOverlapWithCharacterPointer disallowOverlapWithCharacterPointer = UIManager.Instance.disallowOverlapsWithCP[i];
			if (!disallowOverlapWithCharacterPointer.gameObject.activeInHierarchy)
			{
				continue;
			}
			Vector3 vector2 = disallowOverlapWithCharacterPointer.anchoredOffsetMin;
			Vector3 vector3 = disallowOverlapWithCharacterPointer.anchoredOffsetMax;
			if (RectTransformUtility.RectangleContainsScreenPoint(disallowOverlapWithCharacterPointer.rectTransform, p_supposedPosition))
			{
				if (p_direction == DIRECTION.RIGHT)
				{
					result.x = vector3.x + vector.x;
				}
				if (p_direction == DIRECTION.LEFT)
				{
					result.x = vector2.x - vector.x;
				}
				if (p_direction == DIRECTION.UP)
				{
					result.y = vector3.y + vector.y;
				}
				if (p_direction == DIRECTION.DOWN)
				{
					result.y = vector2.y - vector.y;
				}
			}
		}
		return result;
	}

	private bool IsPositionInsideCoordinates(Vector3 p_supposedPosition, Vector3 p_minPosition, Vector3 p_maxPosition)
	{
		Vector3 vector = p_minPosition;
		Vector3 vector2 = p_maxPosition;
		if (p_supposedPosition.x >= vector.x && p_supposedPosition.x <= vector2.x && p_supposedPosition.y >= vector.y && p_supposedPosition.y <= vector2.y)
		{
			return true;
		}
		return false;
	}
}
