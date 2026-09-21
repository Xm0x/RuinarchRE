using UnityEngine;
using UnityEngine.UI;

public class SpellListUI : PopupMenuBase
{
	[SerializeField]
	private Toggle spellsToggle;

	public override void Open()
	{
		base.Open();
		spellsToggle.SetIsOnWithoutNotify(value: true);
	}

	public override void Close()
	{
		base.Close();
		spellsToggle.SetIsOnWithoutNotify(value: false);
	}

	protected override void OnGameObjectEnabled()
	{
		base.OnGameObjectEnabled();
		Messenger.Broadcast(UISignals.TOP_UI_ENABLED);
	}

	protected override void OnGameObjectDisabled()
	{
		base.OnGameObjectDisabled();
		Messenger.Broadcast(UISignals.TOP_UI_DISABLED);
	}
}
