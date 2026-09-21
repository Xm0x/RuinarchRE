using System;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BookmarkUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickHide();

		void OnClickShow();
	}

	public BookmarkUIModel UIModel => _baseAssetModel as BookmarkUIModel;

	public static void Create(Canvas p_canvas, BookmarkUIModel p_assets, Action<BookmarkUIView> p_onCreate)
	{
		BookmarkUIView bookmarkUIView = new GameObject(typeof(BookmarkUIView).ToString()).AddComponent<BookmarkUIView>();
		BookmarkUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		bookmarkUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(bookmarkUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		BookmarkUIModel uIModel = UIModel;
		uIModel.onClickHide = (Action)Delegate.Combine(uIModel.onClickHide, new Action(p_listener.OnClickHide));
		BookmarkUIModel uIModel2 = UIModel;
		uIModel2.onClickShow = (Action)Delegate.Combine(uIModel2.onClickShow, new Action(p_listener.OnClickShow));
	}

	public void Unsubscribe(IListener p_listener)
	{
		BookmarkUIModel uIModel = UIModel;
		uIModel.onClickHide = (Action)Delegate.Remove(uIModel.onClickHide, new Action(p_listener.OnClickHide));
		BookmarkUIModel uIModel2 = UIModel;
		uIModel2.onClickShow = (Action)Delegate.Remove(uIModel2.onClickShow, new Action(p_listener.OnClickShow));
	}

	public void Hide()
	{
		UIModel.rtWindow.anchoredPosition = UIModel.posHidden;
	}

	public void Show()
	{
		UIModel.rtWindow.anchoredPosition = UIModel.posShowing;
	}

	public void CreateBookmarkCategoryItem(BookmarkCategory p_category)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(UIModel.goBookmarkCategoryPrefab.name, Vector3.zero, Quaternion.identity, UIModel.scrollRectBookmarks.content).GetComponent<BookmarkCategoryItemUI>().Initialize(p_category);
		BookmarkCategoryItemUI[] array = (from i in UIModel.scrollRectBookmarks.content.GetComponentsInChildren<BookmarkCategoryItemUI>()
			orderby i.category.GetBookmarkCategoryOrder()
			select i).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			array[num].transform.SetSiblingIndex(num);
		}
	}
}
