using System;
using Inner_Maps.Location_Structures;

namespace Quests;

public class QuestManager : BaseMonoBehaviour
{
	public static QuestManager Instance;

	public VictoryCondition victoryCondition { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
		Messenger.RemoveListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnSingleCharacterAttackedDemonicStructure);
	}

	public void LoadVictoryCondition(SaveDataVictoryCondition data)
	{
		victoryCondition = data.Load();
		victoryCondition.LoadInitialize();
		if (!PlayerManager.Instance.player.hasAlreadyWon)
		{
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(victoryCondition, BOOKMARK_CATEGORY.Win_Condition);
		}
	}

	public void InitializeVictoryCondition()
	{
		switch (WorldSettings.Instance.worldSettingsData.victoryCondition)
		{
		case VICTORY_CONDITION.Progression:
			victoryCondition = new Progression();
			break;
		case VICTORY_CONDITION.Attainment:
			victoryCondition = new Attainment();
			break;
		case VICTORY_CONDITION.Eradication:
			victoryCondition = new Eradication();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		victoryCondition.Initialize();
	}

	public void InitializeAfterGameLoaded()
	{
		Messenger.AddListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnSingleCharacterAttackedDemonicStructure);
		Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "CenterButton");
	}

	public void InitializeAfterLoadoutPicked()
	{
		if (!SaveManager.Instance.useSaveData)
		{
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(victoryCondition, BOOKMARK_CATEGORY.Win_Condition);
		}
	}

	public void OnClickCenterButton()
	{
		Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "CenterButton");
	}

	private void OnSingleCharacterAttackedDemonicStructure(Character character, DemonicStructure demonicStructure)
	{
		if (demonicStructure.currentAttackers.Count == 1 && !InnerMapCameraMove.Instance.CanSee(demonicStructure))
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Under_Attack");
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Under_Attack_Description");
			PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, localizedValue2, "OK", null, demonicStructure.CenterOnStructure);
		}
	}
}
