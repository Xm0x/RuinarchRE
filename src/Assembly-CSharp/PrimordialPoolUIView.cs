using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PrimordialPoolUIView : MVCUIView
{
	public interface IListener
	{
		void OnHumanoidTabClicked(bool isOn);

		void OnDemonicTabClicked(bool isOn);

		void OnUndeadTabClicked(bool isOn);

		void OnBeastTabClicked(bool isOn);

		void OnCloseClicked();
	}

	public PrimordialPoolUIModel UIModel => _baseAssetModel as PrimordialPoolUIModel;

	public static void Create(Canvas p_canvas, PrimordialPoolUIModel p_assets, Action<PrimordialPoolUIView> p_onCreate)
	{
		PrimordialPoolUIView primordialPoolUIView = new GameObject(typeof(PrimordialPoolUIView).ToString()).AddComponent<PrimordialPoolUIView>();
		PrimordialPoolUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		primordialPoolUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(primordialPoolUIView);
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

	public void SetChaoticEnergy(string p_plaguePoints)
	{
		UIModel.txtPlaguePoints.text = p_plaguePoints;
	}

	public void SetTransmissionTabIsOnWithoutNotify(bool p_isOn)
	{
		UIModel.btnHumanoidTab.SetIsOnWithoutNotify(p_isOn);
	}

	public void Subscribe(IListener p_listener)
	{
		PrimordialPoolUIModel uIModel = UIModel;
		uIModel.onHumanoidTabClicked = (Action<bool>)Delegate.Combine(uIModel.onHumanoidTabClicked, new Action<bool>(p_listener.OnHumanoidTabClicked));
		PrimordialPoolUIModel uIModel2 = UIModel;
		uIModel2.onDemonicTabClicked = (Action<bool>)Delegate.Combine(uIModel2.onDemonicTabClicked, new Action<bool>(p_listener.OnDemonicTabClicked));
		PrimordialPoolUIModel uIModel3 = UIModel;
		uIModel3.onUndeadTabClicked = (Action<bool>)Delegate.Combine(uIModel3.onUndeadTabClicked, new Action<bool>(p_listener.OnUndeadTabClicked));
		PrimordialPoolUIModel uIModel4 = UIModel;
		uIModel4.onBeastTabClicked = (Action<bool>)Delegate.Combine(uIModel4.onBeastTabClicked, new Action<bool>(p_listener.OnBeastTabClicked));
		PrimordialPoolUIModel uIModel5 = UIModel;
		uIModel5.onCloseClicked = (Action)Delegate.Combine(uIModel5.onCloseClicked, new Action(p_listener.OnCloseClicked));
	}

	public void Unsubscribe(IListener p_listener)
	{
		PrimordialPoolUIModel uIModel = UIModel;
		uIModel.onHumanoidTabClicked = (Action<bool>)Delegate.Remove(uIModel.onHumanoidTabClicked, new Action<bool>(p_listener.OnHumanoidTabClicked));
		PrimordialPoolUIModel uIModel2 = UIModel;
		uIModel2.onDemonicTabClicked = (Action<bool>)Delegate.Remove(uIModel2.onDemonicTabClicked, new Action<bool>(p_listener.OnDemonicTabClicked));
		PrimordialPoolUIModel uIModel3 = UIModel;
		uIModel3.onUndeadTabClicked = (Action<bool>)Delegate.Remove(uIModel3.onUndeadTabClicked, new Action<bool>(p_listener.OnUndeadTabClicked));
		PrimordialPoolUIModel uIModel4 = UIModel;
		uIModel4.onBeastTabClicked = (Action<bool>)Delegate.Remove(uIModel4.onBeastTabClicked, new Action<bool>(p_listener.OnBeastTabClicked));
		PrimordialPoolUIModel uIModel5 = UIModel;
		uIModel5.onCloseClicked = (Action)Delegate.Remove(uIModel5.onCloseClicked, new Action(p_listener.OnCloseClicked));
	}
}
