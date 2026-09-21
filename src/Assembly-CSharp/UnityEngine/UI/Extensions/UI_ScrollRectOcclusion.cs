using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/UI Scrollrect Occlusion")]
public class UI_ScrollRectOcclusion : MonoBehaviour
{
	public bool InitByUser;

	private ScrollRect _scrollRect;

	private ContentSizeFitter _contentSizeFitter;

	private VerticalLayoutGroup _verticalLayoutGroup;

	private HorizontalLayoutGroup _horizontalLayoutGroup;

	private GridLayoutGroup _gridLayoutGroup;

	private bool _isVertical;

	private bool _isHorizontal;

	private float _disableMarginX;

	private float _disableMarginY;

	private bool hasDisabledGridComponents;

	private List<RectTransform> items = new List<RectTransform>();

	private void Awake()
	{
		if (!InitByUser)
		{
			Init();
		}
	}

	public void Init()
	{
		if (GetComponent<ScrollRect>() != null)
		{
			_scrollRect = GetComponent<ScrollRect>();
			_scrollRect.onValueChanged.AddListener(OnScroll);
			_isHorizontal = _scrollRect.horizontal;
			_isVertical = _scrollRect.vertical;
			for (int i = 0; i < _scrollRect.content.childCount; i++)
			{
				items.Add(_scrollRect.content.GetChild(i).GetComponent<RectTransform>());
			}
			if (_scrollRect.content.GetComponent<VerticalLayoutGroup>() != null)
			{
				_verticalLayoutGroup = _scrollRect.content.GetComponent<VerticalLayoutGroup>();
			}
			if (_scrollRect.content.GetComponent<HorizontalLayoutGroup>() != null)
			{
				_horizontalLayoutGroup = _scrollRect.content.GetComponent<HorizontalLayoutGroup>();
			}
			if (_scrollRect.content.GetComponent<GridLayoutGroup>() != null)
			{
				_gridLayoutGroup = _scrollRect.content.GetComponent<GridLayoutGroup>();
			}
			if (_scrollRect.content.GetComponent<ContentSizeFitter>() != null)
			{
				_contentSizeFitter = _scrollRect.content.GetComponent<ContentSizeFitter>();
			}
		}
		else
		{
			Debug.LogError("UI_ScrollRectOcclusion => No ScrollRect component found");
		}
	}

	private void DisableGridComponents()
	{
		if (_isVertical)
		{
			_disableMarginY = _scrollRect.GetComponent<RectTransform>().rect.height / 2f + items[0].sizeDelta.y;
		}
		if (_isHorizontal)
		{
			_disableMarginX = _scrollRect.GetComponent<RectTransform>().rect.width / 2f + items[0].sizeDelta.x;
		}
		if ((bool)_verticalLayoutGroup)
		{
			_verticalLayoutGroup.enabled = false;
		}
		if ((bool)_horizontalLayoutGroup)
		{
			_horizontalLayoutGroup.enabled = false;
		}
		if ((bool)_contentSizeFitter)
		{
			_contentSizeFitter.enabled = false;
		}
		if ((bool)_gridLayoutGroup)
		{
			_gridLayoutGroup.enabled = false;
		}
		hasDisabledGridComponents = true;
	}

	public void OnScroll(Vector2 pos)
	{
		if (!hasDisabledGridComponents)
		{
			DisableGridComponents();
		}
		for (int i = 0; i < items.Count; i++)
		{
			if (_isVertical && _isHorizontal)
			{
				if (_scrollRect.transform.InverseTransformPoint(items[i].position).y < 0f - _disableMarginY || _scrollRect.transform.InverseTransformPoint(items[i].position).y > _disableMarginY || _scrollRect.transform.InverseTransformPoint(items[i].position).x < 0f - _disableMarginX || _scrollRect.transform.InverseTransformPoint(items[i].position).x > _disableMarginX)
				{
					items[i].gameObject.SetActive(value: false);
				}
				else
				{
					items[i].gameObject.SetActive(value: true);
				}
				continue;
			}
			if (_isVertical)
			{
				if (_scrollRect.transform.InverseTransformPoint(items[i].position).y < 0f - _disableMarginY || _scrollRect.transform.InverseTransformPoint(items[i].position).y > _disableMarginY)
				{
					items[i].gameObject.SetActive(value: false);
				}
				else
				{
					items[i].gameObject.SetActive(value: true);
				}
			}
			if (_isHorizontal)
			{
				if (_scrollRect.transform.InverseTransformPoint(items[i].position).x < 0f - _disableMarginX || _scrollRect.transform.InverseTransformPoint(items[i].position).x > _disableMarginX)
				{
					items[i].gameObject.SetActive(value: false);
				}
				else
				{
					items[i].gameObject.SetActive(value: true);
				}
			}
		}
	}
}
