namespace Traits;

public class Downcast : Status
{
	public Downcast()
	{
		name = "Downcast";
		description = "Feeling negative vibes.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		moodEffect = -10;
		stackLimit = 1;
		stackModifier = 1f;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
	}
}
