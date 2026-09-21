using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SmallSpider : SkinnableAnimal
{
	public const string ClassName = "Small Spider";

	public override SUMMON_TYPE adultSummonType => SUMMON_TYPE.Giant_Spider;

	public override Type serializedData => typeof(SaveDataSmallSpider);

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.SPIDER_SILK;

	public GameDate growUpDate { get; private set; }

	public bool shouldGrowUpOnUnSeize { get; private set; }

	public SmallSpider()
		: base(SUMMON_TYPE.Small_Spider, "Small Spider", RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public SmallSpider(string className)
		: base(SUMMON_TYPE.Small_Spider, className, RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public SmallSpider(SaveDataSmallSpider data)
		: base(data)
	{
		growUpDate = data.growUpDate;
		shouldGrowUpOnUnSeize = data.shouldGrowUpOnUnSeize;
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		base.LoadReferences(data);
		ScheduleGrowUp(growUpDate);
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
		}
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
		if (!base.isDead && (base.faction == null || base.faction.factionType.type != FACTION_TYPE.Demons) && !base.traitContainer.HasTrait("Baby Infestor"))
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
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Giant_Spider, faction, homeLocation, region, locationStructure, "", bypassIdeologyChecking: true);
		if (!base.isUsingDefaultName)
		{
			summon.SetFirstName(name);
		}
		if (area != null)
		{
			summon.SetTerritory(area);
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "become_giant_spider", LOG_TAG.Life_Changes);
		log.AddToFillers(summon, summon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, locationTile);
		TraitManager.Instance.CopyStatuses(this, summon);
		base.faction.LeaveFaction(this);
		Death("Transform Giant Spider");
		if (UIManager.Instance.IsContextMenuShowingForTarget(this))
		{
			UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(summon);
		}
		if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == this)
		{
			UIManager.Instance.monsterInfoUI.CloseMenu();
		}
	}

	private void OnArachnophobiaToggled(bool p_isOn)
	{
		base.visuals.UpdateAllVisuals(this);
	}
}
