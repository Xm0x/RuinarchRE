using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TipsUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();
	}

	public TipsUIModel UIModel => _baseAssetModel as TipsUIModel;

	public static void Create(Canvas p_canvas, TipsUIModel p_assets, Action<TipsUIView> p_onCreate)
	{
		TipsUIView tipsUIView = new GameObject(typeof(TipsUIView).ToString()).AddComponent<TipsUIView>();
		TipsUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		tipsUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(tipsUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		TipsUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Combine(uIModel.onCloseClicked, new Action(p_listener.OnClickClose));
	}

	public void Unsubscribe(IListener p_listener)
	{
		TipsUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Remove(uIModel.onCloseClicked, new Action(p_listener.OnClickClose));
	}

	public Transform GetContentParent()
	{
		return UIModel.scrollViewContent;
	}
}
