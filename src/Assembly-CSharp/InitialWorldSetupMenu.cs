using DG.Tweening;
using Inner_Maps;
using Ruinarch;
using UnityEngine;

public class InitialWorldSetupMenu : MonoBehaviour
{
	[SerializeField]
	public SkillTreeSelector loadOutMenu;

	[SerializeField]
	public GameObject configureLoadoutBtnGO;

	[SerializeField]
	public GameObject regenerateWorldBtnGO;

	[SerializeField]
	public GameObject placePortalBtnGO;

	[SerializeField]
	public RectTransform pickPortalMessage;

	public void Initialize()
	{
		loadOutMenu.Initialize();
	}

	public void Show()
	{
		UIManager.Instance.SetSpeedTogglesState(state: false);
		regenerateWorldBtnGO.SetActive(WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom);
		configureLoadoutBtnGO.gameObject.SetActive(value: false);
		InnerMapManager.Instance.ShowInnerMap(GridMap.Instance.mainRegion);
		base.gameObject.SetActive(value: true);
		UIManager.Instance.DisableContextMenuInteractions();
		PlayerUI.Instance.DisableTopMenuButtons();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Left, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Right, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Up, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Pan_Down, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Zoom_In, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Zoom_Out, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Confirm, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Cancel, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Toggle_Options_Menu, p_state: true);
		OnClickPlacePortal();
	}

	public void OnClickPlacePortal()
	{
		placePortalBtnGO.SetActive(value: false);
		pickPortalMessage.gameObject.SetActive(value: true);
		pickPortalMessage.anchoredPosition = new Vector2(0f, -110f);
		pickPortalMessage.DOAnchorPosY(110f, 0.5f).SetEase(Ease.OutBack);
		PlayerManager.Instance.AddPlayerInputModule(PlayerManager.pickPortalInputModule);
		InnerMapCameraMove.Instance.SetZoom(InnerMapCameraMove.Instance.maxFOV);
		PlayerManager.pickPortalInputModule.AddOnPortalPlacedAction(OnPortalPlaced);
		PlayerManager.Instance.ShowStructurePlacementVisual(STRUCTURE_TYPE.THE_PORTAL);
	}

	private void OnPortalPlaced()
	{
		PlayerManager.Instance.HideStructurePlacementVisual();
		PlayerManager.pickPortalInputModule.RemoveOnPortalPlacedAction(OnPortalPlaced);
		PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.pickPortalInputModule);
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		regenerateWorldBtnGO.SetActive(value: false);
		pickPortalMessage.DOAnchorPosY(-110f, 0.5f).SetEase(Ease.InBack);
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled)
		{
			loadOutMenu.OnClickContinue();
		}
		else
		{
			configureLoadoutBtnGO.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		UIManager.Instance.EnableContextMenuInteractions();
		PlayerUI.Instance.EnableTopMenuButtons();
		UIManager.Instance.SetSpeedTogglesState(state: true);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		base.gameObject.SetActive(value: false);
	}

	public void OnClickReloadWorld()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Regenerate_World");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Regenerate_World_Description");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, ReGenerateWorld, null, showCover: true, 50);
	}

	private void ReGenerateWorld()
	{
		DOTween.Clear(destroy: true);
		MainMenuManager.Instance.StartGame();
	}

	public void OnClickConfigureLoadOut()
	{
		loadOutMenu.Show();
	}
}
