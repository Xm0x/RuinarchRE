namespace Traits;

public class BadWeather : Status
{
	public BadWeather()
	{
		name = "Bad Weather";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6);
		moodEffect = -5;
		isStacking = true;
		stackLimit = 1;
		stackModifier = 1f;
	}
}
