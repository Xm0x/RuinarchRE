public class GorgonBehaviour : BaseMonsterBehaviour
{
	public GorgonBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		return DefaultWildMonsterBehaviour(character, ref log, out producedJob);
	}
}
