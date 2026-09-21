using Traits;

namespace Goap.Job_Checkers;

public class CanCraftPhylactery : CanTakeJobChecker
{
	public override string key => "CanCraftPhylactery";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (TILE_OBJECT_TYPE.PHYLACTERY.CanBeCraftedBy(character))
		{
			Vampire traitOrStatus = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus != null && character.lycanData != null)
			{
				if (!traitOrStatus.dislikedBeingVampire || !character.lycanData.dislikesBeingLycan)
				{
					return false;
				}
			}
			else
			{
				if (traitOrStatus != null && !traitOrStatus.dislikedBeingVampire)
				{
					return false;
				}
				if (character.lycanData != null && !character.lycanData.dislikesBeingLycan)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}
}
