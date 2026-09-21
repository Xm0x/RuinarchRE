using Inner_Maps;
using UtilityScripts;

public class PartyBehaviour : CharacterBehaviour
{
	public PartyBehaviour()
	{
		base.priority = 29;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool hasJob = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive)
		{
			if (currentParty.partyState == PARTY_STATE.Waiting)
			{
				if (currentParty.currentQuest != null && !currentParty.currentQuest.IsInterestedInJoiningQuest(character))
				{
					return false;
				}
				if (PartyMemberPreparationBehaviour(character, ref log, out producedJob))
				{
					return true;
				}
				if (currentParty.meetingPlace != null && !currentParty.meetingPlace.hasBeenDestroyed && currentParty.meetingPlace.passableTiles.Count > 0)
				{
					hasJob = true;
					if (character.currentStructure == currentParty.meetingPlace)
					{
						currentParty.AddMemberThatJoinedQuest(character);
						character.trapStructure.SetForcedStructure(currentParty.meetingPlace);
						character.needsComponent.CheckExtremeNeedsWhileInActiveParty();
						character.jobComponent.TriggerRoamAroundStructure(out producedJob);
					}
					else
					{
						if (!currentParty.CanAMemberGoTo(currentParty.meetingPlace))
						{
							currentParty.SetMeetingPlace();
						}
						LocationGridTile randomPassableTile = currentParty.meetingPlace.GetRandomPassableTile();
						if (randomPassableTile != null)
						{
							if (character.movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile))
							{
								character.jobComponent.CreateGoToWaitingJob(randomPassableTile, out producedJob);
							}
							else
							{
								hasJob = false;
							}
						}
						else
						{
							currentParty.SetMeetingPlace();
							hasJob = false;
						}
					}
				}
				if (currentParty.isPlayerParty)
				{
					return true;
				}
			}
			else if (currentParty.partyState != PARTY_STATE.None)
			{
				if (currentParty.membersThatJoinedQuest.Contains(character))
				{
					NonWaitingJoinedQuestBehaviour(character, currentParty, ref producedJob, ref hasJob, ref log);
					if (hasJob)
					{
						if (producedJob != null)
						{
							producedJob.SetIsThisAPartyJob(state: true);
						}
						return hasJob;
					}
				}
				else
				{
					NonWaitingNotJoinedQuestBehaviour(character, currentParty, ref producedJob, ref hasJob, ref log);
					if (hasJob)
					{
						if (producedJob != null)
						{
							producedJob.SetIsThisAPartyJob(state: true);
						}
						return hasJob;
					}
				}
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return hasJob;
	}

	private void NonWaitingJoinedQuestBehaviour(Character character, Party party, ref JobQueueItem producedJob, ref bool hasJob, ref string log)
	{
		if (!party.IsMemberActive(character))
		{
			return;
		}
		hasJob = DoPartyJobsInPartyJobBoard(character, party, ref producedJob);
		if (!hasJob)
		{
			if (party.partyState == PARTY_STATE.Moving)
			{
				if (party.targetDestination != null && !party.targetDestination.hasBeenDestroyed)
				{
					if (party.targetDestination.IsAtTargetDestination(character))
					{
						if (party.targetDestination == party.partySettlement)
						{
							if (party.currentQuest is DemonSnatchPartyQuest demonSnatchPartyQuest)
							{
								if (party.jobBoard.HasJob(JOB_TYPE.SNATCH, demonSnatchPartyQuest.targetCharacter))
								{
									LocationGridTile randomPassableTileThatIsNotPartOfAStructure = character.areaLocation.gridTileComponent.GetRandomPassableTileThatIsNotPartOfAStructure();
									if (randomPassableTileThatIsNotPartOfAStructure != null)
									{
										character.jobComponent.CreatePartyGoToJob(randomPassableTileThatIsNotPartOfAStructure, out producedJob);
									}
								}
								else
								{
									party.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
								}
							}
							else
							{
								party.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
							}
						}
						else
						{
							party.SetPartyState(PARTY_STATE.Working);
						}
						hasJob = true;
						return;
					}
					if (character.partyComponent.CanFollowBeacon())
					{
						character.partyComponent.FollowBeacon();
					}
					else
					{
						LocationGridTile locationGridTile = null;
						locationGridTile = ((!party.isPlayerParty || party.targetDestination != party.partySettlement) ? party.targetDestination.GetRandomPassableTile() : party.partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL).GetRandomPassableTile());
						if (locationGridTile != null)
						{
							hasJob = character.jobComponent.CreatePartyGoToJob(locationGridTile, out producedJob);
						}
					}
				}
			}
			else if (party.partyState == PARTY_STATE.Resting)
			{
				if (party.targetRestingTavern != null && !party.targetRestingTavern.hasBeenDestroyed && party.targetRestingTavern.passableTiles.Count > 0)
				{
					if (character.currentStructure == party.targetRestingTavern)
					{
						character.needsComponent.CheckExtremeNeedsWhileInActiveParty();
						hasJob = TavernBehaviour(character, party, out producedJob);
					}
					else
					{
						LocationGridTile randomElement = CollectionUtilities.GetRandomElement(party.targetRestingTavern.passableTiles);
						if (randomElement != null)
						{
							hasJob = character.jobComponent.CreatePartyGoToJob(randomElement, out producedJob);
						}
					}
				}
				else
				{
					if (party.targetCamp == null)
					{
						party.SetPartyState(PARTY_STATE.Moving);
						hasJob = true;
						return;
					}
					if (character.gridTileLocation != null && character.areaLocation == party.targetCamp)
					{
						character.needsComponent.CheckExtremeNeedsWhileInActiveParty();
						hasJob = CampBehaviour(character, party, out producedJob);
					}
					else
					{
						LocationGridTile randomPassableTile = party.targetCamp.GetRandomPassableTile();
						hasJob = character.jobComponent.CreatePartyGoToSpecificTileJob(randomPassableTile, out producedJob);
					}
				}
			}
			else if (party.partyState == PARTY_STATE.Working && !party.targetDestination.IsAtTargetDestination(character))
			{
				if (party.hasChangedTargetDestination)
				{
					party.SetHasChangedTargetDestination(state: false);
					party.SetPartyState(PARTY_STATE.Moving);
					hasJob = true;
					return;
				}
				LocationGridTile randomPassableTile2 = party.targetDestination.GetRandomPassableTile();
				hasJob = character.jobComponent.CreatePartyGoToJob(randomPassableTile2, out producedJob);
			}
		}
		hasJob = true;
	}

	private void NonWaitingNotJoinedQuestBehaviour(Character character, Party party, ref JobQueueItem producedJob, ref bool hasJob, ref string log)
	{
		PartyQuest currentQuest = party.currentQuest;
		if (currentQuest.canStillJoinQuestAnytime && currentQuest.IsInterestedInJoiningQuest(character))
		{
			party.AddMemberThatJoinedQuest(character);
			hasJob = true;
		}
	}

	private bool CampBehaviour(Character character, Party party, out JobQueueItem producedJob)
	{
		if (GameUtilities.RollChance(50))
		{
			Campfire partyCampfireInArea = GetPartyCampfireInArea(character.areaLocation, character, party);
			if (partyCampfireInArea != null)
			{
				bool flag = character.jobComponent.TriggerWarmUp(partyCampfireInArea, out producedJob);
				if (flag)
				{
					return flag;
				}
			}
		}
		return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
	}

	private bool TavernBehaviour(Character character, Party party, out JobQueueItem producedJob)
	{
		return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
	}

	private bool CampSetterBehaviour(Character character, Party party, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		if (GetPartyCampfireInArea(character.areaLocation, character, party) == null)
		{
			result = character.jobComponent.TriggerBuildCampfireJob(JOB_TYPE.BUILD_CAMP, out producedJob);
		}
		return result;
	}

	private bool FoodProducerBehaviour(Character character, Party party, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		if (HasMemberThatIsHungryOrStarvingAndThereIsNoFoodInCamp(character.areaLocation, party))
		{
			result = character.jobComponent.CreateProduceFoodForCampJob(out producedJob);
		}
		return result;
	}

	private Campfire GetPartyCampfireInArea(Area p_area, Character character, Party party)
	{
		Campfire result = null;
		for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_area.gridTileComponent.gridTiles[i];
			if (locationGridTile.tileObjectComponent.objHere != null && locationGridTile.tileObjectComponent.objHere is Campfire campfire && (campfire.characterOwner == null || campfire.IsOwnedBy(character) || (!character.IsHostileWith(campfire.characterOwner) && !character.relationshipContainer.IsEnemiesWith(campfire.characterOwner)) || party.IsMember(campfire.characterOwner)))
			{
				result = campfire;
				break;
			}
		}
		return result;
	}

	private bool HasMemberThatIsHungryOrStarvingAndThereIsNoFoodInCamp(Area p_area, Party party)
	{
		bool flag = false;
		for (int i = 0; i < party.membersThatJoinedQuest.Count; i++)
		{
			Character character = party.membersThatJoinedQuest[i];
			if (party.IsMemberActive(character) && (character.needsComponent.isHungry || character.needsComponent.isStarving))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return GetFoodPileInCamp(p_area, party) == null;
		}
		return false;
	}

	private FoodPile GetFoodPileInCamp(Area p_area, Party party)
	{
		FoodPile result = null;
		for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_area.gridTileComponent.gridTiles[i];
			if (locationGridTile.tileObjectComponent.objHere != null && locationGridTile.tileObjectComponent.objHere is FoodPile foodPile && foodPile.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD) >= 12)
			{
				result = foodPile;
				break;
			}
		}
		return result;
	}
}
