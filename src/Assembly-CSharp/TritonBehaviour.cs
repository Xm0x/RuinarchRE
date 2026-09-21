using Inner_Maps;
using Inner_Maps.Location_Structures;

public class TritonBehaviour : BaseMonsterBehaviour
{
	public TritonBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.currentStructure is Kennel)
		{
			return false;
		}
		LocationGridTile gridTileLocation = character.gridTileLocation;
		if (gridTileLocation != null && character is Triton triton)
		{
			if (gridTileLocation == triton.spawnLocationTile)
			{
				if (triton.hasMarker)
				{
					AkSoundEngine.PostEvent("Play_Triton_Vanish", triton.marker.gameObject);
				}
				triton.SetDestroyMarkerOnDeath(state: true);
				triton.Death("disappear");
				return true;
			}
			return character.jobComponent.CreateGoToSpecificTileJob(triton.spawnLocationTile, out producedJob);
		}
		return false;
	}
}
