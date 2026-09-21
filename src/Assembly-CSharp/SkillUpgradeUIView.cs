using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class SkillUpgradeUIView : MVCUIView
{
	public interface IListener
	{
		void OnAfflictionTabClicked(bool isOn);

		void OnSpellTabClicked(bool isOn);

		void OnPlayerActionTabClicked(bool isOn);

		void OnCloseClicked();
	}

	public SkillUpgradeUIModel UIModel => _baseAssetModel as SkillUpgradeUIModel;

	public static void Create(Canvas p_canvas, SkillUpgradeUIModel p_assets, Action<SkillUpgradeUIView> p_onCreate)
	{
		SkillUpgradeUIView skillUpgradeUIView = new GameObject(typeof(SkillUpgradeUIView).ToString()).AddComponent<SkillUpgradeUIView>();
		SkillUpgradeUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		skillUpgradeUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(skillUpgradeUIView);
	}

	public Transform GetSkillParent()
	{
		return UIModel.skillParent;
	}

	public Transform GetTabParentTransform()
	{
		return UIModel.tabPrent;
	}

	public void SetUnlockSkillCount(string p_activeCasesCount)
	{
		UIModel.txtTotalUnlocked.text = p_activeCasesCount;
	}

	public void SetChaticEnergyCount(string p_deathCount)
	{
		UIModel.txtChaoticEnergyAmount.text = p_deathCount;
	}

	public void SetTransmissionTabIsOnWithoutNotify(bool p_isOn)
	{
		UIModel.btnAfflictionTab.SetIsOnWithoutNotify(p_isOn);
	}

	public void Subscribe(IListener p_listener)
	{
		SkillUpgradeUIModel uIModel = UIModel;
		uIModel.onAfflictionTabClicked = (Action<bool>)Delegate.Combine(uIModel.onAfflictionTabClicked, new Action<bool>(p_listener.OnAfflictionTabClicked));
		SkillUpgradeUIModel uIModel2 = UIModel;
		uIModel2.onSpellTabClicked = (Action<bool>)Delegate.Combine(uIModel2.onSpellTabClicked, new Action<bool>(p_listener.OnSpellTabClicked));
		SkillUpgradeUIModel uIModel3 = UIModel;
		uIModel3.onPlayerActionTabClicked = (Action<bool>)Delegate.Combine(uIModel3.onPlayerActionTabClicked, new Action<bool>(p_listener.OnPlayerActionTabClicked));
		SkillUpgradeUIModel uIModel4 = UIModel;
		uIModel4.onCloseClicked = (Action)Delegate.Combine(uIModel4.onCloseClicked, new Action(p_listener.OnCloseClicked));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SkillUpgradeUIModel uIModel = UIModel;
		uIModel.onAfflictionTabClicked = (Action<bool>)Delegate.Remove(uIModel.onAfflictionTabClicked, new Action<bool>(p_listener.OnAfflictionTabClicked));
		SkillUpgradeUIModel uIModel2 = UIModel;
		uIModel2.onSpellTabClicked = (Action<bool>)Delegate.Remove(uIModel2.onSpellTabClicked, new Action<bool>(p_listener.OnSpellTabClicked));
		SkillUpgradeUIModel uIModel3 = UIModel;
		uIModel3.onPlayerActionTabClicked = (Action<bool>)Delegate.Remove(uIModel3.onPlayerActionTabClicked, new Action<bool>(p_listener.OnPlayerActionTabClicked));
		SkillUpgradeUIModel uIModel4 = UIModel;
		uIModel4.onCloseClicked = (Action)Delegate.Remove(uIModel4.onCloseClicked, new Action(p_listener.OnCloseClicked));
	}
}
