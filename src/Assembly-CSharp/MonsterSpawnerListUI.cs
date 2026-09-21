using UnityEngine;
using UnityEngine.UI;

public class MonsterSpawnerListUI : PopupMenuBase
{
	[SerializeField]
	private Toggle toggle;

	public override void Open()
	{
		base.Open();
		toggle.isOn = true;
	}

	public override void Close()
	{
		toggle.isOn = false;
		base.Close();
	}
}
