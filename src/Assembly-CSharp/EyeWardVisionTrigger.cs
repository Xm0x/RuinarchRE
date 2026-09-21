public class EyeWardVisionTrigger : TileObjectVisionTrigger
{
	public override void SetVisionTriggerCollidersState(bool state)
	{
		_mainCollider.enabled = false;
	}

	public override void SetFilterVotes(int votes)
	{
	}

	public override void VoteToMakeVisibleToCharacters()
	{
	}

	public override void VoteToMakeInvisibleToCharacters()
	{
	}

	public override void SetAllCollidersState(bool state)
	{
		_mainCollider.enabled = false;
	}
}
