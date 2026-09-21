using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Ruinarch;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class StructureInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Tabs")]
	[SerializeField]
	private RuinarchToggle prisonersTab;

	[SerializeField]
	private RuinarchToggle residentsTab;

	[SerializeField]
	private RuinarchToggle workersTab;

	[Space(10f)]
	[Header("Content")]
	[SerializeField]
	private GameObject goPrisoners;

	[SerializeField]
	private GameObject goResidents;

	[Space(10f)]
	[Header("Basic Info")]
	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private TextMeshProUGUI extraInfo1Title;

	[SerializeField]
	private TextMeshProUGUI extraInfo1Description;

	[SerializeField]
	private HoverHandler hoverHandlerExtraInfo1Description;

	[SerializeField]
	private TextMeshProUGUI extraInfo2Title;

	[SerializeField]
	private TextMeshProUGUI extraInfo2Description;

	[SerializeField]
	private LocationPortrait locationPortrait;

	[Space(10f)]
	[Header("Info")]
	[SerializeField]
	private TextMeshProUGUI hpLbl;

	[SerializeField]
	private TextMeshProUGUI villageLbl;

	[SerializeField]
	private TextMeshProUGUI descriptionLbl;

	[SerializeField]
	private EventLabel villageEventLbl;

	[SerializeField]
	private GameObject villageParentGO;

	[Header("City Center Info")]
	[SerializeField]
	private Image migrationMeterImg;

	[SerializeField]
	private GameObject migrationMeterGO;

	[SerializeField]
	private GameObject cityCenterDetails;

	[SerializeField]
	private TextMeshProUGUI cityCenterDescriptionLbl;

	[Header("Monster Charges")]
	[SerializeField]
	private GameObject monsterChargesGO;

	[SerializeField]
	private GameObject monsterChargesCooldownMeterGO;

	[SerializeField]
	private Image monsterChargesCooldownMeterImg;

	[SerializeField]
	private TextMeshProUGUI currentMonsterChargesLbl;

	[SerializeField]
	private TextMeshProUGUI nextMonsterChargesLbl;

	[Space(10f)]
	[Header("Characters")]
	[SerializeField]
	private GameObject characterItemPrefab;

	[SerializeField]
	private ScrollRect charactersScrollView;

	[SerializeField]
	private ScrollRect prisonersScrollView;

	[SerializeField]
	private ScrollRect workersScrollView;

	[SerializeField]
	private UIHoverPosition characterNameplateHoverPosition;

	[Space(10f)]
	[Header("Eyes")]
	[SerializeField]
	private RuinarchToggle eyesTab;

	[SerializeField]
	private GameObject tileObjectNameplatePrefab;

	[SerializeField]
	private Transform eyesParentTransform;

	[Space(10f)]
	[Header("Store Target")]
	[SerializeField]
	private StoreTargetButton btnStoreTarget;

	public LocationStructure activeStructure { get; private set; }

	internal override void Initialize()
	{
		base.Initialize();
		Messenger.AddListener<Character, LocationStructure>(StructureSignals.ADDED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
		Messenger.AddListener<Character, LocationStructure>(StructureSignals.REMOVED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, UpdatePrisonersFromSignal);
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, UpdatePrisonersFromSignal);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Watcher>(StructureSignals.UPDATE_EYE_WARDS, UpdateEyeWardsFromSignal);
		Messenger.AddListener<DemonicStructure>(StructureSignals.DEMONIC_STRUCTURE_REPAIRED, OnDemonicStructureRepaired);
		Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_HP_CHANGED, OnStructureHPChanged);
		Messenger.AddListener<Character, ManMadeStructure>(StructureSignals.ON_WORKER_HIRED, UpdateWorkersFromSignal);
		Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
		Messenger.AddListener<Character>(PlayerSignals.PLAYER_PLACED_DEFENDER, OnPlayerPlacedDefender);
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_NAME_UPDATED, OnStructureNameUpdated);
		Messenger.AddListener<LocationStructure>(UISignals.UPDATE_STRUCTURE_EXTRA_INFO, UpdateStructureExtraInfoFromSignal);
		ListenToPlayerActionSignals();
		villageEventLbl.SetOnLeftClickAction(OnLeftClickVillage);
		villageEventLbl.SetOnRightClickAction(OnRightClickVillage);
		hoverHandlerExtraInfo1Description.AddOnHoverOverAction(OnHoverOverExtraInfo1Description);
		hoverHandlerExtraInfo1Description.AddOnHoverOutAction(OnHoverExitExtraInfo1Description);
	}

	private void UpdateStructureExtraInfoFromSignal(LocationStructure p_structure)
	{
		if (isShowing && p_structure == activeStructure)
		{
			UpdateExtraInfo(activeStructure);
		}
	}

	private void OnStructureNameUpdated(LocationStructure p_structure)
	{
		if (isShowing && activeStructure == p_structure)
		{
			UpdateBasicInfo();
		}
	}

	private void OnPlayerPlacedDefender(Character p_character)
	{
		if (isShowing && activeStructure is ThePortal)
		{
			UpdateExtraInfo(activeStructure);
		}
	}

	private void OnPlayerFinishedPortalUpgrade(int p_level)
	{
		if (isShowing && activeStructure is ThePortal)
		{
			UpdateExtraInfo(activeStructure);
		}
	}

	private void OnStructureHPChanged(LocationStructure p_structure)
	{
		if (activeStructure == p_structure)
		{
			UpdateInfo();
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (isShowing && p_character.currentStructure == activeStructure)
		{
			UpdatePrisonersFromSignal(p_character, p_character.currentStructure);
		}
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		if (activeStructure != null)
		{
			Selector.Instance.Deselect();
			GameObject gameObject = null;
			if (activeStructure is ManMadeStructure manMadeStructure)
			{
				gameObject = manMadeStructure.structureObj.gameObject;
			}
			else if (activeStructure is DemonicStructure demonicStructure)
			{
				gameObject = demonicStructure.structureObj.gameObject;
			}
			if (gameObject != null && InnerMapCameraMove.Instance.target == gameObject.transform)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
			}
			activeStructure.ProcessOnSetAsInactiveInStructureInfo();
		}
		activeStructure = null;
	}

	public override void OpenMenu()
	{
		LocationStructure locationStructure = activeStructure;
		activeStructure = _data as LocationStructure;
		base.OpenMenu();
		if (locationStructure != null)
		{
			if (Selector.Instance.IsSelected(locationStructure))
			{
				Selector.Instance.Deselect();
			}
			locationStructure.ProcessOnSetAsInactiveInStructureInfo();
		}
		activeStructure.ShowSelectorOnStructure();
		activeStructure.ProcessOnSetAsActiveInStructureInfo();
		btnStoreTarget.SetTarget(activeStructure);
		UpdateStructureInfoUI();
		UpdateContentToShow();
		if (UsesWorkersTab())
		{
			UpdateResidents();
			UpdateWorkers();
		}
		else if (UsesResidentsTab())
		{
			UpdateResidents();
		}
		else if (UsesPrisonersTab())
		{
			UpdatePrisoners();
		}
		else if (UsesEyesTab())
		{
			UpdateEyes();
		}
		LoadActions(activeStructure);
		StructureData structureData = LandmarkManager.Instance.GetStructureData(activeStructure.structureType);
		if (structureData.uiSFX.IsValid())
		{
			structureData.uiSFX.Post(InnerMapCameraMove.Instance.gameObject);
		}
	}

	protected override bool ShouldCreateActionItem(SkillData p_skill, IPlayerActionTarget p_target)
	{
		if (base.ShouldCreateActionItem(p_skill, p_target) && p_skill.type != PLAYER_SKILL_TYPE.SCHEME)
		{
			return p_skill.type != PLAYER_SKILL_TYPE.RAID;
		}
		return false;
	}

	private bool UsesResidentsTab()
	{
		if (activeStructure is DemonicStructure demonicStructure)
		{
			if (demonicStructure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS)
			{
				_ = demonicStructure.structureType;
				_ = 30;
			}
			return false;
		}
		return true;
	}

	private bool UsesPrisonersTab()
	{
		if (activeStructure is DemonicStructure demonicStructure)
		{
			if (demonicStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS || demonicStructure.structureType == STRUCTURE_TYPE.KENNEL)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool UsesEyesTab()
	{
		if (activeStructure is Watcher)
		{
			return true;
		}
		return false;
	}

	private bool UsesWorkersTab()
	{
		if (activeStructure.structureType.IsJobStructure())
		{
			return true;
		}
		return false;
	}

	private void UpdateContentToShow()
	{
		if (UsesWorkersTab())
		{
			prisonersTab.isOn = false;
			eyesTab.isOn = false;
		}
		else if (UsesResidentsTab())
		{
			prisonersTab.isOn = false;
			eyesTab.isOn = false;
			workersTab.isOn = false;
		}
		else if (UsesPrisonersTab())
		{
			residentsTab.isOn = false;
			eyesTab.isOn = false;
			workersTab.isOn = false;
		}
		else if (UsesEyesTab())
		{
			residentsTab.isOn = false;
			prisonersTab.isOn = false;
			workersTab.isOn = false;
		}
		else
		{
			prisonersTab.isOn = false;
			residentsTab.isOn = false;
			eyesTab.isOn = false;
			workersTab.isOn = false;
		}
	}

	private void UpdateTabs()
	{
		residentsTab.gameObject.SetActive(value: false);
		prisonersTab.gameObject.SetActive(value: false);
		eyesTab.gameObject.SetActive(value: false);
		workersTab.gameObject.SetActive(value: false);
		if (UsesWorkersTab())
		{
			workersTab.gameObject.SetActive(value: true);
			residentsTab.gameObject.SetActive(value: true);
		}
		else if (UsesResidentsTab())
		{
			residentsTab.gameObject.SetActive(value: true);
		}
		else if (UsesPrisonersTab())
		{
			prisonersTab.gameObject.SetActive(value: true);
		}
		else if (UsesEyesTab())
		{
			eyesTab.gameObject.SetActive(value: true);
		}
	}

	public void UpdateStructureInfoUI()
	{
		if (activeStructure != null)
		{
			UpdateTabs();
			UpdateBasicInfo();
			UpdateInfo();
		}
	}

	private void UpdateBasicInfo()
	{
		nameLbl.text = activeStructure.nameplateName ?? "";
		if (activeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER)
		{
			locationPortrait.SetLocation(activeStructure.settlementLocation);
		}
		else
		{
			locationPortrait.ClearLocations();
		}
		locationPortrait.SetPortrait(activeStructure.structureType);
		UpdateExtraInfo(activeStructure);
	}

	private void UpdateExtraInfo(LocationStructure p_locationStructure)
	{
		extraInfo1Title.text = p_locationStructure.extraInfo1Header;
		extraInfo2Title.text = p_locationStructure.extraInfo2Header;
		extraInfo1Description.text = p_locationStructure.extraInfo1Description;
		extraInfo2Description.text = p_locationStructure.extraInfo2Description;
	}

	private void SetDescription(string p_text)
	{
		if (activeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && activeStructure.settlementLocation != null)
		{
			if (activeStructure.settlementLocation is NPCSettlement)
			{
				descriptionLbl.gameObject.SetActive(value: false);
				cityCenterDetails.SetActive(value: true);
				cityCenterDescriptionLbl.text = p_text;
			}
			else
			{
				cityCenterDetails.SetActive(value: false);
				descriptionLbl.gameObject.SetActive(value: true);
				descriptionLbl.text = p_text;
			}
		}
		else
		{
			cityCenterDetails.SetActive(value: false);
			descriptionLbl.gameObject.SetActive(value: true);
			descriptionLbl.text = p_text;
		}
	}

	private void UpdateInfo()
	{
		hpLbl.text = $"{activeStructure.currentHP}/{activeStructure.maxHP}";
		SetDescription(activeStructure.customDescription);
		if (activeStructure.settlementLocation != null && activeStructure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE)
		{
			villageLbl.text = "<link=\"village\">" + Utilities.ColorizeAndBoldName(activeStructure.settlementLocation.name) + "</link>";
			villageParentGO.SetActive(value: true);
		}
		else
		{
			villageParentGO.SetActive(value: false);
		}
		UpdateInfoIfCityCenter();
		UpdateInfoForMonsterProducingDemonicStructure();
	}

	private void UpdateInfoIfCityCenter()
	{
		if (activeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && activeStructure.settlementLocation != null)
		{
			if (activeStructure.settlementLocation is NPCSettlement nPCSettlement)
			{
				cityCenterDetails.SetActive(value: true);
				migrationMeterImg.fillAmount = nPCSettlement.migrationComponent.GetNormalizedMigrationMeterValue();
				migrationMeterGO.SetActive(WorldSettings.Instance.worldSettingsData.victoryCondition != VICTORY_CONDITION.Eradication);
			}
			else
			{
				cityCenterDetails.SetActive(value: false);
				migrationMeterGO.SetActive(value: false);
			}
		}
		else
		{
			cityCenterDetails.SetActive(value: false);
			migrationMeterGO.SetActive(value: false);
		}
	}

	private void UpdateInfoForMonsterProducingDemonicStructure(DemonicStructure p_demonicStructure = null)
	{
		if (p_demonicStructure == null)
		{
			p_demonicStructure = activeStructure as DemonicStructure;
		}
		if (p_demonicStructure != null)
		{
			SUMMON_TYPE housedMonsterType = p_demonicStructure.housedMonsterType;
			if (housedMonsterType != SUMMON_TYPE.None && PlayerManager.Instance.player.underlingsComponent.HasMonsterUnderlingEntry(housedMonsterType))
			{
				MonsterAndDemonUnderlingCharges summonUnderlingChargesBySummonType = PlayerManager.Instance.player.underlingsComponent.GetSummonUnderlingChargesBySummonType(housedMonsterType);
				if (summonUnderlingChargesBySummonType != null)
				{
					int num = (summonUnderlingChargesBySummonType.isDemon ? PlayerManager.Instance.player.GetNumberOfAliveMonstersInPlayerFaction(summonUnderlingChargesBySummonType.minionType) : PlayerManager.Instance.player.GetNumberOfAliveMonstersInPlayerFaction(summonUnderlingChargesBySummonType.monsterType));
					currentMonsterChargesLbl.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Current_Charges") + " " + Utilities.ColorizeAndBoldName($"{num}/{summonUnderlingChargesBySummonType.maxCharges}");
					Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Next_Monster_Charge");
					log.AddToFillers(null, Utilities.ColorizeAndBoldName(housedMonsterType.LocalizedName()), LOG_IDENTIFIER.STRING_1);
					log.AddToFillers(null, Utilities.ColorizeName(summonUnderlingChargesBySummonType.replenishDate.ConvertToContinuousDaysWithTime(nextLineTime: false, capitalizedDay: true)), LOG_IDENTIFIER.STRING_2);
					nextMonsterChargesLbl.text = log.logText;
					LogPool.Release(log);
					monsterChargesCooldownMeterImg.fillAmount = (float)summonUnderlingChargesBySummonType.currentCooldownTick / (float)summonUnderlingChargesBySummonType.cooldown;
					currentMonsterChargesLbl.gameObject.SetActive(value: true);
					monsterChargesCooldownMeterGO.SetActive(value: true);
					monsterChargesGO.SetActive(value: true);
					return;
				}
			}
		}
		else if (activeStructure is LichGraveyard lichGraveyard)
		{
			currentMonsterChargesLbl.gameObject.SetActive(value: false);
			monsterChargesCooldownMeterGO.SetActive(value: false);
			nextMonsterChargesLbl.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "New_Monsters_Spawn") + " " + Utilities.ColorizeName(lichGraveyard.spawnDueDate.ToString()) + ".";
			monsterChargesGO.SetActive(value: true);
			return;
		}
		monsterChargesGO.SetActive(value: false);
	}

	private void UpdateResidents()
	{
		Utilities.DestroyChildren(charactersScrollView.content);
		List<Character> residents = activeStructure.residents;
		if (residents == null || residents.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character != null)
			{
				CharacterPortrait component = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content).GetComponent<CharacterPortrait>();
				component.GeneratePortrait(character);
				component.SetNameState(p_state: true);
				component.SetHoverActions(OnHoverOverCharacterPortrait, OnHoverOutCharacterPortrait);
			}
		}
	}

	private void UpdatePrisoners()
	{
		Utilities.DestroyChildren(prisonersScrollView.content);
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (activeStructure is Kennel { rooms: not null } kennel && kennel.rooms.Length != 0 && kennel.rooms[0] is KennelCell kennelCell)
		{
			kennelCell.PopulateOccupants(list);
		}
		else if (activeStructure is TortureChambers { rooms: not null } tortureChambers && tortureChambers.rooms.Length != 0 && tortureChambers.rooms[0] is PrisonCell prisonCell)
		{
			prisonCell.PopulateOccupants(list);
		}
		else
		{
			list.AddRange(activeStructure.charactersHere);
		}
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Character character = list[i];
				if (character != null)
				{
					CharacterPortrait component = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, prisonersScrollView.content).GetComponent<CharacterPortrait>();
					component.GeneratePortrait(character);
					component.SetNameState(p_state: true);
					component.SetHoverActions(OnHoverOverCharacterPortrait, OnHoverOutCharacterPortrait);
				}
			}
		}
		RuinarchListPool<Character>.Release(list);
	}

	private void UpdateWorkers()
	{
		Utilities.DestroyChildren(workersScrollView.content);
		ManMadeStructure manMadeStructure = activeStructure as ManMadeStructure;
		for (int i = 0; i < manMadeStructure.assignedWorkerIDs.Count; i++)
		{
			string id = manMadeStructure.assignedWorkerIDs[i];
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id);
			if (characterByPersistentID != null)
			{
				CharacterPortrait component = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, workersScrollView.content).GetComponent<CharacterPortrait>();
				component.GeneratePortrait(characterByPersistentID);
				component.SetNameState(p_state: true);
				component.SetHoverActions(OnHoverOverCharacterPortrait, OnHoverOutCharacterPortrait);
			}
		}
	}

	private void UpdateEyes()
	{
		Utilities.DestroyChildren(eyesParentTransform);
		Watcher watcher = activeStructure as Watcher;
		for (int i = 0; i < watcher.eyeWards.Count; i++)
		{
			DemonEye demonEye = watcher.eyeWards[i];
			TileObjectNameplateItem component = UIManager.Instance.InstantiateUIObject(tileObjectNameplatePrefab.name, eyesParentTransform).GetComponent<TileObjectNameplateItem>();
			component.SetObject(demonEye);
			component.SetAsButton();
			component.AddOnClickAction(OnClickEye);
		}
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (GameManager.Instance.gameHasStarted && p_action == SHORTCUT_ACTION.Center_Portal && PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) is ThePortal data)
		{
			SetData(data);
			OpenMenu();
			activeStructure.CenterOnStructure();
		}
	}

	private void OnReceiveKeyCodeSignal(KeyCode p_key)
	{
		if (p_key == KeyCode.Mouse1)
		{
			CloseMenu();
		}
	}

	private void OnHoverOverCharacterPortrait(CharacterPortrait p_portrait)
	{
		UIManager.Instance.ShowCharacterNameplateTooltip(p_portrait.character, characterNameplateHoverPosition);
	}

	private void OnHoverOutCharacterPortrait(CharacterPortrait p_portrait)
	{
		UIManager.Instance.HideCharacterNameplateTooltip();
	}

	private void UpdateResidentsFromSignal(Character resident, LocationStructure structure)
	{
		if (isShowing && activeStructure == structure && UsesResidentsTab())
		{
			UpdateResidents();
		}
	}

	private void UpdateWorkersFromSignal(Character resident, LocationStructure structure)
	{
		if (isShowing && activeStructure == structure && UsesWorkersTab())
		{
			UpdateWorkers();
		}
	}

	private void UpdatePrisonersFromSignal(Character character, LocationStructure structure)
	{
		if (isShowing && activeStructure == structure && UsesPrisonersTab())
		{
			UpdatePrisoners();
		}
	}

	private void UpdateEyeWardsFromSignal(Watcher structure)
	{
		if (isShowing && activeStructure == structure && UsesEyesTab())
		{
			UpdateEyes();
			UpdateExtraInfo(structure);
		}
	}

	private void OnDemonicStructureRepaired(DemonicStructure p_demonicStructure)
	{
		if (isShowing && activeStructure == p_demonicStructure)
		{
			UpdateInfo();
		}
	}

	private void OnLeftClickVillage(object obj)
	{
		if (activeStructure.settlementLocation != null && activeStructure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE)
		{
			UIManager.Instance.ShowSettlementInfo(activeStructure.settlementLocation);
		}
	}

	private void OnRightClickVillage(object obj)
	{
		if (activeStructure.settlementLocation != null && activeStructure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE)
		{
			UIManager.Instance.ShowPlayerActionContextMenu(activeStructure.settlementLocation, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	public void OnClickItem()
	{
		activeStructure.CenterOnStructure();
	}

	public void OnClickEye(TileObject obj)
	{
		if (obj != null)
		{
			Selector.Instance.Select(obj);
			if (obj.worldObject == null && obj.isBeingCarriedBy != null)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(obj.isBeingCarriedBy.worldObject.gameObject);
			}
			else if (obj.worldObject != null && obj.isBeingCarriedBy == null)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(obj.worldObject.gameObject);
			}
			else if (obj.worldObject != null && obj.isBeingCarriedBy != null)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(obj.worldObject.gameObject);
			}
		}
	}

	public void OnHoverEnterMigrationMeter()
	{
		if (activeStructure.settlementLocation != null && activeStructure.settlementLocation is NPCSettlement nPCSettlement)
		{
			string hoverTextOfMigrationMeter = nPCSettlement.migrationComponent.GetHoverTextOfMigrationMeter();
			if (!string.IsNullOrEmpty(hoverTextOfMigrationMeter))
			{
				UIManager.Instance.ShowSmallInfo(hoverTextOfMigrationMeter);
			}
		}
	}

	public void OnHoverExitMigrationMeter()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void ShowStructureTestingInfo()
	{
	}

	public void HideStructureTestingInfo()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverExtraInfo1Description()
	{
		if (activeStructure.partyStructureComponent?.availableBehaviours != null && activeStructure.partyStructureComponent.availableBehaviours.Length != 0)
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_type = activeStructure.partyStructureComponent.availableBehaviours[0];
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", p_type.ToStringEnumWithSpace() + "_Description");
			UIManager.Instance.ShowSmallInfo(localizedValue);
		}
	}

	private void OnHoverExitExtraInfo1Description()
	{
		UIManager.Instance.HideSmallInfo();
	}
}
