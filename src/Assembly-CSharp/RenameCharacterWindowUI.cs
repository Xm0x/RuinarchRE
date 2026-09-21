using TMPro;
using UnityEngine;

public class RenameCharacterWindowUI : MonoBehaviour
{
	public TextMeshProUGUI titleLbl;

	public TMP_InputField inputField;

	public GameObject mainWindow;

	private string _characterPersistentID;

	private string _characterCurrentName;

	private void Start()
	{
		Messenger.AddListener<string, string>(UISignals.EDIT_CHARACTER_NAME, OnListenedEditCharacterName);
	}

	private void OnListenedEditCharacterName(string characterPersistentID, string characterCurrentName)
	{
		RenameCharacterProcess(characterPersistentID, characterCurrentName);
	}

	private void RenameCharacterProcess(string characterPersistentID, string characterCurrentName)
	{
		_characterPersistentID = characterPersistentID;
		_characterCurrentName = characterCurrentName;
		UpdateTitleLbl();
		SetInitialInputFieldText();
		ShowHideWindow(state: true);
	}

	private void ShowHideWindow(bool state)
	{
		mainWindow.SetActive(state);
	}

	private void UpdateTitleLbl()
	{
		titleLbl.text = "Rename " + _characterCurrentName;
	}

	private void SetInitialInputFieldText()
	{
		inputField.text = _characterCurrentName;
	}

	public void OnClickConfirmButton()
	{
		Messenger.Broadcast(CharacterSignals.RENAME_CHARACTER, _characterPersistentID, inputField.text);
		ShowHideWindow(state: false);
	}
}
