using System.Collections;
using System.Collections.Generic;
using Locations.Settlements;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UtilityScripts;

public class FactionInfoHubUI : PopupMenuBase
{
	public static FactionInfoHubUI Instance;

	[Header("General")]
	[SerializeField]
	private Toggle factionInfoHubToggle;

	[SerializeField]
	private Button closeBtn;

	[SerializeField]
	private RuinarchToggle membersToggle;

	[Header("Faction Scroll Snap")]
	[SerializeField]
	private GameObject factionItemPrefab;

	[SerializeField]
	private HorizontalScrollSnap factionScrollSnap;

	[SerializeField]
	private Transform factionScrollSnapContent;

	[Header("Faction Info UI V2")]
	[SerializeField]
	private FactionInfoUIV2 factionInfoUI;

	public List<FactionItem> factionItems;

	public FactionItem currentSelectedFactionItem => GetCurrentFactionItem();

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		factionItems = new List<FactionItem>();
	}

	private void InitializeUI()
	{
		Messenger.AddListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);
		Messenger.AddListener(FactionSignals.FORCE_FACTION_UI_RELOAD, ForceFactionReload);
		Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
	}

	private void OnFactionCreated(Faction p_createdFaction)
	{
		if (p_createdFaction.factionType.type == FACTION_TYPE.Ratmen || p_createdFaction.factionType.type == FACTION_TYPE.Bandits)
		{
			AddFactionItem(p_createdFaction);
		}
	}

	public void InitializeAfterGameLoaded()
	{
		factionInfoUI.Initialize();
		PopulateInitialFactions();
		InitializeUI();
	}

	private void PopulateInitialFactions()
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction.isMajorNonPlayer && !faction.isDisbanded && faction.isActive)
			{
				AddFactionItem(faction);
			}
		}
		if (FactionManager.Instance.vagrantFaction != null)
		{
			AddFactionItem(FactionManager.Instance.vagrantFaction);
		}
		if (FactionManager.Instance.undeadFaction != null)
		{
			AddFactionItem(FactionManager.Instance.undeadFaction);
		}
		if (FactionManager.Instance.ratmenFaction != null)
		{
			AddFactionItem(FactionManager.Instance.ratmenFaction);
		}
		if (FactionManager.Instance.banditFaction != null)
		{
			AddFactionItem(FactionManager.Instance.banditFaction);
		}
		InitialFactionItemStates();
	}

	private IEnumerator RepopulateFactions()
	{
		Utilities.DestroyChildren(factionScrollSnapContent);
		factionItems.Clear();
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction.isMajorNonPlayer && !faction.isDisbanded && faction.isActive)
			{
				FactionItem item = AddFactionItem(faction);
				SetFactionSelection(item, state: false);
			}
		}
		if (FactionManager.Instance.vagrantFaction != null)
		{
			FactionItem item2 = AddFactionItem(FactionManager.Instance.vagrantFaction);
			SetFactionSelection(item2, state: false);
		}
		if (FactionManager.Instance.undeadFaction != null)
		{
			FactionItem item3 = AddFactionItem(FactionManager.Instance.undeadFaction);
			SetFactionSelection(item3, state: false);
		}
		if (FactionManager.Instance.ratmenFaction != null)
		{
			FactionItem item4 = AddFactionItem(FactionManager.Instance.ratmenFaction);
			SetFactionSelection(item4, state: false);
		}
		if (FactionManager.Instance.banditFaction != null)
		{
			FactionItem item5 = AddFactionItem(FactionManager.Instance.banditFaction);
			SetFactionSelection(item5, state: false);
		}
		if (base.isShowing)
		{
			factionScrollSnap.UpdateChildrenAndPagination();
			yield return null;
			factionScrollSnap.GoToScreen(0);
		}
	}

	public override void Open()
	{
		base.Open();
		factionInfoHubToggle.SetIsOnWithoutNotify(value: true);
	}

	public override void Close()
	{
		base.Close();
		factionInfoHubToggle.SetIsOnWithoutNotify(value: false);
	}

	public void ShowMembers()
	{
		membersToggle.isOn = true;
	}

	public void OnClickClose()
	{
		PlayerUI.Instance.SetVillagerTabIsOn(state: false);
	}

	public void OnSelectionChangeEnd(int index)
	{
		if (index < 0 || index >= factionItems.Count)
		{
			StartCoroutine(GoToScreen(0));
			return;
		}
		for (int i = 0; i < factionItems.Count; i++)
		{
			if (index != i)
			{
				SetFactionSelection(factionItems[i], state: false);
			}
		}
		FactionItem item = factionItems[index];
		SetFactionSelection(item, state: true);
	}

	private void InitialFactionItemStates()
	{
		int currentPage = factionScrollSnap.CurrentPage;
		SetFactionSelection(currentSelectedFactionItem, state: true);
		for (int i = 0; i < factionItems.Count; i++)
		{
			FactionItem item = factionItems[i];
			if (currentPage != i)
			{
				SetFactionSelection(item, state: false);
			}
		}
	}

	public void ShowFaction(Faction faction)
	{
		int factionItemIndex = GetFactionItemIndex(faction);
		if (factionItemIndex != -1)
		{
			PlayerUI.Instance.SetVillagerTabIsOn(state: true);
			StartCoroutine(GoToScreen(factionItemIndex));
		}
	}

	private IEnumerator GoToScreen(int index)
	{
		yield return null;
		factionScrollSnap.GoToScreen(index);
	}

	private FactionItem GetCurrentFactionItem()
	{
		if (factionScrollSnap.CurrentPage >= 0 && factionScrollSnap.CurrentPage < factionItems.Count && factionItems.Count > 0)
		{
			return factionItems[factionScrollSnap.CurrentPage];
		}
		return currentSelectedFactionItem;
	}

	private void OnFactionActiveChanged(Faction p_faction)
	{
		if (GameManager.Instance.gameHasStarted && (p_faction.factionType.type == FACTION_TYPE.Demon_Cult || p_faction.factionType.type == FACTION_TYPE.Divine_Church || p_faction.factionType.type == FACTION_TYPE.Wiccans))
		{
			StartCoroutine(RepopulateFactions());
		}
	}

	private void OnFactionCreated(Faction faction, Character creator)
	{
		if (GameManager.Instance.gameHasStarted && (faction.isMajorNonPlayer || faction.factionType.type == FACTION_TYPE.Vagrants || faction.factionType.type == FACTION_TYPE.Undead || faction.factionType.type == FACTION_TYPE.Ratmen || faction.factionType.type == FACTION_TYPE.Bandits))
		{
			StartCoroutine(RepopulateFactions());
		}
	}

	private void OnFactionDisbanded(Faction faction)
	{
		if (GameManager.Instance.gameHasStarted && faction.isMajorNonPlayer)
		{
			StartCoroutine(RepopulateFactions());
		}
	}

	private void ForceFactionReload()
	{
		StartCoroutine(RepopulateFactions());
	}

	private void OnLoadoutSelected()
	{
		if (factionInfoUI.activeFaction != null)
		{
			factionInfoUI.UpdateAllRelationships();
		}
	}

	private FactionItem AddFactionItem(Faction faction)
	{
		if (!HasFactionItem(faction))
		{
			FactionItem factionItem = CreateFactionItem(faction);
			CreateFactionItemPagination();
			factionItems.Add(factionItem);
			return factionItem;
		}
		return null;
	}

	private void RemoveFactionItem(Faction faction)
	{
		int factionItemIndex = GetFactionItemIndex(faction);
		if (factionItemIndex != -1)
		{
			if (factionScrollSnap.CurrentPage == factionItemIndex)
			{
				factionScrollSnap.GoToScreen(0);
			}
			FactionItem factionItem = factionItems[factionItemIndex];
			ObjectPoolManager.Instance.DestroyObject(factionItem.gameObject);
			factionItems.RemoveAt(factionItemIndex);
		}
	}

	private bool HasFactionItem(Faction faction)
	{
		return GetFactionItem(faction) != null;
	}

	private FactionItem GetFactionItem(Faction faction)
	{
		for (int i = 0; i < factionItems.Count; i++)
		{
			FactionItem factionItem = factionItems[i];
			if (factionItem.faction == faction)
			{
				return factionItem;
			}
		}
		return null;
	}

	private int GetFactionItemIndex(Faction faction)
	{
		for (int i = 0; i < factionItems.Count; i++)
		{
			if (factionItems[i].faction == faction)
			{
				return i;
			}
		}
		return -1;
	}

	private FactionItem CreateFactionItem(Faction faction)
	{
		FactionItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(factionItemPrefab.name, Vector3.zero, Quaternion.identity, factionScrollSnapContent).GetComponent<FactionItem>();
		component.SetFaction(faction);
		return component;
	}

	public void UpdateFactionItem(Faction faction)
	{
		FactionItem factionItem = GetFactionItem(faction);
		if (factionItem != null)
		{
			factionItem.UpdateFaction();
		}
	}

	private void CreateFactionItemPagination()
	{
	}

	private void DestroyFactionItemPaginationInIndex(int index)
	{
	}

	public void SetFactionSelection(FactionItem item, bool state)
	{
		item.SetSelected(state);
		if (state && item.faction != null && item.faction != factionInfoUI.activeFaction)
		{
			factionInfoUI.SetFaction(item.faction);
		}
	}

	public bool IsShowing(Faction faction)
	{
		if (factionItems.Count > 0)
		{
			return factionInfoUI.activeFaction == faction;
		}
		return false;
	}

	public void FilterTrait(string traitName)
	{
		factionInfoUI.AddFilteredTrait(traitName);
		factionInfoUI.UpdateSelectAllTraitFiltersToggle();
	}

	public void UnFilterTrait(string traitName)
	{
		factionInfoUI.RemoveFilteredTrait(traitName);
		factionInfoUI.UpdateSelectAllTraitFiltersToggle();
	}

	public void FilterVillage(BaseSettlement village)
	{
		factionInfoUI.AddFilteredVillage(village);
		factionInfoUI.UpdateSelectAllVillagesToggle();
	}

	public void UnFilterVillage(BaseSettlement village)
	{
		factionInfoUI.RemoveFilteredVillage(village);
		factionInfoUI.UpdateSelectAllVillagesToggle();
	}
}
