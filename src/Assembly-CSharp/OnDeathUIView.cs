using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class OnDeathUIView : MVCUIView
{
	public interface IListener
	{
		void OnIgniteUpgradeClicked();

		void OnWalkerZombieUpgradeClicked();

		void OnMana2_3UpgradeClicked();

		void OnRandomSpirit_1UpgradeClicked();

		void OnIgniteHoveredOver(UIHoverPosition hoverPosition);

		void OnWalkerZombieHoveredOver(UIHoverPosition hoverPosition);

		void OnManaHoveredOver(UIHoverPosition hoverPosition);

		void OnSpiritHoveredOver(UIHoverPosition hoverPosition);

		void OnIgniteHoveredOut();

		void OnWalkerZombieHoveredOut();

		void OnManaHoveredOut();

		void OnSpiritHoveredOut();
	}

	public OnDeathUIModel UIModel => _baseAssetModel as OnDeathUIModel;

	public static void Create(Canvas p_canvas, OnDeathUIModel p_assets, Action<OnDeathUIView> p_onCreate)
	{
		OnDeathUIView onDeathUIView = new GameObject(typeof(OnDeathUIView).ToString()).AddComponent<OnDeathUIView>();
		OnDeathUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		onDeathUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(onDeathUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		OnDeathUIModel uIModel = UIModel;
		uIModel.onIgniteUpgradeClicked = (Action)Delegate.Combine(uIModel.onIgniteUpgradeClicked, new Action(p_listener.OnIgniteUpgradeClicked));
		OnDeathUIModel uIModel2 = UIModel;
		uIModel2.onWalkerZombieUpgradeClicked = (Action)Delegate.Combine(uIModel2.onWalkerZombieUpgradeClicked, new Action(p_listener.OnWalkerZombieUpgradeClicked));
		OnDeathUIModel uIModel3 = UIModel;
		uIModel3.onMana2_3UpgradeClicked = (Action)Delegate.Combine(uIModel3.onMana2_3UpgradeClicked, new Action(p_listener.OnMana2_3UpgradeClicked));
		OnDeathUIModel uIModel4 = UIModel;
		uIModel4.onRandomSpirit_1UpgradeClicked = (Action)Delegate.Combine(uIModel4.onRandomSpirit_1UpgradeClicked, new Action(p_listener.OnRandomSpirit_1UpgradeClicked));
		OnDeathUIModel uIModel5 = UIModel;
		uIModel5.onIgniteHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel5.onIgniteHoveredOver, new Action<UIHoverPosition>(p_listener.OnIgniteHoveredOver));
		OnDeathUIModel uIModel6 = UIModel;
		uIModel6.onWalkerZombieHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel6.onWalkerZombieHoveredOver, new Action<UIHoverPosition>(p_listener.OnWalkerZombieHoveredOver));
		OnDeathUIModel uIModel7 = UIModel;
		uIModel7.onManaHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel7.onManaHoveredOver, new Action<UIHoverPosition>(p_listener.OnManaHoveredOver));
		OnDeathUIModel uIModel8 = UIModel;
		uIModel8.onSpiritHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel8.onSpiritHoveredOver, new Action<UIHoverPosition>(p_listener.OnSpiritHoveredOver));
		OnDeathUIModel uIModel9 = UIModel;
		uIModel9.onIgniteHoveredOut = (Action)Delegate.Combine(uIModel9.onIgniteHoveredOut, new Action(p_listener.OnIgniteHoveredOut));
		OnDeathUIModel uIModel10 = UIModel;
		uIModel10.onWalkerZombieHoveredOut = (Action)Delegate.Combine(uIModel10.onWalkerZombieHoveredOut, new Action(p_listener.OnWalkerZombieHoveredOut));
		OnDeathUIModel uIModel11 = UIModel;
		uIModel11.onManaHoveredOut = (Action)Delegate.Combine(uIModel11.onManaHoveredOut, new Action(p_listener.OnManaHoveredOut));
		OnDeathUIModel uIModel12 = UIModel;
		uIModel12.onSpiritHoveredOut = (Action)Delegate.Combine(uIModel12.onSpiritHoveredOut, new Action(p_listener.OnSpiritHoveredOut));
	}

	public void Unsubscribe(IListener p_listener)
	{
		OnDeathUIModel uIModel = UIModel;
		uIModel.onIgniteUpgradeClicked = (Action)Delegate.Remove(uIModel.onIgniteUpgradeClicked, new Action(p_listener.OnIgniteUpgradeClicked));
		OnDeathUIModel uIModel2 = UIModel;
		uIModel2.onWalkerZombieUpgradeClicked = (Action)Delegate.Remove(uIModel2.onWalkerZombieUpgradeClicked, new Action(p_listener.OnWalkerZombieUpgradeClicked));
		OnDeathUIModel uIModel3 = UIModel;
		uIModel3.onMana2_3UpgradeClicked = (Action)Delegate.Remove(uIModel3.onMana2_3UpgradeClicked, new Action(p_listener.OnMana2_3UpgradeClicked));
		OnDeathUIModel uIModel4 = UIModel;
		uIModel4.onRandomSpirit_1UpgradeClicked = (Action)Delegate.Remove(uIModel4.onRandomSpirit_1UpgradeClicked, new Action(p_listener.OnRandomSpirit_1UpgradeClicked));
		OnDeathUIModel uIModel5 = UIModel;
		uIModel5.onIgniteHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel5.onIgniteHoveredOver, new Action<UIHoverPosition>(p_listener.OnIgniteHoveredOver));
		OnDeathUIModel uIModel6 = UIModel;
		uIModel6.onWalkerZombieHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel6.onWalkerZombieHoveredOver, new Action<UIHoverPosition>(p_listener.OnWalkerZombieHoveredOver));
		OnDeathUIModel uIModel7 = UIModel;
		uIModel7.onManaHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel7.onManaHoveredOver, new Action<UIHoverPosition>(p_listener.OnManaHoveredOver));
		OnDeathUIModel uIModel8 = UIModel;
		uIModel8.onSpiritHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel8.onSpiritHoveredOver, new Action<UIHoverPosition>(p_listener.OnSpiritHoveredOver));
		OnDeathUIModel uIModel9 = UIModel;
		uIModel9.onIgniteHoveredOut = (Action)Delegate.Remove(uIModel9.onIgniteHoveredOut, new Action(p_listener.OnIgniteHoveredOut));
		OnDeathUIModel uIModel10 = UIModel;
		uIModel10.onWalkerZombieHoveredOut = (Action)Delegate.Remove(uIModel10.onWalkerZombieHoveredOut, new Action(p_listener.OnWalkerZombieHoveredOut));
		OnDeathUIModel uIModel11 = UIModel;
		uIModel11.onManaHoveredOut = (Action)Delegate.Remove(uIModel11.onManaHoveredOut, new Action(p_listener.OnManaHoveredOut));
		OnDeathUIModel uIModel12 = UIModel;
		uIModel12.onSpiritHoveredOut = (Action)Delegate.Remove(uIModel12.onSpiritHoveredOut, new Action(p_listener.OnSpiritHoveredOut));
	}

	private RuinarchText GetCostTextToUpdate(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		return p_deathEffect switch
		{
			PLAGUE_DEATH_EFFECT.Explosion => UIModel.txtIgniteUpgradeCost, 
			PLAGUE_DEATH_EFFECT.Zombie => UIModel.txtWalkerZombieUpgradeCost, 
			PLAGUE_DEATH_EFFECT.Chaos_Generator => UIModel.txtMana2_3UpgradeCost, 
			PLAGUE_DEATH_EFFECT.Haunted_Spirits => UIModel.txtRandomSpirit_1UpgradeCost, 
			_ => throw new ArgumentOutOfRangeException("p_deathEffect", p_deathEffect, null), 
		};
	}

	private RuinarchText GetEffectTextToUpdate(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		return p_deathEffect switch
		{
			PLAGUE_DEATH_EFFECT.Explosion => UIModel.txtIgniteEffect, 
			PLAGUE_DEATH_EFFECT.Zombie => UIModel.txtWalkerZombieEffect, 
			PLAGUE_DEATH_EFFECT.Chaos_Generator => UIModel.txtMana2_3Effect, 
			PLAGUE_DEATH_EFFECT.Haunted_Spirits => UIModel.txtRandomSpirit_1Effect, 
			_ => throw new ArgumentOutOfRangeException("p_deathEffect", p_deathEffect, null), 
		};
	}

	private Button GetDeathEffectUpgradeButton(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		return p_deathEffect switch
		{
			PLAGUE_DEATH_EFFECT.Explosion => UIModel.btnIgniteUpgrade, 
			PLAGUE_DEATH_EFFECT.Zombie => UIModel.btnWalkerZombieUpgrade, 
			PLAGUE_DEATH_EFFECT.Chaos_Generator => UIModel.btnMana2_3Upgrade, 
			PLAGUE_DEATH_EFFECT.Haunted_Spirits => UIModel.btnRandomSpirit_1Upgrade, 
			_ => throw new ArgumentOutOfRangeException("p_deathEffect", p_deathEffect, null), 
		};
	}

	private TextMeshProUGUI GetDeathEffectUpgradeText(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		return p_deathEffect switch
		{
			PLAGUE_DEATH_EFFECT.Explosion => UIModel.txtIgniteUpgrade, 
			PLAGUE_DEATH_EFFECT.Zombie => UIModel.txtWalkerZombieUpgrade, 
			PLAGUE_DEATH_EFFECT.Chaos_Generator => UIModel.txtMana2_3Upgrade, 
			PLAGUE_DEATH_EFFECT.Haunted_Spirits => UIModel.txtRandomSpirit_1Upgrade, 
			_ => throw new ArgumentOutOfRangeException("p_deathEffect", p_deathEffect, null), 
		};
	}

	public void UpdateDeathEffectCost(PLAGUE_DEATH_EFFECT p_deathEffect, string p_cost)
	{
		GetCostTextToUpdate(p_deathEffect).text = p_cost;
	}

	public void UpdateDeathEffectUpgradeButtonInteractable(PLAGUE_DEATH_EFFECT p_deathEffect, bool p_interactable)
	{
		GetDeathEffectUpgradeButton(p_deathEffect).interactable = p_interactable;
		GetDeathEffectUpgradeText(p_deathEffect).color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateDeathEffectDescription(PLAGUE_DEATH_EFFECT p_deathEffect, string p_effect)
	{
		GetEffectTextToUpdate(p_deathEffect).text = p_effect;
	}
}
