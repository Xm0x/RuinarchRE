using Inner_Maps;
using Inner_Maps.Location_Structures;

public class RescueBehaviour : CharacterBehaviour
{
	public RescueBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool flag = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive)
		{
			RescuePartyQuest rescuePartyQuest = currentParty.currentQuest as RescuePartyQuest;
			if (currentParty.partyState == PARTY_STATE.Working)
			{
				LocationStructure currentStructure = rescuePartyQuest.targetCharacter.currentStructure;
				if (currentStructure != null && currentStructure.structureType.IsPlayerStructure() && character.faction != null)
				{
					if (!character.faction.isAwareOfPlayer)
					{
						rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Nowhere"));
					}
					else
					{
						bool hasEndQuest = false;
						rescuePartyQuest.CultistBetrayalProcessing(ref hasEndQuest);
						if (hasEndQuest)
						{
							return true;
						}
						if (!currentParty.IsMember(character) || !character.partyComponent.isMemberThatJoinedQuest)
						{
							return true;
						}
						if (!currentStructure.hasBeenDestroyed && currentStructure.objectsThatContributeToDamage.Count > 0)
						{
							TileObject tileObject = null;
							IDamageable nearestDamageableThatContributeToHP = currentStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
							if (nearestDamageableThatContributeToHP != null && nearestDamageableThatContributeToHP is TileObject tileObject2)
							{
								tileObject = tileObject2;
							}
							if (tileObject != null)
							{
								character.combatComponent.Fight(tileObject, "Clear_Demonic_Intrusion");
								return true;
							}
						}
					}
				}
				if (rescuePartyQuest.targetCharacter.isBeingSeized)
				{
					LocationGridTile previousGridTile = rescuePartyQuest.targetCharacter.marker.previousGridTile;
					if (character.gridTileLocation == previousGridTile)
					{
						rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Nowhere"));
						return true;
					}
					if (character.jobComponent.CreateGoToSpecificTileJob(previousGridTile, out producedJob))
					{
						return true;
					}
				}
				if (!character.hasMarker || !rescuePartyQuest.targetCharacter.hasMarker || character.gridTileLocation == null || rescuePartyQuest.targetCharacter.gridTileLocation == null)
				{
					rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Nowhere"));
					return true;
				}
				if (character.marker.IsPOIInVision(rescuePartyQuest.targetCharacter))
				{
					if (rescuePartyQuest.targetCharacter.isDead)
					{
						rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
						return true;
					}
					if (!rescuePartyQuest.targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved"))
					{
						rescuePartyQuest.SetIsSuccessful(state: true);
						rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Safe"));
						if (rescuePartyQuest.targetCharacter.traitContainer.HasTrait("Paralyzed") && !rescuePartyQuest.targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.RESCUE_MOVE_CHARACTER))
						{
							character.jobComponent.TryTriggerRescueMoveCharacter(rescuePartyQuest.targetCharacter, out producedJob);
						}
						return true;
					}
					flag = character.jobComponent.TriggerReleaseJob(JOB_TYPE.RELEASE_CHARACTER, rescuePartyQuest.targetCharacter, out producedJob);
					if (flag)
					{
						rescuePartyQuest.SetIsReleasing(state: true);
						return true;
					}
				}
				else
				{
					Character targetCharacter = rescuePartyQuest.targetCharacter;
					if (targetCharacter.grave != null)
					{
						if (character.marker.IsPOIInVision(targetCharacter.grave))
						{
							rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
							return true;
						}
						flag = character.jobComponent.CreateGoToJob(targetCharacter.grave, out producedJob);
					}
					else
					{
						flag = character.jobComponent.CreateGoToJob(targetCharacter, out producedJob, JOB_TYPE.PARTY_GO_TO);
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
