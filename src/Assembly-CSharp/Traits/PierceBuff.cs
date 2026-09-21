namespace Traits;

public class PierceBuff : Status
{
	public PierceBuff()
	{
		name = "Pierce Buff";
		description = "All attacks will neglect resistance.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = false;
		ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
		isHidden = true;
	}
}
