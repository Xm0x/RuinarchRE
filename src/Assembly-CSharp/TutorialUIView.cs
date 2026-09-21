using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TutorialUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();

		void OnClickNextPage();

		void OnClickPreviousPage();
	}

	public TutorialUIModel UIModel => _baseAssetModel as TutorialUIModel;

	public static void Create(Canvas p_canvas, TutorialUIModel p_assets, Action<TutorialUIView> p_onCreate)
	{
		TutorialUIView tutorialUIView = new GameObject(typeof(TutorialUIView).ToString()).AddComponent<TutorialUIView>();
		TutorialUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		tutorialUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(tutorialUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		TutorialUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Combine(uIModel.onClickClose, new Action(p_listener.OnClickClose));
		TutorialUIModel uIModel2 = UIModel;
		uIModel2.onClickNextPage = (Action)Delegate.Combine(uIModel2.onClickNextPage, new Action(p_listener.OnClickNextPage));
		TutorialUIModel uIModel3 = UIModel;
		uIModel3.onClickPreviousPage = (Action)Delegate.Combine(uIModel3.onClickPreviousPage, new Action(p_listener.OnClickPreviousPage));
	}

	public void Unsubscribe(IListener p_listener)
	{
		TutorialUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Remove(uIModel.onClickClose, new Action(p_listener.OnClickClose));
		TutorialUIModel uIModel2 = UIModel;
		uIModel2.onClickNextPage = (Action)Delegate.Remove(uIModel2.onClickNextPage, new Action(p_listener.OnClickNextPage));
		TutorialUIModel uIModel3 = UIModel;
		uIModel3.onClickPreviousPage = (Action)Delegate.Remove(uIModel3.onClickPreviousPage, new Action(p_listener.OnClickPreviousPage));
	}
}
