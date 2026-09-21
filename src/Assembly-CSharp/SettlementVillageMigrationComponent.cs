using System.Collections.Generic;
using Generator.Map_Generation.Components;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class SettlementVillageMigrationComponent : NPCSettlementComponent
{
	private const int MAX_MIGRATION_METER = 1500;

	private readonly GenericTextBookmarkable _migrationBookmark;

	private bool _isAlreadyBookmarked;

	private string _emptyVillageMigrationScheduleKey;

	public int villageMigrationMeter { get; private set; }

	public int perHourIncrement { get; private set; }

	public int longTermModifier { get; private set; }

	public GameDate emptyVillageMigrationDate { get; private set; }

	public SettlementVillageMigrationComponent()
	{
		RandomizePerHourIncrement();
		_migrationBookmark = new GenericTextBookmarkable(GetMigrationMeterBookmarkName, () => BOOKMARK_TYPE.Text, delegate
		{
			UIManager.Instance.ShowStructureInfo(base.owner.cityCenter);
		}, null, null, null);
	}

	public SettlementVillageMigrationComponent(SaveDataSettlementVillageMigrationComponent data)
	{
		villageMigrationMeter = data.villageMigrationMeter;
		perHourIncrement = data.perHourIncrement;
		longTermModifier = data.longTermModifier;
		emptyVillageMigrationDate = data.emptyVillageMigrationDate;
		_migrationBookmark = new GenericTextBookmarkable(GetMigrationMeterBookmarkName, () => BOOKMARK_TYPE.Text, delegate
		{
			UIManager.Instance.ShowStructureInfo(base.owner.cityCenter);
		}, null, null, null);
	}

	public void LoadReferences(SaveDataNPCSettlement saveDataNpcSettlement)
	{
		CheckIfBookmarkShouldBeAdded(GetNormalizedMigrationMeterValue());
		if (emptyVillageMigrationDate.hasValue)
		{
			ScheduleEmptyVillageMigration(emptyVillageMigrationDate);
		}
	}

	private void CheckIfBookmarkShouldBeAdded(float p_migrationMeterNormalized)
	{
		if (p_migrationMeterNormalized >= 0.9f && !_isAlreadyBookmarked)
		{
			_isAlreadyBookmarked = true;
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(_migrationBookmark, BOOKMARK_CATEGORY.Major_Events);
		}
	}

	private void CheckIfBookmarkShouldBeRemoved(float p_migrationMeterNormalized)
	{
		if (p_migrationMeterNormalized < 0.9f && _isAlreadyBookmarked)
		{
			_isAlreadyBookmarked = false;
			PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(_migrationBookmark, BOOKMARK_CATEGORY.Major_Events);
		}
	}

	public void OnHourStarted()
	{
		if (!IsMigrationEventAllowed())
		{
			if (villageMigrationMeter > 0)
			{
				SetVillageMigrationMeter(0);
			}
		}
		else
		{
			IncreaseVillageMigrationMeter(GetPerHourMigrationRate());
		}
	}

	public void OnSettlementTypeChanged()
	{
		RandomizePerHourIncrement();
	}

	public void OnStructureBuilt(LocationStructure structure)
	{
		if (IsMigrationEventAllowed())
		{
			if (structure is Dwelling)
			{
				IncreaseVillageMigrationMeter(GameUtilities.RandomBetweenTwoNumbers(20, 30));
			}
			else
			{
				IncreaseVillageMigrationMeter(GameUtilities.RandomBetweenTwoNumbers(30, 40));
			}
		}
	}

	public void OnFinishedQuest(PartyQuest quest)
	{
		if (quest.madeInLocation != null && quest.madeInLocation == base.owner && quest.isSuccessful && IsMigrationEventAllowed())
		{
			IncreaseVillageMigrationMeter(GameUtilities.RandomBetweenTwoNumbers(20, 30));
		}
	}

	public void OnSettlementAbandoned()
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE)
		{
			CancelCurrentEmptyVillageMigration();
			if (FactionManager.Instance.HasMajorFactionWithAliveMembers())
			{
				GameDate p_schedule = GameManager.Instance.Today();
				p_schedule.AddDays(GameUtilities.RandomBetweenTwoNumbers(1, 5));
				ScheduleEmptyVillageMigration(p_schedule);
			}
		}
	}

	public void OnVillageGainFirstResident()
	{
		CancelCurrentEmptyVillageMigration();
	}

	public void OnSettlementDestroyed()
	{
		CancelCurrentEmptyVillageMigration();
	}

	public void ForceRandomizePerHourIncrement()
	{
		RandomizePerHourIncrement();
	}

	private void RandomizePerHourIncrement()
	{
		perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(3, 8);
	}

	private int GetPerHourMigrationRate()
	{
		if (IsMigrationEventAllowed())
		{
			int additionalMigrationMeterRatePerHour = GetAdditionalMigrationMeterRatePerHour();
			int num = perHourIncrement + additionalMigrationMeterRatePerHour + longTermModifier;
			if (num < 1)
			{
				num = 1;
			}
			return num;
		}
		return 0;
	}

	public void AdjustLongTermModifier(int amount)
	{
		longTermModifier += amount;
	}

	public void ResetLongTermModifier()
	{
		longTermModifier = 0;
	}

	private int ApplyVillageMigrationModifier(int p_amount)
	{
		if (WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed == MIGRATION_SPEED.Slow)
		{
			p_amount = Mathf.CeilToInt((float)p_amount / 2f);
		}
		return p_amount;
	}

	public void IncreaseVillageMigrationMeter(int amount)
	{
		amount = ApplyVillageMigrationModifier(amount);
		villageMigrationMeter += amount;
		villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, 1500);
		CheckIfBookmarkShouldBeAdded(GetNormalizedMigrationMeterValue());
		if (_isAlreadyBookmarked)
		{
			_migrationBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(_migrationBookmark);
		}
		CheckIfMigrationMeterIsFull();
	}

	public void ReduceVillageMigrationMeter(int amount)
	{
		villageMigrationMeter -= amount;
		villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, 1500);
		if (_isAlreadyBookmarked)
		{
			_migrationBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(_migrationBookmark);
		}
		CheckIfBookmarkShouldBeRemoved(GetNormalizedMigrationMeterValue());
	}

	public void SetVillageMigrationMeter(int amount)
	{
		villageMigrationMeter = amount;
		villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, 1500);
		float normalizedMigrationMeterValue = GetNormalizedMigrationMeterValue();
		CheckIfBookmarkShouldBeRemoved(normalizedMigrationMeterValue);
		CheckIfBookmarkShouldBeAdded(normalizedMigrationMeterValue);
		if (_isAlreadyBookmarked)
		{
			_migrationBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(_migrationBookmark);
		}
		CheckIfMigrationMeterIsFull();
	}

	private void CheckIfMigrationMeterIsFull()
	{
		if (villageMigrationMeter == 1500)
		{
			OnFullVillageMigrationMeter();
		}
	}

	private void OnFullVillageMigrationMeter()
	{
		SetVillageMigrationMeter(0);
		VillageMigrationEvent();
	}

	public float GetNormalizedMigrationMeterValue()
	{
		return (float)villageMigrationMeter / 1500f;
	}

	public string GetMigrationMeterValueInText()
	{
		return villageMigrationMeter + "/" + 1500;
	}

	public string GetHoverTextOfMigrationMeter()
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Migration_Current_Value") + ": " + GetMigrationMeterValueInText();
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Migration_Hourly") + ": " + ApplyVillageMigrationModifier(GetPerHourMigrationRate());
		if (!IsMigrationEventAllowed())
		{
			text = ((WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed != MIGRATION_SPEED.None) ? (text + "\n" + Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Migration_Human_Elven"))) : (text + "\n" + Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Migration_Turned_Off"))));
		}
		return text;
	}

	private int GetAdditionalMigrationMeterRatePerHour()
	{
		return base.owner.owner?.factionType.GetAdditionalMigrationMeterGain(base.owner) ?? 0;
	}

	public bool IsMigrationEventAllowed()
	{
		if (WorldSettings.Instance.worldSettingsData.victoryCondition != VICTORY_CONDITION.Eradication && WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed != MIGRATION_SPEED.None && base.owner.locationType == LOCATION_TYPE.VILLAGE && base.owner.owner != null && base.owner.residents.Count > 0 && base.owner.owner.isMajorNonPlayer && WorldSettings.Instance.worldSettingsData.villageSettings.IsMigrationAllowedForFaction(base.owner.owner.factionType.type))
		{
			if (base.owner.owner.factionType.type != FACTION_TYPE.Human_Empire && base.owner.owner.factionType.type != FACTION_TYPE.Elven_Kingdom && base.owner.owner.factionType.type != FACTION_TYPE.Lycan_Clan && base.owner.owner.factionType.type != FACTION_TYPE.Vampire_Clan && base.owner.owner.factionType.type != FACTION_TYPE.Wiccans)
			{
				return base.owner.owner.factionType.type == FACTION_TYPE.Divine_Church;
			}
			return true;
		}
		return false;
	}

	public void InduceMigrationEvent()
	{
		if (base.owner.owner != null)
		{
			VillageMigrationEvent();
		}
		else
		{
			VillageMigrationEventOnEmptySettlement();
		}
	}

	public void EradicationMigrationEvent(int p_migrantCount)
	{
		string debugLog = string.Empty;
		List<PreCharacterData> list = RuinarchListPool<PreCharacterData>.Claim();
		DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitFaction(list, base.owner.owner.race, base.owner.owner);
		if (list.Count > 0)
		{
			Migrate(list, base.owner.owner, ref debugLog, p_migrantCount);
		}
		RuinarchListPool<PreCharacterData>.Release(list);
	}

	private void VillageMigrationEvent()
	{
		string debugLog = string.Empty;
		if (IsMigrationEventAllowed())
		{
			List<PreCharacterData> list = RuinarchListPool<PreCharacterData>.Claim();
			DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitFaction(list, base.owner.owner.race, base.owner.owner);
			if (list.Count > 0)
			{
				Migrate(list, base.owner.owner, ref debugLog);
			}
			RuinarchListPool<PreCharacterData>.Release(list);
		}
	}

	private void VillageMigrationEventOnEmptySettlement()
	{
		string debugLog = string.Empty;
		List<PreCharacterData> list = RuinarchListPool<PreCharacterData>.Claim();
		RACE race = RACE.HUMANS;
		if (GameUtilities.RollChance(50))
		{
			race = RACE.ELVES;
		}
		DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitRace(list, race);
		if (list.Count > 0)
		{
			Faction faction = FactionManager.Instance.CreateNewFaction(FactionManager.Instance.GetFactionTypeForRace(race), "", null, race);
			faction.factionType.SetAsDefault(faction);
			faction.SetLeader(null);
			LandmarkManager.Instance.OwnSettlement(faction, base.owner);
			Migrate(list, faction, ref debugLog);
			Messenger.Broadcast(FactionSignals.FORCE_FACTION_UI_RELOAD);
		}
		RuinarchListPool<PreCharacterData>.Release(list);
	}

	private void Migrate(List<PreCharacterData> unspawnedCharacters, Faction faction, ref string debugLog, int p_migrantsCount = -1)
	{
		AdjustLongTermModifier(-1);
		int num = p_migrantsCount;
		if (num == -1)
		{
			ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
			int num2 = ((WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled) ? 4 : thePortal.level);
			FACTION_TYPE type = faction.factionType.type;
			if ((uint)(type - 14) <= 1u)
			{
				num = GameUtilities.RandomBetweenTwoNumbers(2, 3);
				num += Mathf.FloorToInt((float)num2 / 3f);
			}
			else
			{
				num = GameUtilities.RandomBetweenTwoNumbers(2, 5);
				num += Mathf.FloorToInt((float)num2 / 2f);
			}
		}
		LocationGridTile randomMigrationSpawningTile = base.owner.occupiedVillageSpot.GetRandomMigrationSpawningTile();
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < num; i++)
		{
			if (unspawnedCharacters.Count <= 0)
			{
				break;
			}
			PreCharacterData randomElement = CollectionUtilities.GetRandomElement(unspawnedCharacters);
			randomElement.hasBeenSpawned = true;
			unspawnedCharacters.Remove(randomElement);
			string className = "Farmer";
			Character character = CharacterManager.Instance.CreateNewCharacter(randomElement, className, faction, base.owner);
			if (ChanceData.RollChance(CHANCE_TYPE.Noble_Chance))
			{
				character.classComponent.AssignClass("Noble", isInitial: true);
				character.classComponent.OnUpdateCharacterClass();
			}
			else
			{
				character.classComponent.RandomizeCurrentClassBasedOnAbleClasses();
			}
			RelationshipManager.Instance.ApplyPreGeneratedRelationships(randomElement, character);
			for (int j = 0; j < base.owner.residents.Count; j++)
			{
				Character character2 = base.owner.residents[j];
				if (character2 != character)
				{
					if (!character.relationshipContainer.HasRelationshipWith(character2))
					{
						character.relationshipContainer.CreateNewRelationship(character, character2);
					}
					if (!character2.relationshipContainer.HasRelationshipWith(character))
					{
						character2.relationshipContainer.CreateNewRelationship(character2, character);
					}
				}
			}
			character.CreateRandomInitialTraits();
			if (WorldSettings.Instance.worldSettingsData.villageSettings.blessedMigrants)
			{
				character.traitContainer.AddTrait(character, "Blessed");
			}
			character.CreateMarker();
			character.InitialCharacterPlacement(randomMigrationSpawningTile);
			CharacterFinalization.ApplyFactionTypeRelatedEffectToMember(faction, character);
			character.jobComponent.PlanReturnToVillageCenter(JOB_TYPE.RETURN_HOME_URGENT);
			list.Add(character);
			Messenger.Broadcast(WorldEventSignals.NEW_VILLAGER_ARRIVED, character);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "WorldEvents", "WorldEvents_Table", "new_villager", LOG_TAG.Major);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character.homeRegion, character.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			AkSoundEngine.PostEvent("Play_New_Immigrants", InnerMapCameraMove.Instance.gameObject);
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Nullchild_Migrant))
		{
			Character randomElement2 = CollectionUtilities.GetRandomElement(list);
			randomElement2?.traitContainer.AddTrait(randomElement2, "Nullchild");
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Jinxed_Migrant))
		{
			Character randomElement3 = CollectionUtilities.GetRandomElement(list);
			randomElement3?.traitContainer.AddTrait(randomElement3, "Jinxed");
		}
		RuinarchListPool<Character>.Release(list);
	}

	private string GetMigrationMeterBookmarkName()
	{
		return "<b>" + base.owner.name + "</b> " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Incoming Migrants") + " " + Mathf.FloorToInt(GetNormalizedMigrationMeterValue() * 100f) + "%";
	}

	private void CancelCurrentEmptyVillageMigration()
	{
		if (!string.IsNullOrEmpty(_emptyVillageMigrationScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_emptyVillageMigrationScheduleKey);
			_emptyVillageMigrationScheduleKey = string.Empty;
			emptyVillageMigrationDate = default(GameDate);
		}
	}

	private void ScheduleEmptyVillageMigration(GameDate p_schedule)
	{
		emptyVillageMigrationDate = p_schedule;
		_emptyVillageMigrationScheduleKey = SchedulingManager.Instance.AddEntry(emptyVillageMigrationDate, EmptyVillageMigration, base.owner);
	}

	private void EmptyVillageMigration()
	{
		Faction faction;
		if (FactionManager.Instance.HasMajorFactionWithAliveMembers())
		{
			bool flag = FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions;
			faction = ((GameUtilities.RollChance(70) || flag) ? DatabaseManager.Instance.factionDatabase.GetRandomMajorNonPlayerFaction() : CreateNewFactionForEmptyVillageMigration());
		}
		else
		{
			faction = CreateNewFactionForEmptyVillageMigration();
		}
		int num = base.owner.GetStructuresOfType(STRUCTURE_TYPE.DWELLING)?.Count ?? 0;
		int p_max = Mathf.Max(7, num + 3);
		int p_migrantsCount = GameUtilities.RandomBetweenTwoNumbers(Mathf.Max(4, num - 2), p_max);
		string debugLog = string.Empty;
		List<PreCharacterData> list = RuinarchListPool<PreCharacterData>.Claim();
		DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitFaction(list, faction.race, faction);
		LandmarkManager.Instance.OwnSettlement(faction, base.owner);
		Migrate(list, faction, ref debugLog, p_migrantsCount);
		Messenger.Broadcast(FactionSignals.FORCE_FACTION_UI_RELOAD);
		RuinarchListPool<PreCharacterData>.Release(list);
	}

	private Faction CreateNewFactionForEmptyVillageMigration()
	{
		RACE race = ((base.owner.cityCenter == null || !(base.owner.cityCenter.structureObj != null)) ? ((!GameUtilities.RollChance(50)) ? RACE.HUMANS : RACE.ELVES) : (base.owner.cityCenter.structureObj.thinWallResource switch
		{
			WALL_RESOURCE.Wood => RACE.ELVES, 
			WALL_RESOURCE.Stone => RACE.HUMANS, 
			WALL_RESOURCE.Elven_Wood => RACE.ELVES, 
			WALL_RESOURCE.Divine_Stone => RACE.HUMANS, 
			WALL_RESOURCE.Nature_Vines => RACE.ELVES, 
			_ => (!GameUtilities.RollChance(50)) ? RACE.HUMANS : RACE.ELVES, 
		}));
		Faction faction = FactionManager.Instance.CreateNewFaction(FactionManager.Instance.GetFactionTypeForRace(race), "", null, race);
		faction.factionType.SetAsDefault(faction);
		faction.SetLeader(null);
		return faction;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
