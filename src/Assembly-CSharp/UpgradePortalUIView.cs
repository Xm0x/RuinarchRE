using System;
using System.Linq;
using DG.Tweening;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class UpgradePortalUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();

		void OnClickCancelUpgrade();

		void OnHoverOverCancelUpgrade();

		void OnHoverOutCancelUpgrade();

		void OnClickCloseChooseSkill();
	}

	private Sequence _showSequence;

	private Sequence _showChooseSkillsSequence;

	public UpgradePortalUIModel UIModel => _baseAssetModel as UpgradePortalUIModel;

	public static void Create(Canvas p_canvas, UpgradePortalUIModel p_assets, Action<UpgradePortalUIView> p_onCreate)
	{
		UpgradePortalUIView upgradePortalUIView = new GameObject(typeof(UpgradePortalUIView).ToString()).AddComponent<UpgradePortalUIView>();
		UpgradePortalUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		upgradePortalUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(upgradePortalUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		UpgradePortalUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Combine(uIModel.onClickClose, new Action(p_listener.OnClickClose));
		UpgradePortalUIModel uIModel2 = UIModel;
		uIModel2.onClickCancelUpgrade = (Action)Delegate.Combine(uIModel2.onClickCancelUpgrade, new Action(p_listener.OnClickCancelUpgrade));
		UpgradePortalUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverCancelUpgradePortal = (Action)Delegate.Combine(uIModel3.onHoverOverCancelUpgradePortal, new Action(p_listener.OnHoverOverCancelUpgrade));
		UpgradePortalUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutCancelUpgradePortal = (Action)Delegate.Combine(uIModel4.onHoverOutCancelUpgradePortal, new Action(p_listener.OnHoverOutCancelUpgrade));
		UpgradePortalUIModel uIModel5 = UIModel;
		uIModel5.onClickCloseChooseSkill = (Action)Delegate.Combine(uIModel5.onClickCloseChooseSkill, new Action(p_listener.OnClickCloseChooseSkill));
	}

	public void Unsubscribe(IListener p_listener)
	{
		UpgradePortalUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Remove(uIModel.onClickClose, new Action(p_listener.OnClickClose));
		UpgradePortalUIModel uIModel2 = UIModel;
		uIModel2.onClickCancelUpgrade = (Action)Delegate.Remove(uIModel2.onClickCancelUpgrade, new Action(p_listener.OnClickCancelUpgrade));
		UpgradePortalUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverCancelUpgradePortal = (Action)Delegate.Remove(uIModel3.onHoverOverCancelUpgradePortal, new Action(p_listener.OnHoverOverCancelUpgrade));
		UpgradePortalUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutCancelUpgradePortal = (Action)Delegate.Remove(uIModel4.onHoverOutCancelUpgradePortal, new Action(p_listener.OnHoverOutCancelUpgrade));
		UpgradePortalUIModel uIModel5 = UIModel;
		uIModel5.onClickCloseChooseSkill = (Action)Delegate.Remove(uIModel5.onClickCloseChooseSkill, new Action(p_listener.OnClickCloseChooseSkill));
	}

	public void UpdateItems(ThePortal portal)
	{
		PlayerSkillLoadout selectedLoadout = PlayerSkillManager.Instance.GetSelectedLoadout();
		for (int i = 0; i < UIModel.tierItems.Length; i++)
		{
			UpgradePortalTierItem upgradePortalTierItem = UIModel.tierItems[i];
			PortalUpgradeTier portalUpgradeTier = selectedLoadout.portalUpgradeTiers.ElementAtOrDefault(i);
			if (portalUpgradeTier == null || !PlayerManager.Instance.player.playerSkillComponent.portalUpgradeItems.ContainsKey(portalUpgradeTier.level))
			{
				upgradePortalTierItem.gameObject.SetActive(value: false);
				continue;
			}
			upgradePortalTierItem.gameObject.SetActive(value: true);
			upgradePortalTierItem.Initialize(i + 1, portalUpgradeTier);
		}
	}

	public void SetOnClickLeveLUpTierAction(Action<UpgradePortalTierItem> p_onClickLevelUp)
	{
		for (int i = 0; i < UIModel.tierItems.Length; i++)
		{
			UIModel.tierItems[i].SetLevelUpAction(p_onClickLevelUp);
		}
	}

	public void SetHeader(string p_value)
	{
		UIModel.lblTitle.text = p_value;
	}

	public void SetCurrentSpiritEnergyText(int p_amount)
	{
		UIModel.lblSpiritEnergy.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Current Spirit Energy") + ": " + Utilities.SpiritEnergyIcon() + p_amount;
	}

	public void SetUpgradeTimerState(bool p_state)
	{
		UIModel.goUpgradeTimer.SetActive(p_state);
		if (p_state)
		{
			UIModel.timerUpgradePortal.RefreshName();
		}
	}

	public void PlayShowAnimation()
	{
		UIModel.canvasGroupCover.alpha = 0f;
		UIModel.canvasGroupSkillTree.alpha = 0f;
		UIModel.canvasGroupFrame.alpha = 1f;
		Vector2 defaultFrameSize = UIModel.defaultFrameSize;
		UIModel.rectFrame.sizeDelta = new Vector2(defaultFrameSize.x + 500f, defaultFrameSize.y);
		UIModel.canvasGroupFrameGlow.alpha = 0f;
		UIModel.canvasGroupFrameGlow.DOKill();
		UIModel.canvasGroupFrameGlow.DOFade(1f, 2f).SetEase(Ease.OutQuart).SetLoops(-1, LoopType.Yoyo);
		_showSequence = DOTween.Sequence();
		_showSequence.Join(UIModel.canvasGroupCover.DOFade(1f, 0.5f));
		_showSequence.Join(UIModel.rectFrame.DOSizeDelta(defaultFrameSize, 0.8f));
		_showSequence.Join(UIModel.canvasGroupSkillTree.DOFade(1f, 0.5f));
		_showSequence.Play();
	}

	public void PlayHideAnimation(Action onComplete)
	{
		_showSequence?.Kill(complete: true);
		_showSequence = null;
		Sequence sequence = DOTween.Sequence();
		Vector2 defaultFrameSize = UIModel.defaultFrameSize;
		defaultFrameSize.x += 1000f;
		sequence.Join(UIModel.canvasGroupCover.DOFade(0f, 0.5f));
		sequence.Join(UIModel.canvasGroupChooseSkill.DOFade(0f, 0.5f));
		sequence.Join(UIModel.rectFrame.DOSizeDelta(defaultFrameSize, 0.7f));
		sequence.Join(UIModel.canvasGroupFrame.DOFade(0f, 0.6f));
		sequence.Join(UIModel.canvasGroupSkillTree.DOFade(0f, 0.6f));
		sequence.OnComplete(delegate
		{
			onComplete();
		});
		sequence.Play();
	}

	public void PlayHideChooseSkillAnimation()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Join(UIModel.canvasGroupChooseSkill.DOFade(0f, 0.5f));
		sequence.OnComplete(delegate
		{
			UIModel.goChooseSkill.SetActive(value: false);
		});
		sequence.Play();
	}

	public void AssignChooseItemActions(Action<PLAYER_SKILL_TYPE> p_onClick, Action<PlayerSkillData, ChooseSkillItemUI> p_onHoverOver, Action<PlayerSkillData, ChooseSkillItemUI> p_onHoverOut)
	{
		for (int i = 0; i < UIModel.chooseSkillItems.Length; i++)
		{
			ChooseSkillItemUI obj = UIModel.chooseSkillItems[i];
			obj.onButtonClick = (Action<PLAYER_SKILL_TYPE>)Delegate.Combine(obj.onButtonClick, p_onClick);
			obj.onHoverOver = (Action<PlayerSkillData, ChooseSkillItemUI>)Delegate.Combine(obj.onHoverOver, p_onHoverOver);
			obj.onHoverOut = (Action<PlayerSkillData, ChooseSkillItemUI>)Delegate.Combine(obj.onHoverOut, p_onHoverOut);
		}
	}

	public void ShowSkillsToChoose(PLAYER_SKILL_TYPE[] p_choices, PortalUpgradeItem p_upgradeItem)
	{
		switch (p_upgradeItem.portalUpgradeType)
		{
		case Portal_Upgrade_Type.Optional_Other:
			UIModel.lblChooseSkill.text = "Choose a tier " + p_upgradeItem.otherArchetypeTier + " power";
			break;
		case Portal_Upgrade_Type.Optional_Self:
			UIModel.lblChooseSkill.text = "Choose a tier " + p_upgradeItem.tier + " power";
			break;
		case Portal_Upgrade_Type.Common_Structures:
			UIModel.lblChooseSkill.text = "Choose a common structure";
			break;
		case Portal_Upgrade_Type.Lesser_Demon:
			UIModel.lblChooseSkill.text = "Choose a lesser demon";
			break;
		case Portal_Upgrade_Type.Wildcard:
			UIModel.lblChooseSkill.text = "Choose a wildcard power";
			break;
		}
		for (int i = 0; i < UIModel.chooseSkillItems.Length; i++)
		{
			ChooseSkillItemUI chooseSkillItemUI = UIModel.chooseSkillItems[i];
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = p_choices[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				chooseSkillItemUI.InitItem(pLAYER_SKILL_TYPE);
				chooseSkillItemUI.gameObject.SetActive(value: true);
				if (PlayerSkillLoadout.All_Common_Structures.Contains(pLAYER_SKILL_TYPE))
				{
					chooseSkillItemUI.EnableButton();
				}
				else if (PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE).isInUse)
				{
					chooseSkillItemUI.DisableButton();
				}
				else
				{
					chooseSkillItemUI.EnableButton();
				}
			}
			else
			{
				chooseSkillItemUI.gameObject.SetActive(value: false);
			}
		}
		_showChooseSkillsSequence?.Kill();
		if (UIModel.goChooseSkill.activeSelf)
		{
			UIModel.canvasGroupChooseSkill.alpha = 1f;
			_showChooseSkillsSequence = DOTween.Sequence();
			for (int j = 0; j < UIModel.chooseSkillItems.Length; j++)
			{
				ChooseSkillItemUI chooseSkillItemUI2 = UIModel.chooseSkillItems[j];
				if (chooseSkillItemUI2.gameObject.activeSelf)
				{
					_showChooseSkillsSequence.Join(chooseSkillItemUI2.PrepareAnimation().SetDelay((float)j / 5f));
				}
			}
			_showChooseSkillsSequence.Play();
			return;
		}
		UIModel.canvasGroupChooseSkill.alpha = 0f;
		UIModel.goChooseSkill.SetActive(value: true);
		_showChooseSkillsSequence = DOTween.Sequence();
		_showChooseSkillsSequence.Join(UIModel.canvasGroupChooseSkill.DOFade(1f, 0.5f));
		_showChooseSkillsSequence.AppendInterval(0.02f);
		for (int k = 0; k < UIModel.chooseSkillItems.Length; k++)
		{
			ChooseSkillItemUI chooseSkillItemUI3 = UIModel.chooseSkillItems[k];
			if (chooseSkillItemUI3.gameObject.activeSelf)
			{
				_showChooseSkillsSequence.Join(chooseSkillItemUI3.PrepareAnimation().SetDelay((float)k / 5f));
			}
		}
		_showChooseSkillsSequence.Play();
	}

	public void HideChooseSkillUI()
	{
		UIModel.canvasGroupChooseSkill.alpha = 1f;
		Sequence sequence = DOTween.Sequence();
		sequence.Join(UIModel.canvasGroupChooseSkill.DOFade(0f, 0.5f));
		sequence.OnComplete(delegate
		{
			UIModel.goChooseSkill.SetActive(value: false);
		});
		sequence.Play();
	}
}
