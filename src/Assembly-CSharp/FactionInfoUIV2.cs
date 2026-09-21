using System;
using System.Collections.Generic;
using Crime_System;
using Locations.Settlements;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UtilityScripts;

public class FactionInfoUIV2 : MonoBehaviour
{
	private enum VIEW_MODE
	{
		Overview,
		Members,
		Crimes,
		Relations,
		Logs
	}

	[Space(10f)]
	[Header("Overview")]
	[SerializeField]
	private CharacterNameplateItem leaderNameplateItem;

	[SerializeField]
	private GameObject noLeaderTextGO;

	[SerializeField]
	private TextMeshProUGUI ideologyLbl;

	[SerializeField]
	private CharacterPortrait[] successorPortraits;

	[Space(10f)]
	[Header("Locations")]
	[SerializeField]
	private Transform locationsTransform;

	[SerializeField]
	private GameObject settlementNameplatePrefab;

	private List<SettlementNameplateItem> locationItems;

	[Space(10f)]
	[Header("Relationships")]
	[SerializeField]
	private ScrollRect relationshipsScrollRect;

	[SerializeField]
	private GameObject relationshipPrefab;

	[Space(10f)]
	[Header("Crimes")]
	[SerializeField]
	private TextMeshProUGUI infractionCrimesLbl;

	[SerializeField]
	private TextMeshProUGUI misdemeanourCrimesLbl;

	[SerializeField]
	private TextMeshProUGUI seriousCrimesLbl;

	[SerializeField]
	private TextMeshProUGUI heinousCrimesLbl;

	[SerializeField]
	private ScrollRect crimesScrollRect;

	[SerializeField]
	private RectTransform crimesScrollRectTransform;

	[Space(10f)]
	[Header("Logs")]
	[SerializeField]
	private LogsWindow logsWindow;

	[Space(10f)]
	[Header("Characters")]
	[SerializeField]
	private GameObject characterItemPrefab;

	[SerializeField]
	private ScrollRect charactersScrollView;

	[SerializeField]
	private RuinarchToggle aliveToggle;

	[SerializeField]
	private GameObject traitFilterItemPrefab;

	[FormerlySerializedAs("regionFilterItemPrefab")]
	[SerializeField]
	private GameObject villageFilterItemPrefab;

	[SerializeField]
	private ScrollRect traitFilterScrollRect;

	[FormerlySerializedAs("regionFilterScrollRect")]
	[SerializeField]
	private ScrollRect villageFilterScrollRect;

	[SerializeField]
	private TMP_InputField searchTraitFilterField;

	[FormerlySerializedAs("searchRegionFilterField")]
	[SerializeField]
	private TMP_InputField searchVillageFilterField;

	[SerializeField]
	private RuinarchToggle selectAllTraitsToggle;

	[SerializeField]
	private RuinarchToggle selectAllVillagesToggle;

	private List<CharacterNameplateItem> _characterItems;

	private List<FactionTraitFilterItem> _traitFilterItems;

	private List<FactionVillageFilterItem> _villageFilterItems;

	private List<string> filteredTraits;

	private List<Region> filteredRegions;

	private List<BaseSettlement> filteredVillages;

	public GameObject overviewScrollRectParent;

	public GameObject membersScrollRectParent;

	public GameObject logScrollRectParent;

	public GameObject crimesScrollRectParent;

	public GameObject relationshipScrollRectParent;

	public GameObject btnRevealInfo;

	private VIEW_MODE m_viewMode = VIEW_MODE.Members;

	public Faction activeFaction { get; private set; }

	public void Initialize()
	{
		_characterItems = new List<CharacterNameplateItem>();
		locationItems = new List<SettlementNameplateItem>();
		_traitFilterItems = new List<FactionTraitFilterItem>();
		_villageFilterItems = new List<FactionVillageFilterItem>();
		filteredTraits = new List<string>();
		filteredVillages = new List<BaseSettlement>();
		searchTraitFilterField.onValueChanged.AddListener(OnSearchTraitFilterValueChanged);
		searchVillageFilterField.onValueChanged.AddListener(OnSearchRegionFilterValueChanged);
		selectAllTraitsToggle.onValueChanged.AddListener(OnToggleSelectAllTraits);
		selectAllVillagesToggle.onValueChanged.AddListener(OnToggleSelectAllVillages);
		Messenger.AddListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnPlaguePointsAdjusted);
		PopulateFilterTraits();
		ClearFilteredTraits();
		ClearFilteredVillages();
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
		Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_ADDED, OnFactionSettlementAdded);
		Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_REMOVED, OnFactionSettlementRemoved);
		Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionRelationshipChanged);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
		Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnFactionLeaderChanged);
		Messenger.AddListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateHistory);
		Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateHistory);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_IDEOLOGIES_CHANGED, OnFactionIdeologiesChanged);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_CRIMES_CHANGED, OnFactionCrimesChanged);
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
		Messenger.AddListener<Faction>(FactionSignals.UPDATED_SUCCESSORS, OnFactionUpdatedSuccessors);
		Messenger.AddListener<RELIGION>(CharacterSignals.ACTIVE_RELIGIOUS_CULTISTS_UPDATED, OnActiveReligiousCultistsUpdated);
		logsWindow.Initialize();
		SetButtonRevealPriceDisplay();
		UpdateSelectAllTraitFiltersToggle();
		UpdateSelectAllVillagesToggle();
	}

	private void SetButtonRevealPriceDisplay()
	{
		btnRevealInfo.transform.Find("icon").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealFactionInfoCost().ToString();
	}

	public void SetFaction(Faction faction)
	{
		activeFaction = faction;
		if (activeFaction != null)
		{
			UpdateOverview();
			UpdateOwnedLocations();
			UpdateCrimes();
			UpdateAllRelationships();
			logsWindow.OnParentMenuOpened(activeFaction.persistentID);
			UpdateAllHistoryInfo();
			UpdateAllCharacters();
			ResetScrollPositions();
			ProcessDisplay();
			PopulateFilterVillages();
			InitializeRevealHoverText();
		}
	}

	private void OnCharacterDied(Character character)
	{
		if (activeFaction != null && character.faction == activeFaction)
		{
			FilterCharacters();
		}
	}

	private void OnFactionLeaderChanged(Character character, ILeader previousLeader)
	{
		if (activeFaction != null && character.faction == activeFaction)
		{
			UpdateOverview();
			UpdateAllCharacters();
		}
	}

	private void OnFactionIdeologiesChanged(Faction faction)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction))
		{
			UpdateOverview();
		}
	}

	private void OnFactionCrimesChanged(Faction faction)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction))
		{
			UpdateCrimes();
		}
	}

	private void OnFactionLeaderRemoved(Faction faction, ILeader previousLeader)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction))
		{
			UpdateOverview();
		}
	}

	private void OnFactionUpdatedSuccessors(Faction faction)
	{
		if (activeFaction != null)
		{
			UpdateOverview();
		}
	}

	private void UpdateOverview()
	{
		if (activeFaction.leader is Character character)
		{
			Character character2 = character;
			if (character.isLycanthrope)
			{
				character2 = character.lycanData.activeForm;
			}
			leaderNameplateItem.gameObject.SetActive(value: true);
			leaderNameplateItem.SetObject(character2);
			leaderNameplateItem.SetAsDefaultBehaviour();
			noLeaderTextGO.SetActive(value: false);
		}
		else
		{
			leaderNameplateItem.gameObject.SetActive(value: false);
			noLeaderTextGO.SetActive(value: true);
		}
		Character[] successors = activeFaction.successionComponent.successors;
		for (int i = 0; i < successorPortraits.Length; i++)
		{
			CharacterPortrait characterPortrait = successorPortraits[i];
			Character character3 = null;
			if (i >= 0 && i < successors.Length)
			{
				character3 = successors[i];
			}
			if (character3 == null)
			{
				characterPortrait.gameObject.SetActive(value: false);
				continue;
			}
			characterPortrait.GeneratePortrait(character3);
			characterPortrait.gameObject.SetActive(value: true);
		}
		ideologyLbl.text = string.Empty;
		for (int j = 0; j < activeFaction.factionType.ideologies.Count; j++)
		{
			FactionIdeology factionIdeology = activeFaction.factionType.ideologies[j];
			ideologyLbl.text += $"<sprite=\"Text_Sprites\" name=\"Arrow_Icon\">   <link=\"{j}\">{factionIdeology.GetIdeologyDisplayName()}</link>\n";
		}
	}

	public void OnHoverIdeology(object obj)
	{
		if (obj is string s)
		{
			int index = int.Parse(s);
			FactionIdeology factionIdeology = activeFaction.factionType.ideologies[index];
			UIManager.Instance.ShowSmallInfo(factionIdeology.GetIdeologyDescription());
		}
	}

	public void OnHoverOutIdeology()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateOwnedLocations()
	{
		Utilities.DestroyChildren(locationsTransform);
		locationItems.Clear();
		for (int i = 0; i < activeFaction.ownedSettlements.Count; i++)
		{
			BaseSettlement settlement = activeFaction.ownedSettlements[i];
			CreateNewSettlementItem(settlement);
		}
	}

	private void CreateNewSettlementItem(BaseSettlement settlement)
	{
		SettlementNameplateItem component = UIManager.Instance.InstantiateUIObject(settlementNameplatePrefab.name, locationsTransform).GetComponent<SettlementNameplateItem>();
		component.SetObject(settlement);
		component.SetAsButton();
		component.AddOnClickAction(OnClickSettlementItem);
		locationItems.Add(component);
	}

	private void ReorderSettlementItems()
	{
		List<SettlementNameplateItem> list = RuinarchListPool<SettlementNameplateItem>.Claim();
		list.AddRange(locationItems);
		list.Sort((SettlementNameplateItem item1, SettlementNameplateItem item2) => string.Compare(item1.settlement.name, item2.settlement.name, StringComparison.Ordinal));
		for (int num = 0; num < list.Count; num++)
		{
			list[num].transform.SetSiblingIndex(num);
		}
		RuinarchListPool<SettlementNameplateItem>.Release(list);
	}

	private void OnClickSettlementItem(BaseSettlement settlement)
	{
		UIManager.Instance.ShowSettlementInfo(settlement);
	}

	private SettlementNameplateItem GetLocationItem(BaseSettlement settlement)
	{
		for (int i = 0; i < locationItems.Count; i++)
		{
			SettlementNameplateItem settlementNameplateItem = locationItems[i];
			if (settlementNameplateItem.obj.id == settlement.id)
			{
				return settlementNameplateItem;
			}
		}
		return null;
	}

	private void DestroyLocationItem(BaseSettlement settlement)
	{
		SettlementNameplateItem locationItem = GetLocationItem(settlement);
		if (locationItem != null)
		{
			locationItems.Remove(locationItem);
			ObjectPoolManager.Instance.DestroyObject(locationItem);
		}
	}

	private void OnFactionSettlementAdded(Faction faction, BaseSettlement settlement)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction))
		{
			CreateNewSettlementItem(settlement);
		}
	}

	private void OnFactionSettlementRemoved(Faction faction, BaseSettlement settlement)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction))
		{
			DestroyLocationItem(settlement);
		}
	}

	public void UpdateAllRelationships()
	{
		Utilities.DestroyChildren(relationshipsScrollRect.content);
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in activeFaction.relationships)
		{
			if (relationship.Key.factionType.type != FACTION_TYPE.Wild_Monsters && relationship.Key.factionType.type != FACTION_TYPE.Disguised && relationship.Key.factionType.type != FACTION_TYPE.Retaliator)
			{
				UIManager.Instance.InstantiateUIObject(relationshipPrefab.name, relationshipsScrollRect.content).GetComponent<FactionRelationshipItem>().SetData(relationship.Key, relationship.Value);
			}
		}
	}

	private void OnFactionRelationshipChanged(Faction faction1, Faction faction2, FACTION_RELATIONSHIP_STATUS newStatus, FACTION_RELATIONSHIP_STATUS oldStatus)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction1) || FactionInfoHubUI.Instance.IsShowing(faction2))
		{
			UpdateAllRelationships();
		}
	}

	private void OnFactionActiveChanged(Faction faction)
	{
		if (activeFaction != null)
		{
			UpdateAllRelationships();
		}
	}

	private void UpdateCrimes()
	{
		bool activeInHierarchy = crimesScrollRect.gameObject.activeInHierarchy;
		crimesScrollRect.gameObject.SetActive(value: false);
		infractionCrimesLbl.text = string.Empty;
		misdemeanourCrimesLbl.text = string.Empty;
		seriousCrimesLbl.text = string.Empty;
		heinousCrimesLbl.text = string.Empty;
		TextMeshProUGUI textMeshProUGUI = null;
		foreach (KeyValuePair<CRIME_TYPE, CRIME_SEVERITY> crime in activeFaction.factionType.crimes)
		{
			CrimeType crimeType = CrimeManager.Instance.GetCrimeType(crime.Key);
			if (crime.Value == CRIME_SEVERITY.Infraction)
			{
				textMeshProUGUI = infractionCrimesLbl;
			}
			else if (crime.Value == CRIME_SEVERITY.Misdemeanor)
			{
				textMeshProUGUI = misdemeanourCrimesLbl;
			}
			else if (crime.Value == CRIME_SEVERITY.Serious)
			{
				textMeshProUGUI = seriousCrimesLbl;
			}
			else if (crime.Value == CRIME_SEVERITY.Heinous)
			{
				textMeshProUGUI = heinousCrimesLbl;
			}
			if (textMeshProUGUI != null && crimeType != null)
			{
				TextMeshProUGUI textMeshProUGUI2 = textMeshProUGUI;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "<sprite=\"Text_Sprites\" name=\"Arrow_Icon\">   " + crimeType.localizedName + "\n";
			}
		}
		if (activeInHierarchy)
		{
			crimesScrollRect.gameObject.SetActive(value: true);
		}
	}

	private void OnCharacterSetAsSettlementRuler(Character character, Character previousRuler)
	{
		if (activeFaction != null && ((character != null && character.faction == activeFaction) || (previousRuler != null && previousRuler.faction == activeFaction)))
		{
			UpdateAllCharacters();
		}
	}

	private void OnCharacterSwitchFromLimbo(Character toLimbo, Character fromLimbo)
	{
		if (!toLimbo.isLycanthrope)
		{
			return;
		}
		Faction faction = toLimbo.lycanData.originalForm.faction;
		if (activeFaction == faction)
		{
			CharacterNameplateItem item = GetItem(toLimbo);
			if (item != null)
			{
				item.UpdateObject(fromLimbo);
			}
			if (leaderNameplateItem.character == toLimbo)
			{
				leaderNameplateItem.UpdateObject(fromLimbo);
			}
		}
	}

	private void UpdateAllCharacters()
	{
		Utilities.DestroyChildren(charactersScrollView.content);
		_characterItems.Clear();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < activeFaction.characters.Count; i++)
		{
			Character character = activeFaction.characters[i];
			if (!character.traitContainer.HasTrait("Ephemeral"))
			{
				if (character.isLycanthrope)
				{
					character = character.lycanData.activeForm;
				}
				if (!character.isDead)
				{
					flag = true;
				}
				flag2 = true;
				CreateNewCharacterItem(character, autoSort: false);
			}
		}
		if (!flag && flag2)
		{
			aliveToggle.isOn = false;
		}
	}

	public void FilterCharacters()
	{
		for (int i = 0; i < _characterItems.Count; i++)
		{
			CharacterNameplateItem characterNameplateItem = _characterItems[i];
			if (characterNameplateItem.character != null)
			{
				characterNameplateItem.gameObject.SetActive(ShouldCharacterNameplateBeShown(characterNameplateItem.character));
			}
			else
			{
				characterNameplateItem.gameObject.SetActive(value: false);
			}
		}
	}

	private CharacterNameplateItem GetItem(Character character)
	{
		CharacterNameplateItem[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(charactersScrollView.content.gameObject);
		foreach (CharacterNameplateItem characterNameplateItem in componentsInDirectChildren)
		{
			if (characterNameplateItem.character != null && characterNameplateItem.character.id == character.id)
			{
				return characterNameplateItem;
			}
		}
		return null;
	}

	private CharacterNameplateItem CreateNewCharacterItem(Character character, bool autoSort = true)
	{
		GameObject obj = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
		CharacterNameplateItem component = obj.GetComponent<CharacterNameplateItem>();
		component.SetObject(character);
		component.SetAsDefaultBehaviour();
		obj.SetActive(ShouldCharacterNameplateBeShown(character));
		_characterItems.Add(component);
		return component;
	}

	private CharacterNameplateItem GetCharacterItem(Character character)
	{
		for (int i = 0; i < _characterItems.Count; i++)
		{
			CharacterNameplateItem characterNameplateItem = _characterItems[i];
			if (characterNameplateItem.obj == character)
			{
				return characterNameplateItem;
			}
		}
		return null;
	}

	private void OrderCharacterItems()
	{
		if (activeFaction.leader != null && activeFaction.leader is Character character)
		{
			CharacterNameplateItem item = GetItem(character);
			if (item == null)
			{
				throw new Exception("Leader item in " + activeFaction.name + "'s UI is null! Leader is " + character.name);
			}
			item.transform.SetAsFirstSibling();
		}
	}

	private void OnCharacterAddedToFaction(Character character, Faction faction)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction) && character.race != RACE.ANGEL && GetCharacterItem(character) == null)
		{
			CreateNewCharacterItem(character);
		}
	}

	private void OnCharacterRemovedFromFaction(Character character, Faction faction)
	{
		if (FactionInfoHubUI.Instance.IsShowing(faction))
		{
			CharacterNameplateItem item = GetItem(character);
			if (item != null)
			{
				_characterItems.Remove(item);
				ObjectPoolManager.Instance.DestroyObject(item);
			}
		}
	}

	private bool ShouldCharacterNameplateBeShown(Character character)
	{
		if (IsCharacterFilteredByTraits(character) && IsCharacterFilteredByVillage(character))
		{
			return !IsCharacterFilteredByDeath(character);
		}
		return false;
	}

	private bool IsCharacterFilteredByTraits(Character character)
	{
		if (filteredTraits.Count <= 0)
		{
			return true;
		}
		for (int i = 0; i < filteredTraits.Count; i++)
		{
			if (character.traitContainer.HasTrait(filteredTraits[i]) && character.isInfoUnlocked)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCharacterFilteredByVillage(Character character)
	{
		if (filteredVillages.Count <= 0)
		{
			return true;
		}
		for (int i = 0; i < filteredVillages.Count; i++)
		{
			if (character.homeSettlement == filteredVillages[i] && character.isInfoUnlocked)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCharacterFilteredByDeath(Character character)
	{
		if (aliveToggle.isOn)
		{
			return character.isDead;
		}
		return false;
	}

	private void PopulateFilterTraits()
	{
		List<string> list = RuinarchListPool<string>.Claim();
		list.AddRange(TraitManager.Instance.unhiddenTraitsNotStatuses);
		list.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			string traitName = list[i];
			CreateFactionTraitFilterItem(traitName);
		}
		RuinarchListPool<string>.Release(list);
	}

	private void PopulateFilterVillages()
	{
		if (activeFaction == null)
		{
			return;
		}
		Utilities.DestroyChildrenObjectPool(villageFilterScrollRect.content);
		ClearFilteredVillages();
		for (int i = 0; i < activeFaction.ownedSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = activeFaction.ownedSettlements[i];
			if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				CreateFactionVillageFilterItem(baseSettlement);
			}
		}
		UpdateSelectAllVillagesToggle();
	}

	public void AddFilteredTrait(string traitName)
	{
		filteredTraits.Add(traitName);
		FilterCharacters();
	}

	public void RemoveFilteredTrait(string traitName)
	{
		if (filteredTraits.Remove(traitName))
		{
			FilterCharacters();
		}
	}

	public void UpdateSelectAllTraitFiltersToggle()
	{
		bool isOnWithoutNotify = true;
		for (int i = 0; i < _traitFilterItems.Count; i++)
		{
			if (!_traitFilterItems[i].toggle.isOn)
			{
				isOnWithoutNotify = false;
				break;
			}
		}
		selectAllTraitsToggle.SetIsOnWithoutNotify(isOnWithoutNotify);
	}

	private void OnToggleSelectAllTraits(bool p_isOn)
	{
		if (p_isOn)
		{
			for (int i = 0; i < _traitFilterItems.Count; i++)
			{
				_traitFilterItems[i].toggle.isOn = true;
			}
			FilterCharacters();
		}
		else
		{
			ClearFilteredTraits();
		}
	}

	private void ClearFilteredTraits()
	{
		filteredTraits.Clear();
		FilterCharacters();
		ResetFilterTraits();
	}

	private void ResetFilterTraits()
	{
		for (int i = 0; i < _traitFilterItems.Count; i++)
		{
			_traitFilterItems[i].toggle.isOn = false;
		}
	}

	public void AddFilteredVillage(BaseSettlement village)
	{
		filteredVillages.Add(village);
		FilterCharacters();
	}

	public void RemoveFilteredVillage(BaseSettlement village)
	{
		if (filteredVillages.Remove(village))
		{
			FilterCharacters();
		}
	}

	public void UpdateSelectAllVillagesToggle()
	{
		bool isOnWithoutNotify = true;
		for (int i = 0; i < _villageFilterItems.Count; i++)
		{
			if (!_villageFilterItems[i].toggle.isOn)
			{
				isOnWithoutNotify = false;
				break;
			}
		}
		selectAllVillagesToggle.SetIsOnWithoutNotify(isOnWithoutNotify);
	}

	private void OnToggleSelectAllVillages(bool p_isOn)
	{
		if (p_isOn)
		{
			for (int i = 0; i < _villageFilterItems.Count; i++)
			{
				_villageFilterItems[i].toggle.isOn = true;
			}
			FilterCharacters();
		}
		else
		{
			ClearFilteredVillages();
		}
	}

	private void ClearFilteredVillages()
	{
		filteredVillages.Clear();
		FilterCharacters();
		ResetFilterVillages();
	}

	private void ResetFilterVillages()
	{
		for (int i = 0; i < _villageFilterItems.Count; i++)
		{
			_villageFilterItems[i].toggle.isOn = false;
		}
	}

	private void OnSearchTraitFilterValueChanged(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			ResetSearchFilterTraits();
		}
	}

	private void OnSearchRegionFilterValueChanged(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			ResetSearchFilterRegions();
		}
	}

	private void SearchFilterTraits(string text)
	{
		for (int i = 0; i < _traitFilterItems.Count; i++)
		{
			FactionTraitFilterItem factionTraitFilterItem = _traitFilterItems[i];
			factionTraitFilterItem.gameObject.SetActive(factionTraitFilterItem.nameLbl.text.CaseInsensitiveContains(text));
		}
	}

	private void ResetSearchFilterTraits()
	{
		for (int i = 0; i < _traitFilterItems.Count; i++)
		{
			_traitFilterItems[i].gameObject.SetActive(value: true);
		}
	}

	private void SearchFilterRegions(string text)
	{
		for (int i = 0; i < _villageFilterItems.Count; i++)
		{
			FactionVillageFilterItem factionVillageFilterItem = _villageFilterItems[i];
			factionVillageFilterItem.gameObject.SetActive(factionVillageFilterItem.nameLbl.text.CaseInsensitiveContains(text));
		}
	}

	private void ResetSearchFilterRegions()
	{
		for (int i = 0; i < _villageFilterItems.Count; i++)
		{
			_villageFilterItems[i].gameObject.SetActive(value: true);
		}
	}

	private FactionTraitFilterItem CreateFactionTraitFilterItem(string traitName)
	{
		Transform content = traitFilterScrollRect.content;
		FactionTraitFilterItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(traitFilterItemPrefab.name, Vector3.zero, Quaternion.identity, content).GetComponent<FactionTraitFilterItem>();
		component.SetTraitName(traitName);
		_traitFilterItems.Add(component);
		return component;
	}

	private FactionVillageFilterItem CreateFactionVillageFilterItem(BaseSettlement village)
	{
		FactionVillageFilterItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(villageFilterItemPrefab.name, Vector3.zero, Quaternion.identity, villageFilterScrollRect.content).GetComponent<FactionVillageFilterItem>();
		component.SetVillage(village);
		_villageFilterItems.Add(component);
		return component;
	}

	public void OnClickSearchTraitFilter()
	{
		string text = searchTraitFilterField.text;
		SearchFilterTraits(text);
	}

	public void OnClickSearchRegionFilter()
	{
		string text = searchVillageFilterField.text;
		SearchFilterRegions(text);
	}

	public void OnToggleAlive(bool state)
	{
		FilterCharacters();
	}

	public void RevealInfo()
	{
		if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy >= EditableValuesManager.Instance.GetRevealFactionInfoCost())
		{
			AudioManager.Instance.TryPlayUISFX("Play_Reveal_Info");
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-EditableValuesManager.Instance.GetRevealFactionInfoCost());
			activeFaction.SetIsInfoUnlocked(p_state: true);
			ProcessDisplay();
		}
	}

	private void ProcessDisplay()
	{
		switch (m_viewMode)
		{
		case VIEW_MODE.Overview:
			OnToggleOverview(isOn: true);
			break;
		case VIEW_MODE.Members:
			OnToggleMembers(isOn: true);
			break;
		case VIEW_MODE.Relations:
			OnToggleRelations(isOn: true);
			break;
		case VIEW_MODE.Crimes:
			OnToggleCrimes(isOn: true);
			break;
		case VIEW_MODE.Logs:
			OnToggleLogs(isOn: true);
			break;
		}
	}

	private void ResetScrollPositions()
	{
		charactersScrollView.verticalNormalizedPosition = 1f;
		crimesScrollRect.verticalNormalizedPosition = 1f;
		logsWindow.ResetScrollPosition();
	}

	private void OnPlaguePointsAdjusted(int p_amount, int p_plaguePoints)
	{
		InitializeRevealHoverText();
	}

	private void InitializeRevealHoverText()
	{
		if (PlayerManager.Instance.player != null)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < EditableValuesManager.Instance.GetRevealFactionInfoCost())
			{
				btnRevealInfo.GetComponent<HoverText>()?.SetText("Not_Enough_Chaotic_Energy");
				btnRevealInfo.GetComponent<RuinarchButton>().MakeUnavailable();
			}
			else
			{
				btnRevealInfo.GetComponent<HoverText>()?.SetText("Reveal Faction Info");
				btnRevealInfo.GetComponent<RuinarchButton>().MakeAvailable();
			}
		}
	}

	public void OnToggleOverview(bool isOn)
	{
		HideAllScrollView();
		if (isOn)
		{
			InitializeRevealHoverText();
			m_viewMode = VIEW_MODE.Overview;
			if (activeFaction.isInfoUnlocked)
			{
				overviewScrollRectParent.SetActive(value: true);
				btnRevealInfo.SetActive(value: false);
			}
			else
			{
				overviewScrollRectParent.SetActive(value: false);
				btnRevealInfo.SetActive(value: true);
			}
		}
		else
		{
			overviewScrollRectParent.SetActive(value: false);
		}
	}

	public void OnToggleMembers(bool isOn)
	{
		HideAllScrollView();
		if (isOn)
		{
			InitializeRevealHoverText();
			m_viewMode = VIEW_MODE.Members;
			if (activeFaction.isInfoUnlocked)
			{
				membersScrollRectParent.SetActive(value: true);
				btnRevealInfo.SetActive(value: false);
			}
			else
			{
				membersScrollRectParent.SetActive(value: false);
				btnRevealInfo.SetActive(value: true);
			}
		}
		else
		{
			membersScrollRectParent.SetActive(value: false);
		}
	}

	public void OnToggleRelations(bool isOn)
	{
		HideAllScrollView();
		if (isOn)
		{
			InitializeRevealHoverText();
			m_viewMode = VIEW_MODE.Relations;
			if (activeFaction.isInfoUnlocked)
			{
				relationshipScrollRectParent.SetActive(value: true);
				btnRevealInfo.SetActive(value: false);
			}
			else
			{
				relationshipScrollRectParent.SetActive(value: false);
				btnRevealInfo.SetActive(value: true);
			}
		}
		else
		{
			relationshipScrollRectParent.SetActive(value: false);
		}
	}

	public void OnToggleCrimes(bool isOn)
	{
		HideAllScrollView();
		if (isOn)
		{
			InitializeRevealHoverText();
			m_viewMode = VIEW_MODE.Crimes;
			if (activeFaction.isInfoUnlocked)
			{
				crimesScrollRectParent.SetActive(value: true);
				btnRevealInfo.SetActive(value: false);
			}
			else
			{
				crimesScrollRectParent.SetActive(value: false);
				btnRevealInfo.SetActive(value: true);
			}
		}
		else
		{
			crimesScrollRectParent.SetActive(value: false);
		}
	}

	public void OnToggleLogs(bool isOn)
	{
		HideAllScrollView();
		if (isOn)
		{
			InitializeRevealHoverText();
			m_viewMode = VIEW_MODE.Logs;
			if (activeFaction.isInfoUnlocked)
			{
				logScrollRectParent.SetActive(value: true);
				btnRevealInfo.SetActive(value: false);
			}
			else
			{
				logScrollRectParent.SetActive(value: false);
				btnRevealInfo.SetActive(value: true);
			}
		}
		else
		{
			logScrollRectParent.SetActive(value: false);
		}
	}

	private void HideAllScrollView()
	{
		overviewScrollRectParent.SetActive(value: false);
		membersScrollRectParent.SetActive(value: false);
		relationshipScrollRectParent.SetActive(value: false);
		crimesScrollRectParent.SetActive(value: false);
		logScrollRectParent.SetActive(value: false);
	}

	private void UpdateHistory(Log log)
	{
		if (activeFaction != null && log.IsInvolved(activeFaction))
		{
			UpdateAllHistoryInfo();
		}
	}

	public void UpdateAllHistoryInfo()
	{
		logsWindow.UpdateAllHistoryInfo();
	}

	private void OnActiveReligiousCultistsUpdated(RELIGION p_religion)
	{
		if (base.gameObject.activeInHierarchy)
		{
			SetButtonRevealPriceDisplay();
		}
	}
}
