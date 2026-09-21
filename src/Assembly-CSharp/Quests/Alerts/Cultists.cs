using Traits;

namespace Quests.Alerts;

public class Cultists : GameAlert
{
	public Cultists()
		: base(Game_Alert.Cultists)
	{
	}

	public Cultists(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Cultists)
	{
	}

	public override void SetAsSpawned()
	{
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
	}

	protected override void SetAsCleared()
	{
		base.SetAsCleared();
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	private void OnCharacterGainedTrait(Character p_character, Trait p_trait)
	{
		if (p_trait is DemonCultist)
		{
			SetAsActive();
		}
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		UIManager.Instance.ShowSmallInfo(GetLocalizedString(_strGameAlertType + "_Tooltip_1"), p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		UIManager.Instance.HideSmallInfo();
	}
}
