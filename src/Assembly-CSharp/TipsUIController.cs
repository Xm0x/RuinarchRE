using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using Tutorial;
using UnityEngine;

public class TipsUIController : MVCUIController
{
	private List<TIPS> displayedTips = new List<TIPS>();

	private List<TIPS> clickedTips = new List<TIPS>();

	private int currentHour;

	[SerializeField]
	private TipsItemUI m_tipsItemUI;

	[SerializeField]
	private TipsUIModel m_tipsUIModel;

	private TipsUIView m_tipsUIView;

	public List<TipsItemUI> tipsItems = new List<TipsItemUI>();

	public SaveDataPlayer m_saveDataPlayer;

	private void Start()
	{
		InstantiateUI();
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		TipsUIView.Create(_canvas, m_tipsUIModel, delegate(TipsUIView p_ui)
		{
			m_tipsUIView = p_ui;
			InitUI(p_ui.UIModel, p_ui);
			SubscribeToEvents();
			p_ui.UIModel.transform.SetSiblingIndex(siblingIndex);
		});
	}

	private void OnDisable()
	{
		UnSubscribeToEvents();
	}

	private void SubscribeToEvents()
	{
		Messenger.AddListener(Signals.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(PlayerSignals.CHAOS_ORB_COLLECTED, OnOrbCollected);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyCollected);
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructureObjectPlaced);
		Messenger.AddListener<IPlayerActionTarget>(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, OnContextMenuClicked);
		Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
		Messenger.AddListener(Signals.PROGRESSION_LOADED, OnSavedProgressionLoaded);
		Messenger.AddListener(UISignals.TOP_UI_ENABLED, OnSpellsOrBuildTabEnabled);
		Messenger.AddListener(UISignals.TOP_UI_DISABLED, OnSpellsOrBuildTabDisabled);
	}

	private void UnSubscribeToEvents()
	{
		Messenger.RemoveListener(Signals.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(PlayerSignals.CHAOS_ORB_COLLECTED, OnOrbCollected);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyCollected);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructureObjectPlaced);
		Messenger.RemoveListener<IPlayerActionTarget>(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, OnContextMenuClicked);
		Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
		Messenger.RemoveListener(Signals.PROGRESSION_LOADED, OnSavedProgressionLoaded);
		Messenger.RemoveListener(UISignals.TOP_UI_ENABLED, OnSpellsOrBuildTabEnabled);
		Messenger.RemoveListener(UISignals.TOP_UI_DISABLED, OnSpellsOrBuildTabDisabled);
		tipsItems.ForEach(delegate(TipsItemUI eachItem)
		{
			eachItem.onClickTip = (Action<TIPS>)Delegate.Remove(eachItem.onClickTip, new Action<TIPS>(OnItemClicked));
		});
	}

	private void AddTips(TIPS p_tips)
	{
		if (!displayedTips.Contains(p_tips) && !clickedTips.Contains(p_tips))
		{
			string key = ((TutorialManager.Tutorial_Type)Enum.Parse(typeof(TutorialManager.Tutorial_Type), p_tips.ToStringEnum())).ToStringEnum().ToLower();
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Tutorials_Table", key);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Tutorials");
			TipsItemUI tipsItemUI = UnityEngine.Object.Instantiate(m_tipsItemUI, m_tipsUIView.GetContentParent(), worldPositionStays: true);
			tipsItemUI.SetDescription(localizedValue2 + ": " + localizedValue);
			tipsItemUI.transform.localScale = Vector3.one;
			tipsItemUI.tip = p_tips;
			tipsItemUI.onClickTip = (Action<TIPS>)Delegate.Combine(tipsItemUI.onClickTip, new Action<TIPS>(OnItemClicked));
			tipsItems.Add(tipsItemUI);
			displayedTips.Add(p_tips);
			tipsItemUI.PlayIntroAnimation();
		}
	}

	private void OnItemClicked(TIPS p_tips)
	{
		switch (p_tips)
		{
		case TIPS.Time_Management:
			PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Time_Management);
			break;
		case TIPS.Chaotic_Energy:
			PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Chaotic_Energy);
			break;
		case TIPS.Base_Building:
			PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Base_Building);
			break;
		case TIPS.Unlocking_Bonus_Powers:
			PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Unlocking_Bonus_Powers);
			break;
		case TIPS.Upgrading_The_Portal:
			PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Upgrading_The_Portal);
			break;
		case TIPS.Target_Menu:
			PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Target_Menu);
			break;
		}
		TipsItemUI ti = null;
		tipsItems.ForEach(delegate(TipsItemUI eachItem)
		{
			if (eachItem.tip == p_tips)
			{
				ti = eachItem;
			}
		});
		tipsItems.Remove(ti);
		UnityEngine.Object.Destroy(ti.gameObject);
		clickedTips.Add(p_tips);
		m_saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
		m_saveDataPlayer.unlockedTips.Add(p_tips);
	}

	public void OnClickClose()
	{
		HideUI();
	}

	private void OnContextMenuClicked(IPlayerActionTarget p_action)
	{
		AddTips(TIPS.Target_Menu);
	}

	private void HourlyCheck()
	{
		currentHour++;
		switch (currentHour)
		{
		case 1:
			AddTips(TIPS.Chaotic_Energy);
			break;
		case 2:
			AddTips(TIPS.Target_Menu);
			break;
		case 3:
			AddTips(TIPS.Base_Building);
			break;
		case 4:
			AddTips(TIPS.Unlocking_Bonus_Powers);
			break;
		}
	}

	private void OnGameStarted()
	{
		m_saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
		m_saveDataPlayer.unlockedTips.ForEach(delegate(TIPS eachTip)
		{
			clickedTips.Add(eachTip);
		});
		AddTips(TIPS.Time_Management);
	}

	private void OnSavedProgressionLoaded()
	{
		m_saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
		m_saveDataPlayer.unlockedTips.ForEach(delegate(TIPS eachTip)
		{
			clickedTips.Add(eachTip);
		});
	}

	private void OnSpiritEnergyCollected(int adjustedAmount, int spiritEnergy)
	{
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement != null && PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) is ThePortal thePortal && !thePortal.IsMaxLevel())
		{
			PortalUpgradeTier nextTier = thePortal.nextTier;
			if (spiritEnergy >= nextTier.upgradeCost[0].amount)
			{
				AddTips(TIPS.Upgrading_The_Portal);
			}
		}
	}

	private void OnOrbCollected()
	{
		AddTips(TIPS.Chaotic_Energy);
	}

	private void OnStructureObjectPlaced(LocationStructure p_structure)
	{
		if (GameManager.Instance.gameHasStarted && p_structure is DemonicStructure)
		{
			AddTips(TIPS.Base_Building);
		}
	}

	private void OnSpellsOrBuildTabEnabled()
	{
		Vector3 vector = m_tipsUIView.UIModel.window.anchoredPosition;
		vector.y = -220f;
		m_tipsUIView.UIModel.window.anchoredPosition = vector;
	}

	private void OnSpellsOrBuildTabDisabled()
	{
		if (!PlayerUI.Instance.buildListUI.isShowing && !PlayerUI.Instance.spellListUI.isShowing)
		{
			Vector3 vector = m_tipsUIView.UIModel.window.anchoredPosition;
			vector.y = -93f;
			m_tipsUIView.UIModel.window.anchoredPosition = vector;
		}
	}
}
