namespace Traits;

public class Hibernating : Status
{
	public override bool isSingleton => true;

	public Hibernating()
	{
		name = "Hibernating";
		description = "Indefinitely inactive. There must be something you can do to awaken it.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		hindersWitness = true;
		hindersPerform = true;
	}
}
