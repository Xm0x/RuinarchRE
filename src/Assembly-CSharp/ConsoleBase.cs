using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Managers;
using Object_Pools;
using Pathfinding;
using Quests.Alerts;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using UtilityScripts;

public class ConsoleBase : InfoUIBase
{
	private Dictionary<string, Action<string[]>> _consoleActions;

	public static bool showPOIHoverData;

	public static bool alwaysShowNotifications;

	public static bool fasterPortalUpgrade;

	public static bool checkStructureReferences;

	public static bool checkCharacterReferences;

	private List<string> commandHistory;

	[SerializeField]
	private GameObject consoleGO;

	[SerializeField]
	private Text consoleLbl;

	[SerializeField]
	private InputField consoleInputField;

	[SerializeField]
	private GameObject commandHistoryGO;

	[SerializeField]
	private TextMeshProUGUI commandHistoryLbl;

	[SerializeField]
	private GameObject fullDebugGO;

	[SerializeField]
	private TextMeshProUGUI fullDebugLbl;

	[SerializeField]
	private TextMeshProUGUI fullDebug2Lbl;

	[SerializeField]
	private Toggle tglAlwaysSuccessScheme;

	[SerializeField]
	private Toggle tglShowPOIHoverData;

	[SerializeField]
	private Toggle tglAlwaysShowNotifications;

	[SerializeField]
	private Toggle tglFasterPortalUpgrade;

	[SerializeField]
	private Toggle tglStructureReferenceTester;

	[SerializeField]
	private Toggle tglCharacterReferenceTester;

	[SerializeField]
	private ChanceTheWrapper chanceTheWrapper;

	private List<INTERACTION_TYPE> typesSubscribedTo = new List<INTERACTION_TYPE>();

	private void Awake()
	{
		Initialize();
	}

	internal override void Initialize()
	{
		commandHistory = new List<string>();
		_consoleActions = new Dictionary<string, Action<string[]>>
		{
			{ "/help", ShowHelp },
			{ "/set_faction_rel", ChangeFactionRelationshipStatus },
			{ "/kill", KillCharacter },
			{ "/center_character", CenterOnCharacter },
			{ "/log_location_history", LogLocationHistory },
			{ "/log_area_characters_history", LogAreaCharactersHistory },
			{ "/get_characters_with_item", GetCharactersWithItem },
			{ "/i_toggle_sub", ToggleSubscriptionToInteraction },
			{ "/add_trait_character", AddTraitToCharacter },
			{ "/remove_trait_character", RemoveTraitToCharacter },
			{ "/transfer_character_faction", TransferCharacterToFaction },
			{ "/show_full_debug", ShowFullDebug },
			{ "/t_freeze_char", ToggleFreezeCharacter },
			{ "/set_mood", SetMoodToCharacter },
			{ "/log_awareness", LogAwareness },
			{ "/add_rel", AddRelationship },
			{ "/set_hp", SetHP },
			{ "/gain_summon", GainSummon },
			{ "/gain_artifact", GainArtifact },
			{ "/set_fullness", SetFullness },
			{ "/set_tiredness", SetTiredness },
			{ "/set_happiness", SetHappiness },
			{ "/set_comfort", SetStamina },
			{ "/set_hope", SetHope },
			{ "/gain_i_ability", GainInterventionAbility },
			{ "/destroy_tile_obj", DestroyTileObj },
			{ "/force_update_animation", ForceUpdateAnimation },
			{ "/log_obj_advertisements", LogObjectAdvertisements },
			{ "/adjust_opinion", AdjustOpinion },
			{ "/join_faction", JoinFaction },
			{ "/emotion", TriggerEmotion },
			{ "/change_archetype", ChangeArchetype },
			{ "/elemental_damage", ChangeCharacterElementalDamage },
			{ "/add_item", AddItemToCharacter },
			{ "/null_home", ChangeCharacterHomeToNull },
			{ "/damage_tile", DamageTile },
			{ "/create_faction", CreateFaction },
			{ "/save_scenario", SaveScenarioMap },
			{ "/save_manual", SaveManual },
			{ "/set_party_state", SwitchPartyState },
			{ "/save_db", SaveDatabaseInMemory },
			{ "/find_object", FindTileObject },
			{ "/change_name", ChangeName },
			{ "/adjust_mana", AdjustMana },
			{ "/adjust_pp", AdjustPlaguePoints },
			{ "/remove_needed_class", RemoveNeededClassFromSettlement },
			{ "/activate_settlement_event", ActivateSettlementEvent },
			{ "/trigger_quarantine", TriggerQuarantine },
			{ "/add_ideology", AddFactionIdeology },
			{ "/remove_ideology", RemoveFactionIdeology },
			{ "/check_tiles", CheckTiles },
			{ "/reveal_all", RevealAll },
			{ "/enable_dig", EnableDigging },
			{ "/bonus_charge", BonusCharges },
			{ "/log_alive_villagers", LogAliveVillagers },
			{ "/kill_villagers", KillAllVillagers },
			{ "/adjust_se", AdjustSpiritEnergy },
			{ "/adjust_mm", AdjustMigrationMeter },
			{ "/toggle_vs", ToggleVillageSpots },
			{ "/coins", AdjustCoins },
			{ "/talent_level_up", TalentLevelUp },
			{ "/adjust_resistance", AdjustResistance },
			{ "/log_structure_connectors", LogStructureConnectors },
			{ "/set_combat_skill", SetCombatSkill },
			{ "/change_class", ChangeCharacterClass },
			{ "/expire_body", ForceExpireBody },
			{ "/expire_all_body", ForceExpireAllBody },
			{ "/walkability", Walkability },
			{ "/clear_blacklist", ClearBlacklist },
			{ "/bel_point", IncreaseBeliefPoints },
			{ "/adjust_resource", AdjustResourceInPile },
			{ "/add_power", AddPower },
			{ "/avoid_structure", AvoidStructure },
			{ "/log_deaths", LogDeaths },
			{ "/unavoid_structure", UnavoidStructure },
			{ "/spawn_tile_objects", SpawnTileObjects },
			{ "/spawn_party_quests", SpawnPartyQuests },
			{ "/log_actions", LogActions },
			{ "/interrupt_behaviour", ToggleInterruptTriggerBehaviour },
			{ "/fulfill_achievement", FulfillAchievement },
			{ "/reset_achievement", ResetAchievement },
			{ "/reset_all_achievement", ResetAllAchievement },
			{ "/adjust_ach_stat", AdjustAchievementStat },
			{ "/log_party_names", LogPartyNames },
			{ "/complete_subgoals", CompleteAllSubGoals },
			{ "/complete_goal_tasks", CompleteAllGoalTasks },
			{ "/set_home", SetHomeStructure },
			{ "/go_to", GoTo },
			{ "/iseedeadpeople", IncreaseDemonicEyesForTesting },
			{ "/pf_scan", ScanPathfinding },
			{ "/pf_cc", DoCollisionCheck },
			{ "/spawn_alerts", SpawnAllGameAlerts }
		};
		SchemeData.alwaysSuccessScheme = false;
		tglAlwaysSuccessScheme.SetIsOnWithoutNotify(SchemeData.alwaysSuccessScheme);
		tglAlwaysSuccessScheme.onValueChanged.RemoveAllListeners();
		tglAlwaysSuccessScheme.onValueChanged.AddListener(OnToggleAlwaysSuccessScheme);
		chanceTheWrapper.Initialize();
		tglShowPOIHoverData.SetIsOnWithoutNotify(showPOIHoverData);
		tglShowPOIHoverData.onValueChanged.RemoveAllListeners();
		tglShowPOIHoverData.onValueChanged.AddListener(OnToggleShowPOIHoverData);
		tglAlwaysShowNotifications.SetIsOnWithoutNotify(alwaysShowNotifications);
		tglAlwaysShowNotifications.onValueChanged.RemoveAllListeners();
		tglAlwaysShowNotifications.onValueChanged.AddListener(OnToggleAlwaysShowNotifications);
		tglFasterPortalUpgrade.SetIsOnWithoutNotify(fasterPortalUpgrade);
		tglFasterPortalUpgrade.onValueChanged.RemoveAllListeners();
		tglFasterPortalUpgrade.onValueChanged.AddListener(OnToggleFasterPortalUpgrade);
		tglCharacterReferenceTester.SetIsOnWithoutNotify(checkCharacterReferences);
		tglCharacterReferenceTester.onValueChanged.RemoveAllListeners();
		tglCharacterReferenceTester.onValueChanged.AddListener(OnToggleCheckCharacterReferences);
		tglStructureReferenceTester.SetIsOnWithoutNotify(checkStructureReferences);
		tglStructureReferenceTester.onValueChanged.RemoveAllListeners();
		tglStructureReferenceTester.onValueChanged.AddListener(OnToggleCheckStructureReferences);
	}

	private void OnToggleCheckCharacterReferences(bool p_isOn)
	{
		checkCharacterReferences = p_isOn;
	}

	private void OnToggleCheckStructureReferences(bool p_isOn)
	{
		checkStructureReferences = p_isOn;
	}

	private void Update()
	{
		if (isShowing)
		{
			fullDebugLbl.text = string.Empty;
			fullDebug2Lbl.text = string.Empty;
			string text = "World Settings:";
			text = text + "\nMigration: " + WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed;
			text = text + "\nCooldown: " + WorldSettings.Instance.worldSettingsData.playerSkillSettings.cooldownSpeed;
			text = text + "\nCosts: " + WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount;
			text = text + "\nCharges: " + WorldSettings.Instance.worldSettingsData.playerSkillSettings.chargeAmount;
			text = text + "\nCorruption Charges: " + WorldSettings.Instance.worldSettingsData.playerSkillSettings.corruptionChargeAmount;
			text = text + "\nRetaliation: " + WorldSettings.Instance.worldSettingsData.playerSkillSettings.retaliation;
			text = text + "\nVictory Condition: " + WorldSettings.Instance.worldSettingsData.victoryCondition;
			text = text + "\nInitial Divine Cultist Points: " + WorldSettings.Instance.worldSettingsData.villageSettings.divineCultistPointThreshold;
			text = text + "\nInitial Nature Cultist Points: " + WorldSettings.Instance.worldSettingsData.villageSettings.natureCultistPointThreshold;
			text += "\nPathfinding:";
			if (AstarPath.active.graphs.Length != 0)
			{
				text = text + "\nTotal Nodes: " + AstarPath.active.graphs[0].CountNodes();
			}
			text += "\n\nObject Pooling:";
			text += "\n\tLogs in Pool:";
			text = text + " " + LogPool.GetCurrentLogsInPool();
			fullDebugLbl.text = text;
			string text2 = "Tutorial Settings:";
			text2 = text2 + "\nSpawned Alerts:\n\t " + TutorialManager.Instance.spawnedAlerts.ComafyList();
			text2 = text2 + "\nActive Alerts:\n\t " + TutorialManager.Instance.activeAlerts.ComafyList();
			text2 = text2 + "\nValid Generic Alerts:\n\t " + TutorialManager.Instance.genericAlertPool.ComafyList();
			text2 = text2 + "\nValid Building Alerts:\n\t " + TutorialManager.Instance.buildingAlertPool.ComafyList();
			fullDebugLbl.text = fullDebugLbl.text + "\n" + text2;
			string text3 = "Player Settings:";
			text3 = text3 + "\nSpell Damage Threshold: " + PlayerManager.Instance.player.damageAccumulator.accumulatedDamage + "/" + PlayerManager.Instance.player.playerSkillComponent.chaosOrbExpulsionThreshold;
			text3 = text3 + "\nRaid Damage Threshold: " + PlayerManager.Instance.player.playerSkillComponent.chaosOrbExpulsionThresholdFromRaid;
			text3 = text3 + "\nSpawned Angels: " + PlayerManager.Instance.player.retaliationComponent.spawnedRetaliators.ComafyList();
			text3 = text3 + "\nRetaliator count: " + PlayerManager.Instance.player.retaliationComponent.retaliatorCount;
			fullDebugLbl.text = fullDebugLbl.text + "\n" + text3;
			string text4 = "Utilities:";
			text4 = text4 + "\nLast used character id: " + Utilities.lastCharacterID;
			text4 = text4 + "\nWorld Events: " + WorldEventManager.Instance.activeEvents.ComafyList();
			fullDebugLbl.text = fullDebugLbl.text + "\n" + text4;
			fullDebugGO.SetActive(!string.IsNullOrEmpty(fullDebugLbl.text) || !string.IsNullOrEmpty(fullDebug2Lbl.text));
			if (isShowing && consoleInputField.text != "" && Input.GetKeyDown(KeyCode.Return))
			{
				SubmitCommand();
			}
		}
	}

	private void FullDebugInfo()
	{
		fullDebugLbl.text = string.Empty;
		fullDebug2Lbl.text = string.Empty;
		if (UIManager.Instance != null && UIManager.Instance.characterInfoUI.isShowing)
		{
			fullDebugLbl.text += GetMainCharacterInfo();
			fullDebug2Lbl.text += GetSecondaryCharacterInfo();
		}
	}

	private string GetMainCharacterInfo()
	{
		Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter;
		string text = activeCharacter.name + "'s info:";
		text += $"\n<b>Gender:</b> {activeCharacter.gender}";
		text += $"\n<b>Race:</b> {activeCharacter.race}";
		text = text + "\n<b>Class:</b> " + activeCharacter.characterClass.className;
		text += $"\n<b>Is Dead?:</b> {activeCharacter.isDead}";
		text += $"\n<b>Home Location:</b> {activeCharacter.homeStructure}" ?? "None";
		text += "\n<b>LOCATION INFO:</b>";
		text = text + "\n\t<b>Region Location:</b> " + activeCharacter.currentRegion?.name;
		text += $"\n\t<b>Structure Location:</b> {activeCharacter.currentStructure}" ?? "None";
		text += $"\n\t<b>Grid Location:</b> {activeCharacter.gridTileLocation?.localPlace}" ?? "None";
		text += $"\n\t<b>Previous Grid Location:</b> {activeCharacter.marker?.previousGridTile.localPlace}" ?? "None";
		text = text + "\n<b>Faction:</b> " + activeCharacter.faction?.name;
		text = text + "\n<b>Current Action:</b> " + activeCharacter.currentActionNode?.goapName;
		text += $"\n<b>Is Travelling In World:</b> {activeCharacter.carryComponent.masterCharacter.movementComponent.isTravellingInWorld}";
		if ((bool)activeCharacter.marker)
		{
			text += "\n<b>MARKER DETAILS:</b>";
			text = text + "\n<b>Target POI:</b> " + activeCharacter.marker.targetPOI?.name;
			text += $"\n<b>Destination Tile:</b> {activeCharacter.marker.destinationTile}" ?? "None";
			text += $"\n<b>Stop Movement?:</b> {activeCharacter.marker.pathfindingAI.isStopMovement}";
		}
		return text;
	}

	private string GetSecondaryCharacterInfo()
	{
		Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter;
		return "\n" + activeCharacter.name + "'s Location History:";
	}

	public void ShowConsole()
	{
		isShowing = true;
		consoleGO.SetActive(value: true);
		ClearCommandField();
		consoleInputField.Select();
	}

	public void HideConsole()
	{
		isShowing = false;
		consoleGO.SetActive(value: false);
		HideCommandHistory();
		ClearCommandHistory();
		consoleInputField.DeactivateInputField();
	}

	private void ClearCommandField()
	{
		consoleLbl.text = string.Empty;
	}

	private void ClearCommandHistory()
	{
		commandHistoryLbl.text = string.Empty;
		commandHistory.Clear();
	}

	private void ShowCommandHistory()
	{
		commandHistoryGO.SetActive(value: true);
	}

	private void HideCommandHistory()
	{
		commandHistoryGO.SetActive(value: false);
	}

	public void SubmitCommand()
	{
		string text = consoleLbl.text;
		string text2 = text.Split(' ')[0];
		string[] array = (from Match m in new Regex("\".*?\"").Matches(text)
			select m.Value).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			string text3 = array[num].Trim('"');
			array[num] = text3;
		}
		if (_consoleActions.ContainsKey(text2))
		{
			_consoleActions[text2](array);
			return;
		}
		AddCommandHistory(text);
		AddErrorMessage("Error: there is no such command as " + text2 + "![-]");
	}

	private void AddCommandHistory(string history)
	{
		TextMeshProUGUI textMeshProUGUI = commandHistoryLbl;
		textMeshProUGUI.text = textMeshProUGUI.text + history + "\n";
		commandHistory.Add(history);
		ShowCommandHistory();
	}

	private void AddErrorMessage(string errorMessage)
	{
		errorMessage += ". Use /help for a list of commands";
		TextMeshProUGUI textMeshProUGUI = commandHistoryLbl;
		textMeshProUGUI.text = textMeshProUGUI.text + "<color=#FF0000>" + errorMessage + "</color>\n";
		ShowCommandHistory();
	}

	private void AddSuccessMessage(string successMessage)
	{
		TextMeshProUGUI textMeshProUGUI = commandHistoryLbl;
		textMeshProUGUI.text = textMeshProUGUI.text + "<color=#00FF00>" + successMessage + "</color>\n";
		ShowCommandHistory();
	}

	private void ShowHelp(string[] parameters)
	{
		for (int i = 0; i < _consoleActions.Count; i++)
		{
			AddCommandHistory(_consoleActions.Keys.ElementAt(i));
		}
	}

	public void AddText(string text)
	{
		InputField inputField = consoleInputField;
		inputField.text = inputField.text + " " + text;
	}

	public void ShowFullDebug(string[] parameters)
	{
		GameManager.Instance.showFullDebug = !GameManager.Instance.showFullDebug;
		AddSuccessMessage($"Show Full Debug Info Set to {GameManager.Instance.showFullDebug}");
	}

	public void MoveNextPage(TextMeshProUGUI text)
	{
		text.pageToDisplay++;
	}

	public void MovePreviousPage(TextMeshProUGUI text)
	{
		text.pageToDisplay--;
	}

	public void ToggleShowAllTileTooltip(bool state)
	{
		GameManager.showAllTilesTooltip = state;
	}

	public void ToggleShowAccumulatedDamage(bool state)
	{
		PlayerUI.Instance.accumulatedDamageGO.SetActive(state);
	}

	private void ChangeFactionRelationshipStatus(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		string value = parameters[2];
		Faction faction = null;
		Faction faction2 = null;
		int result = -1;
		int result2 = -1;
		bool num = int.TryParse(text, out result);
		bool flag = int.TryParse(text2, out result2);
		string text3 = text;
		string text4 = text2;
		faction = ((!num) ? FactionManager.Instance.GetFactionBasedOnName(text3) : FactionManager.Instance.GetFactionBasedOnID(result));
		faction2 = ((!flag) ? FactionManager.Instance.GetFactionBasedOnName(text4) : FactionManager.Instance.GetFactionBasedOnID(result2));
		FACTION_RELATIONSHIP_STATUS relationshipStatus;
		try
		{
			relationshipStatus = (FACTION_RELATIONSHIP_STATUS)Enum.Parse(typeof(FACTION_RELATIONSHIP_STATUS), value, ignoreCase: true);
		}
		catch
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
			return;
		}
		if (faction == null || faction2 == null)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
		}
		else
		{
			FactionRelationship relationshipBetween = FactionManager.Instance.GetRelationshipBetween(faction, faction2);
			relationshipBetween.SetRelationshipStatus(relationshipStatus);
			AddSuccessMessage($"Changed relationship status of {faction.name} and {faction2.name} to {relationshipBetween.relationshipStatus}");
		}
	}

	private void KillCharacter(string[] parameters)
	{
		if (parameters.Length < 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /kill");
			return;
		}
		string s = parameters[0];
		string text = parameters.ElementAtOrDefault(1);
		int result;
		bool num = int.TryParse(s, out result);
		Character character = null;
		character = ((!num) ? CharacterManager.Instance.GetCharacterByName(s) : CharacterManager.Instance.GetCharacterByID(result));
		if (character == null)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /kill");
			return;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "normal";
		}
		character.Death(text);
	}

	private void CenterOnCharacter(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of Center on Character");
			return;
		}
		string p_name = parameters[0];
		List<Character> list = RuinarchListPool<Character>.Claim();
		CharacterManager.Instance.GetCharactersByName(list, p_name);
		if (list.Count <= 0)
		{
			AddErrorMessage("There was an error in the command format of Center on Character");
			RuinarchListPool<Character>.Release(list);
		}
		else
		{
			Character character = (UIManager.Instance.characterInfoUI.isShowing ? CollectionUtilities.GetNextElementCyclic(list, UIManager.Instance.characterInfoUI.activeCharacter) : ((!UIManager.Instance.monsterInfoUI.isShowing) ? list[0] : CollectionUtilities.GetNextElementCyclic(list, UIManager.Instance.monsterInfoUI.activeCharacter)));
			UIManager.Instance.ShowCharacterInfo(character, centerOnCharacter: true);
		}
	}

	private void AddItemToCharacter(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AddItemToCharacter");
			return;
		}
		string s = parameters[0];
		string value = parameters[1];
		int result;
		bool num = int.TryParse(s, out result);
		Character character = null;
		character = (num ? CharacterManager.Instance.GetCharacterByID(result) : CharacterManager.Instance.GetCharacterByName(s));
		TILE_OBJECT_TYPE result2;
		if (character == null)
		{
			AddErrorMessage("There was an error in the command format of AddItemToCharacter");
		}
		else if (Enum.TryParse<TILE_OBJECT_TYPE>(value, ignoreCase: true, out result2))
		{
			TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(result2);
			character.ObtainItem(item);
		}
		else
		{
			AddErrorMessage("There was an error in the command format of AddItemToCharacter");
		}
	}

	private void LogLocationHistory(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of LogLocationHistory");
			return;
		}
		string s = parameters[0];
		int result;
		bool num = int.TryParse(s, out result);
		Character character = null;
		character = ((!num) ? CharacterManager.Instance.GetCharacterByName(s) : CharacterManager.Instance.GetCharacterByID(result));
		if (character == null)
		{
			AddErrorMessage("There was an error in the command format of LogLocationHistory");
			return;
		}
		string successMessage = character.name + "'s location history: ";
		AddSuccessMessage(successMessage);
	}

	private void GetCharactersWithItem(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of GetCharactersWithItem");
			return;
		}
		string text = parameters[0];
		List<Character> list = new List<Character>();
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (character.isHoldingItem && character.HasItem(text))
			{
				list.Add(character);
			}
		}
		string text2 = "Characters that have " + text + ": ";
		if (list.Count == 0)
		{
			text2 += "\nNONE";
		}
		else
		{
			for (int j = 0; j < list.Count; j++)
			{
				text2 = text2 + "\n" + list[j].name;
			}
		}
		AddSuccessMessage(text2);
	}

	private void AddTraitToCharacter(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AddTraitToCharacter");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		switch (text2)
		{
		case "Demon Cultist":
			characterByName.religionComponent.ChangeReligion(RELIGION.Demon_Worship);
			break;
		case "Witch":
			characterByName.religionComponent.ChangeReligion(RELIGION.Nature_Worship);
			break;
		case "Cleric":
			characterByName.religionComponent.ChangeReligion(RELIGION.Divine_Worship);
			break;
		}
		if (characterByName.traitContainer.AddTrait(characterByName, text2, null, bypassElementalChance: true, -1, 100f))
		{
			switch (text2)
			{
			case "Demon Cultist":
				if (characterByName.religionComponent.GetBeliefPoints(RELIGION.Demon_Worship) < ReligionComponent.Religious_Cultist_Belief_Threshold)
				{
					characterByName.religionComponent.IncreaseBeliefPoints(RELIGION.Demon_Worship, ReligionComponent.Religious_Cultist_Belief_Threshold);
				}
				break;
			case "Witch":
				if (characterByName.religionComponent.GetBeliefPoints(RELIGION.Nature_Worship) < ReligionComponent.Religious_Cultist_Belief_Threshold)
				{
					characterByName.religionComponent.IncreaseBeliefPoints(RELIGION.Nature_Worship, ReligionComponent.Religious_Cultist_Belief_Threshold);
				}
				break;
			case "Cleric":
				if (characterByName.religionComponent.GetBeliefPoints(RELIGION.Divine_Worship) < ReligionComponent.Religious_Cultist_Belief_Threshold)
				{
					characterByName.religionComponent.IncreaseBeliefPoints(RELIGION.Divine_Worship, ReligionComponent.Religious_Cultist_Belief_Threshold);
				}
				break;
			}
		}
		AddSuccessMessage("Added " + text2 + " to " + characterByName.name);
	}

	private void RemoveTraitToCharacter(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AddTraitToCharacter");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
		}
		else if (characterByName.traitContainer.RemoveTrait(characterByName, text2))
		{
			AddSuccessMessage("Removed " + text2 + " to " + characterByName.name);
		}
		else
		{
			AddErrorMessage(characterByName.name + " has no trait named " + text2);
		}
	}

	private void TransferCharacterToFaction(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of TransferCharacterToFaction");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		Faction factionBasedOnName = FactionManager.Instance.GetFactionBasedOnName(text2);
		if (factionBasedOnName == null)
		{
			AddErrorMessage("There is no faction named " + text2);
			return;
		}
		characterByName.ChangeFactionTo(factionBasedOnName);
		AddSuccessMessage("Transferred " + characterByName.name + " to " + factionBasedOnName.name);
	}

	private void ToggleFreezeCharacter(string[] parameter)
	{
		if (parameter.Length < 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ToggleFreezeCharacter");
			return;
		}
		string text = parameter[0];
		if (CharacterManager.Instance.GetCharacterByName(text) == null)
		{
			AddErrorMessage("There is no character with name " + text);
		}
	}

	private void SetMoodToCharacter(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SetMoodToCharacter");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		string text2 = parameters[1];
		if (!int.TryParse(text2, out var result))
		{
			AddErrorMessage("Mood value parameter is not an integer: " + text2);
			return;
		}
		characterByName.moodComponent.SetMoodValue(result);
		AddSuccessMessage($"Set Mood Value of {characterByName.name} to {result}");
	}

	private void SetFullness(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SetFullness");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		string text2 = parameters[1];
		float result = characterByName.needsComponent.fullness;
		if (!float.TryParse(text2, out result))
		{
			AddErrorMessage("Fullness parameter is not a float: " + text2);
			return;
		}
		characterByName.needsComponent.SetFullness(result);
		AddSuccessMessage($"Set Fullness Value of {characterByName.name} to {result}");
	}

	private void SetHappiness(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SetHappiness");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		string text2 = parameters[1];
		float result = characterByName.needsComponent.happiness;
		if (!float.TryParse(text2, out result))
		{
			AddErrorMessage("Happiness parameter is not a float: " + text2);
			return;
		}
		characterByName.needsComponent.SetHappiness(result);
		AddSuccessMessage($"Set Happiness Value of {characterByName.name} to {result}");
	}

	private void SetTiredness(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SetTiredness");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		string text2 = parameters[1];
		float result = characterByName.needsComponent.tiredness;
		if (!float.TryParse(text2, out result))
		{
			AddErrorMessage("Tiredness parameter is not a float: " + text2);
			return;
		}
		characterByName.needsComponent.SetTiredness(result);
		AddSuccessMessage($"Set Tiredness Value of {characterByName.name} to {result}");
	}

	private void SetStamina(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SetStamina");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		string text2 = parameters[1];
		float result = characterByName.needsComponent.stamina;
		if (!float.TryParse(text2, out result))
		{
			AddErrorMessage("Stamina parameter is not a float: " + text2);
			return;
		}
		characterByName.needsComponent.SetStamina(result);
		AddSuccessMessage($"Set Stamina Value of {characterByName.name} to {result}");
	}

	private void SetHope(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SetHope");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		string text2 = parameters[1];
		float result = characterByName.needsComponent.hope;
		if (!float.TryParse(text2, out result))
		{
			AddErrorMessage("Hope parameter is not a float: " + text2);
			return;
		}
		characterByName.needsComponent.SetHope(result);
		AddSuccessMessage($"Set Hope Value of {characterByName.name} to {result}");
	}

	private void LogAwareness(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of LogAwareness");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
		}
		else
		{
			characterByName.LogAwarenessList();
		}
	}

	private void AddRelationship(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AddRelationship");
			return;
		}
		string text = parameters[0];
		if (!Enum.TryParse<RELATIONSHIP_TYPE>(text, out var result))
		{
			AddErrorMessage("There is no relationship of type " + text);
		}
		string text2 = parameters[1];
		string text3 = parameters[2];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text2);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character with name " + text2);
		}
		Character characterByName2 = CharacterManager.Instance.GetCharacterByName(text3);
		if (characterByName2 == null)
		{
			AddErrorMessage("There is no character with name " + text3);
		}
		RelationshipManager.Instance.CreateNewRelationshipBetween(characterByName, characterByName2, result);
		AddSuccessMessage($"{characterByName.name} and {characterByName2.name} now have relationship {result}");
	}

	private void SetHP(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ForcedRelationshipDegradation");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		int result = 0;
		if (!int.TryParse(text2, out result))
		{
			AddErrorMessage("HP value parameter is not an integer: " + text2);
			return;
		}
		characterByName.SetHP(result);
		AddSuccessMessage($"Set HP of {characterByName.name} to {result}");
	}

	private void ForceUpdateAnimation(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ForceUpdateAnimation");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character with name " + text);
		}
		else
		{
			characterByName.marker.UpdateAnimation();
		}
	}

	private void AdjustOpinion(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AdjustOpinion");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		Character characterByName2 = CharacterManager.Instance.GetCharacterByName(text2);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		if (characterByName2 == null)
		{
			AddErrorMessage("There is no character named " + text2);
			return;
		}
		string text3 = parameters[2];
		int result = 0;
		if (!int.TryParse(text3, out result))
		{
			AddErrorMessage("Opinion parameter is not an integer: " + text3);
			return;
		}
		characterByName.relationshipContainer.AdjustOpinion(characterByName, characterByName2, "Base", result);
		AddSuccessMessage($"Adjusted Opinion of {characterByName.name} towards {characterByName2.name} by {result}");
	}

	private void JoinFaction(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of JoinFaction");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		Faction factionBasedOnName = FactionManager.Instance.GetFactionBasedOnName(text2);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		if (factionBasedOnName == null)
		{
			AddErrorMessage("There is no faction named " + text2);
			return;
		}
		characterByName.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, factionBasedOnName.characters[0], "join_faction_normal");
		AddSuccessMessage(characterByName.name + " joined faction " + factionBasedOnName.name);
	}

	private void CreateFaction(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of JoinFaction");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		characterByName.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, characterByName);
		AddSuccessMessage(characterByName.name + " created faction " + characterByName.faction.name);
	}

	private void TriggerEmotion(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of TriggerEmotion");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		Character characterByName2 = CharacterManager.Instance.GetCharacterByName(text2);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		if (characterByName2 == null)
		{
			AddErrorMessage("There is no character named " + text2);
			return;
		}
		string text3 = parameters[2];
		Emotion emotion = CharacterManager.Instance.GetEmotion(text3);
		if (emotion == null)
		{
			AddErrorMessage("Emotion parameter has no data: " + text3);
			return;
		}
		CharacterManager.Instance.TriggerEmotion(emotion.emotionType, characterByName, characterByName2, REACTION_STATUS.INFORMED);
		AddSuccessMessage("Trigger " + emotion.name + " Emotion of " + characterByName.name + " towards " + characterByName2.name);
	}

	private void ChangeCharacterElementalDamage(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ChangeCharacterElementalDamage");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		ELEMENTAL_TYPE result = ELEMENTAL_TYPE.Normal;
		if (!Enum.TryParse<ELEMENTAL_TYPE>(text2, out result))
		{
			AddErrorMessage("There is no elemental damage type " + text2);
			return;
		}
		characterByName.combatComponent.SetElementalType(result);
		AddSuccessMessage("Changed " + characterByName.name + " elemental damage to " + text2);
	}

	private void ChangeCharacterHomeToNull(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ChangeCharacterHomeToNull");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		characterByName.MigrateHomeTo(null);
		AddSuccessMessage("Changed " + characterByName.name + " home to null");
	}

	private void ChangeName(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ChangeName");
			return;
		}
		string text = parameters[0];
		string firstName = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName != null)
		{
			string text2 = characterByName.name;
			characterByName.SetFirstName(firstName);
			AddSuccessMessage("Successfully set name of " + text2 + " to " + characterByName.name);
		}
		else
		{
			AddErrorMessage("Could not find character named " + text);
		}
	}

	private void LogAliveVillagers(string[] parameters)
	{
		string text = DatabaseManager.Instance.characterDatabase.aliveVillagersList.ComafyList();
		AddSuccessMessage(text);
		Debug.Log(text);
	}

	private void KillAllVillagers(string[] obj)
	{
		int num = 0;
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count; i++)
		{
			Character character = DatabaseManager.Instance.characterDatabase.aliveVillagersList[i];
			if (!character.isDead)
			{
				character.Death();
				num++;
				i--;
			}
		}
		AddSuccessMessage($"Killed {num} villagers!");
	}

	private void AdjustCoins(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AdjustCoins");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		string text2 = parameters[1];
		int result = 0;
		if (!int.TryParse(text2, out result))
		{
			AddErrorMessage("Amount parameter is not an integer: " + text2);
			return;
		}
		characterByName.moneyComponent.AdjustCoins(result);
		AddSuccessMessage($"Adjusted Coins of {characterByName.name} by {result}");
	}

	private void TalentLevelUp(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of TalentLevelUp");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		if (!Enum.TryParse<CHARACTER_TALENT>(text2, out var result))
		{
			AddErrorMessage("There is no talent " + text2);
		}
		if (characterByName.HasTalents())
		{
			characterByName.talentComponent.GetTalent(result).LevelUp(characterByName);
			AddSuccessMessage(characterByName.name + "'s " + text2 + " is leveled up!");
		}
	}

	private void Distance(string[] parameters)
	{
		string s = parameters[0];
		string s2 = parameters[1];
		string s3 = parameters[2];
		string s4 = parameters[3];
		LocationGridTile locationGridTile = InnerMapManager.Instance.currentlyShowingLocation.innerMap.map[int.Parse(s), int.Parse(s2)];
		LocationGridTile tile = InnerMapManager.Instance.currentlyShowingLocation.innerMap.map[int.Parse(s3), int.Parse(s4)];
		float distanceTo = locationGridTile.GetDistanceTo(tile);
		AddSuccessMessage($"Distance: {distanceTo}");
	}

	private void IncreaseBeliefPoints(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /bel_point");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		string text3 = parameters[2];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		RELIGION result;
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
		}
		else if (Enum.TryParse<RELIGION>(text2, out result))
		{
			if (int.TryParse(text3, out var result2))
			{
				characterByName.religionComponent.IncreaseBeliefPoints(result, result2);
				AddSuccessMessage("Increased " + characterByName.name + "'s belief in " + result.ToString() + " by " + result2 + ". New total is " + characterByName.religionComponent.GetBeliefPoints(result));
			}
			else
			{
				AddErrorMessage("Cannot convert " + text3 + " into a number");
			}
		}
		else
		{
			AddErrorMessage("Cannot convert " + text2 + " to a Religion");
		}
	}

	private void AvoidStructure(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /avoid_structure");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		int result;
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
		}
		else if (int.TryParse(text2, out result))
		{
			LocationStructure structureByID = DatabaseManager.Instance.structureDatabase.GetStructureByID(result);
			if (structureByID != null)
			{
				characterByName.movementComponent.AddStructureToAvoidAndScheduleRemoval(structureByID);
				AddSuccessMessage("Added " + structureByID.name + " to " + characterByName.name + "'s avoid structure list.");
			}
			else
			{
				AddErrorMessage("No structure found with id " + result);
			}
		}
		else
		{
			AddErrorMessage("Cannot convert " + text2 + " into a number");
		}
	}

	private void UnavoidStructure(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /avoid_structure");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		int result;
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
		}
		else if (int.TryParse(text2, out result))
		{
			LocationStructure structureByID = DatabaseManager.Instance.structureDatabase.GetStructureByID(result);
			if (structureByID != null)
			{
				characterByName.movementComponent.RemoveStructureToAvoid(structureByID);
				AddSuccessMessage("Removed " + structureByID.name + " from " + characterByName.name + "'s avoid structure list.");
			}
			else
			{
				AddErrorMessage("No structure found with id " + result);
			}
		}
		else
		{
			AddErrorMessage("Cannot convert " + text2 + " into a number");
		}
	}

	private void LogDeaths(string[] parameters)
	{
		if (parameters.Length != 0)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /log_deaths");
			return;
		}
		string text = "Found deaths: ";
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.allCharactersList.Count; i++)
		{
			Character character = DatabaseManager.Instance.characterDatabase.allCharactersList[i];
			if (character.isDead)
			{
				text = text + "\n" + character.name + " - " + character.deathLog.logText;
			}
		}
		Debug.Log(text);
		AddSuccessMessage(text);
	}

	private void AdjustResistance(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of adjust resistance");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		float result = 0f;
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		if (!float.TryParse(parameters[2], out result))
		{
			AddErrorMessage("3rd parameter should be a number");
			return;
		}
		switch (text2)
		{
		case "fire":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, result);
			break;
		case "water":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, result);
			break;
		case "wind":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, result);
			break;
		case "poison":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Poison, result);
			break;
		case "mental":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, result);
			break;
		case "physical":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, result);
			break;
		case "ice":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Ice, result);
			break;
		case "earth":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, result);
			break;
		case "electric":
			characterByName.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Electric, result);
			break;
		default:
			AddErrorMessage("There is no such element " + text2);
			return;
		}
		AddSuccessMessage($"{characterByName.name}'s {text2} Resistance added {result}");
	}

	private void AddFactionIdeology(string[] parameters)
	{
		if (parameters.Length < 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /add_ideology");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Faction factionBasedOnName = FactionManager.Instance.GetFactionBasedOnName(text);
		FACTION_IDEOLOGY result;
		if (factionBasedOnName == null)
		{
			AddErrorMessage("Could not find faction with name " + text);
		}
		else if (Enum.TryParse<FACTION_IDEOLOGY>(text2, out result))
		{
			FactionIdeology factionIdeology = FactionManager.Instance.CreateIdeology<FactionIdeology>(result);
			if (factionIdeology is Exclusive exclusive)
			{
				exclusive.SetRequirement(GENDER.MALE);
				factionBasedOnName.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive, factionBasedOnName);
			}
			factionBasedOnName.factionType.AddIdeology(factionIdeology, factionBasedOnName);
			List<Character> list = RuinarchListPool<Character>.Claim();
			list.AddRange(factionBasedOnName.characters);
			for (int i = 0; i < list.Count; i++)
			{
				Character character = list[i];
				factionBasedOnName.CheckIfCharacterStillFitsIdeology(character);
			}
			RuinarchListPool<Character>.Release(list);
		}
		else
		{
			AddErrorMessage("Could not find ideology named " + text2);
		}
	}

	private void RemoveFactionIdeology(string[] parameters)
	{
		if (parameters.Length < 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /remove_ideology");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Faction factionBasedOnName = FactionManager.Instance.GetFactionBasedOnName(text);
		FACTION_IDEOLOGY result;
		if (factionBasedOnName == null)
		{
			AddErrorMessage("Could not find faction with name " + text);
		}
		else if (Enum.TryParse<FACTION_IDEOLOGY>(text2, out result))
		{
			factionBasedOnName.factionType.RemoveIdeology(result, factionBasedOnName);
			if (result == FACTION_IDEOLOGY.Exclusive)
			{
				factionBasedOnName.factionType.AddIdeology(FACTION_IDEOLOGY.Inclusive, factionBasedOnName);
			}
		}
		else
		{
			AddErrorMessage("Could not find ideology named " + text2);
		}
	}

	private void LogAreaCharactersHistory(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of LogAreaCharactersHistory");
		}
	}

	private void SpawnPartyQuests(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SpawnPartyQuests");
			return;
		}
		string text = parameters[0];
		NPCSettlement nPCSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(text) as NPCSettlement;
		PARTY_QUEST_TYPE[] enumValues = CollectionUtilities.GetEnumValues<PARTY_QUEST_TYPE>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			switch (enumValues[i])
			{
			case PARTY_QUEST_TYPE.Exploration:
				if (!nPCSettlement.owner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Exploration))
				{
					nPCSettlement.owner.partyQuestBoard.CreateExplorationPartyQuest(null, nPCSettlement, nPCSettlement.region);
				}
				break;
			case PARTY_QUEST_TYPE.Rescue:
			{
				Character randomElement4 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList);
				List<Character> list = new List<Character>(nPCSettlement.residents);
				list.Remove(randomElement4);
				Character randomElement5 = CollectionUtilities.GetRandomElement(list);
				if (!nPCSettlement.owner.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, randomElement4) && !nPCSettlement.owner.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, randomElement4))
				{
					nPCSettlement.owner.partyQuestBoard.CreateRescuePartyQuest(randomElement5, nPCSettlement, randomElement4);
				}
				break;
			}
			case PARTY_QUEST_TYPE.Extermination:
			{
				LocationStructure randomElement2 = CollectionUtilities.GetRandomElement(nPCSettlement.region.allSpecialStructures);
				Character randomElement3 = CollectionUtilities.GetRandomElement(nPCSettlement.residents);
				if (!nPCSettlement.owner.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, randomElement2))
				{
					nPCSettlement.owner.partyQuestBoard.CreateExterminatePartyQuest(randomElement3, nPCSettlement, randomElement2);
				}
				break;
			}
			case PARTY_QUEST_TYPE.Counterattack:
				nPCSettlement.owner.partyQuestBoard.CreateCounterattackPartyQuest(null, nPCSettlement);
				break;
			case PARTY_QUEST_TYPE.Raid:
				if (!nPCSettlement.owner.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Raid, nPCSettlement))
				{
					nPCSettlement.owner.partyQuestBoard.CreateRaidPartyQuest(nPCSettlement.owner.leader as Character, null, nPCSettlement);
				}
				break;
			case PARTY_QUEST_TYPE.Morning_Patrol:
				if (!nPCSettlement.owner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Morning_Patrol))
				{
					nPCSettlement.owner.partyQuestBoard.CreateMorningPatrolPartyQuest(null, nPCSettlement);
				}
				break;
			case PARTY_QUEST_TYPE.Hunt_Beast:
				if (!nPCSettlement.owner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Hunt_Beast))
				{
					LocationStructure randomLinkedAliveBeastDen = nPCSettlement.occupiedVillageSpot.GetRandomLinkedAliveBeastDen();
					if (randomLinkedAliveBeastDen != null)
					{
						nPCSettlement.owner.partyQuestBoard.CreateHuntBeastPartyQuest(null, nPCSettlement, randomLinkedAliveBeastDen);
					}
				}
				break;
			case PARTY_QUEST_TYPE.Blood_Hunt:
				nPCSettlement.owner.partyQuestBoard.CreateBloodHuntPartyQuest(nPCSettlement, nPCSettlement.region);
				break;
			case PARTY_QUEST_TYPE.Recruit_Vampires:
				nPCSettlement.owner.partyQuestBoard.CreateRecruitVampiresPartyQuest(nPCSettlement, nPCSettlement.region);
				break;
			case PARTY_QUEST_TYPE.Bounty_Hunt:
			{
				Character randomElement = CollectionUtilities.GetRandomElement(nPCSettlement.residents);
				nPCSettlement.owner.partyQuestBoard.CreateBountyHuntPartyQuest(null, nPCSettlement, randomElement);
				break;
			}
			case PARTY_QUEST_TYPE.Claim_Hallowed_Ground:
			{
				Inner_Maps.Location_Structures.HallowedGround hallowedGround2 = nPCSettlement.region.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) as Inner_Maps.Location_Structures.HallowedGround;
				nPCSettlement.owner.partyQuestBoard.CreateClaimHallowedGroundQuest(nPCSettlement, nPCSettlement.region, hallowedGround2);
				break;
			}
			case PARTY_QUEST_TYPE.Defend_Hallowed_Ground:
			{
				Inner_Maps.Location_Structures.HallowedGround hallowedGround = nPCSettlement.region.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) as Inner_Maps.Location_Structures.HallowedGround;
				nPCSettlement.owner.partyQuestBoard.CreateDefendHallowedGroundQuest(nPCSettlement, nPCSettlement.region, hallowedGround);
				break;
			}
			}
		}
	}

	private void ToggleSubscriptionToInteraction(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SubscribeToInteraction");
			return;
		}
		string text = parameters[0];
		INTERACTION_TYPE result;
		if (text.Equals("All"))
		{
			if (typesSubscribedTo.Count > 0)
			{
				typesSubscribedTo.Clear();
				AddSuccessMessage("Unsubscribed from ALL interactions");
			}
			else
			{
				typesSubscribedTo.AddRange(CollectionUtilities.GetEnumValues<INTERACTION_TYPE>());
				AddSuccessMessage("Subscribed to ALL interactions");
			}
		}
		else if (Enum.TryParse<INTERACTION_TYPE>(text, out result))
		{
			if (typesSubscribedTo.Contains(result))
			{
				typesSubscribedTo.Remove(result);
				AddSuccessMessage($"Unsubscribed from {result} interactions");
			}
			else
			{
				typesSubscribedTo.Add(result);
				AddSuccessMessage($"Subscribed to {result} interactions");
			}
		}
		else
		{
			AddErrorMessage("There is no interaction of type " + text);
		}
	}

	private void GainSummon(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of GainSummon");
		}
		else
		{
			_ = parameters[0];
		}
	}

	private void GainArtifact(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of GainSummon");
			return;
		}
		string text = parameters[0];
		ARTIFACT_TYPE result;
		if (text.Equals("All"))
		{
			CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
		}
		else if (Enum.TryParse<ARTIFACT_TYPE>(text, out result))
		{
			AddSuccessMessage($"Gained new artifact: {result}");
		}
		else
		{
			AddErrorMessage("There is no artifact of type " + text);
		}
	}

	private void GainInterventionAbility(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of GainInterventionAbility");
			return;
		}
		string text = parameters[0];
		if (!Enum.TryParse<PLAYER_SKILL_TYPE>(text, out var _))
		{
			AddErrorMessage("There is no spell of type " + text);
		}
	}

	private void ChangeArchetype(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ChangeArchetype");
			return;
		}
		string text = parameters[0];
		if (Enum.TryParse<PLAYER_ARCHETYPE>(text, out var result))
		{
			AddSuccessMessage($"Changed Player Archetype to: {result}");
		}
		else
		{
			AddErrorMessage("There is no archetype " + text);
		}
	}

	private void AdjustMana(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AdjustMana");
			return;
		}
		string text = parameters[0];
		if (int.TryParse(text, out var result))
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustManaNoLimit(result);
			AddSuccessMessage($"Adjusted mana by {result}. New Mana is {PlayerManager.Instance.player.currenciesComponent.mana}");
		}
		else
		{
			AddErrorMessage("Could not parse value " + text + " to an integer.");
		}
	}

	private void AdjustPlaguePoints(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AdjustMana");
			return;
		}
		string text = parameters[0];
		if (int.TryParse(text, out var result))
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergyNoLimit(result);
			AddSuccessMessage($"Adjusted Chaotic Energy by {result}. New Chaotic Energy is {PlayerManager.Instance.player.currenciesComponent.chaoticEnergy}");
		}
		else
		{
			AddErrorMessage("Could not parse value " + text + " to an integer.");
		}
	}

	private void AdjustSpiritEnergy(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AdjustSpiritEnergy");
			return;
		}
		string text = parameters[0];
		if (int.TryParse(text, out var result))
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustSpiritEnergy(result);
			AddSuccessMessage("Adjusted spirit energy by " + result + ". New Spirit Energy is " + PlayerManager.Instance.player.currenciesComponent.spiritEnergy);
		}
		else
		{
			AddErrorMessage("Could not parse value " + text + " to an integer.");
		}
	}

	private void RevealAll(string[] parameters)
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			CharacterManager.Instance.allCharacters[i].isInfoUnlocked = true;
		}
		for (int j = 0; j < FactionManager.Instance.allFactions.Count; j++)
		{
			Faction faction = FactionManager.Instance.allFactions[j];
			if (faction.isMajorFaction)
			{
				faction.SetIsInfoUnlocked(p_state: true);
			}
		}
		AddSuccessMessage("Revealed all Character and Faction Info");
	}

	private void EnableDigging(string[] parameters)
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			character.movementComponent.SetEnableDigging(!character.movementComponent.enableDigging);
		}
		for (int j = 0; j < CharacterManager.Instance.limboCharacters.Count; j++)
		{
			Character character2 = CharacterManager.Instance.limboCharacters[j];
			character2.movementComponent.SetEnableDigging(!character2.movementComponent.enableDigging);
		}
		AddSuccessMessage("Enabled Digging all Characters");
	}

	private void BonusCharges(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of BonusCharges");
			return;
		}
		string text = parameters[0];
		string s = parameters[1];
		if (Enum.TryParse<PLAYER_SKILL_TYPE>(text, out var result))
		{
			PlayerSkillManager.Instance.GetSkillData(result).AdjustBonusCharges(int.Parse(s));
		}
		else
		{
			AddErrorMessage("There is no skill of type " + text);
		}
	}

	private void AddPower(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AddPower");
			return;
		}
		string text = parameters[0];
		if (Enum.TryParse<PLAYER_SKILL_TYPE>(text, out var result))
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(result);
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(result);
			if (skillData.isInUse)
			{
				skillData.AdjustMaxCharges(scriptableObjPlayerSkillData.unlockChargeOnPortalUpgrade);
				skillData.AdjustCharges(scriptableObjPlayerSkillData.unlockChargeOnPortalUpgrade);
			}
			else
			{
				PlayerManager.Instance.player.playerSkillComponent.AddAndCategorizePlayerSkill(result);
			}
			AddSuccessMessage("Gained new Spell: " + result);
		}
		else
		{
			AddErrorMessage("There is no power of type " + text);
		}
	}

	private void DestroyTileObj(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of DestroyTileObj");
			return;
		}
		string text = parameters[0];
		int num = int.Parse(parameters[1]);
		if (Enum.TryParse<TILE_OBJECT_TYPE>(text, out var result))
		{
			Region mainRegion = GridMap.Instance.mainRegion;
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			mainRegion.PopulateTileObjectsOfType(list, result);
			for (int i = 0; i < list.Count; i++)
			{
				TileObject tileObject = list[i];
				if (tileObject.id == num)
				{
					AddSuccessMessage($"Removed {tileObject} from {tileObject.gridTileLocation} at {tileObject.gridTileLocation.structure}");
					tileObject.gridTileLocation.structure.RemovePOI(tileObject);
					break;
				}
			}
			RuinarchListPool<TileObject>.Release(list);
		}
		else
		{
			AddErrorMessage("There is no tile object of type " + text);
		}
	}

	private void FindTileObject(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of FindTileObject");
			return;
		}
		string p_name = parameters[0];
		Region currentlyShowingLocation = InnerMapManager.Instance.currentlyShowingLocation;
		if (currentlyShowingLocation == null)
		{
			return;
		}
		List<LocationStructure> allStructures = currentlyShowingLocation.allStructures;
		for (int i = 0; i < allStructures.Count; i++)
		{
			TileObject firstTileObjectOfTypeWithName = allStructures[i].GetFirstTileObjectOfTypeWithName<TileObject>(p_name);
			if (firstTileObjectOfTypeWithName != null)
			{
				UIManager.Instance.ShowTileObjectInfo(firstTileObjectOfTypeWithName);
			}
		}
	}

	private void AdjustResourceInPile(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of AdjustResourceInPile");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		if (int.TryParse(text2, out var result) && int.TryParse(text, out var result2))
		{
			Region currentlyShowingLocation = InnerMapManager.Instance.currentlyShowingLocation;
			if (currentlyShowingLocation != null)
			{
				List<LocationStructure> allStructures = currentlyShowingLocation.allStructures;
				for (int i = 0; i < allStructures.Count; i++)
				{
					LocationStructure locationStructure = allStructures[i];
					for (int j = 0; j < locationStructure.pointsOfInterest.Count; j++)
					{
						if (locationStructure.pointsOfInterest.ElementAt(j) is ResourcePile resourcePile && resourcePile.id == result2)
						{
							resourcePile.AdjustResourceInPile(result);
							AddSuccessMessage($"Adjusted resource in {resourcePile} by {result.ToString()}. New resource in pile is {resourcePile.resourceInPile.ToString()}");
							return;
						}
					}
				}
			}
			AddErrorMessage("Could not find pile with id " + text);
		}
		else
		{
			AddErrorMessage("Could not convert " + text2 + " or " + text + " into an integer");
		}
	}

	private void SwitchPartyState(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SwitchPartyState");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Party partyByName = DatabaseManager.Instance.partyDatabase.GetPartyByName(text);
		if (!Enum.TryParse<PARTY_STATE>(text2, out var result))
		{
			AddErrorMessage("There is no poi of type " + text2);
		}
		else
		{
			partyByName.SetPartyState(result);
		}
	}

	private void SpawnTileObjects(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /spawn_tile_objects");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		if (!int.TryParse(text, out var result))
		{
			AddErrorMessage(text + " is not a number!");
			return;
		}
		if (!int.TryParse(text2, out var result2))
		{
			AddErrorMessage(text2 + " is not a number!");
			return;
		}
		LocationGridTile tileFromMapCoordinates = InnerMapManager.Instance.currentlyShowingMap.GetTileFromMapCoordinates(result, result2);
		if (tileFromMapCoordinates == null)
		{
			AddErrorMessage("Could not find tile at " + result + "," + result2);
			return;
		}
		TILE_OBJECT_TYPE[] enumValues = CollectionUtilities.GetEnumValues<TILE_OBJECT_TYPE>();
		LocationGridTile locationGridTile = tileFromMapCoordinates;
		int num = tileFromMapCoordinates.localPlace.y;
		List<EQUIPMENT_PREFIX> list = CollectionUtilities.GetEnumValues<EQUIPMENT_PREFIX>().ToList();
		list.Remove(EQUIPMENT_PREFIX.None);
		new List<int> { 0, 1, 2, 3, 4, 5 };
		for (int i = 0; i < enumValues.Length; i++)
		{
			TILE_OBJECT_TYPE tILE_OBJECT_TYPE = enumValues[i];
			if (tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.BALL_LIGHTNING || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.FIRE_BALL || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.VAPOR || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.LOCUST_SWARM || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.POISON_CLOUD || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.RAVENOUS_SPIRIT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.FEEBLE_SPIRIT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.FORLORN_SPIRIT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.NONE || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.TORNADO || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.THIN_WALL || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.SPIRE_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.BIOLAB_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.BRIMSTONES_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.CRYPT_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.DEFILER_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.DOOR_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.EARTHQUAKE_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.ELECTRIC_STORM_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.ICETEROIDS_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.IMP_HUT_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.MANA_PIT_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.MARAUD_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.MEDDLER_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.PRIMORDIAL_POOL_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.PRISM_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.RAIN_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.STRUCTURE_BLOCKER_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.WATCHER_TILE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.BIG_TREE_OBJECT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.TOMBSTONE || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.ARTIFACT || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.DEMON_EYE || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.QUICKSAND || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.STAMPEDE || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.WURM_HOLE || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.HALLOWED_GROUND || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.FROSTY_FOG || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.CINDER || tILE_OBJECT_TYPE == TILE_OBJECT_TYPE.ICE_BLOCK_WALL)
			{
				continue;
			}
			if (TileObjectDB.TryGetTileObjectData(tILE_OBJECT_TYPE, out var data) && (data.occupiedSize.X > 1 || data.occupiedSize.Y > 1))
			{
				Debug.Log("Unable to place " + tILE_OBJECT_TYPE.ToString() + " since it occupies more than 1 tile");
				continue;
			}
			Debug.Log("Placing " + tILE_OBJECT_TYPE);
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tILE_OBJECT_TYPE);
			locationGridTile.structure.AddPOI(tileObject, locationGridTile);
			if (tileObject is EquipmentItem equipmentItem)
			{
				if (tileObject.tileObjectType.IsStaff())
				{
					equipmentItem.AddRandomElementalElementPrefix();
				}
				else if (tileObject.tileObjectType.IsBow())
				{
					equipmentItem.AddRandomSecondaryElementPrefix();
				}
				else if (list.Count <= 0)
				{
					equipmentItem.TryAddRandomPrefix();
				}
				else
				{
					equipmentItem.ForcePrefixBonus(list[0]);
					list.RemoveAt(0);
				}
			}
			LocationGridTile neighbourAtDirection = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.East);
			if (neighbourAtDirection == null || neighbourAtDirection.tileObjectComponent.objHere != null)
			{
				num--;
				locationGridTile = InnerMapManager.Instance.currentlyShowingMap.GetTileFromMapCoordinates(tileFromMapCoordinates.localPlace.x, num);
			}
			else
			{
				locationGridTile = neighbourAtDirection;
			}
		}
	}

	private void LogObjectAdvertisements(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of LogObjectAdvertisments");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		string s = parameters[2];
		if (!Enum.TryParse<POINT_OF_INTEREST_TYPE>(text, out var result))
		{
			AddErrorMessage("There is no poi of type " + text);
		}
		int id = int.Parse(s);
		if (result != POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			return;
		}
		if (!Enum.TryParse<TILE_OBJECT_TYPE>(text2, out var result2))
		{
			AddErrorMessage("There is no tile object of type " + text2);
		}
		TileObject tileObject = InnerMapManager.Instance.GetTileObject(result2, id);
		string text3 = "Advertised actions of " + tileObject.name + ":";
		if (tileObject.advertisedActions != null && tileObject.advertisedActions.Count > 0)
		{
			for (int i = 0; i < tileObject.advertisedActions.Count; i++)
			{
				text3 += $"\n{tileObject.advertisedActions[i]}";
			}
		}
		else
		{
			text3 += "\nNone";
		}
		AddSuccessMessage(text3);
	}

	public void ResetTutorial()
	{
	}

	private void DamageTile(string[] parameters)
	{
		if (parameters.Length != 3)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of DamageTile");
			return;
		}
		Region mainRegion = GridMap.Instance.mainRegion;
		string text = parameters[1];
		if (!int.TryParse(text, out var result))
		{
			AddErrorMessage(text + " is not an integer!");
			return;
		}
		string text2 = parameters[2];
		if (!int.TryParse(text2, out var result2))
		{
			AddErrorMessage(text2 + " is not an integer!");
			return;
		}
		if (Utilities.IsInRange(result, 0, mainRegion.innerMap.width) && Utilities.IsInRange(result2, 0, mainRegion.innerMap.height))
		{
			LocationGridTile locationGridTile = mainRegion.innerMap.map[result, result2];
			locationGridTile.tileObjectComponent.genericTileObject.AdjustHP(-locationGridTile.tileObjectComponent.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
			AddSuccessMessage("Successfully damaged " + locationGridTile.localPlace.ToString() + "!");
			return;
		}
		AddErrorMessage("No tile with coordinates " + result + "," + result2 + " at " + mainRegion.name + " was found!");
	}

	private void CheckTiles(string[] parameters)
	{
		for (int i = 0; i < GridMap.Instance.mainRegion.innerMap.allTiles.Count; i++)
		{
			LocationGridTile locationGridTile = GridMap.Instance.mainRegion.innerMap.allTiles[i];
			if (locationGridTile.structure == null)
			{
				AddErrorMessage(locationGridTile.ToString() + " has no structure!");
			}
		}
	}

	private void ToggleVillageSpots(string[] obj)
	{
		if (GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.activeInHierarchy)
		{
			GridMap.Instance.mainRegion.innerMap.perlinTilemap.ClearAllTiles();
			GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.SetActive(value: false);
			return;
		}
		GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.SetActive(value: true);
		for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++)
		{
			GridMap.Instance.mainRegion.villageSpots[i].ColorCoreSpot();
		}
	}

	private void SaveScenarioMap(string[] parameters)
	{
		string fileName = string.Empty;
		if (parameters.Length != 0)
		{
			fileName = parameters[0];
		}
		SaveManager.Instance.SaveScenario(fileName);
	}

	private void SaveManual(string[] parameters)
	{
		string text = string.Empty;
		if (parameters.Length != 0)
		{
			text = parameters[0];
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "SAVED_CURRENT_PROGRESS";
		}
		SaveManager.Instance.saveCurrentProgressManager.DoManualSave(text);
	}

	private void SaveDatabaseInMemory(string[] parameters)
	{
		DatabaseManager.Instance.mainSQLDatabase.SaveInMemoryDatabaseToFile(Utilities.gameSavePath + "/Temp/gameDB.db");
	}

	private void LogStructureConnectors(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /log_structure_connectors");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		BaseSettlement settlementByName = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(text);
		NPCSettlement npcSettlement = settlementByName as NPCSettlement;
		if (npcSettlement != null)
		{
			if (Enum.TryParse<STRUCTURE_TYPE>(text2, ignoreCase: true, out var result))
			{
				List<StructureConnector> list = RuinarchListPool<StructureConnector>.Claim();
				npcSettlement.PopulateStructureConnectorsForStructureType(list, result);
				if (result == STRUCTURE_TYPE.MINE)
				{
					list = list.OrderBy((StructureConnector c) => Vector2.Distance(c.transform.position, npcSettlement.cityCenter.tiles.ElementAt(0).centeredWorldLocation)).ToList();
				}
				Debug.Log("Found structure connectors for " + result.ToString() + " at " + npcSettlement.name + " are:\n " + list.ComafyList());
				RuinarchListPool<StructureConnector>.Release(list);
				AddSuccessMessage("Logged structure connectors for " + result.ToString() + " at " + settlementByName.name + ". Check your console.");
			}
			else
			{
				AddErrorMessage("Could not parse " + text2 + " into a STRUCTURE_TYPE");
			}
		}
		else
		{
			AddErrorMessage("Could not find NPCSettlement with name " + text);
		}
	}

	private void RemoveNeededClassFromSettlement(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /remove_needed_class");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		BaseSettlement settlementByName = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(text);
		if (settlementByName is NPCSettlement)
		{
			AddSuccessMessage("Removed needed class " + text2 + " from " + settlementByName.name + "'s needed classes");
		}
		else
		{
			AddErrorMessage("Could not find NPCSettlement with name " + text);
		}
	}

	private void ActivateSettlementEvent(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /activate_settlement_event");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		if (Enum.TryParse<SETTLEMENT_EVENT>(text2, out var result))
		{
			BaseSettlement settlementByName = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(text);
			if (settlementByName is NPCSettlement nPCSettlement)
			{
				nPCSettlement.eventManager.AddNewActiveEvent(result);
				AddSuccessMessage("Activated event " + result.ToString() + " at " + settlementByName.name);
			}
			else
			{
				AddErrorMessage("Could not find NPCSettlement with name " + text);
			}
		}
		else
		{
			AddErrorMessage("No Settlement Event Type " + text2);
		}
	}

	private void TriggerQuarantine(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /trigger_quarantine");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		if (DatabaseManager.Instance.settlementDatabase.GetSettlementByName(text) is NPCSettlement nPCSettlement)
		{
			Character characterByName = CharacterManager.Instance.GetCharacterByName(text2);
			if (characterByName != null)
			{
				nPCSettlement.settlementJobTriggerComponent.TriggerQuarantineJob(characterByName);
			}
			else
			{
				AddErrorMessage("Could not find character with name " + text2);
			}
		}
		else
		{
			AddErrorMessage("Could not find NPCSettlement with name " + text);
		}
	}

	private void AdjustMigrationMeter(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /adjust_mm");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		BaseSettlement settlementByName = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(text);
		if (settlementByName is NPCSettlement nPCSettlement)
		{
			if (int.TryParse(text2, out var result))
			{
				if (result > 0)
				{
					nPCSettlement.migrationComponent.IncreaseVillageMigrationMeter(result);
				}
				else
				{
					nPCSettlement.migrationComponent.ReduceVillageMigrationMeter(result * -1);
				}
				AddSuccessMessage(settlementByName.name + " migration meter is now " + nPCSettlement.migrationComponent.GetMigrationMeterValueInText());
			}
			else
			{
				AddErrorMessage(text2 + " could not be parsed into an integer");
			}
		}
		else
		{
			AddErrorMessage("Could not find NPCSettlement with name " + text);
		}
	}

	private void OnToggleAlwaysSuccessScheme(bool p_isOn)
	{
		SchemeData.alwaysSuccessScheme = p_isOn;
	}

	private void OnToggleShowPOIHoverData(bool p_isOn)
	{
		showPOIHoverData = p_isOn;
	}

	private void OnToggleAlwaysShowNotifications(bool p_isOn)
	{
		alwaysShowNotifications = p_isOn;
	}

	private void OnToggleFasterPortalUpgrade(bool p_isOn)
	{
		fasterPortalUpgrade = p_isOn;
	}

	private void SetCombatSkill(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of SetCombatSkill");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		if (!Enum.TryParse<COMBAT_SPECIAL_SKILL>(text2, out var result))
		{
			AddErrorMessage("There is no combat kill named " + text2);
			return;
		}
		characterByName.combatComponent.specialSkillParent.SetSpecialSkill(result);
		AddSuccessMessage("Set special skill of " + characterByName.name + " to " + characterByName.combatComponent.specialSkillParent.specialSkill.name + "!");
	}

	private void ChangeCharacterClass(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ChangeCharacterClass");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
			return;
		}
		if (!CharacterManager.Instance.HasCharacterClass(text2))
		{
			AddErrorMessage("There is no class named " + text2);
			return;
		}
		characterByName.classComponent.AssignClass(text2);
		AddSuccessMessage("Set class of " + characterByName.name + " to " + characterByName.characterClass.className + "!");
	}

	private void ForceExpireBody(string[] parameters)
	{
		if (parameters.Length != 1)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of ForceExpireBody");
			return;
		}
		string text = parameters[0];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
		}
		else
		{
			characterByName.marker.TryExpire();
		}
	}

	private void ForceExpireAllBody(string[] parameters)
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (character.isDead && character.hasMarker)
			{
				character.marker.TryExpire();
			}
		}
	}

	private void Walkability(string[] parameters)
	{
		float x = float.Parse(parameters[0]);
		float y = float.Parse(parameters[1]);
		string text = parameters[2];
		NNConstraint constraint = NNConstraint.None;
		switch (text)
		{
		case "0":
			constraint = GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph;
			break;
		case "1":
			constraint = GridMap.Instance.mainRegion.innerMap.onlyPathfindingGraph;
			break;
		case "2":
			constraint = NNConstraint.Default;
			break;
		}
		GraphNode node = AstarPath.active.GetNearest(new Vector3(x, y, 0f), constraint).node;
		AddSuccessMessage($"Node is walkable? {node.Walkable}");
	}

	private void ClearBlacklist(string[] parameters)
	{
		for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++)
		{
			DatabaseManager.Instance.jobDatabase.allJobs[i].ClearBlacklist();
		}
	}

	private void LogActions(string[] parameters)
	{
		RevealAll(null);
		INTERACTION_TYPE[] enumValues = CollectionUtilities.GetEnumValues<INTERACTION_TYPE>();
		Character randomActor = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN));
		randomActor.RenameCharacter("Easton");
		PlayerManager.Instance.player.storedTargetsComponent.Store(randomActor);
		Debug.Log("[Action Logger] Will log actions on " + randomActor.name);
		foreach (INTERACTION_TYPE iNTERACTION_TYPE in enumValues)
		{
			if (!InteractionManager.Instance.goapActionData.ContainsKey(iNTERACTION_TYPE))
			{
				continue;
			}
			StateNameAndDuration[] actionStates = GoapActionStateDB.GetActionStates(iNTERACTION_TYPE);
			if (actionStates == null || iNTERACTION_TYPE == INTERACTION_TYPE.NONE || iNTERACTION_TYPE == INTERACTION_TYPE.MONSTER_INVADE)
			{
				continue;
			}
			GoapAction goapAction = InteractionManager.Instance.goapActionData[iNTERACTION_TYPE];
			IPointOfInterest targetForAction = GetTargetForAction(iNTERACTION_TYPE, randomActor);
			ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(goapAction, randomActor, targetForAction, null, 0);
			OtherData[] array = CreateOtherDataForAction(iNTERACTION_TYPE, randomActor, targetForAction);
			if (array != null)
			{
				actualGoapNode.SetOtherData(array);
			}
			actualGoapNode.SetTargetStructure(goapAction.GetTargetStructure(actualGoapNode));
			DoActionsBeforeLogging(iNTERACTION_TYPE, randomActor, targetForAction);
			actualGoapNode.SetCrimeType();
			for (int num2 = 0; num2 < actionStates.Length; num2++)
			{
				StateNameAndDuration stateNameAndDuration = actionStates[num2];
				actualGoapNode.SetAsIllusionForTesting(stateNameAndDuration.name);
				if (iNTERACTION_TYPE == INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE || iNTERACTION_TYPE == INTERACTION_TYPE.TAKE_RESOURCE || iNTERACTION_TYPE == INTERACTION_TYPE.CRY || iNTERACTION_TYPE == INTERACTION_TYPE.ABSORB_WISP || iNTERACTION_TYPE == INTERACTION_TYPE.ENHANCE_RELATIONSHIP)
				{
					actualGoapNode.currentState.preEffect?.Invoke(actualGoapNode);
				}
				AddSpecificFillers(iNTERACTION_TYPE, actualGoapNode.thoughtBubbleLog);
				AddSpecificFillers(iNTERACTION_TYPE, actualGoapNode.thoughtBubbleMovingLog);
				AddSpecificFillers(iNTERACTION_TYPE, actualGoapNode.descriptionLog);
				if (actualGoapNode.thoughtBubbleLog != null && actualGoapNode.thoughtBubbleLog.unreplacedText != "hidden" && actualGoapNode.thoughtBubbleLog.unreplacedText != "ẩn nấp" && actualGoapNode.thoughtBubbleLog.unreplacedText != "скрыто" && actualGoapNode.thoughtBubbleLog.unreplacedText != "приховано" && actualGoapNode.thoughtBubbleLog.unreplacedText != "ซ\u0e48อน")
				{
					actualGoapNode.LogAction(actualGoapNode.thoughtBubbleLog);
				}
				if (actualGoapNode.thoughtBubbleMovingLog != null && actualGoapNode.thoughtBubbleMovingLog.unreplacedText != "hidden" && actualGoapNode.thoughtBubbleMovingLog.unreplacedText != "ẩn nấp" && actualGoapNode.thoughtBubbleMovingLog.unreplacedText != "скрыто" && actualGoapNode.thoughtBubbleMovingLog.unreplacedText != "приховано" && actualGoapNode.thoughtBubbleMovingLog.unreplacedText != "ซ\u0e48อน")
				{
					actualGoapNode.LogAction(actualGoapNode.thoughtBubbleMovingLog);
				}
				if (actualGoapNode.descriptionLog != null && actualGoapNode.descriptionLog.unreplacedText != "hidden" && actualGoapNode.descriptionLog.unreplacedText != "ẩn nấp" && actualGoapNode.descriptionLog.unreplacedText != "скрыто" && actualGoapNode.descriptionLog.unreplacedText != "приховано" && actualGoapNode.descriptionLog.unreplacedText != "ซ\u0e48อน")
				{
					actualGoapNode.LogAction(actualGoapNode.descriptionLog);
				}
			}
			DoActionsAfterLogging(iNTERACTION_TYPE, randomActor, targetForAction);
		}
		Character actionNameActor = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != randomActor));
		actionNameActor.RenameCharacter("Dudley");
		PlayerManager.Instance.player.storedTargetsComponent.Store(actionNameActor);
		Debug.Log("[Action Logger] Will log action names on " + actionNameActor.name);
		foreach (INTERACTION_TYPE iNTERACTION_TYPE2 in enumValues)
		{
			if (InteractionManager.Instance.goapActionData.ContainsKey(iNTERACTION_TYPE2) && GoapActionStateDB.GetActionStates(iNTERACTION_TYPE2) != null && iNTERACTION_TYPE2 != INTERACTION_TYPE.NONE)
			{
				GoapAction goapAction2 = InteractionManager.Instance.goapActionData[iNTERACTION_TYPE2];
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "current_action_abandoned_reason", LOG_TAG.Social);
				log.AddToFillers(actionNameActor, actionNameActor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				string localizedName = goapAction2.localizedName;
				log.AddToFillers(null, localizedName, LOG_IDENTIFIER.STRING_1);
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CancelReasons_Table", GameUtilities.RollChance(50) ? "Important_To_Do" : "Scared_Of_Something");
				log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_2);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
		}
		Character randomElement = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != randomActor && x != actionNameActor));
		randomElement.RenameCharacter("Roxanne");
		PlayerManager.Instance.player.storedTargetsComponent.Store(randomElement);
		Debug.Log("[Action Logger] Will log job names on " + randomElement.name);
		JOB_TYPE[] enumValues2 = CollectionUtilities.GetEnumValues<JOB_TYPE>();
		for (int num4 = 0; num4 < enumValues2.Length; num4++)
		{
			JOB_TYPE jOB_TYPE = enumValues2[num4];
			string value;
			switch (jOB_TYPE)
			{
			case JOB_TYPE.REMOVE_STATUS:
			{
				Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "GoapActionsStrings_Table", "Remove_Status");
				string localizedNameOfTrait = TraitManager.Instance.GetLocalizedNameOfTrait("Unconscious");
				log3.AddToFillers(null, localizedNameOfTrait, LOG_IDENTIFIER.STRING_1);
				value = log3.logText;
				break;
			}
			case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "GoapActionsStrings_Table", "Obtain_Personal_Item");
				string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "Healing Potion");
				log2.AddToFillers(null, localizedValue2, LOG_IDENTIFIER.STRING_1);
				value = log2.logText;
				break;
			}
			default:
				value = LocalizationManager.Instance.GetLocalizedValue("Jobs_Table", jOB_TYPE.ToString());
				break;
			}
			if (jOB_TYPE != JOB_TYPE.NONE)
			{
				if (string.IsNullOrEmpty(value))
				{
					Debug.LogWarning("[Action Logger] No localized value for job " + jOB_TYPE);
					continue;
				}
				Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "current_action_abandoned_reason", LOG_TAG.Social);
				log4.AddToFillers(randomElement, randomElement.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log4.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
				string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("CancelReasons_Table", GameUtilities.RollChance(50) ? "Important_To_Do" : "Scared_Of_Something");
				log4.AddToFillers(null, localizedValue3, LOG_IDENTIFIER.STRING_2);
				log4.AddLogToDatabase(releaseLogAfter: true);
			}
		}
	}

	private IPointOfInterest GetTargetForAction(INTERACTION_TYPE p_type, Character p_actor)
	{
		switch (p_type)
		{
		case INTERACTION_TYPE.MAKE_LOVE:
			return p_actor.relationshipContainer.charactersWithOpinion.First();
		case INTERACTION_TYPE.OPEN:
		{
			TreasureChest tileObjectOfType = InnerMapManager.Instance.currentlyShowingLocation.wilderness.GetTileObjectOfType<TreasureChest>();
			if (tileObjectOfType != null)
			{
				tileObjectOfType.SetObjectInside(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION));
				return tileObjectOfType;
			}
			break;
		}
		case INTERACTION_TYPE.ASSAULT:
			return CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList);
		case INTERACTION_TYPE.REPAIR:
			return InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER).GetTileObjectOfType<WaterWell>();
		}
		for (int i = 0; i < DatabaseManager.Instance.structureDatabase.allStructures.Count; i++)
		{
			List<IPointOfInterest> listOfPOIBasedOnActionType = DatabaseManager.Instance.structureDatabase.allStructures[i].locationAwareness.GetListOfPOIBasedOnActionType(p_type);
			if (listOfPOIBasedOnActionType != null && listOfPOIBasedOnActionType.Count > 0)
			{
				return listOfPOIBasedOnActionType[0];
			}
		}
		for (int j = 0; j < InnerMapManager.Instance.currentlyShowingLocation.areas.Count; j++)
		{
			List<IPointOfInterest> listOfPOIBasedOnActionType2 = InnerMapManager.Instance.currentlyShowingLocation.areas[j].locationAwareness.GetListOfPOIBasedOnActionType(p_type);
			if (listOfPOIBasedOnActionType2 != null && listOfPOIBasedOnActionType2.Count > 0)
			{
				return listOfPOIBasedOnActionType2[0];
			}
		}
		for (int k = 0; k < DatabaseManager.Instance.characterDatabase.allCharactersList.Count; k++)
		{
			Character character = DatabaseManager.Instance.characterDatabase.allCharactersList[k];
			if (character.advertisedActions.Contains(p_type))
			{
				return character;
			}
		}
		switch (p_type)
		{
		case INTERACTION_TYPE.PICK_UP:
		case INTERACTION_TYPE.CRAFT_EQUIPMENT:
		case INTERACTION_TYPE.BUY_ITEM:
		{
			TileObject tileObject3 = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.IRON_SWORD);
			InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER).AddPOI(tileObject3);
			return tileObject3;
		}
		case INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE:
		case INTERACTION_TYPE.TAKE_RESOURCE:
		case INTERACTION_TYPE.BUY_STONE:
		case INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE:
		{
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.STONE_PILE);
			InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER).AddPOI(tileObject);
			return tileObject;
		}
		case INTERACTION_TYPE.BUY_WOOD:
		{
			TileObject tileObject2 = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WOOD_PILE);
			InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER).AddPOI(tileObject2);
			return tileObject2;
		}
		case INTERACTION_TYPE.EAT:
		case INTERACTION_TYPE.POISON:
		case INTERACTION_TYPE.DROP_RESOURCE:
		case INTERACTION_TYPE.CRAFT_TILE_OBJECT:
		case INTERACTION_TYPE.CRAFT_FURNITURE_WOOD:
		case INTERACTION_TYPE.CRAFT_FURNITURE_STONE:
			return p_actor.homeStructure.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.TABLE);
		case INTERACTION_TYPE.HARVEST_PLANT:
			return InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MUSHROOM);
		case INTERACTION_TYPE.REMEMBER_FALLEN:
		{
			Tombstone tombstone2 = InnerMapManager.Instance.CreateNewTileObject<Tombstone>(TILE_OBJECT_TYPE.TOMBSTONE);
			Character character3 = CharacterManager.Instance.CreateNewCharacter("Logger", GameUtilities.RollChance(50) ? RACE.HUMANS : RACE.ELVES, (!GameUtilities.RollChance(50)) ? GENDER.FEMALE : GENDER.MALE, homeRegion: InnerMapManager.Instance.currentlyShowingLocation, faction: FactionManager.Instance.vagrantFaction, homeLocation: null, homeStructure: null, randomizeTraits: false);
			character3.CreateMarker();
			character3.InitialCharacterPlacement(InnerMapManager.Instance.currentlyShowingMap.allTiles[0]);
			character3.marker.UpdatePosition();
			character3.Death();
			tombstone2.SetCharacter(character3);
			return tombstone2;
		}
		case INTERACTION_TYPE.REPAIR_STRUCTURE:
		case INTERACTION_TYPE.DESTROY_HOME:
			return (InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.DWELLING) as ManMadeStructure).structureTileObject;
		case INTERACTION_TYPE.SHEAR_ANIMAL:
		case INTERACTION_TYPE.SKIN_ANIMAL:
			return CharacterManager.Instance.allCharacters.First((Character x) => x is Summon);
		case INTERACTION_TYPE.SPIT:
		{
			Tombstone tombstone = InnerMapManager.Instance.CreateNewTileObject<Tombstone>(TILE_OBJECT_TYPE.TOMBSTONE);
			Character character2 = CharacterManager.Instance.CreateNewCharacter("Logger", GameUtilities.RollChance(50) ? RACE.HUMANS : RACE.ELVES, (!GameUtilities.RollChance(50)) ? GENDER.FEMALE : GENDER.MALE, homeRegion: InnerMapManager.Instance.currentlyShowingLocation, faction: FactionManager.Instance.vagrantFaction, homeLocation: null, homeStructure: null, randomizeTraits: false);
			character2.CreateMarker();
			character2.InitialCharacterPlacement(InnerMapManager.Instance.currentlyShowingMap.allTiles[0]);
			character2.marker.UpdatePosition();
			character2.Death();
			tombstone.SetCharacter(character2);
			return tombstone;
		}
		case INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE:
			return PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL).groupedTileObjects[TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT][0];
		case INTERACTION_TYPE.READ_STRUCTURE_SCROLL:
		{
			StructureScroll structureScroll = InnerMapManager.Instance.CreateNewTileObject<StructureScroll>(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
			structureScroll.SetStructureTypeToLearn(STRUCTURE_TYPE.ARROW_TOWER);
			return structureScroll;
		}
		default:
			return CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList);
		}
	}

	private OtherData[] CreateOtherDataForAction(INTERACTION_TYPE p_type, Character p_actor, IPointOfInterest p_target)
	{
		OtherData[] result = null;
		switch (p_type)
		{
		case INTERACTION_TYPE.DROP_ITEM:
		case INTERACTION_TYPE.STEALTH_TRANSFORM:
			result = new OtherData[1]
			{
				new LocationStructureOtherData(InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.DWELLING))
			};
			break;
		case INTERACTION_TYPE.CRAFT_LEGENDARY_EQUIPMENT:
			result = new OtherData[1]
			{
				new StringOtherData(TILE_OBJECT_TYPE.MANTRA.ToString())
			};
			break;
		case INTERACTION_TYPE.PLACE_BLUEPRINT:
			result = new OtherData[3]
			{
				new StringOtherData("Dwelling 1"),
				new LocationGridTileOtherData(InnerMapManager.Instance.currentlyShowingMap.allTiles[0]),
				new StructureSettingOtherData(new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE))
			};
			break;
		case INTERACTION_TYPE.WELL_JUMP:
		case INTERACTION_TYPE.STRANGLE:
			result = new OtherData[1]
			{
				new StringOtherData("Suicide_Reason_Mental_Break")
			};
			break;
		case INTERACTION_TYPE.BROODMOTHER_ORDER_ATTACK:
			result = new OtherData[1]
			{
				new SettlementOtherData(DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[0])
			};
			break;
		case INTERACTION_TYPE.CHANGE_CLASS:
			result = new OtherData[1]
			{
				new StringOtherData("Logger")
			};
			break;
		case INTERACTION_TYPE.DISPEL:
			result = new OtherData[1]
			{
				new StringOtherData("Lycanthropy")
			};
			break;
		case INTERACTION_TYPE.GIVE_TRAIT:
			result = new OtherData[1]
			{
				new StringOtherData("Lycanthrope")
			};
			break;
		case INTERACTION_TYPE.IS_IMPRISONED:
		case INTERACTION_TYPE.IS_CAPTIVE:
			result = new OtherData[1]
			{
				new LocationStructureOtherData(InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.DWELLING))
			};
			break;
		case INTERACTION_TYPE.JOIN_GATHERING:
			result = new OtherData[1]
			{
				new StringOtherData(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Social"))
			};
			break;
		case INTERACTION_TYPE.REMOVE_BUFF:
			result = new OtherData[1]
			{
				new StringOtherData("Heavy")
			};
			break;
		case INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE:
			result = new OtherData[2]
			{
				new LocationStructureOtherData(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)),
				new LocationStructureOtherData(p_actor.homeSettlement.mainStorage)
			};
			break;
		case INTERACTION_TYPE.TAKE_SHELTER:
			result = new OtherData[2]
			{
				new LocationStructureOtherData(InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.DWELLING)),
				new StringOtherData("Overheating")
			};
			break;
		case INTERACTION_TYPE.VISIT:
			result = new OtherData[1]
			{
				new LocationStructureOtherData(InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.DWELLING))
			};
			break;
		case INTERACTION_TYPE.DROP_RESOURCE:
			result = new OtherData[1]
			{
				new TileObjectOtherData(InnerMapManager.Instance.currentlyShowingLocation.wilderness.GetRandomTileObjectOfTypeThatHasTileLocation<ResourcePile>())
			};
			break;
		case INTERACTION_TYPE.REPORT_MURDER:
		{
			Character randomActor4 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target));
			Character randomElement4 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target && x != randomActor4));
			result = new OtherData[1]
			{
				new ActualGoapNodeOtherData(ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MURDER], randomActor4, randomElement4, null, 0))
			};
			break;
		}
		case INTERACTION_TYPE.REPORT_ABDUCT:
		{
			Character randomActor3 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target));
			Character randomElement3 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target && x != randomActor3));
			result = new OtherData[1]
			{
				new ActualGoapNodeOtherData(ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RESTRAIN_CHARACTER], randomActor3, randomElement3, null, 0))
			};
			break;
		}
		case INTERACTION_TYPE.REPORT_CRIME:
		{
			Character randomActor2 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target));
			Character randomElement2 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target && x != randomActor2));
			result = new OtherData[2]
			{
				new ActualGoapNodeOtherData(ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MURDER], randomActor2, randomElement2, null, 0)),
				null
			};
			break;
		}
		case INTERACTION_TYPE.SHARE_INFORMATION:
		{
			Character randomActor = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target));
			Character randomElement = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character x) => x.race != RACE.RATMAN && x != p_actor && x != p_target && x != randomActor));
			ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MURDER], randomActor, randomElement, null, 0);
			actualGoapNode.SetAsIllusion();
			result = new OtherData[1]
			{
				new ActualGoapNodeOtherData(actualGoapNode)
			};
			break;
		}
		}
		return result;
	}

	private void DoActionsBeforeLogging(INTERACTION_TYPE p_type, Character actor, IPointOfInterest target)
	{
		switch (p_type)
		{
		case INTERACTION_TYPE.BUILD_MONSTER_SPAWNER_STRUCTURE:
			actor.behaviourComponent.SetWildernessMonsterSpawnerStructureType(STRUCTURE_TYPE.MAGE_TOWER);
			break;
		case INTERACTION_TYPE.EXTRACT_ITEM:
			target.traitContainer.AddTrait(target, "Wet");
			break;
		}
	}

	private void DoActionsAfterLogging(INTERACTION_TYPE p_type, Character actor, IPointOfInterest target)
	{
		switch (p_type)
		{
		case INTERACTION_TYPE.BUILD_MONSTER_SPAWNER_STRUCTURE:
			actor.behaviourComponent.SetWildernessMonsterSpawnerStructureType(STRUCTURE_TYPE.NONE);
			break;
		case INTERACTION_TYPE.EXTRACT_ITEM:
			target.traitContainer.RemoveTrait(target, "Wet");
			break;
		}
	}

	private void AddSpecificFillers(INTERACTION_TYPE p_type, Log log)
	{
		if (log != null)
		{
			switch (p_type)
			{
			case INTERACTION_TYPE.HARVEST_PLANT:
				log.AddToFillers(null, "30", LOG_IDENTIFIER.STRING_1);
				break;
			case INTERACTION_TYPE.BUILD_BLUEPRINT:
				log.AddToFillers(null, STRUCTURE_TYPE.DWELLING.LocalizedStructureName(), LOG_IDENTIFIER.STRING_1);
				break;
			case INTERACTION_TYPE.BUY_FOOD:
				log.AddToFillers(null, 10.ToString(), LOG_IDENTIFIER.STRING_1);
				break;
			case INTERACTION_TYPE.DROP_RESOURCE:
			case INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE:
				log.AddToFillers(null, "10", LOG_IDENTIFIER.STRING_1);
				log.AddToFillers(null, RESOURCE.STONE.LocalizedName(), LOG_IDENTIFIER.STRING_2);
				break;
			case INTERACTION_TYPE.STEAL_TRAIT:
				log.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait("Heavy"), LOG_IDENTIFIER.STRING_1);
				break;
			case INTERACTION_TYPE.RELEASE_CHARACTER:
				log.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait("Unconscious"), LOG_IDENTIFIER.STRING_1);
				break;
			case INTERACTION_TYPE.ASSAULT:
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", "Abduct");
				log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
				break;
			}
			}
		}
	}

	private void ToggleInterruptTriggerBehaviour(string[] parameters)
	{
		Character character = null;
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count; i++)
		{
			Character character2 = DatabaseManager.Instance.characterDatabase.aliveVillagersList[i];
			if (character2.CanBeStoredAsTarget() && !character2.traitContainer.IsBlessed() && !character2.traitContainer.HasTrait("Heavy"))
			{
				character = character2;
				break;
			}
		}
		if (character == null)
		{
			AddErrorMessage("No valid character found.");
			return;
		}
		RevealAll(null);
		if (!character.behaviourComponent.HasBehaviour(typeof(InterruptTriggerBehaviour)))
		{
			character.behaviourComponent.AddBehaviourComponent(typeof(InterruptTriggerBehaviour));
			character.RenameCharacter("Dudley");
			PlayerManager.Instance.player.storedTargetsComponent.Store(character);
			AddSuccessMessage("[Action logger] Added interrupt trigger to " + character.name);
		}
	}

	private void FulfillAchievement(string[] parameters)
	{
		string p_achievementID = parameters[0];
		AchievementManager.Instance.FulfillAchievement(p_achievementID);
	}

	private void ResetAchievement(string[] parameters)
	{
		string p_achievementID = parameters[0];
		AchievementManager.Instance.ResetAchievement(p_achievementID);
	}

	private void ResetAllAchievement(string[] parameters)
	{
		AddSuccessMessage("All Achievements reset successful!");
		AchievementManager.Instance.ResetAllStatsAndAchievements();
	}

	private void AdjustAchievementStat(string[] parameters)
	{
		string value = parameters[0];
		int p_amount = int.Parse(parameters[1]);
		AchievementManager.Instance.AdjustAchievementStatProgress((ACHIEVEMENT_STAT)Enum.Parse(typeof(ACHIEVEMENT_STAT), value), p_amount);
	}

	private void LogPartyNames(string[] parameters)
	{
		Character character = null;
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count; i++)
		{
			Character character2 = DatabaseManager.Instance.characterDatabase.aliveVillagersList[i];
			if (character2.CanBeStoredAsTarget())
			{
				character = character2;
				break;
			}
		}
		if (character == null)
		{
			AddErrorMessage("No valid character found.");
			return;
		}
		RevealAll(null);
		character.RenameCharacter("Dudley");
		PlayerManager.Instance.player.storedTargetsComponent.Store(character);
		for (int j = 0; j < PartyManager.Instance.partyNameLayouts.layouts.Count; j++)
		{
			string text = PartyManager.Instance.partyNameLayouts.layouts[j];
			for (int k = 0; k < 10; k++)
			{
				string text2 = PartyManager.Instance.PartyNameLayoutReplacer(text, isPlayerParty: false, character);
				if (LocalizationSettings.SelectedLocale.Identifier.Code == "ja" || LocalizationSettings.SelectedLocale.Identifier.Code == "zh")
				{
					PartyManager.ReplaceWhitespace(text2, "");
				}
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Create Party effect", LOG_TAG.Party);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, text2, LOG_IDENTIFIER.PARTY_1);
				log.AddLogToDatabase();
				Debug.Log("[Action Logger] " + log.persistentID + " - " + text + " - " + text2 + "\n" + log.logText);
			}
		}
	}

	private void SpawnAllGameAlerts(string[] parameters)
	{
		Game_Alert[] enumValues = CollectionUtilities.GetEnumValues<Game_Alert>();
		foreach (Game_Alert game_Alert in enumValues)
		{
			if (game_Alert != Game_Alert.Upgrade_Portal)
			{
				GameAlert gameAlert = TutorialManager.Instance.CreateGameAlert<GameAlert>(game_Alert);
				if (gameAlert is DevastationRitualAlert devastationRitualAlert)
				{
					Character randomElement = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList);
					devastationRitualAlert.SetDevastationRitualActor(randomElement);
				}
				else if (gameAlert is FactionAwareAlert factionAwareAlert)
				{
					Faction randomMajorNonPlayerFaction = DatabaseManager.Instance.factionDatabase.GetRandomMajorNonPlayerFaction();
					factionAwareAlert.SetFaction(randomMajorNonPlayerFaction);
				}
				gameAlert.SetAsActive();
			}
		}
	}

	private void CompleteAllSubGoals(string[] parameters)
	{
		for (int i = 0; i < PlayerManager.Instance.player.goalComponent.subGoals.Length; i++)
		{
			SubGoal p_subGoal = PlayerManager.Instance.player.goalComponent.subGoals[i];
			PlayerManager.Instance.player.goalComponent.CompleteSubGoal(p_subGoal);
		}
	}

	private void CompleteAllGoalTasks(string[] parameters)
	{
		if (PlayerManager.Instance.player.goalComponent.activeGoals == null)
		{
			return;
		}
		for (int i = 0; i < PlayerManager.Instance.player.goalComponent.activeGoals.Length; i++)
		{
			Goal goal = PlayerManager.Instance.player.goalComponent.activeGoals[i];
			for (int j = 0; j < goal.tasks.Length; j++)
			{
				goal.tasks[j].ForceCompleteTask();
			}
		}
	}

	private void SetHomeStructure(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /set_home");
			return;
		}
		string text = parameters[0];
		string text2 = parameters[1];
		Character characterByName = CharacterManager.Instance.GetCharacterByName(text);
		int result;
		if (characterByName == null)
		{
			AddErrorMessage("There is no character named " + text);
		}
		else if (int.TryParse(text2, out result))
		{
			LocationStructure structureByID = DatabaseManager.Instance.structureDatabase.GetStructureByID(result);
			if (structureByID != null)
			{
				characterByName.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, structureByID.tiles.First().tileObjectComponent.genericTileObject);
				AddSuccessMessage(characterByName.name + " set his home to " + structureByID.name);
			}
			else
			{
				AddErrorMessage("No structure found with id " + result);
			}
		}
		else
		{
			AddErrorMessage("Cannot convert " + text2 + " into a number");
		}
	}

	private void GoTo(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /go_to");
			return;
		}
		int num = int.Parse(parameters[0]);
		int num2 = int.Parse(parameters[1]);
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (character.hasMarker)
			{
				character.marker.GoTo(GridMap.Instance.mainRegion.innerMap.map[num, num2]);
			}
		}
	}

	private void IncreaseDemonicEyesForTesting(string[] parameters)
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++)
		{
			if (PlayerManager.Instance.player.playerSettlement.allStructures[i] is Watcher watcher)
			{
				watcher.OverrideEyeRadius(25);
			}
		}
		AddSuccessMessage("Set max eye radius of all existing eyes to 25");
	}

	private void ScanPathfinding(string[] parameters)
	{
		AstarPath.active.Scan();
	}

	private void DoCollisionCheck(string[] parameters)
	{
		if (parameters.Length != 2)
		{
			AddCommandHistory(consoleLbl.text);
			AddErrorMessage("There was an error in the command format of /pf_cc");
			return;
		}
		string s = parameters[0];
		string s2 = parameters[1];
		if (int.TryParse(s, out var result) && int.TryParse(s2, out var result2))
		{
			InnerMapManager.Instance.currentlyShowingMap.GetTileFromMapCoordinates(result, result2);
		}
	}
}
