namespace Traits;

public class FoodComa : Status
{
	public FoodComa()
	{
		name = "Food Coma";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
		hindersMovement = true;
		moodEffect = -4;
		isStacking = false;
	}
}
