using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveWindowUIModel : MVCUIModel
{
	public TextMeshProUGUI titleLbl;

	public Button closeBtn;

	public Button saveLoadBtn;

	public TextMeshProUGUI saveLoadLbl;

	[Header("Save Items")]
	public GameObject saveItemPrefab;

	public ScrollRect saveItemsScrollView;

	public ToggleGroup saveItemsToggleGroup;

	public RectTransform createNewSaveItemRect;

	public Button createNewSaveItemBtn;

	[Header("Save Info")]
	public TMP_InputField saveNameInputField;

	public GameObject saveErrorGO;

	public TextMeshProUGUI saveErrorLbl;

	public TextMeshProUGUI saveInfoLbl;

	public RawImage saveScreenshotImage;

	[Header("Create new save")]
	public GameObject createNewSaveGO;

	public TMP_InputField createNewSaveInputField;

	public Button confirmCreateNewSaveBtn;

	[Header("Rename save")]
	public GameObject renameSaveGO;

	public TMP_InputField renameSaveInputField;

	public Button confirmRenameSaveBtn;

	public Action onClickClose;

	public Action OnClickSaveLoad;

	public Action onClickCreateNewSave;

	public Action onClickConfirmCreateNewSave;

	public Action<string> onSubmitCreateNewSave;

	public Action<string> onValueChangedCreateNewSave;

	public Action onClickConfirmRenameSave;

	public Action onSubmitRenameSave;

	private void OnEnable()
	{
		saveLoadBtn.onClick.AddListener(ClickSaveLoad);
		closeBtn.onClick.AddListener(ClickClose);
		createNewSaveItemBtn.onClick.AddListener(ClickCreateNewSave);
		confirmCreateNewSaveBtn.onClick.AddListener(ClickConfirmCreateNewSave);
		createNewSaveInputField.onSubmit.AddListener(OnSubmitCreateNewSave);
		createNewSaveInputField.onValueChanged.AddListener(OnValueChangedCreateNewSave);
		confirmRenameSaveBtn.onClick.AddListener(OnClickConfirmRenameSave);
		renameSaveInputField.onSubmit.AddListener(OnSubmitRenameSave);
	}

	private void OnDisable()
	{
		saveLoadBtn.onClick.RemoveListener(ClickSaveLoad);
		closeBtn.onClick.RemoveListener(ClickClose);
		createNewSaveItemBtn.onClick.RemoveListener(ClickCreateNewSave);
		confirmCreateNewSaveBtn.onClick.RemoveListener(ClickConfirmCreateNewSave);
		createNewSaveInputField.onSubmit.RemoveListener(OnSubmitCreateNewSave);
		createNewSaveInputField.onValueChanged.RemoveListener(OnValueChangedCreateNewSave);
		confirmRenameSaveBtn.onClick.RemoveListener(OnClickConfirmRenameSave);
		renameSaveInputField.onSubmit.RemoveListener(OnSubmitRenameSave);
	}

	private void ClickClose()
	{
		onClickClose?.Invoke();
	}

	private void ClickSaveLoad()
	{
		OnClickSaveLoad?.Invoke();
	}

	private void ClickCreateNewSave()
	{
		onClickCreateNewSave?.Invoke();
	}

	private void ClickConfirmCreateNewSave()
	{
		onClickConfirmCreateNewSave?.Invoke();
	}

	private void OnSubmitCreateNewSave(string p_value)
	{
		onSubmitCreateNewSave?.Invoke(p_value);
	}

	private void OnValueChangedCreateNewSave(string p_value)
	{
		onValueChangedCreateNewSave?.Invoke(p_value);
	}

	private void OnClickConfirmRenameSave()
	{
		onClickConfirmRenameSave?.Invoke();
	}

	private void OnSubmitRenameSave(string p_value)
	{
		onClickConfirmRenameSave?.Invoke();
	}
}
