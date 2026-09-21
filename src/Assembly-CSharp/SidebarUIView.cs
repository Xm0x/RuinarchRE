using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class SidebarUIView : MVCUIView
{
	public interface IListener
	{
		void OnToggleBookmark(bool p_isOn);

		void OnToggleMinimap(bool p_isOn);

		void OnHoverOverBookmark();

		void OnHoverOutBookmark();

		void OnHoverOverMinimap();

		void OnHoverOutMinimap();
	}

	public SidebarUIModel UIModel => _baseAssetModel as SidebarUIModel;

	public static void Create(Canvas p_canvas, SidebarUIModel p_assets, Action<SidebarUIView> p_onCreate)
	{
		SidebarUIView sidebarUIView = new GameObject(typeof(SidebarUIView).ToString()).AddComponent<SidebarUIView>();
		SidebarUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		sidebarUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(sidebarUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		SidebarUIModel uIModel = UIModel;
		uIModel.onToggleBookmarks = (Action<bool>)Delegate.Combine(uIModel.onToggleBookmarks, new Action<bool>(p_listener.OnToggleBookmark));
		SidebarUIModel uIModel2 = UIModel;
		uIModel2.onToggleMinimap = (Action<bool>)Delegate.Combine(uIModel2.onToggleMinimap, new Action<bool>(p_listener.OnToggleMinimap));
		SidebarUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverBookmark = (Action)Delegate.Combine(uIModel3.onHoverOverBookmark, new Action(p_listener.OnHoverOverBookmark));
		SidebarUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutBookmark = (Action)Delegate.Combine(uIModel4.onHoverOutBookmark, new Action(p_listener.OnHoverOutBookmark));
		SidebarUIModel uIModel5 = UIModel;
		uIModel5.onHoverOverMinimap = (Action)Delegate.Combine(uIModel5.onHoverOverMinimap, new Action(p_listener.OnHoverOverMinimap));
		SidebarUIModel uIModel6 = UIModel;
		uIModel6.onHoverOutMinimap = (Action)Delegate.Combine(uIModel6.onHoverOutMinimap, new Action(p_listener.OnHoverOutMinimap));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SidebarUIModel uIModel = UIModel;
		uIModel.onToggleBookmarks = (Action<bool>)Delegate.Remove(uIModel.onToggleBookmarks, new Action<bool>(p_listener.OnToggleBookmark));
		SidebarUIModel uIModel2 = UIModel;
		uIModel2.onToggleMinimap = (Action<bool>)Delegate.Remove(uIModel2.onToggleMinimap, new Action<bool>(p_listener.OnToggleMinimap));
		SidebarUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverBookmark = (Action)Delegate.Remove(uIModel3.onHoverOverBookmark, new Action(p_listener.OnHoverOverBookmark));
		SidebarUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutBookmark = (Action)Delegate.Remove(uIModel4.onHoverOutBookmark, new Action(p_listener.OnHoverOutBookmark));
		SidebarUIModel uIModel5 = UIModel;
		uIModel5.onHoverOverMinimap = (Action)Delegate.Remove(uIModel5.onHoverOverMinimap, new Action(p_listener.OnHoverOverMinimap));
		SidebarUIModel uIModel6 = UIModel;
		uIModel6.onHoverOutMinimap = (Action)Delegate.Remove(uIModel6.onHoverOutMinimap, new Action(p_listener.OnHoverOutMinimap));
	}
}
