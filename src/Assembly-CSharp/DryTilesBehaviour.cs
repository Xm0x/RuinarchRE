using Inner_Maps;
using Traits;

public class DryTilesBehaviour : CharacterBehaviour
{
	public DryTilesBehaviour()
	{
		base.priority = 430;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		return true;
	}

	private bool DryNearestTile(Character character, out JobQueueItem producedJob)
	{
		LocationGridTile locationGridTile = null;
		if (locationGridTile != null)
		{
			locationGridTile.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Wet>("Wet").SetDryer(character);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DRY_TILES, INTERACTION_TYPE.CLEAN_UP, locationGridTile.tileObjectComponent.genericTileObject, character);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}
}
