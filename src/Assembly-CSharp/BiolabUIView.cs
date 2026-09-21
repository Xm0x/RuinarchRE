using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BiolabUIView : MVCUIView
{
	public interface IListener
	{
		void OnTransmissionTabClicked(bool isOn);

		void OnLifeSpanTabClicked(bool isOn);

		void OnFatalityTabClicked(bool isOn);

		void OnSymptomsTabClicked(bool isOn);

		void OnOnDeathClicked(bool isOn);

		void OnCloseClicked();
	}

	public BiolabUIModel UIModel => _baseAssetModel as BiolabUIModel;

	public static void Create(Canvas p_canvas, BiolabUIModel p_assets, Action<BiolabUIView> p_onCreate)
	{
		BiolabUIView biolabUIView = new GameObject(typeof(BiolabUIView).ToString()).AddComponent<BiolabUIView>();
		BiolabUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		biolabUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(biolabUIView);
	}

	public Transform GetTabParentTransform()
	{
		return UIModel.tabPrent;
	}

	public void SetActiveCases(string p_activeCasesCount)
	{
		UIModel.txtActiveCasesValue.text = p_activeCasesCount;
	}

	public void SetDeathCases(string p_deathCount)
	{
		UIModel.txtDeathsValue.text = p_deathCount;
	}

	public void SetRecoveriesCases(string p_recoveriesCount)
	{
		UIModel.txtRecoveriesValue.text = p_recoveriesCount;
	}

	public void SetPlagueRats(string p_plagueRatsCount)
	{
		UIModel.txtPlagueRatsValue.text = p_plagueRatsCount;
	}

	public void SetPlaguePoints(string p_plaguePoints)
	{
		UIModel.txtPlaguePoints.text = p_plaguePoints;
	}

	public void SetTransmissionTabIsOnWithoutNotify(bool p_isOn)
	{
		UIModel.btnTransmissionTab.SetIsOnWithoutNotify(p_isOn);
	}

	public void Subscribe(IListener p_listener)
	{
		BiolabUIModel uIModel = UIModel;
		uIModel.onTransmissionTabClicked = (Action<bool>)Delegate.Combine(uIModel.onTransmissionTabClicked, new Action<bool>(p_listener.OnTransmissionTabClicked));
		BiolabUIModel uIModel2 = UIModel;
		uIModel2.onLifeSpanTabClicked = (Action<bool>)Delegate.Combine(uIModel2.onLifeSpanTabClicked, new Action<bool>(p_listener.OnLifeSpanTabClicked));
		BiolabUIModel uIModel3 = UIModel;
		uIModel3.onFatalityTabClicked = (Action<bool>)Delegate.Combine(uIModel3.onFatalityTabClicked, new Action<bool>(p_listener.OnFatalityTabClicked));
		BiolabUIModel uIModel4 = UIModel;
		uIModel4.onSymptomsTabClicked = (Action<bool>)Delegate.Combine(uIModel4.onSymptomsTabClicked, new Action<bool>(p_listener.OnSymptomsTabClicked));
		BiolabUIModel uIModel5 = UIModel;
		uIModel5.onOnDeathClicked = (Action<bool>)Delegate.Combine(uIModel5.onOnDeathClicked, new Action<bool>(p_listener.OnOnDeathClicked));
		BiolabUIModel uIModel6 = UIModel;
		uIModel6.onCloseClicked = (Action)Delegate.Combine(uIModel6.onCloseClicked, new Action(p_listener.OnCloseClicked));
	}

	public void Unsubscribe(IListener p_listener)
	{
		BiolabUIModel uIModel = UIModel;
		uIModel.onTransmissionTabClicked = (Action<bool>)Delegate.Remove(uIModel.onTransmissionTabClicked, new Action<bool>(p_listener.OnTransmissionTabClicked));
		BiolabUIModel uIModel2 = UIModel;
		uIModel2.onLifeSpanTabClicked = (Action<bool>)Delegate.Remove(uIModel2.onLifeSpanTabClicked, new Action<bool>(p_listener.OnLifeSpanTabClicked));
		BiolabUIModel uIModel3 = UIModel;
		uIModel3.onFatalityTabClicked = (Action<bool>)Delegate.Remove(uIModel3.onFatalityTabClicked, new Action<bool>(p_listener.OnFatalityTabClicked));
		BiolabUIModel uIModel4 = UIModel;
		uIModel4.onSymptomsTabClicked = (Action<bool>)Delegate.Remove(uIModel4.onSymptomsTabClicked, new Action<bool>(p_listener.OnSymptomsTabClicked));
		BiolabUIModel uIModel5 = UIModel;
		uIModel5.onOnDeathClicked = (Action<bool>)Delegate.Remove(uIModel5.onOnDeathClicked, new Action<bool>(p_listener.OnOnDeathClicked));
		BiolabUIModel uIModel6 = UIModel;
		uIModel6.onCloseClicked = (Action)Delegate.Remove(uIModel6.onCloseClicked, new Action(p_listener.OnCloseClicked));
	}
}
