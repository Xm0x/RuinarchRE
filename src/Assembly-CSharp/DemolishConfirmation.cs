using DG.Tweening;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DemolishConfirmation : PopupMenuBase
{
	[SerializeField]
	protected Toggle _corruptedTilesToggle;

	[SerializeField]
	protected Toggle _demonicWallsToggle;

	[SerializeField]
	protected Toggle _decorationsToggle;

	[SerializeField]
	protected Toggle _structuresToggle;

	[SerializeField]
	protected RuinarchButton _okButton;

	[SerializeField]
	protected CanvasGroup _canvasGroup;

	private UnityAction<bool, bool, bool, bool> _okAction;

	public void ShowDemolishConfirmation(UnityAction<bool, bool, bool, bool> p_okAction)
	{
		if (UIManager.Instance != null)
		{
			if (!UIManager.Instance.IsObjectPickerOpen())
			{
				UIManager.Instance.Pause();
				UIManager.Instance.SetSpeedTogglesState(state: false);
			}
			UIManager.Instance.HideSmallInfo();
		}
		_okAction = p_okAction;
		base.Open();
		base.transform.SetAsLastSibling();
		TweenIn();
	}

	private void TweenIn()
	{
		_canvasGroup.alpha = 0f;
		_canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InSine);
	}

	public void OnClickOK()
	{
		_okAction?.Invoke(_corruptedTilesToggle.isOn, _demonicWallsToggle.isOn, _decorationsToggle.isOn, _structuresToggle.isOn);
		Close();
	}

	public override void Close()
	{
		base.Close();
		_okAction = null;
		if (PlayerUI.Instance != null && UIManager.Instance != null && !PlayerUI.Instance.TryShowPendingUI() && !UIManager.Instance.IsObjectPickerOpen())
		{
			UIManager.Instance.ResumeLastProgressionSpeed();
		}
	}

	public void OnToggleCorruptedTiles(bool p_state)
	{
		if (p_state)
		{
			_demonicWallsToggle.isOn = p_state;
			_decorationsToggle.isOn = p_state;
			_structuresToggle.isOn = p_state;
		}
		_demonicWallsToggle.interactable = !p_state;
		_decorationsToggle.interactable = !p_state;
		_structuresToggle.interactable = !p_state;
	}
}
