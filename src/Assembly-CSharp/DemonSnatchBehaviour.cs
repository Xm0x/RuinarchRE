using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class DemonSnatchBehaviour : CharacterBehaviour
{
	public DemonSnatchBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool flag = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working)
		{
			DemonSnatchPartyQuest demonSnatchPartyQuest = currentParty.currentQuest as DemonSnatchPartyQuest;
			if (demonSnatchPartyQuest.targetCharacter != null)
			{
				if (demonSnatchPartyQuest.targetCharacter.isDead)
				{
					demonSnatchPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
					return true;
				}
				if (!demonSnatchPartyQuest.targetCharacter.hasMarker)
				{
					demonSnatchPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Unavailable"));
					return true;
				}
				if (demonSnatchPartyQuest.targetCharacter.isBeingSeized)
				{
					if (demonSnatchPartyQuest.targetCharacter.marker.previousGridTile == character.gridTileLocation)
					{
						demonSnatchPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Unavailable"));
						return true;
					}
					flag = character.jobComponent.CreateGoToSpecificTileJob(demonSnatchPartyQuest.targetCharacter.marker.previousGridTile, out producedJob);
				}
				else
				{
					Character memberInCombatExcept = currentParty.GetMemberInCombatExcept(character);
					if (memberInCombatExcept != null)
					{
						bool flag2 = false;
						CombatState combatState = memberInCombatExcept.stateComponent.currentState as CombatState;
						if (combatState.currentClosestHostile != null)
						{
							CombatData combatData = memberInCombatExcept.combatComponent.GetCombatData(combatState.currentClosestHostile);
							if (character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal))
							{
								flag2 = true;
							}
						}
						if (flag2)
						{
							producedJob = null;
							return true;
						}
					}
					Character isBeingCarriedBy = demonSnatchPartyQuest.targetCharacter.isBeingCarriedBy;
					if (isBeingCarriedBy != null && character.IsHostileWith(isBeingCarriedBy) && character.combatComponent.Fight(isBeingCarriedBy, "Hostility"))
					{
						producedJob = null;
						return true;
					}
					Prisoner traitOrStatus = demonSnatchPartyQuest.targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
					if (traitOrStatus != null && traitOrStatus.IsFactionPrisonerOf(character.faction))
					{
						if (!currentParty.jobBoard.HasJob(JOB_TYPE.SNATCH))
						{
							demonSnatchPartyQuest.CreateSnatchJobFor(demonSnatchPartyQuest.targetCharacter, currentParty, demonSnatchPartyQuest.dropStructure);
						}
						flag = DoPartyJobsInPartyJobBoard(character, currentParty, ref producedJob);
						if (!flag)
						{
							int radius = 3;
							if (!demonSnatchPartyQuest.targetCharacter.carryComponent.IsNotBeingCarried())
							{
								radius = 1;
							}
							List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
							demonSnatchPartyQuest.targetCharacter.gridTileLocation.PopulateTilesInRadius(list, radius, 0, includeCenterTile: false, includeTilesInDifferentStructure: false, includeImpassable: false);
							if (list.Count > 0)
							{
								LocationGridTile tile = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
								flag = character.jobComponent.CreateGoToSpecificTileJob(tile, out producedJob);
							}
							else
							{
								flag = character.jobComponent.CreateGoToSpecificTileJob(demonSnatchPartyQuest.targetCharacter.gridTileLocation, out producedJob);
							}
							RuinarchListPool<LocationGridTile>.Release(list);
						}
					}
					else
					{
						flag = character.jobComponent.TriggerRestrainJob(demonSnatchPartyQuest.targetCharacter, JOB_TYPE.SNATCH_RESTRAIN, out producedJob);
					}
				}
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return flag;
	}
}
