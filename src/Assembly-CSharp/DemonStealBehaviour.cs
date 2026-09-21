using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class DemonStealBehaviour : CharacterBehaviour
{
	public DemonStealBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working)
		{
			DemonStealPartyQuest demonStealPartyQuest = currentParty.currentQuest as DemonStealPartyQuest;
			if (demonStealPartyQuest.targetItem != null)
			{
				if (demonStealPartyQuest.targetItem.gridTileLocation != null && demonStealPartyQuest.targetItem.gridTileLocation.structure == demonStealPartyQuest.dropStructure)
				{
					demonStealPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Item_At_Target"));
					return true;
				}
				if (demonStealPartyQuest.targetItem.isBeingSeized)
				{
					if (demonStealPartyQuest.targetItem.previousTile == null || demonStealPartyQuest.targetItem.previousTile == character.gridTileLocation)
					{
						demonStealPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Unavailable"));
						return true;
					}
					result = character.jobComponent.CreateGoToSpecificTileJob(demonStealPartyQuest.targetItem.previousTile, out producedJob);
				}
				else
				{
					Character memberInCombatExcept = currentParty.GetMemberInCombatExcept(character);
					if (memberInCombatExcept != null)
					{
						bool flag = false;
						CombatState combatState = memberInCombatExcept.stateComponent.currentState as CombatState;
						if (combatState.currentClosestHostile != null)
						{
							CombatData combatData = memberInCombatExcept.combatComponent.GetCombatData(combatState.currentClosestHostile);
							if (character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal))
							{
								flag = true;
							}
						}
						if (flag)
						{
							producedJob = null;
							return true;
						}
					}
					if (currentParty.membersThatJoinedQuest.Contains(demonStealPartyQuest.targetItem.isBeingCarriedBy) && demonStealPartyQuest.targetItem.isBeingCarriedBy.gridTileLocation != null)
					{
						if (demonStealPartyQuest.targetItem.isBeingCarriedBy == character)
						{
							if (demonStealPartyQuest.dropStructure.hasBeenDestroyed)
							{
								demonStealPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Structure_Destroyed"));
								return true;
							}
							if (demonStealPartyQuest.dropStructure != null && !demonStealPartyQuest.dropStructure.HasUnoccupiedTile())
							{
								demonStealPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Structure_Full"));
								return true;
							}
							result = character.jobComponent.CreateDropItemJob(JOB_TYPE.DEMON_STEAL, demonStealPartyQuest.targetItem, demonStealPartyQuest.dropStructure, out producedJob);
						}
						else
						{
							List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
							demonStealPartyQuest.targetItem.isBeingCarriedBy.gridTileLocation.PopulateTilesInRadius(list, 1, 0, includeCenterTile: false, includeTilesInDifferentStructure: false, includeImpassable: false);
							if (list.Count > 0)
							{
								LocationGridTile tile = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
								result = character.jobComponent.CreateGoToSpecificTileJob(tile, out producedJob);
							}
							else
							{
								result = character.jobComponent.CreateGoToSpecificTileJob(demonStealPartyQuest.targetItem.isBeingCarriedBy.gridTileLocation, out producedJob);
							}
							RuinarchListPool<LocationGridTile>.Release(list);
						}
					}
					else if (demonStealPartyQuest.targetItem.isBeingCarriedBy != null && demonStealPartyQuest.targetItem.isBeingCarriedBy.limiterComponent.canPerform)
					{
						LocationGridTile gridTileLocation = demonStealPartyQuest.targetItem.isBeingCarriedBy.gridTileLocation;
						result = (character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation) ? character.jobComponent.CreateDemonStealKnockoutJob(JOB_TYPE.DEMON_STEAL, demonStealPartyQuest.targetItem.isBeingCarriedBy, out producedJob) : ((!CanAPartyMemberReachTile(gridTileLocation, currentParty, character)) ? character.jobComponent.CreateDemonStealKnockoutJob(JOB_TYPE.DEMON_STEAL, demonStealPartyQuest.targetItem.isBeingCarriedBy, out producedJob) : character.jobComponent.TriggerRoamAroundTile(out producedJob)));
					}
					else
					{
						LocationGridTile gridTileLocation2 = demonStealPartyQuest.targetItem.gridTileLocation;
						if (demonStealPartyQuest.targetItem.gridTileLocation == null && demonStealPartyQuest.targetItem.isBeingCarriedBy == null)
						{
							demonStealPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Unavailable"));
							return true;
						}
						result = (character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation2) ? character.jobComponent.CreateDemonStealJob(JOB_TYPE.DEMON_STEAL, demonStealPartyQuest.targetItem, out producedJob) : ((!CanAPartyMemberReachTile(gridTileLocation2, currentParty, character)) ? character.jobComponent.CreateDemonStealJob(JOB_TYPE.DEMON_STEAL, demonStealPartyQuest.targetItem, out producedJob) : character.jobComponent.TriggerRoamAroundTile(out producedJob)));
					}
				}
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return result;
	}

	private bool CanAPartyMemberReachTile(LocationGridTile tileLocation, Party party, Character exception)
	{
		for (int i = 0; i < party.members.Count; i++)
		{
			Character character = party.members[i];
			if ((exception == null || exception != character) && character.movementComponent.HasPathToEvenIfDiffRegion(tileLocation))
			{
				return true;
			}
		}
		return false;
	}
}
