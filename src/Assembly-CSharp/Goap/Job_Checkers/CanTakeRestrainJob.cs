namespace Goap.Job_Checkers;

public class CanTakeRestrainJob : CanTakeJobChecker
{
	public override string key => "CanTakeRestrain";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		Character character2 = (jobQueueItem as GoapPlanJob).targetPOI as Character;
		if (character2.traitContainer.HasTrait("Restrained"))
		{
			return false;
		}
		if (character.traitContainer.HasTrait("Coward"))
		{
			return false;
		}
		if (CharacterManager.Instance.IsCultistOfSameReligion(character2, character))
		{
			return false;
		}
		if (character.faction != null && character.faction.isPlayerFaction && character2.isAlliedWithPlayer)
		{
			return false;
		}
		if (character.isAlliedWithPlayer && character2.faction != null && character2.faction.isPlayerFaction)
		{
			return false;
		}
		return true;
	}
}
