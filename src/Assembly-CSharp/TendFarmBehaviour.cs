public class TendFarmBehaviour : CharacterBehaviour
{
	public TendFarmBehaviour()
	{
		base.priority = 440;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		return true;
	}

	private bool IsCornCropUntended(CornCrop crop)
	{
		if (!crop.traitContainer.HasTrait("Tended"))
		{
			return crop.state == POI_STATE.ACTIVE;
		}
		return false;
	}
}
