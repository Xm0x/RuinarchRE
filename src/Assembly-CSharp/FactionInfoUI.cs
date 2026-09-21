using System;
using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class FactionInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Content")]
	[SerializeField]
	private TextMeshProUGUI factionNameLbl;

	[SerializeField]
	private TextMeshProUGUI factionTypeLbl;

	[SerializeField]
	private FactionEmblem emblem;

	[Space(10f)]
	[Header("Overview")]
	[SerializeField]
	private TextMeshProUGUI overviewFactionNameLbl;

	[SerializeField]
	private TextMeshProUGUI overviewFactionTypeLbl;

	[SerializeField]
	private CharacterNameplateItem leaderNameplateItem;

	[SerializeField]
	private TextMeshProUGUI ideologyLbl;

	[Space(10f)]
	[Header("Characters")]
	[SerializeField]
	private GameObject characterItemPrefab;

	[SerializeField]
	private ScrollRect charactersScrollView;

	private List<CharacterNameplateItem> _characterItems;

	[Space(10f)]
	[Header("Regions")]
	[SerializeField]
	private ScrollRect locationsScrollView;

	[SerializeField]
	private GameObject settlementNameplatePrefab;

	private List<SettlementNameplateItem> locationItems;

	[Space(10f)]
	[Header("Relationships")]
	[SerializeField]
	private RectTransform relationshipsParent;

	[SerializeField]
	private GameObject relationshipPrefab;

	[Space(10f)]
	[Header("Logs")]
	[SerializeField]
	private LogsWindow logsWindow;

	internal Faction currentlyShowingFaction => _data as Faction;

	private Faction activeFaction { get; set; }

	internal override void Initialize()
	{
		base.Initialize();
		_characterItems = new List<CharacterNameplateItem>();
		locationItems = new List<SettlementNameplateItem>();
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
		Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_ADDED, OnFactionSettlementAdded);
		Messenger.AddListener<Faction, BaseSettlement>(FactionSignals.FACTION_OWNED_SETTLEMENT_REMOVED, OnFactionSettlementRemoved);
		Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionRelationshipChanged);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
		Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnFactionLeaderChanged);
		Messenger.AddListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
		Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateHistory);
		Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateHistory);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_IDEOLOGIES_CHANGED, OnFactionIdeologiesChanged);
		logsWindow.Initialize();
	}

	public override void OpenMenu()
	{
		_ = activeFaction;
		activeFaction = _data as Faction;
		base.OpenMenu();
		if (UIManager.Instance.IsConversationMenuOpen())
		{
			backButton.interactable = false;
		}
		UpdateOverview();
		UpdateFactionInfo();
		UpdateAllCharacters();
		UpdateOwnedLocations();
		UpdateAllRelationships();
		logsWindow.OnParentMenuOpened(activeFaction.persistentID);
		UpdateAllHistoryInfo();
		ResetScrollPositions();
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		activeFaction = null;
	}

	public void UpdateFactionInfo()
	{
		if (activeFaction != null)
		{
			UpdateBasicInfo();
		}
	}

	private void UpdateBasicInfo()
	{
		factionNameLbl.text = activeFaction.nameWithColor;
		emblem.SetFaction(activeFaction);
	}

	private void UpdateAllCharacters()
	{
		Utilities.DestroyChildren(charactersScrollView.content);
		_characterItems.Clear();
		for (int i = 0; i < activeFaction.characters.Count; i++)
		{
			Character character = activeFaction.characters[i];
			if (character.race != RACE.ANGEL)
			{
				CreateNewCharacterItem(character, autoSort: false);
			}
		}
		OrderCharacterItems();
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
		CharacterNameplateItem component = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content).GetComponent<CharacterNameplateItem>();
		component.SetObject(character);
		component.SetAsDefaultBehaviour();
		_characterItems.Add(component);
		if (autoSort)
		{
			OrderCharacterItems();
		}
		return component;
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
		if (isShowing && activeFaction.id == faction.id && character.race != RACE.ANGEL)
		{
			CreateNewCharacterItem(character);
		}
	}

	private void OnCharacterRemovedFromFaction(Character character, Faction faction)
	{
		if (isShowing && activeFaction != null && activeFaction.id == faction.id)
		{
			CharacterNameplateItem item = GetItem(character);
			if (item != null)
			{
				_characterItems.Remove(item);
				ObjectPoolManager.Instance.DestroyObject(item);
				OrderCharacterItems();
			}
		}
	}

	private void UpdateOwnedLocations()
	{
		Utilities.DestroyChildren(locationsScrollView.content);
		locationItems.Clear();
		for (int i = 0; i < activeFaction.ownedSettlements.Count; i++)
		{
			BaseSettlement settlement = activeFaction.ownedSettlements[i];
			CreateNewSettlementItem(settlement);
		}
	}

	private void CreateNewSettlementItem(BaseSettlement settlement)
	{
		SettlementNameplateItem component = UIManager.Instance.InstantiateUIObject(settlementNameplatePrefab.name, locationsScrollView.content).GetComponent<SettlementNameplateItem>();
		component.SetObject(settlement);
		component.SetAsButton();
		component.AddOnClickAction(OnClickSettlementItem);
		locationItems.Add(component);
	}

	private void OnClickSettlementItem(BaseSettlement settlement)
	{
		if (settlement.areas.Count <= 0)
		{
			return;
		}
		Area area = settlement.areas[0];
		if (InnerMapManager.Instance.isAnInnerMapShowing)
		{
			if (InnerMapManager.Instance.currentlyShowingLocation != area.region)
			{
				InnerMapManager.Instance.TryShowLocationMap(area.region);
			}
			InnerMapCameraMove.Instance.CenterCameraOnTile(area);
		}
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
		if (isShowing && activeFaction.id == faction.id)
		{
			CreateNewSettlementItem(settlement);
		}
	}

	private void OnFactionSettlementRemoved(Faction faction, BaseSettlement settlement)
	{
		if (isShowing && activeFaction.id == faction.id)
		{
			DestroyLocationItem(settlement);
		}
	}

	private void UpdateAllRelationships()
	{
		Utilities.DestroyChildren(relationshipsParent);
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in activeFaction.relationships)
		{
			if (relationship.Key.isActive && (relationship.Key.factionType.type != FACTION_TYPE.Undead || relationship.Key.leader != null))
			{
				UIManager.Instance.InstantiateUIObject(relationshipPrefab.name, relationshipsParent).GetComponent<FactionRelationshipItem>().SetData(relationship.Key, relationship.Value);
			}
		}
	}

	private void OnFactionRelationshipChanged(Faction faction1, Faction faction2, FACTION_RELATIONSHIP_STATUS newStatus, FACTION_RELATIONSHIP_STATUS oldStatus)
	{
		if (isShowing && (faction1.id == activeFaction.id || faction2.id == activeFaction.id))
		{
			UpdateAllRelationships();
		}
	}

	private void OnFactionActiveChanged(Faction faction)
	{
		if (isShowing)
		{
			UpdateAllRelationships();
		}
	}

	public void OnClickCloseBtn()
	{
		CloseMenu();
	}

	private void ResetScrollPositions()
	{
		charactersScrollView.verticalNormalizedPosition = 1f;
		locationsScrollView.verticalNormalizedPosition = 1f;
		logsWindow.ResetScrollPosition();
	}

	private void OnFactionLeaderChanged(Character character, ILeader previousLeader)
	{
		if (isShowing)
		{
			UpdateOverview();
		}
	}

	private void OnFactionIdeologiesChanged(Faction faction)
	{
		if (isShowing && faction == activeFaction)
		{
			UpdateOverview();
		}
	}

	private void OnFactionLeaderRemoved(Faction faction, ILeader previousLeader)
	{
		if (isShowing && faction == activeFaction)
		{
			UpdateOverview();
		}
	}

	private void UpdateOverview()
	{
		overviewFactionNameLbl.text = activeFaction.nameWithColor;
		overviewFactionTypeLbl.text = activeFaction.factionType.displayName;
		if (activeFaction.leader is Character character)
		{
			leaderNameplateItem.gameObject.SetActive(value: true);
			leaderNameplateItem.SetObject(character);
		}
		else
		{
			leaderNameplateItem.gameObject.SetActive(value: false);
		}
		ideologyLbl.text = string.Empty;
		for (int i = 0; i < activeFaction.factionType.ideologies.Count; i++)
		{
			FactionIdeology factionIdeology = activeFaction.factionType.ideologies[i];
			ideologyLbl.text += $"<sprite=\"Text_Sprites\" name=\"Arrow_Icon\">   <link=\"{i}\">{factionIdeology.GetIdeologyDisplayName()}</link>\n";
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

	private void UpdateHistory(Log log)
	{
		if (isShowing && log.IsInvolved(activeFaction))
		{
			UpdateAllHistoryInfo();
		}
	}

	public void UpdateAllHistoryInfo()
	{
		logsWindow.UpdateAllHistoryInfo();
	}
}
