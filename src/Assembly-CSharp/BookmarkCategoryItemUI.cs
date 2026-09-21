using System;
using System.Collections;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class BookmarkCategoryItemUI : PooledObject, BookmarkCategory.IListener
{
	[SerializeField]
	private TextMeshProUGUI lblHeaderName;

	[SerializeField]
	private Button btnHeader;

	[SerializeField]
	private GameObject goContent;

	[SerializeField]
	private Transform contentParent;

	[SerializeField]
	private GameObject goExpand;

	[SerializeField]
	private GameObject goCollapse;

	private Action _onResetAction;

	public BOOKMARK_CATEGORY category { get; private set; }

	private void Awake()
	{
		btnHeader.onClick.AddListener(ToggleContent);
	}

	public void Initialize(BookmarkCategory p_category)
	{
		lblHeaderName.text = p_category.displayName;
		category = p_category.bookmarkCategory;
		p_category.SubscribeToEvents(this);
		_onResetAction = (Action)Delegate.Combine(_onResetAction, (Action)delegate
		{
			p_category.UnsubscribeToEvents(this);
		});
		for (int num = 0; num < p_category.bookmarked.Count; num++)
		{
			IBookmarkable p_bookmarkable = p_category.bookmarked[num];
			CreateNewBookmarkItem(p_bookmarkable);
		}
		UpdateExpandCollapseVisual();
	}

	private void ToggleContent()
	{
		goContent.SetActive(!goContent.activeSelf);
		UpdateExpandCollapseVisual();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
	}

	private void UpdateExpandCollapseVisual()
	{
		if (goContent.activeSelf)
		{
			goExpand.SetActive(value: false);
			goCollapse.SetActive(value: true);
		}
		else
		{
			goExpand.SetActive(value: true);
			goCollapse.SetActive(value: false);
		}
	}

	public void OnBookmarkAdded(IBookmarkable p_bookmarkable)
	{
		base.gameObject.SetActive(value: true);
		CreateNewBookmarkItem(p_bookmarkable);
	}

	public void OnBookmarkCategoryEmptiedOut(BookmarkCategory p_category)
	{
		base.gameObject.SetActive(value: false);
	}

	private void CreateNewBookmarkItem(IBookmarkable p_bookmarkable)
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool(GetPrefabName(p_bookmarkable.bookmarkType), Vector3.zero, Quaternion.identity, contentParent);
		switch (p_bookmarkable.bookmarkType)
		{
		case BOOKMARK_TYPE.Progress_Bar:
		{
			RuinarchProgressable progressable = p_bookmarkable as RuinarchProgressable;
			gameObject.GetComponent<BookmarkProgressItemUI>().SetProgressable(progressable);
			break;
		}
		case BOOKMARK_TYPE.Text:
		case BOOKMARK_TYPE.Text_With_Cancel:
		{
			BookmarkTextItemUI component = gameObject.GetComponent<BookmarkTextItemUI>();
			component.SetBookmark(p_bookmarkable);
			if (category == BOOKMARK_CATEGORY.Alerts)
			{
				AudioManager.Instance.PlayAlertNotificationSound();
				component.PlayShowAnimation();
			}
			break;
		}
		case BOOKMARK_TYPE.Special:
			gameObject.GetComponent<SpecialBookmarkTextItemUI>().SetBookmark(p_bookmarkable);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		StartCoroutine(RebuildLayout());
	}

	private IEnumerator RebuildLayout()
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	private string GetPrefabName(BOOKMARK_TYPE p_bookmarkType)
	{
		switch (p_bookmarkType)
		{
		case BOOKMARK_TYPE.Text:
		case BOOKMARK_TYPE.Text_With_Cancel:
			return "Bookmark_Item_Text_Prefab";
		case BOOKMARK_TYPE.Special:
			return "Special_Bookmark_Item_Text_Prefab";
		default:
			return "Bookmark_Item_" + p_bookmarkType.ToStringEnum() + "_Prefab";
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onResetAction?.Invoke();
		_onResetAction = null;
		category = BOOKMARK_CATEGORY.None;
	}
}
