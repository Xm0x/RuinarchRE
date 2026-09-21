namespace Goap.Job_Checkers;

public class BurySettlementApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsBurySettlementApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		Character character = (job as GoapPlanJob).targetPOI as Character;
		NPCSettlement nPCSettlement = job.originalOwner as NPCSettlement;
		if (character.jobComponent.IsCharacterGhost(character))
		{
			return false;
		}
		if (character.race.IsSkinnable() && nPCSettlement.HasStructureOfTypeThatIsAssigned(STRUCTURE_TYPE.HUNTER_LODGE))
		{
			return false;
		}
		if (!nPCSettlement.HasStructure(STRUCTURE_TYPE.CEMETERY) && character.previousCharacterDataComponent.homeSettlementOnDeath != nPCSettlement)
		{
			return false;
		}
		if (character.gridTileLocation != null && character.gridTileLocation.IsNextToOrPartOfSettlement(nPCSettlement))
		{
			return character.hasMarker;
		}
		return false;
	}
}
