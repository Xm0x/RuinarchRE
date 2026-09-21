using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;

public class SidebarUIModel : MVCUIModel
{
	public RuinarchToggle tglBookmarks;

	public RuinarchToggle tglMinimap;

	public HoverHandler bookmarksHoverHandler;

	public HoverHandler minimapHoverHandler;

	public UIHoverPosition tooltipHoverPos;

	public Action<bool> onToggleBookmarks;

	public Action<bool> onToggleMinimap;

	public Action onHoverOverBookmark;

	public Action onHoverOutBookmark;

	public Action onHoverOverMinimap;

	public Action onHoverOutMinimap;

	private void OnEnable()
	{
		tglBookmarks.onValueChanged.AddListener(OnToggleBookmark);
		tglMinimap.onValueChanged.AddListener(OnToggleMinimap);
		bookmarksHoverHandler.AddOnHoverOverAction(OnHoverOverBookmark);
		bookmarksHoverHandler.AddOnHoverOutAction(OnHoverOutBookmark);
		minimapHoverHandler.AddOnHoverOverAction(OnHoverOverMinimap);
		minimapHoverHandler.AddOnHoverOutAction(OnHoverOutMinimap);
	}

	private void OnDisable()
	{
		tglBookmarks.onValueChanged.RemoveListener(OnToggleBookmark);
		tglMinimap.onValueChanged.RemoveListener(OnToggleMinimap);
		bookmarksHoverHandler.RemoveOnHoverOverAction(OnHoverOverBookmark);
		bookmarksHoverHandler.RemoveOnHoverOutAction(OnHoverOutBookmark);
		minimapHoverHandler.RemoveOnHoverOverAction(OnHoverOverMinimap);
		minimapHoverHandler.RemoveOnHoverOutAction(OnHoverOutMinimap);
	}

	private void OnToggleBookmark(bool p_state)
	{
		onToggleBookmarks?.Invoke(p_state);
	}

	private void OnToggleMinimap(bool p_state)
	{
		onToggleMinimap?.Invoke(p_state);
	}

	private void OnHoverOverBookmark()
	{
		onHoverOverBookmark?.Invoke();
	}

	private void OnHoverOutBookmark()
	{
		onHoverOutBookmark?.Invoke();
	}

	private void OnHoverOverMinimap()
	{
		onHoverOverMinimap?.Invoke();
	}

	private void OnHoverOutMinimap()
	{
		onHoverOutMinimap?.Invoke();
	}
}
