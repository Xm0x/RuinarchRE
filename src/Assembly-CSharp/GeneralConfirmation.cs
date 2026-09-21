using System;
using DG.Tweening;
using Ruinarch;
using UnityEngine;
using UnityEngine.UI;

public class GeneralConfirmation : PopupMenuBase
{
	[SerializeField]
	protected RuinarchText generalConfirmationTitleText;

	[SerializeField]
	protected RuinarchText generalConfirmationBodyText;

	[SerializeField]
	protected Button generalConfirmationButton;

	[SerializeField]
	protected RuinarchText generalConfirmationButtonText;

	[SerializeField]
	protected CanvasGroup _canvasGroup;

	[SerializeField]
	protected Button _centerButton;

	public virtual void ShowGeneralConfirmation(string header, string body, string buttonText = "OK", Action onClickOK = null, Action onClickCenter = null, bool autoReplaceBodyText = true)
	{
		if (PlayerUI.Instance != null && PlayerUI.Instance.IsMajorUIShowing())
		{
			PlayerUI.Instance.AddPendingUI(delegate
			{
				ShowGeneralConfirmation(header, body, buttonText, onClickOK, onClickCenter);
			});
			return;
		}
		InputManager.Instance.SetInputMapState("Confirmation Window", p_state: true);
		InputManager.Instance.SetGamepadCursorState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Confirm, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Cancel, p_state: true);
		if (UIManager.Instance != null)
		{
			if (!UIManager.Instance.IsObjectPickerOpen())
			{
				UIManager.Instance.Pause();
				UIManager.Instance.SetSpeedTogglesState(state: false);
			}
			UIManager.Instance.HideSmallInfo();
		}
		generalConfirmationTitleText.SetTextAndReplaceWithIcons(header.ToUpper());
		if (autoReplaceBodyText)
		{
			generalConfirmationBodyText.SetTextAndReplaceWithIcons(body);
		}
		else
		{
			generalConfirmationBodyText.SetText(body);
		}
		string text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", buttonText);
		if (string.IsNullOrEmpty(text))
		{
			text = buttonText;
		}
		generalConfirmationButtonText.SetTextAndReplaceWithIcons(text);
		_centerButton.onClick.RemoveAllListeners();
		if (onClickCenter != null)
		{
			_centerButton.gameObject.SetActive(value: true);
			_centerButton.onClick.AddListener(OnClickOKGeneralConfirmation);
			_centerButton.onClick.AddListener(onClickCenter.Invoke);
		}
		else
		{
			_centerButton.gameObject.SetActive(value: false);
		}
		base.Open();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_canvasGroup.transform as RectTransform);
		generalConfirmationButton.onClick.RemoveAllListeners();
		generalConfirmationButton.onClick.AddListener(OnClickOKGeneralConfirmation);
		if (onClickOK != null)
		{
			generalConfirmationButton.onClick.AddListener(onClickOK.Invoke);
		}
		base.transform.SetAsLastSibling();
		TweenIn();
	}

	private void TweenIn()
	{
		_canvasGroup.alpha = 0f;
		RectTransform rectTransform = _canvasGroup.transform as RectTransform;
		rectTransform.anchoredPosition = new Vector2(0f, -30f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
		sequence.Join(DOTween.To(() => _canvasGroup.alpha, delegate(float x)
		{
			_canvasGroup.alpha = x;
		}, 1f, 0.5f).SetEase(Ease.InSine));
		sequence.PrependInterval(0.2f);
		sequence.Play();
	}

	public void OnClickOKGeneralConfirmation()
	{
		Close();
	}

	public override void Close()
	{
		base.Close();
		InputManager.Instance.SetInputMapState("Confirmation Window", p_state: false);
		InputManager.Instance.SetGamepadCursorState(p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Confirm, p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Cancel, p_state: false);
		if (PlayerUI.Instance != null && UIManager.Instance != null && !PlayerUI.Instance.TryShowPendingUI() && !UIManager.Instance.IsObjectPickerOpen())
		{
			UIManager.Instance.ResumeLastProgressionSpeed();
		}
	}
}
