namespace Goap.Job_Checkers;

public class ApprehendSettlementApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsApprehendSettlementApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		Character character = (job as GoapPlanJob).targetPOI as Character;
		NPCSettlement nPCSettlement = job.originalOwner as NPCSettlement;
		bool flag = !character.traitContainer.HasTrait("Restrained") || character.currentStructure != nPCSettlement.prison;
		if (character.gridTileLocation != null && flag)
		{
			if (character.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(nPCSettlement))
			{
				return true;
			}
			if (job.assignedCharacter != null)
			{
				return job.assignedCharacter.combatComponent.IsInActualCombatWith(character);
			}
			return false;
		}
		return false;
	}
}
