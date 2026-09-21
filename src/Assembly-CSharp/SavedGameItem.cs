using System;
using System.IO;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class SavedGameItem : PooledObject
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private TextMeshProUGUI itemLbl;

	[SerializeField]
	private TextMeshProUGUI subLbl;

	[SerializeField]
	private Button deleteBtn;

	[SerializeField]
	private Button renameBtn;

	private Action<SavedGameItem> _onToggleSaveItem;

	private Action<SavedGameItem> _onClickDelete;

	private Action<SavedGameItem> _onClickRename;

	private string _savePath;

	private string _fileName;

	public string savePath => _savePath;

	public string fileName => _fileName;

	private void Awake()
	{
		toggle.onValueChanged.RemoveAllListeners();
		deleteBtn.onClick.RemoveAllListeners();
		renameBtn.onClick.RemoveAllListeners();
		toggle.onValueChanged.AddListener(OnToggleItem);
		deleteBtn.onClick.AddListener(OnClickDelete);
		renameBtn.onClick.AddListener(OnClickRename);
	}

	public void Initialize(string p_savePath, Action<SavedGameItem> p_onToggleSaveItem, Action<SavedGameItem> p_onClickDelete, Action<SavedGameItem> p_onClickRename, ToggleGroup p_toggleGroup, bool isFromAutosave)
	{
		_savePath = p_savePath;
		_onToggleSaveItem = p_onToggleSaveItem;
		_onClickDelete = p_onClickDelete;
		_onClickRename = p_onClickRename;
		toggle.group = p_toggleGroup;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(savePath);
		_fileName = fileNameWithoutExtension;
		if (isFromAutosave)
		{
			itemLbl.text = _fileName.Substring(_fileName.IndexOf('_') + 1);
		}
		else
		{
			itemLbl.text = fileName;
		}
		DateTime lastWriteTime = File.GetLastWriteTime(p_savePath);
		subLbl.text = lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
		renameBtn.gameObject.SetActive(!isFromAutosave);
	}

	public void ToggleOn()
	{
		toggle.isOn = true;
	}

	public void Rename(string p_newName)
	{
		_fileName = p_newName;
		string text = Utilities.gameSavePath + p_newName + ".zip";
		File.Move(_savePath, text);
		itemLbl.text = fileName;
		_savePath = text;
		DateTime lastWriteTime = File.GetLastWriteTime(text);
		subLbl.text = lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
	}

	public override void Reset()
	{
		base.Reset();
		_savePath = string.Empty;
		toggle.SetIsOnWithoutNotify(value: false);
	}

	private void OnToggleItem(bool p_isOn)
	{
		if (p_isOn)
		{
			_onToggleSaveItem?.Invoke(this);
		}
	}

	private void OnClickRename()
	{
		_onClickRename?.Invoke(this);
	}

	private void OnClickDelete()
	{
		_onClickDelete?.Invoke(this);
	}
}
