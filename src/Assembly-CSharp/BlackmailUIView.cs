using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BlackmailUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();

		void OnClickConfirm();
	}

	public BlackmailUIModel UIModel => _baseAssetModel as BlackmailUIModel;

	public static void Create(Canvas p_canvas, BlackmailUIModel p_assets, Action<BlackmailUIView> p_onCreate)
	{
		BlackmailUIView blackmailUIView = new GameObject(typeof(BlackmailUIView).ToString()).AddComponent<BlackmailUIView>();
		BlackmailUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		blackmailUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(blackmailUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		BlackmailUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Combine(uIModel.onCloseClicked, new Action(p_listener.OnClickClose));
		BlackmailUIModel uIModel2 = UIModel;
		uIModel2.onClickConfirm = (Action)Delegate.Combine(uIModel2.onClickConfirm, new Action(p_listener.OnClickConfirm));
	}

	public void Unsubscribe(IListener p_listener)
	{
		BlackmailUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Remove(uIModel.onCloseClicked, new Action(p_listener.OnClickClose));
		BlackmailUIModel uIModel2 = UIModel;
		uIModel2.onClickConfirm = (Action)Delegate.Remove(uIModel2.onClickConfirm, new Action(p_listener.OnClickConfirm));
	}

	public void DisplayBlackmailItems(List<IIntel> p_intel, List<IIntel> p_alreadyChosenBlackmail)
	{
		for (int i = 0; i < UIModel.blackmailUIItems.Length; i++)
		{
			BlackmailUIItem blackmailUIItem = UIModel.blackmailUIItems[i];
			IIntel intel = p_intel.ElementAtOrDefault(i);
			if (intel != null)
			{
				blackmailUIItem.SetInitialItemDetails(intel, !p_alreadyChosenBlackmail.Contains(intel));
				blackmailUIItem.gameObject.SetActive(value: true);
			}
			else
			{
				blackmailUIItem.gameObject.SetActive(value: false);
			}
		}
	}
}
