using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class GoalsUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();
	}

	public GoalsUIModel UIModel => _baseAssetModel as GoalsUIModel;

	public static void Create(Canvas p_canvas, GoalsUIModel p_assets, Action<GoalsUIView> p_onCreate)
	{
		GoalsUIView goalsUIView = new GameObject(typeof(GoalsUIView).ToString()).AddComponent<GoalsUIView>();
		GoalsUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		goalsUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(goalsUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		GoalsUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Combine(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void Unsubscribe(IListener p_listener)
	{
		GoalsUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Remove(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}
}
