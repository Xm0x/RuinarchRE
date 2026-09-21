using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class Wyvernling : Summon
{
	public GameDate growUpDate { get; private set; }

	public bool shouldGrowUpOnUnSeize { get; private set; }

	public override bool defaultDigMode => true;

	public override Type serializedData => typeof(SaveDataWyvernling);

	public Wyvernling()
		: base(SUMMON_TYPE.Wyvernling, "Wyvernling", RACE.WYVERN, Utilities.GetRandomGender())
	{
	}

	public Wyvernling(string className)
		: base(SUMMON_TYPE.Wyvernling, className, RACE.WYVERN, Utilities.GetRandomGender())
	{
	}

	public Wyvernling(SaveDataWyvernling data)
		: base(data)
	{
		growUpDate = data.growUpDate;
		shouldGrowUpOnUnSeize = data.shouldGrowUpOnUnSeize;
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
	}

	public override void OnPlaceSummon(LocationGridTile tile)
	{
		base.OnPlaceSummon(tile);
		ScheduleGrowUp(GameManager.Instance.Today().AddDays(1));
	}

	private void ScheduleGrowUp(GameDate p_growUpDate)
	{
		growUpDate = p_growUpDate;
		SchedulingManager.Instance.AddEntry(p_growUpDate, GrowUpNormally, this);
	}

	private void GrowUpNormally()
	{
		if (!base.isDead && (base.faction == null || base.faction.factionType.type != FACTION_TYPE.Demons))
		{
			if (base.isBeingSeized && PlayerManager.Instance.player.seizeComponent.isPreparingToBeUnseized)
			{
				ScheduleGrowUp(GameManager.Instance.Today().AddTicks(20));
			}
			else if (IsPOICurrentlyTargetedByOtherCharacterPerformingAction())
			{
				ScheduleGrowUp(GameManager.Instance.Today().AddTicks(20));
			}
			else
			{
				GrowUpBase();
			}
		}
	}

	private void GrowUpBase()
	{
		SetDestroyMarkerOnDeath(state: true);
		LocationGridTile locationTile = base.gridTileLocation;
		Faction faction = base.faction;
		LocationStructure locationStructure = base.homeStructure;
		NPCSettlement homeLocation = base.homeSettlement;
		Region region = base.homeRegion;
		Area area = base.territory;
		SetShowNotificationOnDeath(showNotificationOnDeath: false);
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Wyvern, faction, homeLocation, region, locationStructure, "", bypassIdeologyChecking: true);
		if (!base.isUsingDefaultName)
		{
			summon.SetFirstName(name);
		}
		if (area != null)
		{
			summon.SetTerritory(area);
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "become_wyvern", LOG_TAG.Life_Changes);
		log.AddToFillers(summon, summon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, locationTile);
		TraitManager.Instance.CopyStatuses(this, summon);
		faction.LeaveFaction(this);
		Death("Transform Wyvern");
		if (UIManager.Instance.IsContextMenuShowingForTarget(this))
		{
			UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(summon);
		}
		if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == this)
		{
			UIManager.Instance.monsterInfoUI.CloseMenu();
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		base.LoadReferences(data);
		ScheduleGrowUp(growUpDate);
	}
}
