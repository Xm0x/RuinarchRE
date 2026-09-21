using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class SnatchObjectUIController : MVCUIController, SnatchObjectUIView.IListener
{
	[SerializeField]
	private SnatchObjectUIModel m_snatchObjectUIModel;

	private SnatchObjectUIView m_snatchObjectUIView;

	private LocationGridTile _chosenSpawnTile;

	private SnatchObjectSlotItem _currentlyClickedSnatchObjectSlotItem;

	private IStoredTarget _chosenTarget;

	private Character _chosenLeader;

	private List<Character> _chosenSummons;

	private List<IStoredTarget> _allValidDropLocations;

	private LocationStructure _snatchDropLocation;

	private string _strUnlockSlot;

	public bool isShowing;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SnatchObjectUIView.Create(_canvas, m_snatchObjectUIModel, delegate(SnatchObjectUIView p_ui)
		{
			m_snatchObjectUIView = p_ui;
			m_snatchObjectUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			HideUI();
		});
	}

	private void Start()
	{
		InstantiateUI();
		_chosenSummons = new List<Character>(10);
		_allValidDropLocations = new List<IStoredTarget>(10);
		SubscribeListeners();
		m_snatchObjectUIView.UIModel.leaderSlotItem.SetInteractionActions(OnLeftClickLeaderItem, OnRightClickLeaderItem, OnHoverOverLeaderItem, OnHoverOutLeaderItem);
		for (int i = 0; i < m_snatchObjectUIView.UIModel.summonSlotItems.Length; i++)
		{
			m_snatchObjectUIView.UIModel.summonSlotItems[i].SetInteractionActions(OnLeftClickSummonItem, OnRightClickSummonSlot, OnHoverOverSummonSlot, OnHoverOutSummonSlot);
		}
		_strUnlockSlot = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Add_Follower_Slot");
	}

	private void OnDestroy()
	{
		_chosenSummons = null;
		_allValidDropLocations = null;
		m_snatchObjectUIView?.Unsubscribe(this);
		UnsubscribeListeners();
		m_snatchObjectUIView.UIModel.leaderSlotItem.ClearInteractionActions();
		for (int i = 0; i < m_snatchObjectUIView.UIModel.summonSlotItems.Length; i++)
		{
			m_snatchObjectUIView.UIModel.summonSlotItems[i].ClearInteractionActions();
		}
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnChaoticEnergyAdjusted);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
		Messenger.AddListener<LocationGridTile>(PartySignals.SNATCH_OBJECT_PARTY_TILE_CHOSEN_FOR_SPAWNING, OnTileChosenForSpawning);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedStoredTarget);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnChaoticEnergyAdjusted);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
		Messenger.RemoveListener<LocationGridTile>(PartySignals.SNATCH_OBJECT_PARTY_TILE_CHOSEN_FOR_SPAWNING, OnTileChosenForSpawning);
		Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedStoredTarget);
	}

	private void OnPlayerStoredTarget(IStoredTarget p_target)
	{
		if (isShowing && p_target is LocationStructure)
		{
			UpdateTargetLocationUI();
			if (_allValidDropLocations.Count > 0 && _snatchDropLocation == null)
			{
				_snatchDropLocation = _allValidDropLocations[0] as LocationStructure;
			}
			if (_snatchDropLocation != null)
			{
				m_snatchObjectUIView.UpdateTargetLocationDisplay(_snatchDropLocation, GetIndexOfTargetLocation(_snatchDropLocation));
			}
			UpdateSnatchObjectButtonInteractableState();
		}
	}

	private void OnPlayerRemovedStoredTarget(IStoredTarget p_target)
	{
		if (isShowing && p_target is LocationStructure)
		{
			UpdateTargetLocationUI();
			if (_allValidDropLocations.Count > 0 && _snatchDropLocation == null)
			{
				_snatchDropLocation = _allValidDropLocations[0] as LocationStructure;
			}
			if (_snatchDropLocation != null)
			{
				m_snatchObjectUIView.UpdateTargetLocationDisplay(_snatchDropLocation, GetIndexOfTargetLocation(_snatchDropLocation));
			}
		}
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
			UpdateSnatchObjectButtonInteractableState();
		}
	}

	private void OnTileChosenForSpawning(LocationGridTile p_chosenTile)
	{
		if (_chosenLeader != null)
		{
			_chosenSpawnTile = p_chosenTile;
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Deploy_Party");
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("cost", GetTotalDeployCost() + Utilities.ManaIcon());
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Deploy_Party_Description", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnYesDeploy, null, showCover: true, 150);
		}
	}

	public void Show(IStoredTarget p_target)
	{
		_chosenTarget = p_target;
		ShowUI();
		m_snatchObjectUIView.UpdateTargetDisplay(_chosenTarget);
		m_snatchObjectUIView.UpdateUnlockedSummonSlots(GetUnlockedSummonSlots());
		UpdateTargetLocationUI();
		if (_allValidDropLocations.Count > 0)
		{
			_snatchDropLocation = _allValidDropLocations[0] as LocationStructure;
			m_snatchObjectUIView.UpdateTargetLocationDisplay(_snatchDropLocation, GetIndexOfTargetLocation(_snatchDropLocation));
		}
		UpdateSnatchObjectBtnName();
		UpdateAddPartySlotBtnState();
		UpdateAddPartySlotBtnInteractableState();
		UpdateSnatchObjectButtonInteractableState();
	}

	public override void HideUI()
	{
		base.HideUI();
		if (isShowing)
		{
			m_snatchObjectUIView.UIModel.rtWindow.anchoredPosition = m_snatchObjectUIView.UIModel.defaultPos;
			isShowing = false;
			if (GameManager.Instance.gameHasStarted)
			{
				PlayerUI.Instance.EnableTopMenuButtons();
				UIManager.Instance.ResumeLastProgressionSpeed();
			}
			m_snatchObjectUIView.HideObjectPicker();
			m_snatchObjectUIView.HideCharacterTooltip();
			m_snatchObjectUIView.UIModel.monsterToolTipUI.HideToolTip();
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
		for (int i = 0; i < m_snatchObjectUIView.UIModel.summonSlotItems.Length; i++)
		{
			m_snatchObjectUIView.UIModel.summonSlotItems[i].ClearMonsterUnderling();
		}
		_chosenLeader = null;
		m_snatchObjectUIView.UIModel.leaderSlotItem.ClearMonsterUnderling();
		UIManager.Instance.sidebarUIController.HideUI();
	}

	public void HideViaShortcutKey()
	{
		HideUI();
	}

	private void OnLeftClickLeaderItem(SnatchObjectSlotItem p_item)
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
		m_snatchObjectUIView.ShowObjectPicker(list, OnChooseLeaderItem, OnHoverOverObjectPickerItem, OnHoverOutObjectPickerItem, CanChooseLeader);
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

	private void OnRightClickLeaderItem(SnatchObjectSlotItem p_item)
	{
		if (_chosenLeader != null)
		{
			_chosenLeader = null;
			m_snatchObjectUIView.UIModel.leaderSlotItem.ClearMonsterUnderling();
			UpdateSnatchObjectBtnName();
			UpdateSnatchObjectButtonInteractableState();
		}
	}

	private void OnChooseLeaderItem(Character p_character)
	{
		if (_chosenLeader == p_character)
		{
			_chosenLeader = null;
			m_snatchObjectUIView.UIModel.leaderSlotItem.ClearMonsterUnderling();
		}
		else
		{
			_chosenLeader = p_character;
			m_snatchObjectUIView.UIModel.leaderSlotItem.SetCharacter(_chosenLeader);
		}
		m_snatchObjectUIView.HideCharacterTooltip();
		m_snatchObjectUIView.HideObjectPicker();
		m_snatchObjectUIView.UIModel.monsterToolTipUI.HideToolTip();
		UpdateSnatchObjectBtnName();
		UpdateSnatchObjectButtonInteractableState();
	}

	private void OnHoverOverLeaderItem(SnatchObjectSlotItem p_item)
	{
		if (p_item.data != null)
		{
			m_snatchObjectUIView.ShowCharacterTooltip(p_item.data);
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_item.data.characterClass.className);
			CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
			m_snatchObjectUIView.UIModel.monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.displayName, combatBehaviour.description);
		}
	}

	private void OnHoverOutLeaderItem(SnatchObjectSlotItem p_item)
	{
		m_snatchObjectUIView.HideCharacterTooltip();
		m_snatchObjectUIView.UIModel.monsterToolTipUI.HideToolTip();
	}

	private void OnClickLeaderItem(SnatchObjectSummonItem p_item)
	{
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
		bool unlockSlotBtnState = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForSnatchObject() < GetMaxSummonCount();
		m_snatchObjectUIView.SetUnlockSlotBtnState(unlockSlotBtnState);
	}

	private void UpdateAddPartySlotBtnInteractableState()
	{
		int summonCountForSnatchObject = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForSnatchObject();
		bool unlockSlotBtnInteractable = PlayerManager.Instance.player.currenciesComponent.chaoticEnergy >= GetPartySlotUnlockCost(summonCountForSnatchObject);
		m_snatchObjectUIView.SetUnlockSlotBtnInteractable(unlockSlotBtnInteractable);
	}

	private int GetMaxSummonCount()
	{
		return 5;
	}

	private int GetUnlockedSummonSlots()
	{
		return PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForSnatchObject() - 1;
	}

	private void OnLeftClickSummonItem(SnatchObjectSlotItem p_item)
	{
		_currentlyClickedSnatchObjectSlotItem = p_item;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++)
		{
			Character character = PlayerManager.Instance.player.playerFaction.characters[i];
			if (!character.isDead && character.minion == null)
			{
				list.Add(character);
			}
		}
		m_snatchObjectUIView.ShowObjectPicker(list, OnChooseSummonItem, OnHoverOverObjectPickerItem, OnHoverOutObjectPickerItem, CanChooseSummon);
		RuinarchListPool<Character>.Release(list);
	}

	private void SelectNextUnlockedSummonSlot()
	{
		SnatchObjectSlotItem snatchObjectSlotItem = null;
		snatchObjectSlotItem = ((!(_currentlyClickedSnatchObjectSlotItem == null)) ? CollectionUtilities.GetNextElementCyclic(m_snatchObjectUIView.unlockedSummonSlots, _currentlyClickedSnatchObjectSlotItem) : m_snatchObjectUIView.unlockedSummonSlots.First());
		_currentlyClickedSnatchObjectSlotItem = snatchObjectSlotItem;
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
	}

	private void OnChooseSummonItem(Character p_character)
	{
		Debug.Log("Choose " + p_character.name);
		if (_currentlyClickedSnatchObjectSlotItem.data != null)
		{
			_chosenSummons.Remove(_currentlyClickedSnatchObjectSlotItem.data);
		}
		_currentlyClickedSnatchObjectSlotItem.SetCharacter(p_character);
		_chosenSummons.Add(p_character);
		UpdateSnatchObjectBtnName();
		UpdateSnatchObjectButtonInteractableState();
		UpdateSummonObjectPickerItems();
		SelectNextUnlockedSummonSlot();
	}

	private void OnRightClickSummonSlot(SnatchObjectSlotItem p_item)
	{
		Character data = p_item.data;
		if (data != null && _chosenSummons.Remove(data))
		{
			p_item.ClearMonsterUnderling();
			UpdateSnatchObjectBtnName();
			UpdateSnatchObjectButtonInteractableState();
			UpdateSummonObjectPickerItems();
		}
	}

	private void OnHoverOutSummonSlot(SnatchObjectSlotItem p_item)
	{
		m_snatchObjectUIView.HideCharacterTooltip();
		m_snatchObjectUIView.UIModel.monsterToolTipUI.HideToolTip();
	}

	private void OnHoverOverSummonSlot(SnatchObjectSlotItem p_item)
	{
		if (p_item.data != null)
		{
			m_snatchObjectUIView.ShowCharacterTooltip(p_item.data);
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_item.data.characterClass.className);
			CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
			m_snatchObjectUIView.UIModel.monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.displayName, combatBehaviour.description);
		}
	}

	private void UpdateTargetLocationUI()
	{
		m_snatchObjectUIView.SetTargetLocationState(p_state: true);
		m_snatchObjectUIView.SetTargetLocationInteractableState(p_state: true);
		ConstructDropLocationChoices();
	}

	private bool IsStructureValidForTarget(LocationStructure p_structure, IStoredTarget p_target)
	{
		if (p_target == null || p_target is Character)
		{
			return true;
		}
		if (p_target is TileObject tileObject)
		{
			if (p_structure is DemonicStructure)
			{
				return false;
			}
			if (!p_structure.HasUnoccupiedTile() || tileObject.gridTileLocation.structure == p_structure)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	private void ConstructDropLocationChoices()
	{
		_allValidDropLocations.Clear();
		List<IStoredTarget> list = RuinarchListPool<IStoredTarget>.Claim();
		list.AddRange(PlayerManager.Instance.player.storedTargetsComponent.storedStructures);
		List<TMP_Dropdown.OptionData> list2 = RuinarchListPool<TMP_Dropdown.OptionData>.Claim();
		for (int i = 0; i < list.Count; i++)
		{
			IStoredTarget storedTarget = list[i];
			if (storedTarget is LocationStructure p_structure && IsStructureValidForTarget(p_structure, _chosenTarget))
			{
				TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData(storedTarget.bookmarkName, storedTarget.GetPortraitSprite());
				list2.Add(item);
				_allValidDropLocations.Add(storedTarget);
			}
		}
		m_snatchObjectUIView.SetTargetLocationDropdownOptions(list2);
		RuinarchListPool<TMP_Dropdown.OptionData>.Release(list2);
		RuinarchListPool<IStoredTarget>.Release(list);
	}

	private int GetIndexOfTargetLocation(LocationStructure p_target)
	{
		if (p_target == null)
		{
			return -1;
		}
		for (int i = 0; i < _allValidDropLocations.Count; i++)
		{
			if (_allValidDropLocations[i] == p_target)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetTotalDeployCost()
	{
		int num = 0;
		if (_chosenLeader != null)
		{
			num += 50;
		}
		return num + _chosenSummons.Count * 50;
	}

	public void OnClickAddPartySlot()
	{
		int summonCountForSnatchObject = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForSnatchObject();
		int partySlotUnlockCost = GetPartySlotUnlockCost(summonCountForSnatchObject);
		AudioManager.Instance.TryPlayUISFX("Play_Party_UI_Unlock_Slot");
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-partySlotUnlockCost);
		PlayerManager.Instance.player.partyStructureDataHandler.AddSummonCountForSnatchObject();
		m_snatchObjectUIView.UpdateUnlockedSummonSlots(GetUnlockedSummonSlots());
		UpdateAddPartySlotBtnState();
	}

	public void OnHoverOverAddPartySlot()
	{
		int summonCountForSnatchObject = PlayerManager.Instance.player.partyStructureDataHandler.GetSummonCountForSnatchObject();
		UIManager.Instance.ShowSmallInfo($"{_strUnlockSlot} - {GetPartySlotUnlockCost(summonCountForSnatchObject)}{Utilities.ChaoticEnergyIcon()}");
	}

	public void OnHoverOutAddPartySlot()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnClickSnatchObject()
	{
		(PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_PARTY) as SpawnPartyData).ActivateFromSnatchObject();
		HideUI();
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void OnTargetLocationDropdownValueChanged(int p_value)
	{
		_snatchDropLocation = _allValidDropLocations[p_value] as LocationStructure;
	}

	public void OnClickCloseObjectPicker()
	{
		m_snatchObjectUIView.HideObjectPicker();
	}

	private void UpdateSnatchObjectBtnName()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Deploy_Party");
		m_snatchObjectUIView.SetSnatchObjectBtnLabelName(localizedValue + " - " + GetTotalDeployCost() + Utilities.ManaIcon());
	}

	private void UpdateSnatchObjectButtonInteractableState()
	{
		bool snatchObjectInteractableState = true;
		if (_chosenLeader == null || _chosenTarget == null || _snatchDropLocation == null)
		{
			snatchObjectInteractableState = false;
		}
		else if (!PlayerManager.Instance.player.currenciesComponent.CanAfford(CURRENCY.Mana, GetTotalDeployCost()))
		{
			snatchObjectInteractableState = false;
		}
		m_snatchObjectUIView.SetSnatchObjectInteractableState(snatchObjectInteractableState);
	}

	private void OnYesDeploy()
	{
		Deploy();
	}

	private void Deploy()
	{
		TileObject tileObject = _chosenTarget as TileObject;
		Character chosenLeader = _chosenLeader;
		chosenLeader.SetDestroyMarkerOnDeath(state: true);
		PlayerManager.Instance.player.underlingsComponent.persistentDefendParty?.RemoveMember(chosenLeader);
		Party party = tileObject.partyComponent.CreateSnatchObjectParty(chosenLeader);
		CharacterManager.Instance.Teleport(chosenLeader, _chosenSpawnTile);
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
			summon.SetDestroyMarkerOnDeath(state: true);
			PlayerManager.Instance.player.underlingsComponent.persistentDefendParty?.RemoveMember(summon);
			party.AddMember(summon);
			summon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		tileObject.partyComponent.DeployParty(party, tileObject, _snatchDropLocation);
		PlayerManager.Instance.player.currenciesComponent.AdjustMana(-GetTotalDeployCost());
		PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SNATCH_OBJECT).OnExecutePlayerSkill();
		OnClickClose();
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		AudioManager.Instance.TryPlayUISFX("Play_Deploy_Party");
	}

	private void OnHoverOverObjectPickerItem(Character p_data)
	{
		m_snatchObjectUIView.ShowCharacterTooltip(p_data);
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_data.characterClass.className);
		CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(characterClass.combatBehaviourType);
		m_snatchObjectUIView.UIModel.monsterToolTipUI.DisplayToolTipWithoutCharge(combatBehaviour.displayName, combatBehaviour.description);
	}

	private void OnHoverOutObjectPickerItem(Character p_data)
	{
		m_snatchObjectUIView.HideCharacterTooltip();
		m_snatchObjectUIView.UIModel.monsterToolTipUI.HideToolTip();
	}
}
