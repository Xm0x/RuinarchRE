using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[ExecuteInEditMode]
[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("UI/Extensions/Scroll Snap")]
public class ScrollSnap : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler, IScrollSnap
{
	public enum ScrollDirection
	{
		Horizontal,
		Vertical
	}

	public delegate void PageSnapChange(int page);

	private ScrollRect _scroll_rect;

	private RectTransform _scrollRectTransform;

	private Transform _listContainerTransform;

	private int _pages;

	private int _startingPage;

	private Vector3[] _pageAnchorPositions;

	private Vector3 _lerpTarget;

	private bool _lerp;

	private float _listContainerMinPosition;

	private float _listContainerMaxPosition;

	private float _listContainerSize;

	private RectTransform _listContainerRectTransform;

	private Vector2 _listContainerCachedSize;

	private float _itemSize;

	private int _itemsCount;

	private bool _startDrag = true;

	private Vector3 _positionOnDragStart;

	private int _pageOnDragStart;

	private bool _fastSwipeTimer;

	private int _fastSwipeCounter;

	private int _fastSwipeTarget = 10;

	[Tooltip("Button to go to the next page. (optional)")]
	public Button NextButton;

	[Tooltip("Button to go to the previous page. (optional)")]
	public Button PrevButton;

	[Tooltip("Number of items visible in one page of scroll frame.")]
	[Range(1f, 100f)]
	public int ItemsVisibleAtOnce = 1;

	[Tooltip("Sets minimum width of list items to 1/itemsVisibleAtOnce.")]
	public bool AutoLayoutItems = true;

	[Tooltip("If you wish to update scrollbar numberOfSteps to number of active children on list.")]
	public bool LinkScrolbarSteps;

	[Tooltip("If you wish to update scrollrect sensitivity to size of list element.")]
	public bool LinkScrolrectScrollSensitivity;

	public bool UseFastSwipe = true;

	public int FastSwipeThreshold = 100;

	public ScrollDirection direction;

	private bool fastSwipe;

	public event PageSnapChange onPageChange;

	private void Start()
	{
		_lerp = false;
		_scroll_rect = base.gameObject.GetComponent<ScrollRect>();
		_scrollRectTransform = base.gameObject.GetComponent<RectTransform>();
		_listContainerTransform = _scroll_rect.content;
		_listContainerRectTransform = _listContainerTransform.GetComponent<RectTransform>();
		UpdateListItemsSize();
		UpdateListItemPositions();
		PageChanged(CurrentPage());
		if ((bool)NextButton)
		{
			NextButton.GetComponent<Button>().onClick.AddListener(delegate
			{
				NextScreen();
			});
		}
		if ((bool)PrevButton)
		{
			PrevButton.GetComponent<Button>().onClick.AddListener(delegate
			{
				PreviousScreen();
			});
		}
		if (_scroll_rect.horizontalScrollbar != null && _scroll_rect.horizontal)
		{
			_scroll_rect.horizontalScrollbar.gameObject.AddComponent<ScrollSnapScrollbarHelper>().ss = this;
		}
		if (_scroll_rect.verticalScrollbar != null && _scroll_rect.vertical)
		{
			_scroll_rect.verticalScrollbar.gameObject.AddComponent<ScrollSnapScrollbarHelper>().ss = this;
		}
	}

	public void UpdateListItemsSize()
	{
		float num = 0f;
		float num2 = 0f;
		if (direction == ScrollDirection.Horizontal)
		{
			num = _scrollRectTransform.rect.width / (float)ItemsVisibleAtOnce;
			num2 = _listContainerRectTransform.rect.width / (float)_itemsCount;
		}
		else
		{
			num = _scrollRectTransform.rect.height / (float)ItemsVisibleAtOnce;
			num2 = _listContainerRectTransform.rect.height / (float)_itemsCount;
		}
		_itemSize = num;
		if (LinkScrolrectScrollSensitivity)
		{
			_scroll_rect.scrollSensitivity = _itemSize;
		}
		if (!AutoLayoutItems || num2 == num || _itemsCount <= 0)
		{
			return;
		}
		if (direction == ScrollDirection.Horizontal)
		{
			foreach (Transform item in _listContainerTransform)
			{
				GameObject gameObject = item.gameObject;
				if (gameObject.activeInHierarchy)
				{
					LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
					if (layoutElement == null)
					{
						layoutElement = gameObject.AddComponent<LayoutElement>();
					}
					layoutElement.minWidth = _itemSize;
				}
			}
			return;
		}
		foreach (Transform item2 in _listContainerTransform)
		{
			GameObject gameObject2 = item2.gameObject;
			if (gameObject2.activeInHierarchy)
			{
				LayoutElement layoutElement2 = gameObject2.GetComponent<LayoutElement>();
				if (layoutElement2 == null)
				{
					layoutElement2 = gameObject2.AddComponent<LayoutElement>();
				}
				layoutElement2.minHeight = _itemSize;
			}
		}
	}

	public void UpdateListItemPositions()
	{
		if (_listContainerRectTransform.rect.size.Equals(_listContainerCachedSize))
		{
			return;
		}
		int num = 0;
		foreach (Transform item in _listContainerTransform)
		{
			if (item.gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		_itemsCount = 0;
		Array.Resize(ref _pageAnchorPositions, num);
		if (num > 0)
		{
			_pages = Mathf.Max(num - ItemsVisibleAtOnce + 1, 1);
			if (direction == ScrollDirection.Horizontal)
			{
				_scroll_rect.horizontalNormalizedPosition = 0f;
				_listContainerMaxPosition = _listContainerTransform.localPosition.x;
				_scroll_rect.horizontalNormalizedPosition = 1f;
				_listContainerMinPosition = _listContainerTransform.localPosition.x;
				_listContainerSize = _listContainerMaxPosition - _listContainerMinPosition;
				for (int i = 0; i < _pages; i++)
				{
					_pageAnchorPositions[i] = new Vector3(_listContainerMaxPosition - _itemSize * (float)i, _listContainerTransform.localPosition.y, _listContainerTransform.localPosition.z);
				}
			}
			else
			{
				_scroll_rect.verticalNormalizedPosition = 1f;
				_listContainerMinPosition = _listContainerTransform.localPosition.y;
				_scroll_rect.verticalNormalizedPosition = 0f;
				_listContainerMaxPosition = _listContainerTransform.localPosition.y;
				_listContainerSize = _listContainerMaxPosition - _listContainerMinPosition;
				for (int j = 0; j < _pages; j++)
				{
					_pageAnchorPositions[j] = new Vector3(_listContainerTransform.localPosition.x, _listContainerMinPosition + _itemSize * (float)j, _listContainerTransform.localPosition.z);
				}
			}
			UpdateScrollbar(LinkScrolbarSteps);
			_startingPage = Mathf.Min(_startingPage, _pages);
			ResetPage();
		}
		if (_itemsCount != num)
		{
			PageChanged(CurrentPage());
		}
		_itemsCount = num;
		_listContainerCachedSize.Set(_listContainerRectTransform.rect.size.x, _listContainerRectTransform.rect.size.y);
	}

	public void ResetPage()
	{
		if (direction == ScrollDirection.Horizontal)
		{
			_scroll_rect.horizontalNormalizedPosition = ((_pages > 1) ? ((float)_startingPage / (float)(_pages - 1)) : 0f);
		}
		else
		{
			_scroll_rect.verticalNormalizedPosition = ((_pages > 1) ? ((float)(_pages - _startingPage - 1) / (float)(_pages - 1)) : 0f);
		}
	}

	private void UpdateScrollbar(bool linkSteps)
	{
		if (linkSteps)
		{
			if (direction == ScrollDirection.Horizontal)
			{
				if (_scroll_rect.horizontalScrollbar != null)
				{
					_scroll_rect.horizontalScrollbar.numberOfSteps = _pages;
				}
			}
			else if (_scroll_rect.verticalScrollbar != null)
			{
				_scroll_rect.verticalScrollbar.numberOfSteps = _pages;
			}
		}
		else if (direction == ScrollDirection.Horizontal)
		{
			if (_scroll_rect.horizontalScrollbar != null)
			{
				_scroll_rect.horizontalScrollbar.numberOfSteps = 0;
			}
		}
		else if (_scroll_rect.verticalScrollbar != null)
		{
			_scroll_rect.verticalScrollbar.numberOfSteps = 0;
		}
	}

	private void LateUpdate()
	{
		UpdateListItemsSize();
		UpdateListItemPositions();
		if (_lerp)
		{
			UpdateScrollbar(linkSteps: false);
			_listContainerTransform.localPosition = Vector3.Lerp(_listContainerTransform.localPosition, _lerpTarget, 7.5f * Time.deltaTime);
			if (Vector3.Distance(_listContainerTransform.localPosition, _lerpTarget) < 0.001f)
			{
				_listContainerTransform.localPosition = _lerpTarget;
				_lerp = false;
				UpdateScrollbar(LinkScrolbarSteps);
			}
			if (Vector3.Distance(_listContainerTransform.localPosition, _lerpTarget) < 10f)
			{
				PageChanged(CurrentPage());
			}
		}
		if (_fastSwipeTimer)
		{
			_fastSwipeCounter++;
		}
	}

	public void NextScreen()
	{
		UpdateListItemPositions();
		if (CurrentPage() < _pages - 1)
		{
			_lerp = true;
			_lerpTarget = _pageAnchorPositions[CurrentPage() + 1];
			PageChanged(CurrentPage() + 1);
		}
	}

	public void PreviousScreen()
	{
		UpdateListItemPositions();
		if (CurrentPage() > 0)
		{
			_lerp = true;
			_lerpTarget = _pageAnchorPositions[CurrentPage() - 1];
			PageChanged(CurrentPage() - 1);
		}
	}

	private void NextScreenCommand()
	{
		if (_pageOnDragStart < _pages - 1)
		{
			int num = Mathf.Min(_pages - 1, _pageOnDragStart + ItemsVisibleAtOnce);
			_lerp = true;
			_lerpTarget = _pageAnchorPositions[num];
			PageChanged(num);
		}
	}

	private void PrevScreenCommand()
	{
		if (_pageOnDragStart > 0)
		{
			int num = Mathf.Max(0, _pageOnDragStart - ItemsVisibleAtOnce);
			_lerp = true;
			_lerpTarget = _pageAnchorPositions[num];
			PageChanged(num);
		}
	}

	public int CurrentPage()
	{
		float value;
		if (direction == ScrollDirection.Horizontal)
		{
			value = _listContainerMaxPosition - _listContainerTransform.localPosition.x;
			value = Mathf.Clamp(value, 0f, _listContainerSize);
		}
		else
		{
			value = _listContainerTransform.localPosition.y - _listContainerMinPosition;
			value = Mathf.Clamp(value, 0f, _listContainerSize);
		}
		return Mathf.Clamp(Mathf.RoundToInt(value / _itemSize), 0, _pages);
	}

	public void SetLerp(bool value)
	{
		_lerp = value;
	}

	public void ChangePage(int page)
	{
		if (0 <= page && page < _pages)
		{
			_lerp = true;
			_lerpTarget = _pageAnchorPositions[page];
			PageChanged(page);
		}
	}

	private void PageChanged(int currentPage)
	{
		_startingPage = currentPage;
		if ((bool)NextButton)
		{
			NextButton.interactable = currentPage < _pages - 1;
		}
		if ((bool)PrevButton)
		{
			PrevButton.interactable = currentPage > 0;
		}
		if (this.onPageChange != null)
		{
			this.onPageChange(currentPage);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		UpdateScrollbar(linkSteps: false);
		_fastSwipeCounter = 0;
		_fastSwipeTimer = true;
		_positionOnDragStart = eventData.position;
		_pageOnDragStart = CurrentPage();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_startDrag = true;
		float num = 0f;
		num = ((direction != ScrollDirection.Horizontal) ? (0f - _positionOnDragStart.y + eventData.position.y) : (_positionOnDragStart.x - eventData.position.x));
		if (UseFastSwipe)
		{
			fastSwipe = false;
			_fastSwipeTimer = false;
			if (_fastSwipeCounter <= _fastSwipeTarget && Math.Abs(num) > (float)FastSwipeThreshold)
			{
				fastSwipe = true;
			}
			if (fastSwipe)
			{
				if (num > 0f)
				{
					NextScreenCommand();
				}
				else
				{
					PrevScreenCommand();
				}
			}
			else
			{
				_lerp = true;
				_lerpTarget = _pageAnchorPositions[CurrentPage()];
			}
		}
		else
		{
			_lerp = true;
			_lerpTarget = _pageAnchorPositions[CurrentPage()];
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		_lerp = false;
		if (_startDrag)
		{
			OnBeginDrag(eventData);
			_startDrag = false;
		}
	}

	public void StartScreenChange()
	{
	}
}
