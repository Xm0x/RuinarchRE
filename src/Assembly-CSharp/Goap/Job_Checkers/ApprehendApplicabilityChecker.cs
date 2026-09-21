namespace Goap.Job_Checkers;

public class ApprehendApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsApprehendApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		GoapPlanJob obj = job as GoapPlanJob;
		Character character = obj.targetPOI as Character;
		Character assignedCharacter = obj.assignedCharacter;
		if (assignedCharacter == null)
		{
			return false;
		}
		bool flag = !character.traitContainer.HasTrait("Restrained") || !character.IsInPrison();
		return assignedCharacter != null && assignedCharacter.homeSettlement != null && character.gridTileLocation != null && character.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(assignedCharacter.homeSettlement) && flag;
	}
}
