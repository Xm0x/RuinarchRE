using Inner_Maps;
using Inner_Maps.Location_Structures;

public class BountyHuntBehaviour : CharacterBehaviour
{
	public BountyHuntBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive)
		{
			BountyHuntPartyQuest bountyHuntPartyQuest = currentParty.currentQuest as BountyHuntPartyQuest;
			if (currentParty.partyState == PARTY_STATE.Working)
			{
				LocationStructure currentStructure = bountyHuntPartyQuest.targetCharacter.currentStructure;
				LocationStructure locationStructure = (currentParty.partySettlement as NPCSettlement)?.prison;
				if (locationStructure == null)
				{
					bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("No_Prison"));
					return true;
				}
				if (currentStructure == locationStructure)
				{
					bountyHuntPartyQuest.SetIsSuccessful(state: true);
					bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
					return true;
				}
				if (currentStructure != null && currentStructure.structureType.IsPlayerStructure())
				{
					if (character.faction == null || !character.faction.isAwareOfPlayer)
					{
						bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Nowhere"));
						return true;
					}
					bool hasEndQuest = false;
					bountyHuntPartyQuest.CultistBetrayalProcessing(ref hasEndQuest);
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
				if (bountyHuntPartyQuest.targetCharacter.isBeingSeized)
				{
					LocationGridTile previousGridTile = bountyHuntPartyQuest.targetCharacter.marker.previousGridTile;
					if (character.gridTileLocation == previousGridTile)
					{
						bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Nowhere"));
						return true;
					}
					if (character.jobComponent.CreateGoToSpecificTileJob(previousGridTile, out producedJob))
					{
						return true;
					}
				}
				if (!character.hasMarker || !bountyHuntPartyQuest.targetCharacter.hasMarker || character.gridTileLocation == null || bountyHuntPartyQuest.targetCharacter.gridTileLocation == null)
				{
					bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Nowhere"));
					return true;
				}
				if (character.marker.IsPOIInVision(bountyHuntPartyQuest.targetCharacter))
				{
					if (bountyHuntPartyQuest.targetCharacter.isDead)
					{
						bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
						return true;
					}
					DoPartyJobsInPartyJobBoard(character, currentParty, ref producedJob);
					if (producedJob == null)
					{
						bool canDoJob = false;
						currentParty.jobComponent.TryCreateApprehend(character, bountyHuntPartyQuest.targetCharacter, currentParty, locationStructure, ref canDoJob, out producedJob);
						if (!canDoJob)
						{
							bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Should_Not_Apprehend"));
							return true;
						}
						if (producedJob != null)
						{
							return true;
						}
						return character.jobComponent.TriggerRoamAroundTile(out producedJob);
					}
					return true;
				}
				Character character2 = bountyHuntPartyQuest.targetCharacter;
				if (character2.grave != null)
				{
					if (character.marker.IsPOIInVision(character2.grave))
					{
						bountyHuntPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
						return true;
					}
					result = character.jobComponent.CreateGoToJob(character2.grave, out producedJob);
				}
				else
				{
					if (bountyHuntPartyQuest.targetCharacter.isBeingCarriedBy != null)
					{
						character2 = bountyHuntPartyQuest.targetCharacter.isBeingCarriedBy;
					}
					result = character.jobComponent.CreateGoToJob(character2, out producedJob);
				}
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return result;
	}
}
