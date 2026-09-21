public class ExploreBehaviour : CharacterBehaviour
{
	public ExploreBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.targetDestination.IsAtTargetDestination(character) && currentParty.currentQuest is ExplorationPartyQuest explorationPartyQuest)
		{
			Character randomResidentForInvasionTargetThatIsInsideStructureAndHostileWithFaction = explorationPartyQuest.targetStructure.GetRandomResidentForInvasionTargetThatIsInsideStructureAndHostileWithFaction(character.faction, character);
			if (randomResidentForInvasionTargetThatIsInsideStructureAndHostileWithFaction != null)
			{
				character.combatComponent.Fight(randomResidentForInvasionTargetThatIsInsideStructureAndHostileWithFaction, "Hostility");
				producedJob = null;
				return true;
			}
			Character memberInCombatExcept = currentParty.GetMemberInCombatExcept(character);
			if (memberInCombatExcept != null && currentParty.targetDestination.IsAtTargetDestination(memberInCombatExcept))
			{
				bool flag = false;
				if (memberInCombatExcept.stateComponent.currentState is CombatState { currentClosestHostile: not null } combatState)
				{
					CombatData combatData = memberInCombatExcept.combatComponent.GetCombatData(combatState.currentClosestHostile);
					character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
					flag = true;
				}
				if (flag)
				{
					producedJob = null;
					return true;
				}
				result = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
			}
			else
			{
				result = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return result;
	}
}
