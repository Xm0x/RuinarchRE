using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class SubGoalsUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();
	}

	public SubGoalsUIModel UIModel => _baseAssetModel as SubGoalsUIModel;

	public static void Create(Canvas p_canvas, SubGoalsUIModel p_assets, Action<SubGoalsUIView> p_onCreate)
	{
		SubGoalsUIView subGoalsUIView = new GameObject(typeof(SubGoalsUIView).ToString()).AddComponent<SubGoalsUIView>();
		SubGoalsUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		subGoalsUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(subGoalsUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		SubGoalsUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Combine(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SubGoalsUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Remove(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}
}
