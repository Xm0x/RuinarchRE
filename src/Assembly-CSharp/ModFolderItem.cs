using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ModFolderItem : MonoBehaviour
{
	[SerializeField]
	private RuinarchText _toggleText;

	[SerializeField]
	private Toggle _toggle;

	private DirectoryInfo _dirInfo;

	private Action<ModFolderItem, bool> _toggleAction;

	public string fullFolderPath => _dirInfo.FullName;

	public string directoryName => _dirInfo.Name;

	public Toggle toggle => _toggle;

	private void Start()
	{
		_toggle.onValueChanged.AddListener(OnToggle);
	}

	public void Initialize(ToggleGroup p_group)
	{
		_toggle.group = p_group;
	}

	public void ResetItem()
	{
		_dirInfo = null;
		_toggleAction = null;
	}

	public void SetFullFilePath(string p_fullFilePath)
	{
		_dirInfo = new DirectoryInfo(p_fullFilePath);
		_toggleText.text = _dirInfo.Name;
	}

	public void SetToggleAction(Action<ModFolderItem, bool> p_action)
	{
		_toggleAction = p_action;
	}

	private void OnToggle(bool p_state)
	{
		_toggleAction?.Invoke(this, p_state);
	}
}
