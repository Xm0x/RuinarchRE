using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;

public class KeybindItem : MonoBehaviour
{
	public SHORTCUT_ACTION shortcutAction;

	[SerializeField]
	private InputActionReference _inputAction;

	[SerializeField]
	private TextMeshProUGUI commandLbl;

	[SerializeField]
	private RuinarchButton mainKeybindButton;

	[SerializeField]
	private TextMeshProUGUI mainKeybindLbl;

	[SerializeField]
	private RuinarchButton resetButton;

	[SerializeField]
	private CustomRebindActionUI rebindAction;

	private string _hoverMessage;

	private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

	private void OnEnable()
	{
		mainKeybindButton.onClick.AddListener(OnClickRebindMain);
		resetButton.onClick.AddListener(OnClickReset);
	}

	private void OnDisable()
	{
		mainKeybindButton.onClick.RemoveListener(OnClickRebindMain);
		resetButton.onClick.RemoveListener(OnClickReset);
	}

	public void Initialize(GameObject keybindCover, TextMeshProUGUI keybindCoverText)
	{
		rebindAction.rebindOverlay = keybindCover;
		rebindAction.rebindPrompt = keybindCoverText;
	}

	public void UpdateKeybindDisplay()
	{
		rebindAction.UpdateBindingDisplay();
	}

	private void OnClickRebindMain()
	{
		rebindAction.StartInteractiveRebind();
		mainKeybindLbl.text = "Waiting...";
	}

	private void OnClickReset()
	{
		rebindAction.ResetToDefault();
	}
}
