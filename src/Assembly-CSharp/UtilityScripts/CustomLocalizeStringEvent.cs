using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace UtilityScripts;

public class CustomLocalizeStringEvent : LocalizeStringEvent
{
	[SerializeField]
	private TextMeshProUGUI _textMeshProUGUI;

	private void Awake()
	{
		TryAssignTextMesh();
		if (_textMeshProUGUI != null)
		{
			base.OnUpdateString.AddListener(UpdateTextMeshText);
		}
	}

	private void UpdateTextMeshText(string s)
	{
		_textMeshProUGUI.text = s;
	}

	public void TryAssignTextMesh()
	{
		if (_textMeshProUGUI == null)
		{
			_textMeshProUGUI = GetComponent<TextMeshProUGUI>();
		}
	}
}
