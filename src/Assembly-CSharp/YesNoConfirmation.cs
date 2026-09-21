using System;
using DG.Tweening;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YesNoConfirmation : PopupMenuBase
{
	public GameObject yesNoGO;

	[SerializeField]
	private CanvasGroup yesNoCanvasGroup;

	[SerializeField]
	private GameObject yesNoCover;

	[SerializeField]
	private TextMeshProUGUI yesNoHeaderLbl;

	[SerializeField]
	private TextMeshProUGUI yesNoDescriptionLbl;

	[SerializeField]
	private Button yesBtn;

	[SerializeField]
	private Button noBtn;

	[SerializeField]
	private Button closeBtn;

	[SerializeField]
	private TextMeshProUGUI yesBtnLbl;

	[SerializeField]
	private TextMeshProUGUI noBtnLbl;

	[SerializeField]
	private HoverHandler yesBtnUnInteractableHoverHandler;

	private Action _onHideUIAction;

	public void ShowYesNoConfirmation(string header, string question, Action onClickYesAction = null, Action onClickNoAction = null, bool showCover = false, int layer = 21, string yesBtnText = "Yes", string noBtnText = "No", bool yesBtnInteractable = true, bool noBtnInteractable = true, bool yesBtnActive = true, bool noBtnActive = true, Action yesBtnInactiveHoverAction = null, Action yesBtnInactiveHoverExitAction = null, Action onClickCloseAction = null, Action onHideUIAction = null)
	{
		InputManager.Instance.SetInputMapState("Confirmation Window", p_state: true);
		InputManager.Instance.SetGamepadCursorState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Confirm, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Cancel, p_state: true);
		yesNoHeaderLbl.text = header;
		yesNoDescriptionLbl.text = question;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", yesBtnText);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", noBtnText);
		yesBtnLbl.text = localizedValue;
		noBtnLbl.text = localizedValue2;
		yesBtn.gameObject.SetActive(yesBtnActive);
		noBtn.gameObject.SetActive(noBtnActive);
		yesBtn.interactable = yesBtnInteractable;
		noBtn.interactable = noBtnInteractable;
		yesBtn.onClick.RemoveAllListeners();
		noBtn.onClick.RemoveAllListeners();
		closeBtn.onClick.RemoveAllListeners();
		if (UIManager.Instance != null)
		{
			yesBtn.onClick.AddListener(UIManager.Instance.HideYesNoConfirmation);
			noBtn.onClick.AddListener(UIManager.Instance.HideYesNoConfirmation);
			closeBtn.onClick.AddListener(UIManager.Instance.HideYesNoConfirmation);
		}
		else
		{
			yesBtn.onClick.AddListener(Close);
			noBtn.onClick.AddListener(Close);
			closeBtn.onClick.AddListener(Close);
		}
		if (onClickYesAction != null)
		{
			yesBtn.onClick.AddListener(onClickYesAction.Invoke);
		}
		if (onClickNoAction != null)
		{
			noBtn.onClick.AddListener(onClickNoAction.Invoke);
		}
		if (onClickCloseAction != null)
		{
			closeBtn.onClick.AddListener(onClickCloseAction.Invoke);
		}
		yesBtnUnInteractableHoverHandler.gameObject.SetActive(!yesBtn.interactable);
		if (yesBtnInactiveHoverAction != null)
		{
			yesBtnUnInteractableHoverHandler.SetOnHoverOverAction(yesBtnInactiveHoverAction.Invoke);
		}
		if (yesBtnInactiveHoverExitAction != null)
		{
			yesBtnUnInteractableHoverHandler.SetOnHoverOutAction(yesBtnInactiveHoverExitAction.Invoke);
		}
		_onHideUIAction = onHideUIAction;
		yesNoGO.SetActive(value: true);
		yesNoGO.transform.SetSiblingIndex(layer);
		yesNoCover.SetActive(showCover);
		TweenIn(yesNoCanvasGroup);
	}

	public override void Close()
	{
		base.Close();
		InputManager.Instance.SetInputMapState("Confirmation Window", p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Confirm, p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Cancel, p_state: false);
		InputManager.Instance.SetGamepadCursorState(p_state: true);
		yesNoGO.SetActive(value: false);
		_onHideUIAction?.Invoke();
	}

	private void TweenIn(CanvasGroup canvasGroup)
	{
		canvasGroup.alpha = 0f;
		RectTransform rectTransform = canvasGroup.transform as RectTransform;
		rectTransform.anchoredPosition = new Vector2(0f, -30f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
		sequence.Join(DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 0.5f).SetEase(Ease.InSine));
		sequence.PrependInterval(0.2f);
		sequence.Play();
	}
}
