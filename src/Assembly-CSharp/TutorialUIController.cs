using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using Ruinarch.MVCFramework;
using Tutorial;
using UnityEngine;
using UtilityScripts;

public class TutorialUIController : MVCUIController, TutorialUIView.IListener
{
	[SerializeField]
	private TutorialUIModel m_tutorialUIModel;

	private TutorialUIView m_tutorialUIView;

	private List<TutorialItemUI> _items;

	private TutorialManager.Tutorial_Type _currentlySelectedTutorial;

	public bool isShowing { get; private set; }

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		TutorialUIView.Create(_canvas, m_tutorialUIModel, delegate(TutorialUIView p_ui)
		{
			m_tutorialUIView = p_ui;
			m_tutorialUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void Awake()
	{
		_items = new List<TutorialItemUI>();
		TutorialItemUI.onTutorialItemToggledOn = OnTutorialItemSelected;
		TutorialItemUI.onTutorialItemToggledOff = OnTutorialItemDeselected;
	}

	private void Start()
	{
		InstantiateUI();
		HideUI();
		m_tutorialUIView.UIModel.goTutorialPages.SetActive(value: false);
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.AddListener<string>(ControlsSignals.CONTROL_DEVICE_CHANGED, OnControlDeviceChanged);
	}

	private void OnDestroy()
	{
		m_tutorialUIView?.Unsubscribe(this);
		Messenger.RemoveListener<string>(ControlsSignals.CONTROL_DEVICE_CHANGED, OnControlDeviceChanged);
	}

	private void OnGameLoaded()
	{
		CreateInitialItems();
	}

	private void CreateInitialItems()
	{
		List<TutorialManager.Tutorial_Type> list = ((SaveManager.Instance == null) ? CollectionUtilities.GetEnumValues<TutorialManager.Tutorial_Type>().ToList() : SaveManager.Instance.currentSaveDataPlayer.unlockedTutorials.OrderBy((TutorialManager.Tutorial_Type t) => t.GetTutorialOrder()).ToList());
		for (int num = 0; num < list.Count; num++)
		{
			TutorialManager.Tutorial_Type tutorial_Type = list[num];
			if (ShouldShowTutorialInCurrentWorld(tutorial_Type))
			{
				CreateTutorialItem(tutorial_Type);
			}
		}
	}

	private bool ShouldShowTutorialInCurrentWorld(TutorialManager.Tutorial_Type p_tutorialType)
	{
		if (p_tutorialType == TutorialManager.Tutorial_Type.Upgrading_The_Portal || p_tutorialType == TutorialManager.Tutorial_Type.Spirit_Energy)
		{
			return WorldSettings.Instance.worldSettingsData.victoryCondition != VICTORY_CONDITION.Eradication;
		}
		return true;
	}

	private void CreateTutorialItem(TutorialManager.Tutorial_Type p_type)
	{
		TutorialItemUI component = ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialItemUI", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.scrollRectTutorialItems.content).GetComponent<TutorialItemUI>();
		component.Initialize(p_type, m_tutorialUIView.UIModel.toggleGroupTutorialItems);
		_items.Add(component);
	}

	private void OnTutorialItemSelected(TutorialManager.Tutorial_Type p_type)
	{
		m_tutorialUIView.UIModel.goTutorialPages.SetActive(value: true);
		LoadPagesForTutorial(p_type);
		m_tutorialUIView.UIModel.tutorialPagesScrollSnap.GoToScreen(0);
		if (SaveManager.Instance != null)
		{
			SaveManager.Instance.savePlayerManager.currentSaveDataPlayer.SetTutorialAsRead(p_type);
		}
	}

	private void OnTutorialItemDeselected(TutorialManager.Tutorial_Type p_type)
	{
		m_tutorialUIView.UIModel.goTutorialPages.SetActive(value: false);
	}

	private void LoadPagesForTutorial(TutorialManager.Tutorial_Type p_type)
	{
		_currentlySelectedTutorial = p_type;
		Utilities.DestroyChildrenObjectPool(m_tutorialUIView.UIModel.tutorialPagesScrollSnapContent);
		Utilities.DestroyChildren(m_tutorialUIView.UIModel.tutorialPaginationParent);
		TutorialScriptableObjectData tutorialData = TutorialManager.Instance.GetTutorialData(p_type);
		for (int i = 0; i < tutorialData.pages.Count; i++)
		{
			TutorialPage p_page = tutorialData.pages[i];
			ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialPageItem", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.tutorialPagesScrollSnapContent).GetComponent<TutorialPageItem>().Initialize(p_page);
			ObjectPoolManager.Instance.InstantiateObjectFromPool("TutorialPagination", Vector3.zero, Quaternion.identity, m_tutorialUIView.UIModel.tutorialPaginationParent);
			m_tutorialUIView.UIModel.tutorialPagesScrollSnap.UpdateChildrenAndPagination();
		}
	}

	private void UpdateTutorialTexts(TutorialManager.Tutorial_Type p_type)
	{
		TutorialPageItem[] componentsInChildren = m_tutorialUIView.UIModel.tutorialPagesScrollSnap.GetComponentsInChildren<TutorialPageItem>();
		TutorialScriptableObjectData tutorialData = TutorialManager.Instance.GetTutorialData(p_type);
		if (componentsInChildren == null)
		{
			return;
		}
		for (int i = 0; i < tutorialData.pages.Count; i++)
		{
			TutorialPage p_page = tutorialData.pages[i];
			TutorialPageItem tutorialPageItem = componentsInChildren.ElementAtOrDefault(i);
			if (tutorialPageItem != null)
			{
				tutorialPageItem.UpdateTutorialTexts(p_page);
			}
		}
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void OnClickNextPage()
	{
		AudioManager.Instance.TryPlayUISFX("Play_Turn_Page");
	}

	public void OnClickPreviousPage()
	{
		AudioManager.Instance.TryPlayUISFX("Play_Turn_Page");
	}

	public override void HideUI()
	{
		m_tutorialUIView.UIModel.toggleGroupTutorialItems.SetAllTogglesOff();
		m_tutorialUIView.UIModel.goTutorialPages.SetActive(value: false);
		Utilities.DestroyChildren(m_tutorialUIView.UIModel.tutorialPaginationParent);
		base.HideUI();
		isShowing = false;
		if (PlayerUI.Instance != null)
		{
			PlayerUI.Instance.OnCloseTutorialUI();
		}
		if (TutorialManager.Instance != null)
		{
			TutorialManager.Instance.UnloadTutorialAssets();
		}
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
	}

	public override void ShowUI()
	{
		base.ShowUI();
		isShowing = true;
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cycle_Top_Menus, p_state: true);
		InnerMapCameraMove.Instance.DisableMovement();
		m_tutorialUIView.UIModel.scrollRectTutorialItems.verticalNormalizedPosition = 1f;
	}

	public void HideViaShortcutKey()
	{
		HideUI();
	}

	public void JumpToSpecificTutorial(TutorialManager.Tutorial_Type p_type)
	{
		TutorialItemUI tutorialItem = GetTutorialItem(p_type);
		if (tutorialItem != null)
		{
			tutorialItem.ManualSelect();
		}
	}

	private TutorialItemUI GetTutorialItem(TutorialManager.Tutorial_Type p_type)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			TutorialItemUI tutorialItemUI = _items[i];
			if (tutorialItemUI.tutorialType == p_type)
			{
				return tutorialItemUI;
			}
		}
		return null;
	}

	private void OnControlDeviceChanged(string p_deviceName)
	{
		if (isShowing)
		{
			UpdateTutorialTexts(_currentlySelectedTutorial);
		}
	}
}
