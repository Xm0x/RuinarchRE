using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MorningPatrolBehaviour : CharacterBehaviour
{
	public MorningPatrolBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		LocationStructure locationStructure = ((character.homeSettlement == null) ? character.currentRegion.GetRandomStructure() : character.homeSettlement.GetRandomStructure());
		LocationGridTile randomTile = locationStructure.GetRandomTile();
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.PATROL, character, character);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.PATROL, new object[1] { randomTile });
		goapPlanJob.SetCannotBePushedBack(state: true);
		producedJob = goapPlanJob;
		return true;
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.SetCombatModeBeforePatrolling(character.combatComponent.combatMode);
		character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.combatComponent.SetCombatMode(character.behaviourComponent.combatModeBeforePatrolling);
	}
}
