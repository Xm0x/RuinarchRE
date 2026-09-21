using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class PatrolBehaviour : CharacterBehaviour
{
	public PatrolBehaviour()
	{
		base.priority = 450;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty != null && currentParty.isActive)
		{
			PartyQuest currentQuest = currentParty.currentQuest;
			if (currentParty.partyState == PARTY_STATE.Waiting)
			{
				if (currentQuest.IsInterestedInJoiningQuest(character))
				{
					character.traitContainer.RemoveTrait(character, "Patrolling");
					return true;
				}
			}
			else if (currentParty.partyState != PARTY_STATE.None && !currentParty.membersThatJoinedQuest.Contains(character) && currentQuest.canStillJoinQuestAnytime && currentQuest.IsInterestedInJoiningQuest(character))
			{
				character.traitContainer.RemoveTrait(character, "Patrolling");
				return true;
			}
		}
		LocationStructure locationStructure = null;
		if (character.homeSettlement != null)
		{
			locationStructure = character.homeSettlement.GetRandomStructure();
		}
		else if (character.territory != null)
		{
			locationStructure = character.territory.GetRandomStructureFromAllSettlements();
		}
		else if (character.faction != null)
		{
			BaseSettlement randomElement = CollectionUtilities.GetRandomElement(character.faction.ownedSettlements);
			if (randomElement != null)
			{
				locationStructure = randomElement.GetRandomStructure();
			}
		}
		LocationGridTile locationGridTile = ((locationStructure == null) ? character.areaLocation.GetRandomPassableTile() : locationStructure.GetRandomTile());
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.PATROL, character, character);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.PATROL, new object[1] { locationGridTile });
		goapPlanJob.SetCannotBePushedBack(state: true);
		producedJob = goapPlanJob;
		return true;
	}
}
