using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

namespace Interrupts;

public class SetHome : Interrupt
{
	public SetHome()
		: base(INTERRUPT.Set_Home)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		LocationStructure homeStructure = actor.homeStructure;
		if (interruptHolder.target != null && interruptHolder.target != actor)
		{
			if (interruptHolder.target is Character character)
			{
				actor.MigrateHomeStructureTo(character.homeStructure);
			}
			else if (interruptHolder.target is GenericTileObject genericTileObject)
			{
				actor.MigrateHomeStructureTo(genericTileObject.gridTileLocation.structure);
			}
		}
		else
		{
			SetNewHomeSettlement(actor);
		}
		if (actor.homeStructure != null && actor.homeStructure != actor.previousCharacterDataComponent.previousHomeStructure && actor.homeStructure != homeStructure)
		{
			if (overrideEffectLog != null)
			{
				LogPool.Release(overrideEffectLog);
			}
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " set_new_home_structure", base.logTags);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(actor.homeStructure, actor.homeStructure.name, LOG_IDENTIFIER.LANDMARK_1);
		}
		return true;
	}

	private void SetNewHomeSettlement(Character actor)
	{
		string log = string.Empty;
		Region currentRegion = actor.currentRegion;
		if (actor is Summon)
		{
			LocationStructure randomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles = currentRegion.GetRandomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles();
			if (randomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles != null)
			{
				actor.ClearTerritoryAndMigrateHomeStructureTo(randomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles);
				return;
			}
			if (!actor.HasTerritory() && Random.Range(0, 2) == 0)
			{
				Area randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage = currentRegion.GetRandomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage(actor);
				if (randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage != null)
				{
					actor.SetTerritory(randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage);
					return;
				}
			}
		}
		else if (actor.isVagrantOrFactionless)
		{
			SetNewHomeSettlementForVagrant(actor, ref log);
		}
		else
		{
			SetNewHomeSettlementForNonVagrant(actor, ref log);
		}
		if (actor.homeStructure != null && actor.homeStructure.hasBeenDestroyed)
		{
			actor.MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement: false);
		}
	}

	private void SetNewHomeSettlementForVagrant(Character actor, ref string log)
	{
		Region currentRegion = actor.currentRegion;
		if (actor.homeStructure != null && !actor.homeStructure.hasBeenDestroyed)
		{
			return;
		}
		if (Random.Range(0, 100) < 40)
		{
			LocationStructure randomStructureThatIsHabitableAndUnoccupiedButNot = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
			if (randomStructureThatIsHabitableAndUnoccupiedButNot != null)
			{
				actor.ClearTerritoryAndMigrateHomeStructureTo(randomStructureThatIsHabitableAndUnoccupiedButNot);
				return;
			}
		}
		if (Random.Range(0, 100) < 20)
		{
			BaseSettlement firstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf = currentRegion.GetFirstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf(actor);
			if (firstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf != null)
			{
				LocationStructure structureInSettlementPrioritizeDwellingsExceptPrevious = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(firstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf, actor);
				if (structureInSettlementPrioritizeDwellingsExceptPrevious != null)
				{
					actor.ClearTerritoryAndMigrateHomeStructureTo(structureInSettlementPrioritizeDwellingsExceptPrevious);
					return;
				}
			}
		}
		if (!actor.HasTerritory())
		{
			Area randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption = currentRegion.GetRandomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption(actor);
			if (randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption != null)
			{
				actor.SetTerritory(randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption);
			}
		}
	}

	private void SetNewHomeSettlementForNonVagrant(Character actor, ref string log)
	{
		Region currentRegion = actor.currentRegion;
		if (actor.homeSettlement != null && actor.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			if (Random.Range(0, 100) >= 35 || !actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) || !actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement))
			{
				return;
			}
			LocationStructure randomStructureThatIsHabitableAndUnoccupiedButNot = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
			if (randomStructureThatIsHabitableAndUnoccupiedButNot != null)
			{
				actor.ClearTerritoryAndMigrateHomeStructureTo(randomStructureThatIsHabitableAndUnoccupiedButNot);
				return;
			}
			Area randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption = currentRegion.GetRandomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption(actor);
			if (randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption != null)
			{
				actor.ClearTerritory();
				actor.SetTerritory(randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption);
			}
		}
		else if (actor.homeStructure != null && !actor.homeStructure.hasBeenDestroyed && actor.homeStructure.HasStructureTag(STRUCTURE_TAG.Shelter))
		{
			int chance = 20;
			if (actor.homeStructure.HasAliveResident(actor))
			{
				chance = 3;
			}
			if (GameUtilities.RollChance(chance))
			{
				if (GameUtilities.RollChance(30) && actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement))
				{
					FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(actor, ref log);
				}
				else
				{
					FindNewVillageProcessing(actor, checkIfThereAreOtherFindVillageJob: true, ref log);
				}
			}
		}
		else
		{
			if ((Random.Range(0, 100) < 35 && actor.isFactionLeader && !actor.faction.HasOwnedVillages() && actor.currentRegion != null && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached() && FindNewVillageProcessing(actor, checkIfThereAreOtherFindVillageJob: false, ref log)) || (Random.Range(0, 100) < 3 && !WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !actor.currentRegion.IsRegionVillageCapacityReached() && FindNewVillageProcessing(actor, checkIfThereAreOtherFindVillageJob: true, ref log)))
			{
				return;
			}
			int num = Random.Range(0, 100);
			LocationStructure locationStructure = null;
			if (num < 80 && actor.faction.HasOwnedSettlementExcept(actor.homeSettlement) && actor.faction.HasOwnedSettlementExcept(actor.previousCharacterDataComponent.previousHomeSettlement) && FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(actor, ref log))
			{
				return;
			}
			if (Random.Range(0, 100) < 15)
			{
				BaseSettlement firstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf = currentRegion.GetFirstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf(actor);
				if (firstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf != null)
				{
					locationStructure = GetStructureInSettlementPrioritizeDwellingsExceptPrevious(firstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf, actor);
					if (locationStructure != null)
					{
						actor.ClearTerritoryAndMigrateHomeStructureTo(locationStructure);
						return;
					}
				}
			}
			if (Random.Range(0, 100) < 15)
			{
				locationStructure = currentRegion.GetRandomStructureThatIsHabitableAndUnoccupiedButNot(actor.previousCharacterDataComponent.previousHomeStructure);
				if (locationStructure != null)
				{
					actor.ClearTerritoryAndMigrateHomeStructureTo(locationStructure);
					return;
				}
			}
			Area randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption2 = currentRegion.GetRandomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption(actor);
			if (randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption2 != null)
			{
				actor.ClearTerritory();
				actor.SetTerritory(randomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption2);
			}
		}
	}

	private LocationStructure FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(Character actor, ref string identifier)
	{
		LocationStructure locationStructure = null;
		LocationStructure locationStructure2 = null;
		LocationStructure locationStructure3 = null;
		for (int i = 0; i < actor.faction.ownedSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = actor.faction.ownedSettlements[i];
			if (baseSettlement == actor.homeSettlement || baseSettlement == actor.previousCharacterDataComponent.previousHomeSettlement)
			{
				continue;
			}
			if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE && baseSettlement is NPCSettlement nPCSettlement)
			{
				locationStructure3 = nPCSettlement.GetFirstStructureThatIsUnoccupiedDwelling(actor.previousCharacterDataComponent.previousHomeStructure);
				if (locationStructure3 != null)
				{
					identifier = "unoccupied";
					return locationStructure3;
				}
				locationStructure = GetDwellingWithCloseFriendOrNonRivalEnemyRelativeInSameFaction(nPCSettlement, actor);
			}
			if (locationStructure2 != null || baseSettlement.locationType != LOCATION_TYPE.DUNGEON)
			{
				continue;
			}
			for (int j = 0; j < baseSettlement.allStructures.Count; j++)
			{
				LocationStructure locationStructure4 = baseSettlement.allStructures[j];
				if (locationStructure4 != actor.previousCharacterDataComponent.previousHomeStructure && !locationStructure4.HasReachedMaxResidentCapacity() && !IsSameAsCurrentHomeStructure(locationStructure4, actor) && locationStructure4.HasCloseFriendOrNonEnemyRivalRelativeInSameFaction(actor))
				{
					locationStructure2 = locationStructure4;
					break;
				}
			}
		}
		if (locationStructure != null)
		{
			identifier = "occupied";
			return locationStructure;
		}
		if (locationStructure2 != null)
		{
			identifier = "habitable";
			return locationStructure2;
		}
		return null;
	}

	private bool FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlements(Character actor, ref string log)
	{
		string identifier = string.Empty;
		LocationStructure locationStructure = FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsProcessing(actor, ref identifier);
		if (locationStructure != null && identifier == "unoccupied")
		{
			actor.ClearTerritoryAndMigrateHomeStructureTo(locationStructure);
			return true;
		}
		if (locationStructure != null && identifier == "occupied")
		{
			actor.ClearTerritoryAndMigrateHomeStructureTo(locationStructure);
			return true;
		}
		if (locationStructure != null && identifier == "habitable")
		{
			actor.ClearTerritoryAndMigrateHomeStructureTo(locationStructure);
			return true;
		}
		return false;
	}

	private bool FindHabitableStructureOrUnoccupiedHouseInOneOfOwnedSettlementsOrCityCenterWithLeastNumberOfVillagers(Character actor, ref string log)
	{
		LocationStructure firstStructureOfTypeFromOwnedSettlementsWithLeastVillagers = GetFirstStructureOfTypeFromOwnedSettlementsWithLeastVillagers(STRUCTURE_TYPE.CITY_CENTER, actor.faction, actor);
		if (firstStructureOfTypeFromOwnedSettlementsWithLeastVillagers != null)
		{
			actor.ClearTerritoryAndMigrateHomeStructureTo(firstStructureOfTypeFromOwnedSettlementsWithLeastVillagers);
			return true;
		}
		return false;
	}

	private bool FindNewVillageProcessing(Character actor, bool checkIfThereAreOtherFindVillageJob, ref string log)
	{
		if (actor.traitContainer.HasTrait("Enslaved"))
		{
			return false;
		}
		VillageSpot villageSpot = ((actor.faction != null) ? actor.currentRegion.GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(actor.faction.factionType.type) : actor.currentRegion.GetFirstUnoccupiedVillageSpot());
		if (villageSpot != null)
		{
			Area coreSpot = villageSpot.coreSpot;
			if (!checkIfThereAreOtherFindVillageJob || !FactionMemberAlreadyHasFindVillageJob(actor.faction))
			{
				StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, actor.faction.factionType.mainResource);
				GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(actor.faction.factionType.type, structureSetting));
				actor.jobComponent.TriggerFindNewVillage(coreSpot.gridTileComponent.centerGridTile, randomElement.name);
				return true;
			}
		}
		return false;
	}

	private LocationStructure GetDwellingWithCloseFriendOrNonRivalEnemyRelativeInSameFaction(NPCSettlement settlement, Character actor)
	{
		LocationStructure result = null;
		List<LocationStructure> structuresOfType = settlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				LocationStructure locationStructure = structuresOfType[i];
				if (locationStructure != actor.previousCharacterDataComponent.previousHomeStructure && !locationStructure.HasReachedMaxResidentCapacity() && locationStructure.residents.Count > 0 && !IsSameAsCurrentHomeStructure(locationStructure, actor) && locationStructure.HasCloseFriendOrNonEnemyRivalRelativeInSameFaction(actor))
				{
					result = locationStructure;
					break;
				}
			}
		}
		return result;
	}

	private LocationStructure GetStructureInSettlementPrioritizeDwellingsExceptPrevious(BaseSettlement settlement, Character actor)
	{
		LocationStructure locationStructure = null;
		for (int i = 0; i < settlement.allStructures.Count; i++)
		{
			LocationStructure locationStructure2 = settlement.allStructures[i];
			if (locationStructure2 != actor.previousCharacterDataComponent.previousHomeStructure && locationStructure2 != actor.homeStructure)
			{
				if (locationStructure2 is Dwelling)
				{
					return locationStructure2;
				}
				if (locationStructure == null)
				{
					locationStructure = locationStructure2;
				}
			}
		}
		return locationStructure;
	}

	private LocationStructure GetFirstStructureOfTypeFromOwnedSettlementsWithLeastVillagers(STRUCTURE_TYPE structureType, Faction faction, Character actor)
	{
		BaseSettlement baseSettlement = null;
		LocationStructure result = null;
		for (int i = 0; i < faction.ownedSettlements.Count; i++)
		{
			BaseSettlement baseSettlement2 = faction.ownedSettlements[i];
			if (baseSettlement2 != actor.previousCharacterDataComponent.previousHomeSettlement && baseSettlement2 != actor.homeSettlement)
			{
				LocationStructure firstStructureWithStructureType = baseSettlement2.GetFirstStructureWithStructureType(structureType, actor.previousCharacterDataComponent.previousHomeStructure, actor.homeStructure);
				if (firstStructureWithStructureType != null && (baseSettlement == null || baseSettlement2.residents.Count < baseSettlement.residents.Count))
				{
					baseSettlement = baseSettlement2;
					result = firstStructureWithStructureType;
				}
			}
		}
		return result;
	}

	private bool IsSameAsCurrentHomeStructure(LocationStructure p_structure, Character p_character)
	{
		return p_structure == p_character.homeStructure;
	}

	private bool FactionMemberAlreadyHasFindVillageJob(Faction faction)
	{
		for (int i = 0; i < faction.characters.Count; i++)
		{
			Character character = faction.characters[i];
			if (!character.isDead && character.jobQueue.HasJob(JOB_TYPE.FIND_NEW_VILLAGE))
			{
				return true;
			}
		}
		return false;
	}
}
