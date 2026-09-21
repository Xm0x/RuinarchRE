namespace Traits;

public class Disoriented : Status
{
	public override bool isSingleton => true;

	public Disoriented()
	{
		name = "Disoriented";
		description = "Feeling dizzy.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
		moodEffect = -5;
		hindersMovement = true;
		hindersPerform = true;
	}
}
