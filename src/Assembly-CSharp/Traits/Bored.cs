namespace Traits;

public class Bored : Status
{
	public override bool isSingleton => true;

	public Bored()
	{
		name = "Bored";
		description = "Is lacking some sort of entertainment.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		moodEffect = -8;
	}
}
