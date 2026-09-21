using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Ruinarch;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TileObjectInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Basic Info")]
	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[Space(10f)]
	[Header("Info")]
	[SerializeField]
	private TextMeshProUGUI hpLbl;

	[SerializeField]
	private TextMeshProUGUI quantityLbl;

	[SerializeField]
	private TextMeshProUGUI generalDescription;

	[SerializeField]
	private TextMeshProUGUI ownerLbl;

	[SerializeField]
	private EventLabel ownerEventLbl;

	[SerializeField]
	private TextMeshProUGUI carriedByLbl;

	[SerializeField]
	private EventLabel carriedByEventLbl;

	[SerializeField]
	private TextMeshProUGUI statusTraitsLbl;

	[SerializeField]
	private GameObject equipBonusGO;

	[SerializeField]
	private TextMeshProUGUI equipBonusLbl;

	[SerializeField]
	private TextMeshProUGUI normalTraitsLbl;

	[SerializeField]
	private EventLabel statusTraitsEventLbl;

	[SerializeField]
	private EventLabel normalTraitsEventLbl;

	[SerializeField]
	private TileObjectPortrait tileObjectPortrait;

	[SerializeField]
	private Toggle infoTgl;

	[Space(10f)]
	[Header("Characters")]
	[SerializeField]
	private Toggle charactersToggle;

	[SerializeField]
	private TextMeshProUGUI charactersToggleLbl;

	[SerializeField]
	private GameObject characterItemPrefab;

	[SerializeField]
	private ScrollRect charactersScrollView;

	[SerializeField]
	private UIHoverPosition characterNameplateHoverPosition;

	[Space(10f)]
	[Header("Item Effects/Monster Spawner")]
	[SerializeField]
	private GameObject specialTexts;

	[SerializeField]
	private TextMeshProUGUI itemEffectsLbl;

	[SerializeField]
	private GameObject itemEffectsGO;

	[SerializeField]
	private GameObject monsterSpawnerEffectsGO;

	[SerializeField]
	private TextMeshProUGUI monsterSpawnerLbl;

	[Space(10f)]
	[Header("Logs")]
	[SerializeField]
	private LogsWindow logsWindow;

	[SerializeField]
	private Toggle logsTgl;

	[Space(10f)]
	[Header("Store Target")]
	[SerializeField]
	private StoreTargetButton btnStoreTarget;

	[Space(10f)]
	[Header("Message Board")]
	[SerializeField]
	private Toggle questsTgl;

	[SerializeField]
	private Toggle wantedTgl;

	[SerializeField]
	private GameObject settlementQuestItemPrefab;

	[SerializeField]
	private GameObject noQuestsAvailableGO;

	[SerializeField]
	private ScrollRect questsScrollRect;

	[SerializeField]
	private TextMeshProUGUI wantedCrimesLbl;

	[SerializeField]
	private CharacterPortrait wantedCriminalPortrait;

	[SerializeField]
	private TextMeshProUGUI wantedCriminalNameLbl;

	[SerializeField]
	private GameObject noWantedCriminalsGO;

	[SerializeField]
	private Button showNextCriminalBtn;

	[SerializeField]
	private Button showPreviousCriminalBtn;

	[SerializeField]
	private Button removeCrimesBtn;

	private List<PartyQuestItem> _partyQuestItems;

	private Character _currentlySelectedCriminal;

	public TileObject activeTileObject { get; private set; }

	internal override void Initialize()
	{
		base.Initialize();
		Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateLogsFromSignal);
		Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateLogsFromSignal);
		Messenger.AddListener<Character>(UISignals.LOG_MENTIONING_CHARACTER_UPDATED, OnLogMentioningCharacterUpdated);
		Messenger.AddListener<TileObject, Character>(TileObjectSignals.ADD_TILE_OBJECT_USER, UpdateUsersFromSignal);
		Messenger.AddListener<TileObject, Character>(TileObjectSignals.REMOVE_TILE_OBJECT_USER, UpdateUsersFromSignal);
		Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_ADDED, UpdateTraitsFromSignal);
		Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_REMOVED, UpdateTraitsFromSignal);
		Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_STACKED, UpdateTraitsFromSignal);
		Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
		Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
		Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
		Messenger.AddListener<PartyQuest, Faction>(FactionSignals.PARTY_QUEST_ADDED, OnPartyQuestAdded);
		Messenger.AddListener<PartyQuest, Faction>(FactionSignals.PARTY_QUEST_REMOVED, OnPartyQuestRemoved);
		Messenger.AddListener<Character, Faction>(FactionSignals.NO_LONGER_WANTED_CRIMINAL_OF_FACTION, OnNoLongerWantedCriminalOfFaction);
		ListenToPlayerActionSignals();
		ownerEventLbl.SetOnLeftClickAction(OnLeftClickOwner);
		ownerEventLbl.SetOnRightClickAction(OnRightClickOwner);
		carriedByEventLbl.SetOnLeftClickAction(OnLeftClickLocation);
		carriedByEventLbl.SetOnRightClickAction(OnRightClickLocation);
		tileObjectPortrait.SetRightClickAction(OnRightClickPortrait);
		logsWindow.Initialize();
		_partyQuestItems = new List<PartyQuestItem>();
		showPreviousCriminalBtn.onClick.RemoveAllListeners();
		showNextCriminalBtn.onClick.RemoveAllListeners();
		removeCrimesBtn.onClick.RemoveAllListeners();
		showPreviousCriminalBtn.onClick.AddListener(OnClickShowPreviousCriminal);
		showNextCriminalBtn.onClick.AddListener(OnClickShowNextCriminal);
		removeCrimesBtn.onClick.AddListener(OnClickRemoveCrimes);
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		Selector.Instance.Deselect();
		if (activeTileObject != null)
		{
			if (activeTileObject.mapVisual != null)
			{
				activeTileObject.mapVisual.UpdateSortingOrders(activeTileObject);
				if (InnerMapCameraMove.Instance.target == activeTileObject.mapObjectVisual.transform)
				{
					InnerMapCameraMove.Instance.CenterCameraOn(null);
				}
			}
			if (activeTileObject is DemonEye demonEye)
			{
				demonEye.HideEyeWardHighlight();
			}
			activeTileObject.ProcessOnSetAsInactiveInTileObjectInfo();
		}
		activeTileObject = null;
		_currentlySelectedCriminal = null;
		btnStoreTarget.SetTarget(null);
	}

	public override void OpenMenu()
	{
		TileObject tileObject = activeTileObject;
		_currentlySelectedCriminal = null;
		activeTileObject = _data as TileObject;
		if (tileObject != null)
		{
			if (tileObject.mapVisual != null)
			{
				tileObject.mapVisual.UpdateSortingOrders(tileObject);
			}
			if (tileObject is DemonEye demonEye)
			{
				demonEye.HideEyeWardHighlight();
			}
			tileObject.ProcessOnSetAsInactiveInTileObjectInfo();
		}
		activeTileObject?.CenterOnTileObject();
		base.OpenMenu();
		if (activeTileObject.mapObjectVisual != null)
		{
			Selector.Instance.Select(activeTileObject, activeTileObject.mapObjectVisual.transform);
			activeTileObject.mapVisual.UpdateSortingOrders(activeTileObject);
		}
		if (activeTileObject is DemonEye demonEye2)
		{
			demonEye2.ShowEyeWardHighlight();
		}
		activeTileObject.ProcessOnSetAsActiveInTileObjectInfo();
		btnStoreTarget.SetTarget(activeTileObject);
		UIManager.Instance.HideObjectPicker();
		UpdateTabsBasedOnActiveObject();
		UpdateBasicInfo();
		UpdateInfo();
		UpdateTraits();
		UpdateUsers();
		logsWindow.OnParentMenuOpened(activeTileObject.persistentID);
		UpdateLogs();
		LoadActions(activeTileObject);
		if (activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD)
		{
			LoadSettlementQuests();
			TryShowFirstCriminal();
		}
		if (activeTileObject is Artifact artifact)
		{
			if (ScriptableObjectsManager.Instance.artifactDataDictionary.ContainsKey(artifact.type))
			{
				TileObjectScriptableObject tileObjectScriptableObject = ScriptableObjectsManager.Instance.artifactDataDictionary[artifact.type].tileObjectScriptableObject;
				if (tileObjectScriptableObject.uiSFX.IsValid())
				{
					tileObjectScriptableObject.uiSFX.Post(InnerMapCameraMove.Instance.gameObject);
				}
			}
		}
		else
		{
			TileObjectScriptableObject tileObjectScriptableObject2 = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(activeTileObject.tileObjectType);
			if (tileObjectScriptableObject2.uiSFX.IsValid())
			{
				tileObjectScriptableObject2.uiSFX.Post(InnerMapCameraMove.Instance.gameObject);
			}
		}
	}

	protected override bool ShouldCreateActionItem(SkillData p_skill, IPlayerActionTarget p_target)
	{
		if (base.ShouldCreateActionItem(p_skill, p_target))
		{
			if (p_skill.type != PLAYER_SKILL_TYPE.DESTROY_EYE_WARD && p_skill.type != PLAYER_SKILL_TYPE.ATTACK_VILLAGE && p_skill.type != PLAYER_SKILL_TYPE.LURE && p_skill.type != PLAYER_SKILL_TYPE.INFUSE && p_skill.type != PLAYER_SKILL_TYPE.SNATCH_OBJECT && p_skill.type != PLAYER_SKILL_TYPE.SNATCH_VILLAGER && p_skill.type != PLAYER_SKILL_TYPE.KILL_VILLAGER && p_skill.type != PLAYER_SKILL_TYPE.DESTROY_SUPPLIES && p_skill.type != PLAYER_SKILL_TYPE.DESTROY_DEFENSES && p_skill.type != PLAYER_SKILL_TYPE.HARASS_VILLAGERS)
			{
				return p_skill.type == PLAYER_SKILL_TYPE.DESTROY_STRUCTURES;
			}
			return true;
		}
		return false;
	}

	public void UpdateTileObjectInfo()
	{
		if (activeTileObject != null)
		{
			UpdateBasicInfo();
			UpdateInfo();
		}
	}

	private void UpdateBasicInfo()
	{
		nameLbl.text = activeTileObject.nameplateName;
	}

	private void UpdateInfo()
	{
		hpLbl.text = $"{activeTileObject.currentHP}/{activeTileObject.maxHP}";
		int num = 1;
		if (activeTileObject is ResourcePile resourcePile)
		{
			num = resourcePile.resourceInPile;
		}
		else if (activeTileObject is Table)
		{
			num = activeTileObject.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD);
		}
		quantityLbl.text = $"{num}";
		if (activeTileObject.tileObjectType.IsTileObjectWithCount())
		{
			quantityLbl.text = GetCountQuantityBaseOnType(activeTileObject).ToString();
		}
		generalDescription.text = activeTileObject.description;
		ownerLbl.text = ((activeTileObject.characterOwner != null) ? ("<link=\"1\">" + Utilities.ColorizeAndBoldName(activeTileObject.characterOwner.name) + "</link>") : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None"));
		UpdateLocationInfo();
		tileObjectPortrait.SetTileObject(activeTileObject);
	}

	public int GetCountQuantityBaseOnType(TileObject p_object)
	{
		switch (p_object.tileObjectType)
		{
		case TILE_OBJECT_TYPE.ROCK:
			return (p_object as Rock).count;
		case TILE_OBJECT_TYPE.ORE:
			return (p_object as Ore).count;
		case TILE_OBJECT_TYPE.SMALL_TREE_OBJECT:
		case TILE_OBJECT_TYPE.BIG_TREE_OBJECT:
			return (p_object as TreeObject).count;
		default:
			return 1;
		}
	}

	private void OnRightClickPortrait(TileObject p_tileObject)
	{
		UIManager.Instance.ShowPlayerActionContextMenu(p_tileObject, InputManager.Instance.mousePosition, p_isScreenPosition: true);
	}

	private void UpdateLocationInfo()
	{
		if (activeTileObject.isBeingCarriedBy != null)
		{
			carriedByLbl.text = "<link=\"1\">" + Utilities.ColorizeAndBoldName(activeTileObject.isBeingCarriedBy.name) + "</link>";
		}
		else if (activeTileObject.gridTileLocation != null)
		{
			if (activeTileObject.gridTileLocation.structure is Wilderness)
			{
				carriedByLbl.text = "Wilderness";
			}
			else
			{
				carriedByLbl.text = "<link=\"1\">" + Utilities.ColorizeAndBoldName(activeTileObject.gridTileLocation.structure.name) + "</link>";
			}
		}
		else
		{
			carriedByLbl.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None");
		}
	}

	private void UpdateTraits()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		_ = string.Empty;
		for (int i = 0; i < activeTileObject.traitContainer.statuses.Count; i++)
		{
			Status status = activeTileObject.traitContainer.statuses[i];
			if (!status.isHidden)
			{
				string text3 = "#CEB67C";
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text = $"{text}<b><color={text3}><link=\"{i}\">{status.GetNameInUI(activeTileObject)}</link></color></b>";
			}
		}
		for (int j = 0; j < activeTileObject.traitContainer.traits.Count; j++)
		{
			Trait trait = activeTileObject.traitContainer.traits[j];
			if (!trait.isHidden)
			{
				string text4 = "#CEB67C";
				if (trait.type == TRAIT_TYPE.BUFF)
				{
					text4 = "#39FF14";
				}
				else if (trait.type == TRAIT_TYPE.FLAW)
				{
					text4 = "#FF073A";
				}
				if (!string.IsNullOrEmpty(text2))
				{
					text2 += ", ";
				}
				text2 = $"{text2}<b><color={text4}><link=\"{j}\">{trait.GetNameInUI(activeTileObject)}</link></color></b>";
			}
		}
		statusTraitsLbl.text = string.Empty;
		if (activeTileObject is EquipmentItem equipmentItem)
		{
			specialTexts.gameObject.SetActive(value: true);
			equipBonusGO.gameObject.SetActive(value: false);
			itemEffectsGO.gameObject.SetActive(value: true);
			monsterSpawnerEffectsGO.gameObject.SetActive(value: false);
			itemEffectsLbl.text = equipmentItem.GetBonusDescription();
		}
		else if (activeTileObject is MonsterSpawner monsterSpawner)
		{
			specialTexts.gameObject.SetActive(value: true);
			equipBonusGO.gameObject.SetActive(value: false);
			itemEffectsGO.gameObject.SetActive(value: false);
			monsterSpawnerEffectsGO.gameObject.SetActive(value: true);
			monsterSpawnerLbl.text = monsterSpawner.GetMonsterSpawnerNextSpawnTime();
		}
		else
		{
			specialTexts.gameObject.SetActive(value: false);
			equipBonusGO.gameObject.SetActive(value: false);
		}
		if (!string.IsNullOrEmpty(text))
		{
			statusTraitsLbl.text = text;
		}
		normalTraitsLbl.text = string.Empty;
		if (!string.IsNullOrEmpty(text2))
		{
			normalTraitsLbl.text = text2;
		}
	}

	private void UpdateUsers()
	{
		Utilities.DestroyChildren(charactersScrollView.content);
		Character[] users = activeTileObject.users;
		if (users == null || users.Length == 0)
		{
			return;
		}
		foreach (Character character in users)
		{
			if (character != null)
			{
				CharacterPortrait component = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content).GetComponent<CharacterPortrait>();
				component.GeneratePortrait(character);
				component.SetNameState(p_state: true);
				component.SetHoverActions(OnHoverOverCharacterPortrait, OnHoverOutCharacterPortrait);
			}
		}
	}

	public void UpdateLogs()
	{
		logsWindow.UpdateAllHistoryInfo();
	}

	private void OnHoverOverCharacterPortrait(CharacterPortrait p_portrait)
	{
		UIManager.Instance.ShowCharacterNameplateTooltip(p_portrait.character, characterNameplateHoverPosition);
	}

	private void OnHoverOutCharacterPortrait(CharacterPortrait p_portrait)
	{
		UIManager.Instance.HideCharacterNameplateTooltip();
	}

	private void UpdateLogsFromSignal(Log log)
	{
		if (isShowing && log.IsInvolved(activeTileObject))
		{
			UpdateLogs();
		}
	}

	private void OnLogMentioningCharacterUpdated(Character character)
	{
		if (isShowing)
		{
			UpdateLogs();
		}
	}

	private void UpdateUsersFromSignal(TileObject tileObject, Character user)
	{
		if (isShowing && activeTileObject == tileObject)
		{
			UpdateUsers();
		}
	}

	private void UpdateTraitsFromSignal(TileObject tileObject, Trait trait)
	{
		if (isShowing && activeTileObject == tileObject)
		{
			UpdateTraits();
		}
	}

	private void OnReceiveKeyCodeSignal(KeyCode p_key)
	{
		if (p_key == KeyCode.Mouse1)
		{
			CloseMenu();
		}
	}

	private void OnPartyQuestAdded(PartyQuest p_quest, Faction p_faction)
	{
		if (isShowing && activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD && activeTileObject.currentSettlement != null && activeTileObject.currentSettlement.owner == p_faction)
		{
			CreatePartyQuestItem(p_quest, p_faction);
			UpdateNoAvailableQuestsCover();
		}
	}

	private void OnPartyQuestRemoved(PartyQuest p_quest, Faction p_faction)
	{
		if (isShowing && activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD && activeTileObject.currentSettlement != null && activeTileObject.currentSettlement.owner == p_faction)
		{
			RemovePartyQuestItem(p_quest);
			UpdateNoAvailableQuestsCover();
		}
	}

	private void OnNoLongerWantedCriminalOfFaction(Character p_character, Faction p_faction)
	{
		if (isShowing && activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD && activeTileObject.currentSettlement != null && activeTileObject.currentSettlement.owner == p_faction && _currentlySelectedCriminal == p_character)
		{
			TryShowFirstCriminal();
		}
	}

	private void OnLeftClickOwner(object obj)
	{
		if (activeTileObject.characterOwner != null)
		{
			UIManager.Instance.ShowCharacterInfo(activeTileObject.characterOwner, centerOnCharacter: true);
		}
	}

	private void OnRightClickOwner(object obj)
	{
		IPlayerActionTarget playerActionTarget = obj as IPlayerActionTarget;
		if (playerActionTarget != null)
		{
			if (playerActionTarget is Character { isLycanthrope: not false } character)
			{
				playerActionTarget = character.lycanData.activeForm;
			}
			UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	private void OnLeftClickLocation(object obj)
	{
		if (activeTileObject.isBeingCarriedBy != null)
		{
			UIManager.Instance.ShowCharacterInfo(activeTileObject.isBeingCarriedBy, centerOnCharacter: true);
		}
		else if (activeTileObject.gridTileLocation != null && !(activeTileObject.gridTileLocation.structure is Wilderness))
		{
			UIManager.Instance.ShowStructureInfo(activeTileObject.gridTileLocation.structure);
		}
	}

	private void OnRightClickLocation(object obj)
	{
		IPlayerActionTarget playerActionTarget = obj as IPlayerActionTarget;
		if (playerActionTarget != null)
		{
			if (playerActionTarget is Character { isLycanthrope: not false } character)
			{
				playerActionTarget = character.lycanData.activeForm;
			}
			UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	public void OnClickItem()
	{
		if (activeTileObject != null)
		{
			activeTileObject.CenterOnTileObject();
		}
	}

	public void OnHoverTrait(object obj)
	{
		if (obj is string)
		{
			int num = int.Parse((string)obj);
			if (num < activeTileObject.traitContainer.traits.Count)
			{
				string descriptionInUI = activeTileObject.traitContainer.traits[num].descriptionInUI;
				UIManager.Instance.ShowSmallInfo(descriptionInUI, "", autoReplaceText: false);
			}
		}
	}

	public void OnHoverStatus(object obj)
	{
		if (obj is string)
		{
			int num = int.Parse((string)obj);
			if (num < activeTileObject.traitContainer.statuses.Count)
			{
				string descriptionInUI = activeTileObject.traitContainer.statuses[num].descriptionInUI;
				UIManager.Instance.ShowSmallInfo(descriptionInUI, "", autoReplaceText: false);
			}
		}
	}

	public void OnHoverOutTrait()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateTabsBasedOnActiveObject()
	{
		EnableTab(infoTgl);
		EnableTab(logsTgl);
		if (activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD && TryGetFactionLocationOfTileObject(out var _))
		{
			EnableTab(questsTgl);
			EnableTab(wantedTgl);
		}
		else
		{
			DisableTab(questsTgl);
			DisableTab(wantedTgl);
		}
		if (activeTileObject.users != null)
		{
			EnableTab(charactersToggle);
		}
		else
		{
			DisableTab(charactersToggle);
		}
	}

	private void EnableTab(Toggle p_tab)
	{
		p_tab.gameObject.SetActive(value: true);
	}

	private void DisableTab(Toggle p_tab)
	{
		p_tab.isOn = false;
		p_tab.gameObject.SetActive(value: false);
	}

	private bool TryGetFactionLocationOfTileObject(out Faction p_faction)
	{
		if (activeTileObject.currentSettlement is NPCSettlement { owner: not null } nPCSettlement)
		{
			p_faction = nPCSettlement.owner;
			return true;
		}
		p_faction = null;
		return false;
	}

	private void LoadSettlementQuests()
	{
		Utilities.DestroyChildrenObjectPool(questsScrollRect.content);
		_partyQuestItems.Clear();
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			for (int i = 0; i < p_faction.partyQuestBoard.availablePartyQuests.Count; i++)
			{
				PartyQuest p_quest = p_faction.partyQuestBoard.availablePartyQuests[i];
				CreatePartyQuestItem(p_quest, p_faction);
			}
		}
		UpdateNoAvailableQuestsCover();
	}

	private void UpdateNoAvailableQuestsCover()
	{
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			bool flag = p_faction.partyQuestBoard.availablePartyQuests.Count > 0;
			noQuestsAvailableGO.SetActive(!flag);
		}
		else
		{
			noQuestsAvailableGO.SetActive(value: false);
		}
	}

	private void CreatePartyQuestItem(PartyQuest p_quest, Faction p_faction)
	{
		PartyQuestItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(settlementQuestItemPrefab.name, Vector3.zero, Quaternion.identity, questsScrollRect.content).GetComponent<PartyQuestItem>();
		component.Initialize(p_quest, OnClickCancelPartyQuest, p_faction);
		_partyQuestItems.Add(component);
	}

	private void RemovePartyQuestItem(PartyQuest p_quest)
	{
		PartyQuestItem partyQuestItem = GetPartyQuestItem(p_quest);
		if (partyQuestItem != null)
		{
			_partyQuestItems.Remove(partyQuestItem);
			ObjectPoolManager.Instance.DestroyObject(partyQuestItem);
		}
	}

	private PartyQuestItem GetPartyQuestItem(PartyQuest p_quest)
	{
		for (int i = 0; i < _partyQuestItems.Count; i++)
		{
			PartyQuestItem partyQuestItem = _partyQuestItems[i];
			if (partyQuestItem.quest == p_quest)
			{
				return partyQuestItem;
			}
		}
		return null;
	}

	private void OnClickCancelPartyQuest(PartyQuest p_partyQuest)
	{
		Cost cancelPartyQuestCost = EditableValuesManager.Instance.cancelPartyQuestCost;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Quest");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("questName", p_partyQuest.GetPartyQuestName());
		dictionary.Add("cost", cancelPartyQuestCost.GetCostStringWithIcon());
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Quest_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
		{
			OnConfirmCancelPartyQuest(p_partyQuest);
		}, null, showCover: true, 21, "Yes", "No", PlayerManager.Instance.player.currenciesComponent.CanAfford(cancelPartyQuestCost), noBtnInteractable: true, pauseAndResume: true);
	}

	private void OnConfirmCancelPartyQuest(PartyQuest p_partyQuest)
	{
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			Cost cancelPartyQuestCost = EditableValuesManager.Instance.cancelPartyQuestCost;
			PlayerManager.Instance.player.currenciesComponent.ReduceCurrency(cancelPartyQuestCost);
			p_partyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Player_Cancelled"));
			p_faction.partyQuestBoard.RemovePartyQuest(p_partyQuest);
		}
	}

	private void TryShowFirstCriminal()
	{
		UpdateNoWantedCriminalsCover();
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			Character character = p_faction.crimeComponent.wantedCharacters.FirstOrDefault();
			if (character != null)
			{
				ShowWantedCriminal(character, p_faction);
			}
		}
	}

	private void UpdateNoWantedCriminalsCover()
	{
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			bool flag = p_faction.crimeComponent.wantedCharacters.Count > 0;
			noWantedCriminalsGO.SetActive(!flag);
		}
		else
		{
			noWantedCriminalsGO.SetActive(value: false);
		}
	}

	private void ShowWantedCriminal(Character p_criminal, Faction p_faction)
	{
		_currentlySelectedCriminal = p_criminal;
		List<string> listOfCrimeNamesWantedByNoDuplicates = p_criminal.crimeComponent.GetListOfCrimeNamesWantedByNoDuplicates(p_faction);
		wantedCrimesLbl.text = listOfCrimeNamesWantedByNoDuplicates.ComafyListNoAnd();
		wantedCriminalPortrait.GeneratePortrait(p_criminal);
		wantedCriminalNameLbl.text = p_criminal.name;
	}

	private void OnClickShowNextCriminal()
	{
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			Character nextElementCyclic = CollectionUtilities.GetNextElementCyclic(p_faction.crimeComponent.wantedCharacters, _currentlySelectedCriminal);
			ShowWantedCriminal(nextElementCyclic, p_faction);
		}
	}

	private void OnClickShowPreviousCriminal()
	{
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			Character previousElementCyclic = CollectionUtilities.GetPreviousElementCyclic(p_faction.crimeComponent.wantedCharacters, _currentlySelectedCriminal);
			ShowWantedCriminal(previousElementCyclic, p_faction);
		}
	}

	private void OnClickRemoveCrimes()
	{
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			Cost totalRemoveAllCrimesCost = GetTotalRemoveAllCrimesCost(_currentlySelectedCriminal, p_faction);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remove_Crimes");
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("targetName", _currentlySelectedCriminal.bookmarkName);
			dictionary.Add("factionName", p_faction.nameWithColor);
			dictionary.Add("cost", totalRemoveAllCrimesCost.GetCostStringWithIcon());
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remove_Crimes_Description", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmRemoveCrimes, null, showCover: true, 21, "Yes", "No", PlayerManager.Instance.player.currenciesComponent.CanAfford(totalRemoveAllCrimesCost), noBtnInteractable: true, pauseAndResume: true);
		}
	}

	private void OnConfirmRemoveCrimes()
	{
		if (TryGetFactionLocationOfTileObject(out var p_faction))
		{
			Cost totalRemoveAllCrimesCost = GetTotalRemoveAllCrimesCost(_currentlySelectedCriminal, p_faction);
			PlayerManager.Instance.player.currenciesComponent.ReduceCurrency(totalRemoveAllCrimesCost);
			_currentlySelectedCriminal.crimeComponent.RemoveAllCrimesWantedBy(p_faction);
		}
	}

	private Cost GetTotalRemoveAllCrimesCost(Character p_criminal, Faction p_faction)
	{
		Cost result;
		if (p_faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			result = new Cost(EditableValuesManager.Instance.removePerCrimesCost.currency, 0);
		}
		else
		{
			List<CrimeData> listOfCrimesWantedBy = p_criminal.crimeComponent.GetListOfCrimesWantedBy(p_faction);
			int p_crimeCount = listOfCrimesWantedBy?.Count ?? 0;
			result = EditableValuesManager.Instance.GetTotalRemoveAllCrimesCost(p_crimeCount);
			if (listOfCrimesWantedBy != null)
			{
				RuinarchListPool<CrimeData>.Release(listOfCrimesWantedBy);
			}
		}
		return result;
	}

	public void ShowTileObjectTestingInfo()
	{
	}

	public void HideTileObjectTestingInfo()
	{
	}
}
