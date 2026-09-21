namespace Traits;

public class Heavy : Trait
{
	public override bool isSingleton => true;

	public Heavy()
	{
		name = "Heavy";
		description = "Too bulky to be seized!";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
	}
}
