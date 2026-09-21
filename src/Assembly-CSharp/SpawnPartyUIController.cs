using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Location_Structures.Components;
using Locations.Settlements;
using Maccima_Games.Util;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class SpawnPartyUIController : MVCUIController, SpawnPartyUIView.IListener
{
	[SerializeField]
	private SpawnPartyUIModel m_spawnPartyUIModel;

	private SpawnPartyUIView m_spawnPartyUIView;

	private STRUCTURE_PARTY_TYPE _partyType;

	private PartyStructureComponent _partyStructureComponent;

	private LocationGridTile _chosenSpawnTile;

	private SpawnPartySlotItem _currentlyClickedSpawnPartySlotItem;

	private List<IStoredTarget> _allValidTargets;

	private IStoredTarget _chosenTarget;

	private Character _demonLeader;

	private List<Character> _chosenSummons;

	private List<IStoredTarget> _allValidAttackerLocations;

	private LocationStructure _attackerLocation;

	private SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR _chosenBehaviour;

	private string _strUnlockSlot;

	public bool isShowing;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SpawnPartyUIView.Create(_canvas, m_spawnPartyUIModel, delegate(SpawnPartyUIView p_ui)
		{
			m_spawnPartyUIView = p_ui;
			m_spawnPartyUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			HideUI();
		});
	}

	private void Start()
	{
		InstantiateUI();
		_chosenSummons = new List<Character>(10);
		_allValidTargets = new List<IStoredTarget>(10);
		_allValidAttackerLocations = new List<IStoredTarget>(10);
		SubscribeListeners();
		m_spawnPartyUIView.UIModel.leaderSlotItem.SetInteractionActions(OnLeftClickLeaderItem, OnRightClickLeaderItem, OnHoverOverLeaderItem, OnHoverOutLeaderItem);
		for (int i = 0; i < m_spawnPartyUIView.UIModel.summonSlotItems.Length; i++)
		{
			m_spawnPartyUIView.UIModel.summonSlotItems[i].SetInteractionActions(OnLeftClickSummonItem, OnRightClickSummonSlot, OnHoverOverSummonSlot, OnHoverOutSummonSlot);
		}
		_strUnlockSlot = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Add_Follower_Slot");
	}

	private void OnDestroy()
	{
		_chosenSummons = null;
		_allValidTargets = null;
		_allValidAttackerLocations = null;
		m_spawnPartyUIView?.Unsubscribe(this);
		UnsubscribeListeners();
		m_spawnPartyUIView.UIModel.leaderSlotItem.ClearInteractionActions();
		for (int i = 0; i < m_spawnPartyUIView.UIModel.summonSlotItems.Length; i++)
		{
			m_spawnPartyUIView.UIModel.summonSlotItems[i].ClearInteractionActions();
		}
	}

	public void Show(LocationStructure p_structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour, IStoredTarget p_target = null, bool p_attackerDropdownInteractable = true, bool p_targetDropdownInteractable = true)
	{
		_chosenTarget = p_target;
		SetAttackingLocation(p_structure);
		_chosenBehaviour = p_behaviour;
		m_spawnPartyUIView.UpdateBehaviourDropdownDisplay(GetIndexOfTarget(p_behaviour));
		ShowUI();
		UpdateWindowTitle();
		UpdateTargetsDropdown(p_target);
		UpdateAttackerLocationUI();
		UpdateSpawnPartyBtnName();
		UpdateAddPartySlotBtnState();
		UpdateAddPartySlotBtnInteractableState();
		UpdateSpawnPartyBtnInteractableState();
		if (_attackerLocation != null)
		{
			m_spawnPartyUIView.UpdateAttackerLocationDisplay(_attackerLocation, GetIndexOfTargetLocation(_attackerLocation));
		}
		m_spawnPartyUIView.SetAttackerDropdownInteractableState(p_attackerDropdownInteractable);
		m_spawnPartyUIView.SetTargetDropdownInteractableState(p_targetDropdownInteractable);
	}

	public override void HideUI()
	{
		base.HideUI();
		if (isShowing)
		{
			m_spawnPartyUIView.UIModel.rtWindow.anchoredPosition = m_spawnPartyUIView.UIModel.defaultPos;
			isShowing = false;
			if (GameManager.Instance.gameHasStarted)
			{
				PlayerUI.Instance.EnableTopMenuButtons();
				UIManager.Instance.ResumeLastProgressionSpeed();
			}
			m_spawnPartyUIView.HideObjectPicker();
			m_spawnPartyUIView.UIModel.monsterToolTipUI.HideToolTip();
			UIManager.Instance.sidebarUIController.ShowUI();
		}
	}

	public override void ShowUI()
	{
		base.ShowUI();
		isShowing = true;
		if (GameManager.Instance.gameHasStarted)
		{
			PlayerUI.Instance.CloseAllTopMenus();
			PlayerUI.Instance.DisableTopMenuButtons();
			UIManager.Instance.Pause();
			UIManager.Instance.SetSpeedTogglesState(state: false);
		}
		_chosenSummons?.Clear();
		for (int i = 0; i < m_spawnPartyUIView.UIModel.summonSlotItems.Length; i++)
		{
			m_spawnPartyUIView.UIModel.summonSlotItems[i].ClearMonsterUnderling();
		}
		_demonLeader = null;
		m_spawnPartyUIView.UIModel.leaderSlotItem.ClearMonsterUnderling();
		_currentlyClickedSpawnPartySlotItem = null;
		UIManager.Instance.sidebarUIController.HideUI();
	}

	public void HideViaShortcutKey()
	{
		HideUI();
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnChaoticEnergyAdjusted);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
		Messenger.AddListener<LocationGridTile>(PartySignals.PARTY_TILE_CHOSEN_FOR_SPAWNING, OnTileChosenForSpawning);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedStoredTarget);
		Messenger.AddListener<Character, LocationStructure>(StructureSignals.REMOVED_STRUCTURE_RESIDENT, OnStructureResidentsUpdated);
		Messenger.AddListener<Character, LocationStructure>(StructureSignals.ADDED_STRUCTURE_RESIDENT, OnStructureResidentsUpdated);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnChaoticEnergyAdjusted);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
		Messenger.RemoveListener<LocationGridTile>(PartySignals.PARTY_TILE_CHOSEN_FOR_SPAWNING, OnTileChosenForSpawning);
		Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedStoredTarget);
		Messenger.RemoveListener<Character, LocationStructure>(StructureSignals.REMOVED_STRUCTURE_RESIDENT, OnStructureResidentsUpdated);
		Messenger.RemoveListener<Character, LocationStructure>(StructureSignals.ADDED_STRUCTURE_RESIDENT, OnStructureResidentsUpdated);
	}

	private void OnChaoticEnergyAdjusted(int p_amountAdjusted, int p_totalChaoticEnergy)
	{
		if (isShowing)
		{
			UpdateAddPartySlotBtnInteractableState();
		}
	}

	private void OnManaAdjusted(int p_amountAdjusted, int p_totalMana)
	{
		if (isShowing)
		{
			UpdateSpawnPartyBtnInteractableState();
		}
	}

	private void OnTileChosenForSpawning(LocationGridTile p_chosenTile)
	{
		_chosenSpawnTile = p_chosenTile;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Deploy_Party");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("cost", GetTotalDeployCost() + Utilities.ManaIcon());
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Deploy_Party_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnYesSpawnPartyDeploy, null, showCover: true, 150);
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (!GameManager.Instance.gameHasStarted)
		{
			return;
		}
		object currentlySelectedObject = UIManager.Instance.GetCurrentlySelectedObject();
		if (!(currentlySelectedObject is DemonicStructure demonicStructure))
		{
			return;
		}
		switch (p_action)
		{
		case SHORTCUT_ACTION.Snatch_Villager:
			if (currentlySelectedObject is TortureChambers tortureChambers && demonicStructure.charactersHere.Count <= 0 && tortureChambers.partyStructureComponent.party == null && tortureChambers.partyStructureComponent.HasValidStoredTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager))
			{
				Show(tortureChambers, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager, null, p_attackerDropdownInteractable: false);
			}
			break;
		case SHORTCUT_ACTION.Raid:
			if (currentlySelectedObject is Maraud maraud && maraud.partyStructureComponent.party == null)
			{
				Show(maraud, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies, null, p_attackerDropdownInteractable: false);
			}
			break;
		case SHORTCUT_ACTION.Snatch_Monster:
			if (currentlySelectedObject is Kennel kennel && demonicStructure.charactersHere.Count <= 0 && kennel.partyStructureComponent.party == null && kennel.partyStructureComponent.HasValidStoredTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster))
			{
				Show(kennel, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster, null, p_attackerDropdownInteractable: false);
			}
			break;
		}
	}

	private void OnPlayerStoredTarget(IStoredTarget p_target)
	{
		if (!isShowing)
		{
			return;
		}
		if (p_target is LocationStructure)
		{
			UpdateAttackerLocationUI();
			if (_allValidAttackerLocations.Count > 0 && _attackerLocation == null)
			{
				_attackerLocation = _allValidAttackerLocations[0] as LocationStructure;
			}
			if (_attackerLocation != null)
			{
				m_spawnPartyUIView.UpdateAttackerLocationDisplay(_attackerLocation, GetIndexOfTargetLocation(_attackerLocation));
			}
		}
		else if (p_target is Character)
		{
			UpdateTargetsDropdown(_chosenTarget);
		}
	}

	private void OnPlayerRemovedStoredTarget(IStoredTarget p_target)
	{
		if (!isShowing)
		{
			return;
		}
		if (p_target is LocationStructure)
		{
			UpdateAttackerLocationUI();
			if (_allValidAttackerLocations.Count > 0 && _attackerLocation == null)
			{
				_attackerLocation = _allValidAttackerLocations[0] as LocationStructure;
			}
			if (_attackerLocation != null)
			{
				m_spawnPartyUIView.UpdateAttackerLocationDisplay(_attackerLocation, GetIndexOfTargetLocation(_attackerLocation));
			}
		}
		else if (p_target is Character)
		{
			UpdateTargetsDropdown();
		}
	}

	private void OnStructureResidentsUpdated(Character p_character, LocationStructure p_structure)
	{
		if (isShowing && p_structure == _attackerLocation)
		{
			UpdateSpawnPartyBtnName();
			UpdateSpawnPartyBtnInteractableState();
		}
	}

	private void UpdateWindowTitle()
	{
		switch (_partyType)
		{
		case STRUCTURE_PARTY_TYPE.Snatch_Villager:
		case STRUCTURE_PARTY_TYPE.Snatch_Monster:
			m_spawnPartyUIView.SetTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Snatch_UI_Title"));
			break;
		case STRUCTURE_PARTY_TYPE.Kill:
			m_spawnPartyUIView.SetTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Kill_UI_Title"));
			break;
		case STRUCTURE_PARTY_TYPE.Raid:
			m_spawnPartyUIView.SetTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Raid_UI_Title"));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void ShowBehaviourTypeTooltip(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		string text = p_behaviour.ToStringEnumWithSpace();
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", text);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", text + "_Description");
		UIManager.Instance.ShowSmallInfo(localizedValue2, m_spawnPartyUIView.UIModel.tooltipPos, localizedValue);
	}

	private void OnLeftClickLeaderItem(SpawnPartySlotItem p_item)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++)
		{
			Character character = PlayerManager.Instance.player.playerFaction.characters[i];
			if (!character.isDead && character.minion != null)
			{
				list.Add(character);
			}
		}
		m_spawnPartyUIView.ShowObjectPicker(list, OnChooseLeaderItem, OnHoverOverObjectPickerItem, OnHoverOutObjectPickerItem, CanChooseLeader);
		RuinarchListPool<Character>.Release(list);
	}

	private bool CanChooseLeader(Character p_character)
	{
		if (!p_character.isDead)
		{
			if (p_character.partyComponent.hasParty)
			{
				return p_character.partyComponent.currentParty == PlayerManager.Instance.player.underlingsComponent.persistentDefendParty;
			}
			return true;
		}
		return false;
	}

	private void OnRightClickLeaderItem(SpawnPartySlotItem p_item)
	{
		if (_demonLeader != null)
		{
			_demonLeader = null;
			m_spawnPartyUIView.UIModel.leaderSlotItem.ClearMonsterUnderling();
			UpdateSpawnPartyBtnName();
			UpdateSpawnPartyBtnInteractableState();
		}
	}

	private void OnChooseLeaderItem(Character p_character)
	{
		if (_demonLeader == p_character)
		{
			_demonLeader = null;
			m_spawnPartyUIView.UIModel.leaderSlotItem.ClearMonsterUnderling();
		}
		else
		{
			_demonLeader = p_character;
			m_spawnPartyUIView.UIModel.leaderSlotItem.SetMonsterUnderling(_demonLeader);
		}
		m_spawnPartyUIView.HideObjectPicker();
		m_spawnPartyUIView.HideCharacterTooltip();
		m_spawnPartyUIView.UIModel.monsterToolTipUI.HideToolTip();
		UpdateSpawnPartyBtnName();
		UpdateSpawnPartyBtnInteractableState();
	}

	private void OnHoverOverLeaderItem(SpawnPartySlotItem p_item)
	{
		if (p_item.character != null)
		{
			m_spawnPartyUIView.ShowCharacterTooltip(p_item.character);
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_item.character.characterClass.className);
			CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
			m_spawnPartyUIView.UIModel.monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.displayName, combatBehaviour.description);
		}
	}

	private void OnHoverOutLeaderItem(SpawnPartySlotItem p_item)
	{
		m_spawnPartyUIView.HideCharacterTooltip();
		m_spawnPartyUIView.UIModel.monsterToolTipUI.HideToolTip();
	}

	private int GetPartySlotUnlockCost(int p_slotIndex)
	{
		int p_baseCost;
		switch (p_slotIndex)
		{
		case 0:
		case 1:
			p_baseCost = 0;
			break;
		case 2:
			p_baseCost = 100;
			break;
		case 3:
			p_baseCost = 200;
			break;
		case 4:
			p_baseCost = 300;
			break;
		default:
			p_baseCost = 300;
			break;
		}
		return SpellUtilities.GetModifiedSpellCost(p_baseCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	private void UpdateAddPartySlotBtnState()
	{
		bool unlockSlotBtnState = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForStructureType(_attackerLocation) < GetMaxSummonCount();
		m_spawnPartyUIView.SetUnlockSlotBtnState(unlockSlotBtnState);
	}

	private void UpdateAddPartySlotBtnInteractableState()
	{
		int summonCountForStructureType = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForStructureType(_attackerLocation);
		bool unlockSlotBtnInteractable = PlayerManager.Instance.player.currenciesComponent.chaoticEnergy >= GetPartySlotUnlockCost(summonCountForStructureType);
		m_spawnPartyUIView.SetUnlockSlotBtnInteractable(unlockSlotBtnInteractable);
	}

	private int GetMaxSummonCount()
	{
		return 5;
	}

	private int GetUnlockedSummonSlots()
	{
		return PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForStructureType(_attackerLocation) - 1;
	}

	private void OnLeftClickSummonItem(SpawnPartySlotItem p_item)
	{
		_currentlyClickedSpawnPartySlotItem = p_item;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++)
		{
			Character character = PlayerManager.Instance.player.playerFaction.characters[i];
			if (!character.isDead && character.minion == null)
			{
				list.Add(character);
			}
		}
		m_spawnPartyUIView.ShowObjectPicker(list, OnChooseSummonItem, OnHoverOverObjectPickerItem, OnHoverOutObjectPickerItem, CanChooseSummon);
		RuinarchListPool<Character>.Release(list);
	}

	private void SelectNextUnlockedSummonSlot()
	{
		SpawnPartySlotItem currentlyClickedSpawnPartySlotItem = ((_currentlyClickedSpawnPartySlotItem == null) ? m_spawnPartyUIView.unlockedSummonSlots.First() : CollectionUtilities.GetNextElementCyclic(m_spawnPartyUIView.unlockedSummonSlots, _currentlyClickedSpawnPartySlotItem));
		_currentlyClickedSpawnPartySlotItem = currentlyClickedSpawnPartySlotItem;
	}

	private bool CanChooseSummon(Character p_character)
	{
		if (!p_character.isDead && (!p_character.partyComponent.hasParty || p_character.partyComponent.currentParty == PlayerManager.Instance.player.underlingsComponent.persistentDefendParty))
		{
			return !_chosenSummons.Contains(p_character);
		}
		return false;
	}

	private void UpdateSummonObjectPickerItems()
	{
		m_spawnPartyUIView.UpdateObjectPickerItems(CanChooseSummon);
	}

	private void OnChooseSummonItem(Character p_character)
	{
		if (_currentlyClickedSpawnPartySlotItem.character != null)
		{
			_chosenSummons.Remove(_currentlyClickedSpawnPartySlotItem.character);
		}
		_currentlyClickedSpawnPartySlotItem.SetMonsterUnderling(p_character);
		_chosenSummons.Add(p_character);
		UpdateSpawnPartyBtnName();
		UpdateSpawnPartyBtnInteractableState();
		UpdateSummonObjectPickerItems();
		SelectNextUnlockedSummonSlot();
	}

	private void OnRightClickSummonSlot(SpawnPartySlotItem p_item)
	{
		Character character = p_item.character;
		if (character != null && _chosenSummons.Remove(character))
		{
			p_item.ClearMonsterUnderling();
			m_spawnPartyUIView.HideCharacterTooltip();
			m_spawnPartyUIView.UIModel.monsterToolTipUI.HideToolTip();
			UpdateSpawnPartyBtnName();
			UpdateSpawnPartyBtnInteractableState();
			UpdateSummonObjectPickerItems();
		}
	}

	private void OnHoverOutSummonSlot(SpawnPartySlotItem p_item)
	{
		m_spawnPartyUIView.HideCharacterTooltip();
		m_spawnPartyUIView.UIModel.monsterToolTipUI.HideToolTip();
	}

	private void OnHoverOverSummonSlot(SpawnPartySlotItem p_item)
	{
		if (p_item.character != null)
		{
			m_spawnPartyUIView.ShowCharacterTooltip(p_item.character);
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_item.character.characterClass.className);
			CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
			m_spawnPartyUIView.UIModel.monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.displayName, combatBehaviour.description);
		}
	}

	private void UpdateTargetsDropdown(IStoredTarget p_target = null)
	{
		ConstructValidTargets(p_target);
		m_spawnPartyUIView.UpdateTargetOptions(_allValidTargets);
		if (_chosenTarget == null)
		{
			_chosenTarget = _allValidTargets.ElementAtOrDefault(0);
		}
		if (_chosenTarget != null)
		{
			m_spawnPartyUIView.UpdateTargetDisplay(_chosenTarget, GetIndexOfTarget(_chosenTarget));
		}
	}

	private int GetIndexOfTarget(IStoredTarget p_target)
	{
		if (p_target == null)
		{
			return -1;
		}
		for (int i = 0; i < _allValidTargets.Count; i++)
		{
			if (_allValidTargets[i] == p_target)
			{
				return i;
			}
		}
		return -1;
	}

	private void ConstructValidTargets(IStoredTarget p_additionalTarget)
	{
		_allValidTargets.Clear();
		List<IStoredTarget> allPossibleTargets = _partyStructureComponent.GetAllPossibleTargets(_chosenBehaviour);
		for (int i = 0; i < allPossibleTargets.Count; i++)
		{
			IStoredTarget storedTarget = allPossibleTargets[i];
			if (storedTarget.IsValidTargetForPartyStructure(_attackerLocation))
			{
				_allValidTargets.Add(storedTarget);
			}
		}
		if (p_additionalTarget != null && !_allValidTargets.Contains(p_additionalTarget) && p_additionalTarget.IsValidTargetForPartyStructure(_attackerLocation))
		{
			_allValidTargets.Add(p_additionalTarget);
		}
	}

	public void OnHoverOverTargetInDropdown(Transform p_dropDownItem)
	{
		int num = p_dropDownItem.GetSiblingIndex() - 1;
		if (_allValidTargets.IsIndexInList(num) && _allValidTargets[num] is Character p_data)
		{
			m_spawnPartyUIView.ShowCharacterTooltip(p_data);
		}
	}

	public void OnHoverOutTargetInDropdown(Transform p_dropDownItem)
	{
		m_spawnPartyUIView.HideCharacterTooltip();
	}

	private void UpdateAttackerLocationUI()
	{
		m_spawnPartyUIView.SetTargetLocationState(p_state: true);
		m_spawnPartyUIView.SetAttackerDropdownInteractableState(p_interactable: true);
		ConstructAttackerLocationChoices();
	}

	private bool IsStructureValidForTarget(LocationStructure p_structure, IStoredTarget p_target)
	{
		if (p_structure.partyStructureComponent == null)
		{
			return false;
		}
		if (p_target == null)
		{
			return true;
		}
		switch (p_structure.partyStructureComponent.structurePartyType)
		{
		case STRUCTURE_PARTY_TYPE.Snatch_Villager:
		case STRUCTURE_PARTY_TYPE.Kill:
			if (p_target is Character character2)
			{
				return character2.isNormalCharacter;
			}
			return false;
		case STRUCTURE_PARTY_TYPE.Snatch_Monster:
			if (!(p_target is Summon))
			{
				if (p_target is Character character)
				{
					return character.race == RACE.RATMAN;
				}
				return false;
			}
			return true;
		case STRUCTURE_PARTY_TYPE.Raid:
			return p_target is BaseSettlement;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void ConstructAttackerLocationChoices()
	{
		_allValidAttackerLocations.Clear();
		TryAddStructuresToValidAttackerLocationsList(PlayerManager.Instance.player.storedTargetsComponent.storedStructures, _allValidAttackerLocations);
		if (_attackerLocation != null)
		{
			TryAddStructuresToValidAttackerLocationsList(_attackerLocation, _allValidAttackerLocations);
		}
		for (int i = 0; i < InnerMapManager.Instance.currentMonsterSpawners.Count; i++)
		{
			MonsterSpawner monsterSpawner = InnerMapManager.Instance.currentMonsterSpawners[i];
			if (monsterSpawner.gridTileLocation != null && monsterSpawner.gridTileLocation.structure != null)
			{
				TryAddStructuresToValidAttackerLocationsList(monsterSpawner.gridTileLocation.structure, _allValidAttackerLocations);
			}
		}
		TryAddStructuresToValidAttackerLocationsList(PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.KENNEL), _allValidAttackerLocations);
		TryAddStructuresToValidAttackerLocationsList(PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.TORTURE_CHAMBERS), _allValidAttackerLocations);
		TryAddStructuresToValidAttackerLocationsList(PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.MARAUD), _allValidAttackerLocations);
		List<TMP_Dropdown.OptionData> list = RuinarchListPool<TMP_Dropdown.OptionData>.Claim();
		for (int j = 0; j < _allValidAttackerLocations.Count; j++)
		{
			IStoredTarget storedTarget = _allValidAttackerLocations[j];
			TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData(storedTarget.bookmarkName, storedTarget.GetPortraitSprite());
			list.Add(item);
		}
		m_spawnPartyUIView.SetTargetLocationDropdownOptions(list);
		RuinarchListPool<TMP_Dropdown.OptionData>.Release(list);
	}

	private void TryAddStructuresToValidAttackerLocationsList(List<IStoredTarget> p_structures, List<IStoredTarget> p_allValidAttackerLocations)
	{
		if (p_structures == null)
		{
			return;
		}
		for (int i = 0; i < p_structures.Count; i++)
		{
			if (p_structures[i] is LocationStructure p_structure)
			{
				TryAddStructuresToValidAttackerLocationsList(p_structure, p_allValidAttackerLocations);
			}
		}
	}

	private void TryAddStructuresToValidAttackerLocationsList(List<LocationStructure> p_structures, List<IStoredTarget> p_allValidAttackerLocations)
	{
		if (p_structures != null)
		{
			for (int i = 0; i < p_structures.Count; i++)
			{
				TryAddStructuresToValidAttackerLocationsList(p_structures[i], p_allValidAttackerLocations);
			}
		}
	}

	private void TryAddStructuresToValidAttackerLocationsList(LocationStructure p_structure, List<IStoredTarget> p_allValidAttackerLocations)
	{
		if (!p_allValidAttackerLocations.Contains(p_structure) && (p_structure.structureType.IsSpecialStructure() || p_structure.structureType == STRUCTURE_TYPE.KENNEL || p_structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS || p_structure.structureType == STRUCTURE_TYPE.MARAUD) && IsStructureValidForTarget(p_structure, _chosenTarget) && (p_structure.structureType.IsSpecialStructure() || !p_structure.partyStructureComponent.HasValidResidents()) && p_structure.partyStructureComponent.party == null && (!(p_structure is Kennel kennel) || kennel.partyStructureComponent.IsAvailable()) && (!(p_structure is TortureChambers tortureChambers) || tortureChambers.partyStructureComponent.IsAvailable()))
		{
			p_allValidAttackerLocations.Add(p_structure);
		}
	}

	private int GetIndexOfTargetLocation(LocationStructure p_target)
	{
		if (p_target == null)
		{
			return -1;
		}
		for (int i = 0; i < _allValidAttackerLocations.Count; i++)
		{
			if (_allValidAttackerLocations[i] == p_target)
			{
				return i;
			}
		}
		return -1;
	}

	private void SetAttackingLocation(LocationStructure p_structure)
	{
		_attackerLocation = p_structure;
		if (p_structure != null)
		{
			_partyType = p_structure.partyStructureComponent.structurePartyType;
			_partyStructureComponent = p_structure.partyStructureComponent;
		}
		m_spawnPartyUIView.UpdateAttackerLocationDisplay(_attackerLocation, GetIndexOfTargetLocation(_attackerLocation));
		m_spawnPartyUIView.UpdateBehaviourOptions(_attackerLocation.partyStructureComponent.availableBehaviours);
		m_spawnPartyUIView.UIModel.dropDownBehaviours.value = 0;
		if (_attackerLocation is DemonicStructure)
		{
			m_spawnPartyUIView.SetDemonLeaderAndSummonsState(p_state: true);
			m_spawnPartyUIView.SetStructureResidentsState(p_state: false);
			m_spawnPartyUIView.UpdateUnlockedSummonSlots(GetUnlockedSummonSlots());
		}
		else
		{
			m_spawnPartyUIView.SetDemonLeaderAndSummonsState(p_state: false);
			m_spawnPartyUIView.SetStructureResidentsState(p_state: true);
			List<Character> list = RuinarchListPool<Character>.Claim(_attackerLocation.residents.Count);
			for (int i = 0; i < _attackerLocation.residents.Count; i++)
			{
				Character character = _attackerLocation.residents[i];
				if (_attackerLocation.partyStructureComponent.IsResidentValidForParty(character))
				{
					list.Add(character);
				}
			}
			m_spawnPartyUIView.UpdateStructureResidents(list);
			RuinarchListPool<Character>.Release(list);
		}
		UpdateSpawnPartyBtnName();
		UpdateSpawnPartyBtnInteractableState();
	}

	private int GetTotalDeployCost()
	{
		int num = 0;
		if (_attackerLocation is DemonicStructure)
		{
			if (_demonLeader != null)
			{
				num += 50;
			}
			return num + _chosenSummons.Count * 50;
		}
		return 100;
	}

	public void OnTargetDropdownValueChanged(int p_value)
	{
		_chosenTarget = _allValidTargets[p_value];
		m_spawnPartyUIView.HideCharacterTooltip();
		UpdateAttackerLocationUI();
	}

	public void OnClickAddPartySlot()
	{
		int summonCountForStructureType = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForStructureType(_attackerLocation);
		int partySlotUnlockCost = GetPartySlotUnlockCost(summonCountForStructureType);
		AudioManager.Instance.TryPlayUISFX("Play_Party_UI_Unlock_Slot");
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-partySlotUnlockCost);
		PlayerManager.Instance.player.partyStructureDataHandler.UpdateSummonCountForStructureType(_attackerLocation);
		m_spawnPartyUIView.UpdateUnlockedSummonSlots(GetUnlockedSummonSlots());
		UpdateAddPartySlotBtnState();
	}

	public void OnHoverOverAddPartySlot()
	{
		int summonCountForStructureType = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForStructureType(_attackerLocation);
		UIManager.Instance.ShowSmallInfo(_strUnlockSlot + " - " + GetPartySlotUnlockCost(summonCountForStructureType) + Utilities.ChaoticEnergyIcon());
	}

	public void OnHoverOutAddPartySlot()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnClickSpawnParty()
	{
		if (_attackerLocation is DemonicStructure)
		{
			(PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_PARTY) as SpawnPartyData).Activate();
			HideUI();
		}
		else
		{
			DeployMonsterSpawnerParty();
		}
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void OnTargetLocationDropdownValueChanged(int p_value)
	{
		SetAttackingLocation(_attackerLocation = _allValidAttackerLocations[p_value] as LocationStructure);
	}

	public void OnClickCloseObjectPicker()
	{
		m_spawnPartyUIView.HideObjectPicker();
	}

	public void OnBehaviourDropdownValueChanged(int p_value)
	{
		_chosenBehaviour = _attackerLocation.partyStructureComponent.availableBehaviours[p_value];
	}

	public void OnHoverOverBehaviourDropdown(int p_value)
	{
		if (_attackerLocation.partyStructureComponent.availableBehaviours.IsIndexInArray(p_value))
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour = _attackerLocation.partyStructureComponent.availableBehaviours[p_value];
			ShowBehaviourTypeTooltip(p_behaviour);
		}
	}

	public void OnHoverOutBehaviourDropdown(int p_value)
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverOverBehaviourDropdownItem(Transform p_dropDownItem)
	{
		int num = p_dropDownItem.GetSiblingIndex() - 1;
		if (_attackerLocation.partyStructureComponent.availableBehaviours.IsIndexInArray(num))
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour = _attackerLocation.partyStructureComponent.availableBehaviours[num];
			ShowBehaviourTypeTooltip(p_behaviour);
		}
	}

	public void OnHoverOutBehaviourDropdownItem(Transform p_dropDownItem)
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateSpawnPartyBtnName()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Deploy_Party");
		m_spawnPartyUIView.SetSpawnPartyBtnLabelName(localizedValue + " - " + GetTotalDeployCost() + Utilities.ManaIcon());
	}

	private void UpdateSpawnPartyBtnInteractableState()
	{
		bool spawnPartyInteractableState = true;
		if (_attackerLocation is DemonicStructure && _demonLeader == null)
		{
			spawnPartyInteractableState = false;
		}
		else if (_attackerLocation.structureType.IsSpecialStructure() && (_attackerLocation.partyStructureComponent == null || !_attackerLocation.partyStructureComponent.HasValidResidents()))
		{
			spawnPartyInteractableState = false;
		}
		else if (!PlayerManager.Instance.player.currenciesComponent.CanAfford(CURRENCY.Mana, GetTotalDeployCost()))
		{
			spawnPartyInteractableState = false;
		}
		m_spawnPartyUIView.SetSpawnPartyInteractableState(spawnPartyInteractableState);
	}

	private void OnYesSpawnPartyDeploy()
	{
		DeployDemonicParty();
	}

	private void DeployMonsterSpawnerParty()
	{
		for (int i = 0; i < _attackerLocation.residents.Count; i++)
		{
			Character p_character = _attackerLocation.residents[i];
			if (_attackerLocation.partyStructureComponent.IsResidentValidForParty(p_character))
			{
				_partyStructureComponent.AddMonsterFromMonsterSpawner(p_character);
			}
		}
		ExecuteDeployActionsAfterPartyMembersRegistered();
	}

	private void DeployDemonicParty()
	{
		if (_demonLeader != null)
		{
			Character demonLeader = _demonLeader;
			demonLeader.SetDeployedAtStructure(_attackerLocation);
			_partyStructureComponent.AddDeployedItem(demonLeader);
			PlayerManager.Instance.player.underlingsComponent.persistentDefendParty?.RemoveMember(demonLeader);
			CharacterManager.Instance.Teleport(demonLeader, _chosenSpawnTile);
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		list.AddRange(_chosenSpawnTile.neighbourList);
		for (int i = 0; i < _chosenSummons.Count; i++)
		{
			Summon summon = _chosenSummons[i] as Summon;
			LocationGridTile locationGridTile = _chosenSpawnTile.GetRandomNeighborWithoutCharacters();
			if (locationGridTile == null)
			{
				if (list.Count <= 0)
				{
					list.AddRange(_chosenSpawnTile.neighbourList);
					if (list.Count <= 0)
					{
						list.Add(_chosenSpawnTile);
					}
				}
				locationGridTile = CollectionUtilities.GetRandomElement(list);
				list.Remove(locationGridTile);
			}
			CharacterManager.Instance.Teleport(summon, locationGridTile);
			summon.OnSummonAsPlayerMonster();
			summon.SetDeployedAtStructure(_attackerLocation);
			_partyStructureComponent.AddDeployedItem(summon);
			PlayerManager.Instance.player.underlingsComponent.persistentDefendParty?.RemoveMember(summon);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		ExecuteDeployActionsAfterPartyMembersRegistered();
		AudioManager.Instance.TryPlayUISFX("Play_Deploy_Party");
	}

	private void ExecuteDeployActionsAfterPartyMembersRegistered()
	{
		_partyStructureComponent.SetTarget(_chosenTarget);
		_partyStructureComponent.partyData.SetTargetStructure(_attackerLocation);
		_partyStructureComponent.DeployParty(_chosenBehaviour);
		PlayerManager.Instance.player.currenciesComponent.AdjustMana(-GetTotalDeployCost());
		HideUI();
	}

	private void OnHoverOverObjectPickerItem(Character p_data)
	{
		m_spawnPartyUIView.ShowCharacterTooltip(p_data);
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_data.characterClass.className);
		CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
		m_spawnPartyUIView.UIModel.monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.displayName, combatBehaviour.description);
	}

	private void OnHoverOutObjectPickerItem(Character p_data)
	{
		m_spawnPartyUIView.HideCharacterTooltip();
		m_spawnPartyUIView.UIModel.monsterToolTipUI.HideToolTip();
	}

	private int GetIndexOfTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_target)
	{
		for (int i = 0; i < _attackerLocation.partyStructureComponent.availableBehaviours.Length; i++)
		{
			if (_attackerLocation.partyStructureComponent.availableBehaviours[i] == p_target)
			{
				return i;
			}
		}
		return -1;
	}
}
