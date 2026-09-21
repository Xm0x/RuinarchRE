using System;
using System.Collections.Generic;
using Locations.Settlements;
using Maccima_Games.Util;
using UtilityScripts;

namespace Events.World_Events;

public class EradicationVillagerMigration : VillagerMigrationEvent
{
	private string _migrationScheduleKey;

	private int _ticksInBetweenMigrations;

	private string _migrationBookmarkTooltip;

	public GameDate nextMigrationDate { get; private set; }

	public RuinarchTimer migrationTimer { get; private set; }

	public EradicationVillagerMigration()
	{
		DetermineMigrationFrequency();
		migrationTimer = new RuinarchTimer("Fixed Migration");
		migrationTimer.SetOnHoverOverAction(OnHoverOverMigrationBookmark);
		migrationTimer.SetOnHoverOutAction(OnHoverOutMigrationBookmark);
		ScheduleNextMigration();
	}

	public EradicationVillagerMigration(SaveDataEradicationVillagerMigrationEvent data)
	{
		DetermineMigrationFrequency();
		migrationTimer = data.migrationTimer;
		migrationTimer.LoadStart();
		migrationTimer.SetOnHoverOverAction(OnHoverOverMigrationBookmark);
		migrationTimer.SetOnHoverOutAction(OnHoverOutMigrationBookmark);
		LoadMigration(data.nextMigrationDate);
	}

	private void DetermineMigrationFrequency()
	{
		switch (WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed)
		{
		case MIGRATION_SPEED.Slow:
			_ticksInBetweenMigrations = GameManager.Instance.GetTicksBasedOnHour(120);
			break;
		case MIGRATION_SPEED.Normal:
			_ticksInBetweenMigrations = GameManager.Instance.GetTicksBasedOnHour(96);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void InitializeAfterLoadoutPicked()
	{
		base.InitializeAfterLoadoutPicked();
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(migrationTimer, BOOKMARK_CATEGORY.Major_Events);
		UpdateMigrationBookmarkTooltip();
	}

	private void ScheduleNextMigration()
	{
		nextMigrationDate = GameManager.Instance.Today();
		nextMigrationDate = nextMigrationDate.AddTicks(_ticksInBetweenMigrations);
		_migrationScheduleKey = SchedulingManager.Instance.AddEntry(nextMigrationDate, TriggerFixedMigration, this);
		migrationTimer.SetTimerName(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Incoming Migrants"));
		migrationTimer.Stop();
		migrationTimer.Start(GameManager.Instance.Today(), nextMigrationDate);
		migrationTimer.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(migrationTimer);
		if (GameManager.Instance.gameHasStarted)
		{
			UpdateMigrationBookmarkTooltip();
		}
	}

	private void LoadMigration(GameDate p_date)
	{
		nextMigrationDate = p_date;
		_migrationScheduleKey = SchedulingManager.Instance.AddEntry(nextMigrationDate, TriggerFixedMigration, this);
	}

	public override void InitializeEvent()
	{
	}

	private void TriggerFixedMigration()
	{
		TryTriggerEradicationMigrationEvent();
		ScheduleNextMigration();
	}

	private void TryTriggerEradicationMigrationEvent()
	{
		string log = string.Empty;
		int activeVillagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
		int chance = ChanceData.GetChance(CHANCE_TYPE.Eradication_Spawn_New_Faction);
		if (activeVillagerFactionCount >= FactionManager.Instance.maxActiveVillagerFactions)
		{
			chance = 0;
		}
		bool flag = false;
		if (GameUtilities.RollChance(chance, ref log))
		{
			flag = TryTriggerGenericMigrationEvent(100, 100);
		}
		if (flag)
		{
			return;
		}
		List<Faction> list = RuinarchListPool<Faction>.Claim(DatabaseManager.Instance.factionDatabase.allFactionsList.Count);
		List<Faction> list2 = RuinarchListPool<Faction>.Claim(DatabaseManager.Instance.factionDatabase.allFactionsList.Count);
		for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++)
		{
			Faction faction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
			if (faction.isMajorNonPlayer && !faction.isDisbanded && faction.isActive && !faction.IsFriendlyWith(PlayerManager.Instance.player.playerFaction))
			{
				if (faction.HasOwnedVillages())
				{
					list.Add(faction);
				}
				else
				{
					list2.Add(faction);
				}
			}
		}
		Faction randomElement = CollectionUtilities.GetRandomElement(list);
		if (randomElement == null)
		{
			randomElement = CollectionUtilities.GetRandomElement(list2);
		}
		RuinarchListPool<Faction>.Release(list);
		if (randomElement == null)
		{
			return;
		}
		NPCSettlement nPCSettlement = null;
		if (randomElement.ownedSettlements.Count > 0)
		{
			List<NPCSettlement> list3 = RuinarchListPool<NPCSettlement>.Claim(randomElement.ownedSettlements.Count);
			for (int j = 0; j < randomElement.ownedSettlements.Count; j++)
			{
				BaseSettlement baseSettlement = randomElement.ownedSettlements[j];
				if (baseSettlement is NPCSettlement item && baseSettlement.locationType == LOCATION_TYPE.VILLAGE)
				{
					list3.Add(item);
				}
			}
			nPCSettlement = CollectionUtilities.GetRandomElement(list3);
		}
		if (nPCSettlement != null)
		{
			nPCSettlement.migrationComponent.EradicationMigrationEvent(GetMigrantCount());
			return;
		}
		List<PreCharacterData> list4 = RuinarchListPool<PreCharacterData>.Claim();
		RACE raceForFactionType = randomElement.factionType.type.GetRaceForFactionType(randomizeDefault: true);
		DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitRace(list4, raceForFactionType);
		Migrate(list4, randomElement, GetMigrantCount(), null, null);
	}

	public override SaveDataWorldEvent Save()
	{
		SaveDataEradicationVillagerMigrationEvent saveDataEradicationVillagerMigrationEvent = new SaveDataEradicationVillagerMigrationEvent();
		saveDataEradicationVillagerMigrationEvent.Save(this);
		return saveDataEradicationVillagerMigrationEvent;
	}

	private void OnHoverOverMigrationBookmark(UIHoverPosition p_hoverPos)
	{
		UIManager.Instance.ShowSmallInfo(_migrationBookmarkTooltip, p_hoverPos);
	}

	private void OnHoverOutMigrationBookmark()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateMigrationBookmarkTooltip()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(2);
		dictionary.Add("migrantCount", GetMigrantCount().ToString());
		dictionary.Add("date", nextMigrationDate.ConvertToContinuousDaysWithTime(nextLineTime: false, capitalizedDay: true));
		_migrationBookmarkTooltip = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Migration_Date", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
	}
}
