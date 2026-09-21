using UnityEngine;
using UnityEngine.UI;

public class FactionInfoHubPopup : PopupMenuBase
{
	[SerializeField]
	private Toggle villagersToggle;

	public override void Close()
	{
		base.Close();
		villagersToggle.SetIsOnWithoutNotify(value: false);
	}

	public override void Open()
	{
		base.Open();
		villagersToggle.SetIsOnWithoutNotify(value: true);
	}
}
