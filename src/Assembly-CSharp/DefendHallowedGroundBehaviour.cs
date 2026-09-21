using Inner_Maps;

public class DefendHallowedGroundBehaviour : CharacterBehaviour
{
	public DefendHallowedGroundBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.targetDestination.IsAtTargetDestination(character) && currentParty.currentQuest is DefendHallowedGroundPartyQuest defendHallowedGroundPartyQuest)
		{
			LocationGridTile randomTile = defendHallowedGroundPartyQuest.targetStructure.GetRandomTile();
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.PATROL, character, character);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.PATROL, new object[1] { randomTile });
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			result = true;
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return result;
	}
}
