public class KillVillagerPartyBehaviour : CharacterBehaviour
{
	public KillVillagerPartyBehaviour()
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
			KillVillagerPartyQuest killVillagerPartyQuest = currentParty.currentQuest as KillVillagerPartyQuest;
			if (killVillagerPartyQuest.targetCharacter != null)
			{
				if (killVillagerPartyQuest.targetCharacter.isDead)
				{
					killVillagerPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
					return true;
				}
				if (!killVillagerPartyQuest.targetCharacter.hasMarker)
				{
					killVillagerPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Unavailable"));
					return true;
				}
				if (killVillagerPartyQuest.targetCharacter.isBeingSeized)
				{
					if (killVillagerPartyQuest.targetCharacter.marker.previousGridTile == character.gridTileLocation)
					{
						killVillagerPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Unavailable"));
						return true;
					}
					flag = character.jobComponent.CreateGoToSpecificTileJob(killVillagerPartyQuest.targetCharacter.marker.previousGridTile, out producedJob);
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
					if (!killVillagerPartyQuest.targetCharacter.isDead)
					{
						if (!currentParty.jobBoard.HasJob(JOB_TYPE.DEMON_KILL))
						{
							killVillagerPartyQuest.CreateKillJobFor(killVillagerPartyQuest.targetCharacter, currentParty);
						}
						flag = DoPartyJobsInPartyJobBoard(character, currentParty, ref producedJob);
						if (!flag)
						{
							flag = character.jobComponent.CreateKillVillagerJob(killVillagerPartyQuest.targetCharacter, out producedJob);
						}
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
