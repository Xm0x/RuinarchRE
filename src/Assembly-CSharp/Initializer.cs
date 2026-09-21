using Inner_Maps;
using Managers;
using Quests;
using Tutorial;
using UnityEngine;

public class Initializer : MonoBehaviour
{
	public void InitializeDataBeforeWorldCreationMainThread()
	{
		LocalizationManager.Instance.Initialize();
		GameManager.Instance.Initialize();
		SaveManager.Instance.PrepareTempDirectory();
		DatabaseManager.Instance.Initialize();
		CharacterManager.Instance.Initialize();
		TraitManager.Instance.Initialize();
		PlayerManager.Instance.Initialize();
		InnerMapManager.Instance.Initialize();
		UIManager.Instance.InitializeUI();
		PlayerUI.Instance.Initialize();
		WorldEventManager.Instance.Initialize();
		WorldSettings.Instance.worldSettingsData.villageSettings.RandomizeCultistCreationThresholds();
	}

	public void InitializeDataBeforeWorldCreationOtherThread(object state)
	{
		LoadThreadQueueItem obj = state as LoadThreadQueueItem;
		RaceManager.Instance.Initialize();
		LandmarkManager.Instance.Initialize();
		CrimeManager.Instance.Initialize();
		InteractionManager.Instance.Initialize();
		JobManager.Instance.Initialize();
		PlayerSkillManager.Instance.ResetSpellsInUse();
		PlayerSkillManager.Instance.ResetSummonPlayerSkills();
		PlayerSkillManager.Instance.ResetCachedTexts();
		CombatManager.Instance.Initialize();
		obj.isDone = true;
	}

	public void InitializeDataAfterWorldCreation()
	{
		PlayerUI.Instance.InitializeAfterGameLoaded();
		FactionInfoHubUI.Instance.InitializeAfterGameLoaded();
		LightingManager.Instance.Initialize();
		QuestManager.Instance.InitializeAfterGameLoaded();
		AudioManager.Instance.OnGameLoaded();
		QuestManager.Instance.InitializeVictoryCondition();
	}

	public void InitializeDataAfterLoadoutSelection()
	{
		if (!SaveManager.Instance.useSaveData)
		{
			PlayerManager.Instance.player.LoadPlayerData(SaveManager.Instance.currentSaveDataPlayer);
		}
		QuestManager.Instance.InitializeAfterLoadoutPicked();
		WorldEventManager.Instance.InitializeAfterLoadoutPicked();
		UIManager.Instance.InitializeAfterLoadOutPicked();
		PlayerUI.Instance.InitializeAfterLoadOutPicked();
		PlayerManager.Instance.player.InitializeAfterLoadoutPicked();
		TutorialManager.Instance.Initialize();
		if (!SaveManager.Instance.useSaveData)
		{
			MapGenerationFinalization.ItemGenerationAfterPickingLoadout();
		}
		AudioManager.Instance.OnLoadoutSelected();
	}
}
