using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickMinimapImage(PointerEventData p_data);
	}

	public MinimapUIModel UIModel => _baseAssetModel as MinimapUIModel;

	public static void Create(Canvas p_canvas, MinimapUIModel p_assets, Action<MinimapUIView> p_onCreate)
	{
		MinimapUIView minimapUIView = new GameObject(typeof(MinimapUIView).ToString()).AddComponent<MinimapUIView>();
		MinimapUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		minimapUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(minimapUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		UIModel.minimapImage.SetOnClickAction(p_listener.OnClickMinimapImage);
	}

	public void Unsubscribe(IListener p_listener)
	{
		UIModel.minimapImage.SetOnClickAction(null);
	}
}
