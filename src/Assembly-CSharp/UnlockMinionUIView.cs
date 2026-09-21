using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockMinionUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();
	}

	public UnlockMinionUIModel UIModel => _baseAssetModel as UnlockMinionUIModel;

	public static void Create(Canvas p_canvas, UnlockMinionUIModel p_assets, Action<UnlockMinionUIView> p_onCreate)
	{
		UnlockMinionUIView unlockMinionUIView = new GameObject(typeof(UnlockMinionUIView).ToString()).AddComponent<UnlockMinionUIView>();
		UnlockMinionUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		unlockMinionUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(unlockMinionUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		UnlockMinionUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Combine(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void Unsubscribe(IListener p_listener)
	{
		UnlockMinionUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Remove(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void UpdateMinionItemsSelectableStates()
	{
		for (int i = 0; i < UIModel.minionItems.Length; i++)
		{
			UIModel.minionItems[i].UpdateSelectableState();
		}
	}
}
