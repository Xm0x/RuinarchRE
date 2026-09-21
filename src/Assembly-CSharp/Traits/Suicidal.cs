namespace Traits;

public class Suicidal : Status
{
	public Suicidal()
	{
		name = "Suicidal";
		description = "Might end up killing itself anytime soon.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		hindersSocials = true;
		moodEffect = -8;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(48);
	}
}
