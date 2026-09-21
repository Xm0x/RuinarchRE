namespace Goap.Job_Checkers;

public class CanTakeFactionIdeologyJob : CanTakeJobChecker
{
	public override string key => "CanTakeFactionIdeologyJob";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (character.faction != null)
		{
			FactionIdeology factionIdeology = character.faction.ideologyComponent.GetFactionIdeology(jobQueueItem.connectedFactionIdeology);
			if (factionIdeology != null)
			{
				return factionIdeology.CanCharacterDoIdeologyEvent(character);
			}
		}
		return false;
	}
}
