using System.Collections;
using System.Collections.Generic;
using Quests;
using UnityEngine;
using UtilityScripts;

public class LoadAdditionalPlayerRelatedSaveData : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadGameAlerts(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadSeizedPOI(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadVictoryCondition(saveData));
		yield return MapGenerator.Instance.StartCoroutine(RemoveInvalidOpinionModifiers(saveData));
		yield return MapGenerator.Instance.StartCoroutine(RemoveInvalidSettlementJobs(saveData));
		yield return MapGenerator.Instance.StartCoroutine(RemoveInvalidDataFromCharacters(saveData));
		yield return MapGenerator.Instance.StartCoroutine(RemoveInvalidIntel(saveData));
		yield return MapGenerator.Instance.StartCoroutine(RemoveInvalidPartyQuests(saveData));
	}

	private IEnumerator LoadGameAlerts(SaveDataCurrentProgress saveData)
	{
		saveData.LoadGameAlerts();
		yield return null;
	}

	private IEnumerator LoadVictoryCondition(SaveDataCurrentProgress saveData)
	{
		QuestManager.Instance.LoadVictoryCondition(saveData.victoryCondition);
		yield return null;
	}

	private IEnumerator LoadSeizedPOI(SaveDataCurrentProgress saveData)
	{
		PlayerManager.Instance.player.seizeComponent.LoadSeizedPOI(saveData.playerSave);
		yield return null;
	}

	private IEnumerator RemoveInvalidOpinionModifiers(SaveDataCurrentProgress saveData)
	{
		if (DatabaseManager.Instance.sharedOpinionDatabase.allSharedOpinionModifiers.Count > 0)
		{
			List<SharedOpinionModifier> list = RuinarchListPool<SharedOpinionModifier>.Claim();
			list.AddRange(DatabaseManager.Instance.sharedOpinionDatabase.allSharedOpinionModifiers.Values);
			for (int i = 0; i < list.Count; i++)
			{
				SharedOpinionModifier sharedOpinionModifier = list[i];
				if (sharedOpinionModifier.targetCharacter == null)
				{
					Debug.Log("Removed invalid shared opinion modifier with id " + sharedOpinionModifier.persistentID);
					sharedOpinionModifier.eventDispatcher.ExecuteModifierExpired(sharedOpinionModifier);
					DatabaseManager.Instance.sharedOpinionDatabase.RemoveSharedOpinion(sharedOpinionModifier);
				}
			}
			RuinarchListPool<SharedOpinionModifier>.Release(list);
		}
		yield return null;
	}

	private IEnumerator RemoveInvalidSettlementJobs(SaveDataCurrentProgress saveData)
	{
		if (DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count > 0)
		{
			for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++)
			{
				NPCSettlement nPCSettlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
				if (nPCSettlement.availableJobs.Count <= 0)
				{
					continue;
				}
				List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
				list.AddRange(nPCSettlement.availableJobs);
				for (int j = 0; j < list.Count; j++)
				{
					JobQueueItem jobQueueItem = list[j];
					if (jobQueueItem is GoapPlanJob { goal: null, targetInteractionType: INTERACTION_TYPE.NONE })
					{
						Debug.Log("Removed invalid job from " + nPCSettlement.name + " with id " + jobQueueItem.persistentID);
						nPCSettlement.RemoveFromAvailableJobs(jobQueueItem);
					}
				}
			}
		}
		yield return null;
	}

	private IEnumerator RemoveInvalidDataFromCharacters(SaveDataCurrentProgress saveData)
	{
		int batchCount = 0;
		if (DatabaseManager.Instance.characterDatabase.allCharacters.Count > 0)
		{
			foreach (KeyValuePair<string, Character> allCharacter in DatabaseManager.Instance.characterDatabase.allCharacters)
			{
				Character value = allCharacter.Value;
				if (value.rumorComponent.negativeInfoPool.Count > 0)
				{
					List<ActualGoapNode> list = RuinarchListPool<ActualGoapNode>.Claim(value.rumorComponent.negativeInfoPool.Count);
					list.AddRange(value.rumorComponent.negativeInfoPool);
					for (int i = 0; i < list.Count; i++)
					{
						ActualGoapNode actualGoapNode = list[i];
						if (actualGoapNode.IsNodeObjectInvalid())
						{
							Debug.Log("Removed invalid rumor from " + value.name + " with id " + actualGoapNode.persistentID);
							value.rumorComponent.negativeInfoPool.Remove(actualGoapNode);
							actualGoapNode.SetIsNegativeInfo(p_state: false);
							if (actualGoapNode.isSupposedToBeInPool && !actualGoapNode.hasBeenReset)
							{
								actualGoapNode.ProcessReturnToPool();
							}
						}
					}
					RuinarchListPool<ActualGoapNode>.Release(list);
				}
				if (value.crimeComponent != null)
				{
					List<CrimeData> list2 = RuinarchListPool<CrimeData>.Claim();
					list2.AddRange(value.crimeComponent.activeCrimes);
					list2.AddRange(value.crimeComponent.reportedCrimes);
					list2.AddRange(value.crimeComponent.witnessedCrimes);
					list2.AddRange(value.crimeComponent.previousCrimes);
					for (int j = 0; j < list2.Count; j++)
					{
						CrimeData crimeData = list2[j];
						bool flag = false;
						if (crimeData.crime is ActualGoapNode actualGoapNode2 && actualGoapNode2.IsNodeObjectInvalid())
						{
							flag = true;
						}
						if (flag)
						{
							Debug.Log("Removed invalid crime from " + value.name + " with id " + crimeData.persistentID);
							if (crimeData.criminal != null)
							{
								Messenger.Broadcast(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, crimeData.criminal, crimeData);
							}
							DatabaseManager.Instance.crimeDatabase.RemoveCrime(crimeData);
						}
					}
				}
				batchCount++;
				if (batchCount >= 50)
				{
					yield return null;
				}
			}
		}
		yield return null;
	}

	private IEnumerator RemoveInvalidIntel(SaveDataCurrentProgress saveData)
	{
		if (PlayerManager.Instance.player.allIntel.Count > 0)
		{
			List<IIntel> list = RuinarchListPool<IIntel>.Claim(PlayerManager.Instance.player.allIntel.Count);
			list.AddRange(PlayerManager.Instance.player.allIntel);
			for (int i = 0; i < list.Count; i++)
			{
				IIntel intel = list[i];
				if (intel is ActionIntel actionIntel)
				{
					if (actionIntel.node.IsNodeObjectInvalid())
					{
						Debug.Log("Removed invalid intel from player " + intel.log.logText);
						PlayerManager.Instance.player.RemoveIntel(intel);
					}
				}
				else if (intel is InterruptIntel interruptIntel && interruptIntel.interruptHolder.actor == null)
				{
					Debug.Log("Removed invalid intel from player " + intel.log.logText);
					PlayerManager.Instance.player.RemoveIntel(intel);
				}
			}
			RuinarchListPool<IIntel>.Release(list);
		}
		yield return null;
	}

	private IEnumerator RemoveInvalidNotifications(SaveDataCurrentProgress saveData)
	{
		for (int i = 0; i < UIManager.Instance.activeNotifications.Count; i++)
		{
			if (!(UIManager.Instance.activeNotifications[i] is IntelNotificationItem { intel: var intel }))
			{
				continue;
			}
			if (intel is ActionIntel actionIntel)
			{
				if (!actionIntel.node.IsNodeObjectInvalid())
				{
				}
			}
			else if (intel is InterruptIntel interruptIntel)
			{
				interruptIntel.interruptHolder.IsImportantDataNull();
			}
		}
		yield return null;
	}

	private IEnumerator RemoveInvalidPartyQuests(SaveDataCurrentProgress saveData)
	{
		foreach (KeyValuePair<string, PartyQuest> allPartyQuest in DatabaseManager.Instance.partyQuestDatabase.allPartyQuests)
		{
			PartyQuest value = allPartyQuest.Value;
			if (value.target != null)
			{
				continue;
			}
			if (value.postedFaction == null)
			{
				if (value.assignedParty != null)
				{
					value.assignedParty.DropQuest(null, PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
				}
			}
			else
			{
				value.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
			}
		}
		yield return null;
	}
}
