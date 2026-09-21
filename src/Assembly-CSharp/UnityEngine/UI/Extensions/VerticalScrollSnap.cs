using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("Layout/Extensions/Vertical Scroll Snap")]
public class VerticalScrollSnap : ScrollSnapBase, IEndDragHandler, IEventSystemHandler
{
	private void Start()
	{
		_isVertical = true;
		_childAnchorPoint = new Vector2(0.5f, 0f);
		_currentPage = StartingScreen;
		panelDimensions = base.gameObject.GetComponent<RectTransform>().rect;
		UpdateLayout();
	}

	private void Update()
	{
		if (!_lerp && _scroll_rect.velocity == Vector2.zero)
		{
			if (!_settled && !_pointerDown && !IsRectSettledOnaPage(_screensContainer.localPosition))
			{
				ScrollToClosestElement();
			}
			return;
		}
		if (_lerp)
		{
			_screensContainer.localPosition = Vector3.Lerp(_screensContainer.localPosition, _lerp_target, transitionSpeed * Time.deltaTime);
			if (Vector3.Distance(_screensContainer.localPosition, _lerp_target) < 0.1f)
			{
				_screensContainer.localPosition = _lerp_target;
				_lerp = false;
				EndScreenChange();
			}
		}
		base.CurrentPage = GetPageforPosition(_screensContainer.localPosition);
		if (!_pointerDown && ((double)_scroll_rect.velocity.y > 0.01 || (double)_scroll_rect.velocity.y < -0.01) && IsRectMovingSlowerThanThreshold(0f))
		{
			ScrollToClosestElement();
		}
	}

	private bool IsRectMovingSlowerThanThreshold(float startingSpeed)
	{
		if (!(_scroll_rect.velocity.y > startingSpeed) || !(_scroll_rect.velocity.y < (float)SwipeVelocityThreshold))
		{
			if (_scroll_rect.velocity.y < startingSpeed)
			{
				return _scroll_rect.velocity.y > (float)(-SwipeVelocityThreshold);
			}
			return false;
		}
		return true;
	}

	public void DistributePages()
	{
		_screens = _screensContainer.childCount;
		_scroll_rect.verticalNormalizedPosition = 0f;
		float num = 0f;
		float num2 = 0f;
		Rect rect = base.gameObject.GetComponent<RectTransform>().rect;
		float num3 = 0f;
		float num4 = (_childSize = (float)(int)rect.height * ((PageStep == 0f) ? 3f : PageStep));
		for (int i = 0; i < _screensContainer.transform.childCount; i++)
		{
			RectTransform component = _screensContainer.transform.GetChild(i).gameObject.GetComponent<RectTransform>();
			num3 = num + (float)i * num4;
			component.sizeDelta = new Vector2(rect.width, rect.height);
			component.anchoredPosition = new Vector2(0f, num3);
			Vector2 vector = (component.pivot = _childAnchorPoint);
			Vector2 anchorMin = (component.anchorMax = vector);
			component.anchorMin = anchorMin;
		}
		num2 = num3 + num * -1f;
		_screensContainer.GetComponent<RectTransform>().offsetMax = new Vector2(0f, num2);
	}

	public void AddChild(GameObject GO)
	{
		AddChild(GO, WorldPositionStays: false);
	}

	public void AddChild(GameObject GO, bool WorldPositionStays)
	{
		_scroll_rect.verticalNormalizedPosition = 0f;
		GO.transform.SetParent(_screensContainer, WorldPositionStays);
		InitialiseChildObjectsFromScene();
		DistributePages();
		if ((bool)MaskArea)
		{
			UpdateVisible();
		}
		SetScrollContainerPosition();
	}

	public void RemoveChild(int index, out GameObject ChildRemoved)
	{
		RemoveChild(index, WorldPositionStays: false, out ChildRemoved);
	}

	public void RemoveChild(int index, bool WorldPositionStays, out GameObject ChildRemoved)
	{
		ChildRemoved = null;
		if (index >= 0 && index <= _screensContainer.childCount)
		{
			_scroll_rect.verticalNormalizedPosition = 0f;
			Transform child = _screensContainer.transform.GetChild(index);
			child.SetParent(null, WorldPositionStays);
			ChildRemoved = child.gameObject;
			InitialiseChildObjectsFromScene();
			DistributePages();
			if ((bool)MaskArea)
			{
				UpdateVisible();
			}
			if (_currentPage > _screens - 1)
			{
				base.CurrentPage = _screens - 1;
			}
			SetScrollContainerPosition();
		}
	}

	public void RemoveAllChildren(out GameObject[] ChildrenRemoved)
	{
		RemoveAllChildren(WorldPositionStays: false, out ChildrenRemoved);
	}

	public void RemoveAllChildren(bool WorldPositionStays, out GameObject[] ChildrenRemoved)
	{
		int childCount = _screensContainer.childCount;
		ChildrenRemoved = new GameObject[childCount];
		for (int num = childCount - 1; num >= 0; num--)
		{
			ChildrenRemoved[num] = _screensContainer.GetChild(num).gameObject;
			ChildrenRemoved[num].transform.SetParent(null, WorldPositionStays);
		}
		_scroll_rect.verticalNormalizedPosition = 0f;
		base.CurrentPage = 0;
		InitialiseChildObjectsFromScene();
		DistributePages();
		if ((bool)MaskArea)
		{
			UpdateVisible();
		}
	}

	private void SetScrollContainerPosition()
	{
		_scrollStartPosition = _screensContainer.localPosition.y;
		_scroll_rect.verticalNormalizedPosition = (float)_currentPage / (float)(_screens - 1);
		OnCurrentScreenChange(_currentPage);
	}

	public void UpdateLayout()
	{
		_lerp = false;
		DistributePages();
		if ((bool)MaskArea)
		{
			UpdateVisible();
		}
		SetScrollContainerPosition();
		OnCurrentScreenChange(_currentPage);
	}

	private void OnRectTransformDimensionsChange()
	{
		if (_childAnchorPoint != Vector2.zero)
		{
			UpdateLayout();
		}
	}

	private void OnEnable()
	{
		InitialiseChildObjectsFromScene();
		DistributePages();
		if ((bool)MaskArea)
		{
			UpdateVisible();
		}
		if (JumpOnEnable || !RestartOnEnable)
		{
			SetScrollContainerPosition();
		}
		if (RestartOnEnable)
		{
			GoToScreen(StartingScreen);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_pointerDown = false;
		if (!_scroll_rect.vertical)
		{
			return;
		}
		float num = Vector3.Distance(_startPosition, _screensContainer.localPosition);
		if (UseFastSwipe && num < panelDimensions.height + (float)FastSwipeThreshold && num >= 1f)
		{
			_scroll_rect.velocity = Vector3.zero;
			if (_startPosition.y - _screensContainer.localPosition.y > 0f)
			{
				NextScreen();
			}
			else
			{
				PreviousScreen();
			}
		}
	}
}
