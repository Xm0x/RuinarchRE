using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements.Settlement_Events;
using TMPro;
using UnityEngine;

namespace UtilityScripts;

public static class TestingUtilities
{
	public static void ShowLocationInfo(Region region, NPCSettlement p_settlement)
	{
		string text = region.name + " Info:";
		text += "\nSpells in Region:";
		foreach (KeyValuePair<LocationGridTile, List<AOESpellTileObject>> item in region.regionSpellsComponent.activeSpellsInRegion)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				AOESpellTileObject aOESpellTileObject = item.Value[i];
				text = text + "\n- " + aOESpellTileObject.nameWithID + " at " + item.Key.ToString() + ":" + aOESpellTileObject.GetAOESpellTestingData();
			}
		}
		text += "\n-----------------------------";
		text += "\nLocations Info:";
		text = text + "\nLast claim date: " + p_settlement.dateVillageWasLastClaimed.ToString();
		text = text + "\nLinked Beast Dens: " + p_settlement.occupiedVillageSpot?.GetLinkedBeastDensSummary();
		text = text + "\nLinked Structures: " + p_settlement.structureComponent.GetLinkedStructuresSummary();
		text = text + "\nFires in settlement: " + p_settlement.firesToDouseInSettlement.ComafyList();
		if (p_settlement.expirationComponent.expirationDate.hasValue)
		{
			text = text + "\nExpiration date: " + p_settlement.expirationComponent.expirationDate.ToString();
		}
		Faction owner = p_settlement.owner;
		bool flag = owner != null && owner.factionType.type == FACTION_TYPE.Ratmen;
		if (p_settlement.locationType != LOCATION_TYPE.VILLAGE && p_settlement.locationType != LOCATION_TYPE.PSEUDO_VILLAGE && !flag)
		{
			return;
		}
		if (!flag)
		{
			text = text + "\n<b>" + p_settlement.name + "</b> Settlement Type: " + (p_settlement.settlementType?.settlementType.ToString() ?? "None");
			text = text + "\nIs Under Siege: " + p_settlement.isUnderSiege;
			text += $"\nMax Facilities: {p_settlement.settlementType?.maxFacilities}";
			text += $"\nMax Dwellings: {p_settlement.settlementType?.maxDwellings}";
			text = text + "\nMorning Needed Class: " + p_settlement.classComponent.morningScheduleDateForProcessingOfNeededClasses.ToString();
			text = text + "\nAfternoon Needed Class: " + p_settlement.classComponent.afternoonScheduleDateForProcessingOfNeededClasses.ToString();
			text = text + "\nParty Quests Processing: " + p_settlement.partyComponent.scheduleDateForProcessingOfPartyQuests.ToString();
			text = text + "\nFaction Ideology Processing: " + p_settlement.factionIdeologyComponent.scheduleDateForProcessingOfEvents.ToString();
			text += $"\nBuilt Resource Piles in Settlement: {p_settlement.SettlementResources.GetBuiltResourcePileCount(p_settlement)}";
			text = text + "\nStorage: " + (p_settlement.mainStorage?.name ?? "None") + ". Prison: " + (p_settlement.prison?.name ?? "None");
			text += $"\nTrees Count: {p_settlement.SettlementResources.GetTreesCount(p_settlement)}";
			text += $"\nFishing Spots Count: {p_settlement.SettlementResources.GetFishingSpotCount(p_settlement)}";
			text += $"\nCharacters Inside: {p_settlement.SettlementResources.GetCharacterCount(p_settlement)}";
			text += "\nNeeded Items: ";
			for (int j = 0; j < p_settlement.neededObjects.Count; j++)
			{
				text = text + "|" + p_settlement.neededObjects[j].ToString() + "|";
			}
			text += "\nActive Events: ";
			for (int k = 0; k < p_settlement.eventManager.activeEvents.Count; k++)
			{
				SettlementEvent settlementEvent = p_settlement.eventManager.activeEvents[k];
				text = text + "|" + settlementEvent.GetTestingInfo() + "|";
			}
		}
		if (p_settlement.owner != null)
		{
			text = text + "\n" + p_settlement.name + " Location Job Queue:";
			if (p_settlement.availableJobs.Count > 0)
			{
				for (int l = 0; l < p_settlement.availableJobs.Count; l++)
				{
					JobQueueItem jobQueueItem = p_settlement.availableJobs[l];
					if (jobQueueItem is GoapPlanJob goapPlanJob)
					{
						text = text + "\n<b>" + goapPlanJob.name + " Targeting " + (goapPlanJob.targetPOI?.ToString() ?? "None") + "</b>";
						if (Input.GetKey(KeyCode.LeftShift))
						{
							text += "\nAdditional data";
							text = text + "\n\t-Plan: " + goapPlanJob.assignedPlan?.LogPlan();
							if (goapPlanJob.otherData != null)
							{
								text += "\n\t-Other Data:";
								foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in goapPlanJob.otherData)
								{
									text = text + "\n\t\t-" + otherDatum.Key.ToString() + ", " + otherDatum.Value.ComafyList() + ":";
								}
							}
						}
						else
						{
							text += ", ";
						}
					}
					else
					{
						text = text + "\n<b>" + jobQueueItem.name + "</b>";
					}
					text = text + "\n Assigned Character: " + jobQueueItem.assignedCharacter?.name;
				}
			}
			else
			{
				text += "\nNone";
			}
			if (!flag && p_settlement.owner != null)
			{
				text += $"\nAdditional Migration Gain: {p_settlement.owner.factionType.GetAdditionalMigrationMeterGain(p_settlement)}";
				text += "\n-----------------------------";
				text = text + "\n" + p_settlement.owner.name + " Faction Job Queue:";
				if (p_settlement.owner.availableJobs.Count > 0)
				{
					for (int m = 0; m < p_settlement.owner.availableJobs.Count; m++)
					{
						JobQueueItem jobQueueItem2 = p_settlement.owner.availableJobs[m];
						if (jobQueueItem2 is GoapPlanJob goapPlanJob2)
						{
							text = text + "\n<b>" + goapPlanJob2.name + " Targeting " + (goapPlanJob2.targetPOI?.ToString() ?? "None") + "</b>";
							if (Input.GetKey(KeyCode.LeftShift))
							{
								text += "\nAdditional data";
								text = text + "\n\t-Plan: " + goapPlanJob2.assignedPlan?.LogPlan();
								if (goapPlanJob2.otherData != null)
								{
									text += "\n\t-Other Data:";
									foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum2 in goapPlanJob2.otherData)
									{
										text = text + "\n\t\t-" + otherDatum2.Key.ToString() + ", " + otherDatum2.Value.ComafyList() + ":";
									}
								}
							}
							else
							{
								text += ", ";
							}
						}
						else
						{
							text = text + "\n<b>" + jobQueueItem2.name + "</b>";
						}
						text = text + "\n Assigned Character: " + jobQueueItem2.assignedCharacter?.name;
					}
				}
				else
				{
					text += "\nNone";
				}
				text += "\n-----------------------------";
				text = text + "\n" + p_settlement.owner.name + " Party Quests:";
				if (p_settlement.owner.partyQuestBoard.availablePartyQuests.Count > 0)
				{
					for (int n = 0; n < p_settlement.owner.partyQuestBoard.availablePartyQuests.Count; n++)
					{
						PartyQuest partyQuest = p_settlement.owner.partyQuestBoard.availablePartyQuests[n];
						text = text + "\n<b>" + partyQuest.partyQuestType.ToString() + " targeting " + partyQuest.target?.ToString() + "</b>";
						text = text + "(Assigned Party: " + partyQuest.assignedParty?.partyName + ")";
					}
				}
				else
				{
					text += "\nNone";
				}
			}
		}
		text += "\n";
		UIManager.Instance.ShowSmallInfo(text);
	}

	public static void HideLocationInfo()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private static List<NPCSettlement> GetSettlementsInRegion(Region region)
	{
		List<NPCSettlement> list = new List<NPCSettlement>();
		for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement item = LandmarkManager.Instance.allNonPlayerSettlements[i];
			list.Add(item);
		}
		return list;
	}

	public static void ShowCharacterTestingInfo(Character activeCharacter)
	{
		RaceManager.Instance.GetRaceData(activeCharacter.race);
	}

	public static void HideCharacterTestingInfo()
	{
	}

	public static void ShowTileObjectTestingInfo(TileObject p_tileObject)
	{
		string arg = p_tileObject.name + " info:";
		arg = $"{arg}\nID: {p_tileObject.id}";
		arg = arg + "\nPID: " + p_tileObject.persistentID;
		arg = arg + "\n<b>Object State:</b> " + p_tileObject.state;
		arg = arg + "\n<b>Is Available:</b> " + p_tileObject.IsAvailable();
		arg = arg + "\n<b>Character Owner:</b> " + (p_tileObject.characterOwner?.name ?? "None");
		arg = arg + "\n<b>Faction Owner:</b> " + (p_tileObject.factionOwner?.name ?? "None");
		arg += p_tileObject.GetAdditionalTestingData();
		UIManager.Instance.ShowSmallInfo(arg, "", autoReplaceText: true, TextOverflowModes.Page);
	}

	public static void HideTileObjectTestingInfo()
	{
	}
}
