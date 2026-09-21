using UnityEngine;
using UnityEngine.UI;

public class IntelListUI : PopupMenuBase
{
	[SerializeField]
	private Toggle _intelToggle;

	public override void Open()
	{
		base.Open();
		_intelToggle.SetIsOnWithoutNotify(value: true);
	}

	public override void Close()
	{
		base.Close();
		_intelToggle.SetIsOnWithoutNotify(value: false);
	}
}
