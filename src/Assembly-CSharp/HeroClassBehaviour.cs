public class HeroClassBehaviour : CharacterClassBehaviour
{
	public override bool TryDoBehaviour(Character p_character, ref JobQueueItem p_producedJob, ref string log)
	{
		if (PhysicalUserBehaviour(p_character, ref log, ref p_producedJob))
		{
			return true;
		}
		return false;
	}
}
