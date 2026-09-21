using Ruinarch.MVCFramework;
using UnityEngine;

public class BookmarkUIController : MVCUIController, BookmarkUIView.IListener
{
	[SerializeField]
	private BookmarkUIModel m_bookmarkUIModel;

	private BookmarkUIView m_bookmarkUIView;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		BookmarkUIView.Create(_canvas, m_bookmarkUIModel, delegate(BookmarkUIView p_ui)
		{
			m_bookmarkUIView = p_ui;
			m_bookmarkUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			m_bookmarkUIView.Hide();
			p_ui.UIModel.transform.SetSiblingIndex(siblingIndex);
		});
	}

	private void Awake()
	{
		Messenger.AddListener<BookmarkCategory>(PlayerSignals.BOOKMARK_CATEGORY_ADDED, OnBookmarkCategoryAdded);
	}

	private void OnDestroy()
	{
		m_bookmarkUIView?.Unsubscribe(this);
	}

	private void OnBookmarkCategoryAdded(BookmarkCategory p_category)
	{
		m_bookmarkUIView.CreateBookmarkCategoryItem(p_category);
	}

	public void OnClickHide()
	{
		m_bookmarkUIView.Hide();
		UIManager.Instance.OnBookmarkMenuHide();
	}

	public void OnClickShow()
	{
		m_bookmarkUIView.Show();
		UIManager.Instance.OnBookmarkMenuShow();
	}

	public UIHoverPosition GetHoverPosition()
	{
		return m_bookmarkUIView.UIModel.tooltipHoverPosition;
	}

	public Transform GetUIModelTransform()
	{
		return m_bookmarkUIView.UIModel.transform;
	}
}
