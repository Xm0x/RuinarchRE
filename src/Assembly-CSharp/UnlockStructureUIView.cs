using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockStructureUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();
	}

	public UnlockStructureUIModel UIModel => _baseAssetModel as UnlockStructureUIModel;

	public static void Create(Canvas p_canvas, UnlockStructureUIModel p_assets, Action<UnlockStructureUIView> p_onCreate)
	{
		UnlockStructureUIView unlockStructureUIView = new GameObject(typeof(UnlockStructureUIView).ToString()).AddComponent<UnlockStructureUIView>();
		UnlockStructureUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		unlockStructureUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(unlockStructureUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		UnlockStructureUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Combine(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void Unsubscribe(IListener p_listener)
	{
		UnlockStructureUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Remove(uIModel.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void UpdateStructureItemsSelectableStates()
	{
		for (int i = 0; i < UIModel.structureItems.Length; i++)
		{
			UIModel.structureItems[i].UpdateSelectableState();
		}
	}
}
