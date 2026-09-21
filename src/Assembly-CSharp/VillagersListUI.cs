using UnityEngine;
using UnityEngine.UI;

public class VillagersListUI : PopupMenuBase
{
	[SerializeField]
	private Toggle toggle;

	public override void Open()
	{
		base.Open();
		toggle.SetIsOnWithoutNotify(value: true);
	}

	public override void Close()
	{
		base.Close();
		toggle.SetIsOnWithoutNotify(value: false);
	}
}
