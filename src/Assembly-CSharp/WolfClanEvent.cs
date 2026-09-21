using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

public class WolfClanEvent : PrismEvent
{
	public int numberOfAliveMasterLycan { get; private set; }

	public WolfClanEvent(PrismEventData p_data)
		: base(p_data)
	{
		base.requirements = new PrismEventRequirement[2]
		{
			new PrismEventRequirement("Wolf_Clan_Event_Requirement_1", IsMasterLycanRequirementSatisfied),
			new PrismEventRequirement("Wolf_Clan_Event_Requirement_2", IsLycanFactionRequirementSatisfied)
		};
	}

	public void AdjustNumberOfAliveMasterLycan(int p_amount)
	{
		numberOfAliveMasterLycan += p_amount;
	}

	public void SetNumberOfAliveMasterLycan(int p_amount)
	{
		numberOfAliveMasterLycan = p_amount;
	}

	private void GenerateLycansAndLycanFaction()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Prism", "PrismEvents_Table", "Wolf_Clan_Migration_Effect", LOG_TAG.Major);
		log.AddLogToDatabase();
		PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		LocationGridTile randomPassableEdgeTile = GridMap.Instance.mainRegion.innerMap.GetRandomPassableEdgeTile();
		Character character = null;
		RACE race = RACE.HUMANS;
		if (GameUtilities.RollChance(50))
		{
			race = RACE.ELVES;
		}
		for (int i = 0; i < 5; i++)
		{
			CharacterClass characterClass = ((i != 1) ? CharacterManager.Instance.GetRandomLowTierCombatant() : (GameUtilities.RandomBetweenTwoNumbers(0, 3) switch
			{
				0 => CharacterManager.Instance.GetCharacterClass("Farmer"), 
				1 => CharacterManager.Instance.GetCharacterClass("Fisher"), 
				_ => CharacterManager.Instance.GetCharacterClass("Miner"), 
			}));
			GENDER randomGender = Utilities.GetRandomGender();
			Character character2 = CharacterManager.Instance.CreateNewCharacter(characterClass.className, race, randomGender);
			character2.CreateMarker();
			character2.InitialCharacterPlacement(randomPassableEdgeTile);
			new LycanthropeData(character2);
			if (character == null)
			{
				character2.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character2, "create_lycan_clan");
				character = character2;
			}
			else
			{
				character2.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, character, "join_faction_lycan_prism_event");
			}
		}
		VillageSpot firstUnoccupiedVillageSpotThatCanAccomodateFaction = character.currentRegion.GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(FACTION_TYPE.Lycan_Clan);
		if (firstUnoccupiedVillageSpotThatCanAccomodateFaction != null)
		{
			Area coreSpot = firstUnoccupiedVillageSpotThatCanAccomodateFaction.coreSpot;
			StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, character.faction.factionType.mainResource);
			GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(character.faction.factionType.type, structureSetting));
			if (LandmarkManager.Instance.HasEnoughSpaceForStructure(randomElement.name, coreSpot.gridTileComponent.centerGridTile))
			{
				character.jobComponent.TriggerFindNewVillage(coreSpot.gridTileComponent.centerGridTile, randomElement.name);
			}
		}
	}

	private bool IsMasterLycanRequirementSatisfied()
	{
		return numberOfAliveMasterLycan > 0;
	}

	private bool IsLycanFactionRequirementSatisfied()
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (faction.factionType.type == FACTION_TYPE.Lycan_Clan && faction.HasAliveMember())
			{
				return false;
			}
		}
		return true;
	}

	protected override void TriggerEventBase()
	{
		base.TriggerEventBase();
		GenerateLycansAndLycanFaction();
	}
}
