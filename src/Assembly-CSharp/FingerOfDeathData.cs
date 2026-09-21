public class FingerOfDeathData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FINGER_OF_DEATH;

	public override string name => "Finger Of Death";

	public override string description => "This Ability will instantly kill any living creature. Characters with high Mental Resistance may be able to resist this spell’s dark whisper.";

	public FingerOfDeathData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			if (character.hasMarker)
			{
				AkSoundEngine.PostEvent("Play_Finger_Of_Death", character.marker.gameObject);
			}
			character.Death(name, null, null, null, null, null, null, isPlayerSource: true);
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag && target is Character { isDead: not false })
		{
			return false;
		}
		return flag;
	}
}
