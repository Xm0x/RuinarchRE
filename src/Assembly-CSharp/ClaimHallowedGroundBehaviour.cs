using Inner_Maps.Location_Structures;

public class ClaimHallowedGroundBehaviour : CharacterBehaviour
{
	public ClaimHallowedGroundBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		Party currentParty = character.partyComponent.currentParty;
		producedJob = null;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.targetDestination.IsAtTargetDestination(character))
		{
			ClaimHallowedGroundPartyQuest claimHallowedGroundPartyQuest = currentParty.currentQuest as ClaimHallowedGroundPartyQuest;
			bool result = false;
			if (claimHallowedGroundPartyQuest.targetStructure is Inner_Maps.Location_Structures.HallowedGround hallowedGround)
			{
				if (hallowedGround.claimedByReligion == character.religionComponent.religion)
				{
					claimHallowedGroundPartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Hallowed_Ground_Claimed"));
					return true;
				}
				bool flag = false;
				for (int i = 0; i < currentParty.members.Count; i++)
				{
					if (currentParty.members[i].jobQueue.HasJob(JOB_TYPE.CLAIM_LOCATION))
					{
						flag = true;
					}
				}
				if (!flag)
				{
					HallowedGround tileObjectOfType = hallowedGround.GetTileObjectOfType<HallowedGround>();
					if (tileObjectOfType != null)
					{
						result = character.jobComponent.CreateClaimHallowedGroundJob(tileObjectOfType, out producedJob);
					}
				}
				else
				{
					result = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
				}
				if (producedJob != null)
				{
					producedJob.SetIsThisAPartyJob(state: true);
				}
				return result;
			}
		}
		producedJob = null;
		return false;
	}
}
