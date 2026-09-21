using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("Layout/Extensions/Horizontal Scroll Snap")]
public class HorizontalScrollSnap : ScrollSnapBase, IEndDragHandler, IEventSystemHandler
{
	private void Start()
	{
		_isVertical = false;
		_childAnchorPoint = new Vector2(0f, 0.5f);
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
			if (Vector3.Distance(_screensContainer.localPosition, _lerp_target) <= 0.1f)
			{
				_screensContainer.localPosition = _lerp_target;
				_lerp = false;
				EndScreenChange();
			}
		}
		if (!_pointerDown && ((double)_scroll_rect.velocity.x > 0.01 || (double)_scroll_rect.velocity.x < 0.01) && IsRectMovingSlowerThanThreshold(0f))
		{
			ScrollToClosestElement();
		}
	}

	private bool IsRectMovingSlowerThanThreshold(float startingSpeed)
	{
		if (!(_scroll_rect.velocity.x > startingSpeed) || !(_scroll_rect.velocity.x < (float)SwipeVelocityThreshold))
		{
			if (_scroll_rect.velocity.x < startingSpeed)
			{
				return _scroll_rect.velocity.x > (float)(-SwipeVelocityThreshold);
			}
			return false;
		}
		return true;
	}

	private void DistributePages()
	{
		_screens = _screensContainer.childCount;
		_scroll_rect.horizontalNormalizedPosition = 0f;
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = (_childSize = (float)(int)panelDimensions.width * ((PageStep == 0f) ? 3f : PageStep));
		for (int i = 0; i < _screensContainer.transform.childCount; i++)
		{
			RectTransform component = _screensContainer.transform.GetChild(i).gameObject.GetComponent<RectTransform>();
			num3 = num + (int)((float)i * num4);
			component.sizeDelta = new Vector2(panelDimensions.width, panelDimensions.height);
			component.anchoredPosition = new Vector2(num3, 0f);
			Vector2 vector = (component.pivot = _childAnchorPoint);
			Vector2 anchorMin = (component.anchorMax = vector);
			component.anchorMin = anchorMin;
		}
		num2 = num3 + (float)(num * -1);
		_screensContainer.offsetMax = new Vector2(num2, 0f);
	}

	public void AddChild(GameObject GO)
	{
		AddChild(GO, WorldPositionStays: false);
	}

	public void AddChild(GameObject GO, bool WorldPositionStays)
	{
		_scroll_rect.horizontalNormalizedPosition = 0f;
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
			_scroll_rect.horizontalNormalizedPosition = 0f;
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
		_scroll_rect.horizontalNormalizedPosition = 0f;
		base.CurrentPage = 0;
		InitialiseChildObjectsFromScene();
		DistributePages();
		if ((bool)MaskArea)
		{
			UpdateVisible();
		}
	}

	public void UpdateChildrenAndPagination()
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
		if ((bool)Pagination)
		{
			Pagination.GetComponent<PaginationManager>().ReInitializeChildren();
		}
	}

	private void SetScrollContainerPosition()
	{
		_scrollStartPosition = _screensContainer.localPosition.x;
		float num = (float)_currentPage / (float)(_screens - 1);
		if (float.IsInfinity(num))
		{
			num = 0f;
		}
		_scroll_rect.horizontalNormalizedPosition = num;
		OnCurrentScreenChange(_currentPage);
	}

	public IEnumerator UpdateLayoutCoroutine()
	{
		yield return null;
		UpdateLayout();
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
		if (!_scroll_rect.horizontal)
		{
			return;
		}
		float num = Vector3.Distance(_startPosition, _screensContainer.localPosition);
		if (UseFastSwipe && num < panelDimensions.width && num >= (float)FastSwipeThreshold)
		{
			_scroll_rect.velocity = Vector3.zero;
			if (_startPosition.x - _screensContainer.localPosition.x > 0f)
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
