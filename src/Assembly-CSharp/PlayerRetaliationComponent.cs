using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine;
using UnityEngine.Localization;
using UtilityScripts;

public class PlayerRetaliationComponent : LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	private const int MAX_RETALIATION_COUNTER = 5;

	public int retaliationCounter { get; private set; }

	public int retaliatorCount { get; private set; }

	public int destroyedStructuresByRetaliatorCounter { get; private set; }

	public List<Character> spawnedRetaliators { get; private set; }

	public bool isRetaliating { get; private set; }

	public RACE retaliatorRace { get; private set; }

	public RuinarchBasicProgress retaliationProgress { get; private set; }

	public PlayerRetaliationComponent()
	{
		spawnedRetaliators = new List<Character>();
		SetRetaliatorCount((WorldSettings.Instance.worldSettingsData.playerSkillSettings.retaliation == RETALIATION.Strong) ? 3 : 2);
		retaliationProgress = new RuinarchBasicProgress(GetRetaliationBookmarkText(), BOOKMARK_TYPE.Progress_Bar);
		retaliationProgress.SetOnHoverOverAction(OnHoverOverRetaliationBookmark);
		retaliationProgress.SetOnHoverOutAction(OnHoverOutRetaliationBookmark);
		retaliationProgress.SetOnSelectAction(OnClickRetaliationBookmark);
		retaliationProgress.Initialize(0, 5);
	}

	public PlayerRetaliationComponent(SaveDataPlayerRetaliationComponent data)
	{
		retaliationCounter = data.retaliationCounter;
		retaliatorCount = data.retaliatorCount;
		destroyedStructuresByRetaliatorCounter = data.destroyedStructuresByRetaliatorCounter;
		isRetaliating = data.isRetaliating;
		retaliationProgress = data.retaliationProgress;
		retaliatorRace = data.retaliatorRace;
		retaliationProgress.SetOnHoverOverAction(OnHoverOverRetaliationBookmark);
		retaliationProgress.SetOnHoverOutAction(OnHoverOutRetaliationBookmark);
		retaliationProgress.SetOnSelectAction(OnClickRetaliationBookmark);
	}

	public void OnCharacterDeath(Character p_character)
	{
		if ((p_character.race == RACE.ANGEL || p_character.race == RACE.SPIRIT) && spawnedRetaliators.Remove(p_character))
		{
			CheckRetaliators();
		}
	}

	public void OnCharacterDisabled(Character p_character)
	{
		if (p_character.race == RACE.ANGEL || p_character.race == RACE.SPIRIT)
		{
			CheckRetaliators();
		}
	}

	private bool AddRetaliationCounter()
	{
		if (!WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed())
		{
			return false;
		}
		if (isRetaliating)
		{
			return false;
		}
		retaliationCounter++;
		PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().SetRetaliationMeter(retaliationCounter);
		retaliationProgress.SetName(GetRetaliationBookmarkText());
		retaliationProgress.SetProgress(retaliationCounter);
		Messenger.Broadcast(PlayerSignals.RETALIATION_INCREASED, retaliationCounter);
		if (retaliationCounter >= 5)
		{
			MaxRetaliationCounterReached();
		}
		return true;
	}

	private void StopRetaliation()
	{
		if (isRetaliating)
		{
			isRetaliating = false;
			retaliationCounter = 0;
			PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().SetRetaliationMeter(retaliationCounter);
			retaliationProgress.SetName(GetRetaliationBookmarkText());
			retaliationProgress.SetProgress(retaliationCounter);
			UnlinkAllRetaliators();
			ResetRetaliatorDestroyedStructuresCounter();
			Messenger.Broadcast(PlayerSignals.STOP_THREAT_EFFECT);
			string value = "Angels";
			if (retaliatorRace == RACE.SPIRIT)
			{
				value = "Nature Spirits";
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "stop_retaliation", LOG_TAG.Player, LOG_TAG.Major);
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}

	private void MaxRetaliationCounterReached()
	{
		ResetRetaliatorDestroyedStructuresCounter();
		DivineIntervention();
		SetRetaliatorCount(retaliatorCount + 1);
		PlayerManager.Instance.player.ClearCharactersThatHaveReported();
	}

	private void DivineIntervention()
	{
		LocationStructure firstStructureOfType = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
		if (firstStructureOfType == null)
		{
			return;
		}
		Region region = firstStructureOfType.region;
		Area randomAreaThatIsNotMountainWaterAndNoCorruption = region.GetRandomAreaThatIsNotMountainWaterAndNoCorruption();
		int num = 0;
		int num2 = 0;
		retaliatorRace = RACE.ANGEL;
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction.isMajorNonPlayer && !faction.isDisbanded)
			{
				if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Nature_Worship))
				{
					num++;
				}
				if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Divine_Worship))
				{
					num2++;
				}
			}
		}
		int chance = 25 * num;
		if (num == 0 && num2 == 0)
		{
			chance = 50;
		}
		else if (num == 0 && num2 > 0)
		{
			chance = 0;
		}
		else if (num > 0 && num2 == 0)
		{
			chance = 100;
		}
		if (GameUtilities.RollChance(chance))
		{
			retaliatorRace = RACE.SPIRIT;
		}
		spawnedRetaliators.Clear();
		for (int j = 0; j < retaliatorCount; j++)
		{
			SUMMON_TYPE summonType = SUMMON_TYPE.Warrior_Angel;
			if (retaliatorRace == RACE.SPIRIT)
			{
				summonType = SUMMON_TYPE.Nature_Spirit;
			}
			else if (UnityEngine.Random.Range(0, 2) == 0)
			{
				summonType = SUMMON_TYPE.Magical_Angel;
			}
			LocationGridTile randomTile = randomAreaThatIsNotMountainWaterAndNoCorruption.gridTileComponent.GetRandomTile();
			Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.retaliatorFaction, null, region);
			summon.behaviourComponent.SetIsAttackingDemonicStructure(state: true, firstStructureOfType as DemonicStructure);
			CharacterManager.Instance.PlaceSummonInitially(summon, randomTile);
			summon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			summon.SetDestroyMarkerOnDeath(state: true);
			spawnedRetaliators.Add(summon);
		}
		isRetaliating = true;
		Messenger.Broadcast(PlayerSignals.START_THREAT_EFFECT);
		string value = "Angels";
		if (retaliatorRace == RACE.SPIRIT)
		{
			value = "Nature Spirits";
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "retaliation", LOG_TAG.Player, LOG_TAG.Major);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private string GetRetaliationBookmarkText()
	{
		return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Retaliation") + ": " + retaliationCounter + "/" + 5;
	}

	private void SetRetaliatorCount(int amount)
	{
		retaliatorCount = amount;
	}

	private void ResetRetaliatorDestroyedStructuresCounter()
	{
		destroyedStructuresByRetaliatorCounter = 0;
	}

	public void AddDestroyedStructureByRetaliators()
	{
		destroyedStructuresByRetaliatorCounter++;
		if (destroyedStructuresByRetaliatorCounter >= retaliatorCount - 1)
		{
			StopRetaliation();
		}
	}

	private void UnlinkAllRetaliators()
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(spawnedRetaliators);
		for (int i = 0; i < list.Count; i++)
		{
			Character character = list[i];
			if (!character.isDead)
			{
				character.behaviourComponent.SetIsAttackingDemonicStructure(state: false, null);
				if (!character.traitContainer.HasTrait("Restrained", "Unconscious", "Paralyzed"))
				{
					character.Death("disappear");
				}
				else
				{
					character.ChangeFactionTo(FactionManager.Instance.wildMonsterFaction);
				}
			}
		}
		RuinarchListPool<Character>.Release(list);
		spawnedRetaliators.Clear();
	}

	private void CheckRetaliators()
	{
		if (!HasActiveRetaliator())
		{
			StopRetaliation();
		}
	}

	private bool HasActiveRetaliator()
	{
		for (int i = 0; i < spawnedRetaliators.Count; i++)
		{
			Character character = spawnedRetaliators[i];
			if (!character.traitContainer.HasTrait("Restrained", "Unconscious", "Paralyzed") && !character.isDead)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRetaliator(Character p_character)
	{
		return spawnedRetaliators.Contains(p_character);
	}

	public void CharacterDeathRetaliation(Character p_character)
	{
		string log = string.Empty;
		if (GameUtilities.RollChance(WorldSettings.Instance.worldSettingsData.playerSkillSettings.retaliation switch
		{
			RETALIATION.Normal => ChanceData.GetChance(CHANCE_TYPE.Retaliation_Character_Death), 
			RETALIATION.Disabled => 0, 
			RETALIATION.Strong => 100, 
			_ => throw new ArgumentOutOfRangeException(), 
		}, ref log) && !p_character.traitContainer.HasTrait("Demon Cultist") && p_character.isNormalCharacter && p_character.faction != null && p_character.faction.isMajorNonPlayer && AddRetaliationCounter())
		{
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "retaliation_character_death", LOG_TAG.Player, LOG_TAG.Major);
			log2.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
		}
	}

	public void StructureDestroyedRetaliation(LocationStructure p_structure)
	{
		if (p_structure.settlementLocation != null && p_structure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE && p_structure.settlementLocation.owner != null && p_structure.settlementLocation.owner.isMajorNonPlayer && !p_structure.settlementLocation.owner.IsFriendlyWith(PlayerManager.Instance.player.playerFaction))
		{
			string log = string.Empty;
			if (GameUtilities.RollChance(WorldSettings.Instance.worldSettingsData.playerSkillSettings.retaliation switch
			{
				RETALIATION.Normal => ChanceData.GetChance(CHANCE_TYPE.Retaliation_Structure_Destroy), 
				RETALIATION.Disabled => 0, 
				RETALIATION.Strong => 100, 
				_ => throw new ArgumentOutOfRangeException(), 
			}, ref log) && AddRetaliationCounter())
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "retaliation_structure_destroyed", LOG_TAG.Player, LOG_TAG.Major);
				log2.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
			}
		}
	}

	public void ResourcePileRetaliation(TileObject p_pile, LocationGridTile removedFrom)
	{
		if (removedFrom != null && removedFrom.structure.settlementLocation != null && removedFrom.structure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE && removedFrom.structure.settlementLocation.owner != null && !removedFrom.structure.settlementLocation.owner.IsFriendlyWith(PlayerManager.Instance.player.playerFaction))
		{
			string log = string.Empty;
			if (GameUtilities.RollChance(WorldSettings.Instance.worldSettingsData.playerSkillSettings.retaliation switch
			{
				RETALIATION.Normal => ChanceData.GetChance(CHANCE_TYPE.Retaliation_Resource_Pile), 
				RETALIATION.Strong => 100, 
				RETALIATION.Disabled => 0, 
				_ => throw new ArgumentOutOfRangeException(), 
			}, ref log) && AddRetaliationCounter())
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "retaliation_pile_loss", LOG_TAG.Player, LOG_TAG.Major);
				log2.AddToFillers(p_pile, p_pile.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
			}
		}
	}

	public void ReportDemonicStructureRetaliation(Character p_character)
	{
		if (AddRetaliationCounter())
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "retaliation_report_structure", LOG_TAG.Player, LOG_TAG.Major);
			log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}

	public void UnderSiegeRetaliation(NPCSettlement p_settlement, Character p_siegeTrigger)
	{
		string log = string.Empty;
		if (GameUtilities.RollChance(WorldSettings.Instance.worldSettingsData.playerSkillSettings.retaliation switch
		{
			RETALIATION.Normal => ChanceData.GetChance(CHANCE_TYPE.Retaliation_Under_Siege), 
			RETALIATION.Disabled => 0, 
			RETALIATION.Strong => 100, 
			_ => throw new ArgumentOutOfRangeException(), 
		}, ref log) && p_settlement.owner != null && p_settlement.owner.factionType.type != FACTION_TYPE.Demon_Cult && p_settlement.owner.isMajorFaction && p_siegeTrigger.faction != null && p_siegeTrigger.faction.isPlayerFaction && p_siegeTrigger.partyComponent.hasParty && p_siegeTrigger.partyComponent.currentParty.isPlayerParty && AddRetaliationCounter())
		{
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "retaliation_siege", LOG_TAG.Player, LOG_TAG.Major);
			log2.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
			log2.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
		}
	}

	public void LoadReferences(SaveDataPlayerRetaliationComponent data)
	{
		spawnedRetaliators = SaveUtilities.ConvertIDListToCharacters(data.spawnedRetaliators);
	}

	private void OnHoverOverRetaliationBookmark(UIHoverPosition position)
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Retaliation_Bookmark_Tooltip"), position);
	}

	private void OnHoverOutRetaliationBookmark()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnClickRetaliationBookmark()
	{
		if (spawnedRetaliators.Count > 0)
		{
			CharacterCenterCycle(spawnedRetaliators);
		}
	}

	private void CharacterCenterCycle(List<Character> characters)
	{
		if (characters != null && characters.Count > 0)
		{
			ISelectable nextCharacterToCenter = GetNextCharacterToCenter(characters);
			if (nextCharacterToCenter != null)
			{
				InputManager.Instance.Select(nextCharacterToCenter);
			}
		}
	}

	private Character GetNextCharacterToCenter(List<Character> selectables)
	{
		Character character = null;
		for (int i = 0; i < selectables.Count; i++)
		{
			if (selectables[i].IsCurrentlySelected())
			{
				character = CollectionUtilities.GetNextElementCyclic(selectables, i);
				break;
			}
		}
		if (character == null)
		{
			character = selectables[0];
		}
		return character;
	}

	public void OnLocaleChanged(Locale locale)
	{
		retaliationProgress.SetName(GetRetaliationBookmarkText());
	}
}
