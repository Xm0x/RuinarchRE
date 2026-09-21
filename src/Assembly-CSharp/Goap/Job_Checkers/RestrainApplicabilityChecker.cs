namespace Goap.Job_Checkers;

public class RestrainApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsRestrainApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		Character character = (job as GoapPlanJob).targetPOI as Character;
		NPCSettlement nPCSettlement = job.originalOwner as NPCSettlement;
		if (nPCSettlement.owner != null && character.faction != null && !nPCSettlement.owner.IsHostileWith(character.faction))
		{
			return false;
		}
		bool flag = !character.traitContainer.HasTrait("Restrained");
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
