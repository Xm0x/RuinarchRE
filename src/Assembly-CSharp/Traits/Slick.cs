namespace Traits;

public class Slick : Trait
{
	public override bool isSingleton => true;

	public Slick()
	{
		name = "Slick";
		description = "Cannot be stored as a Target.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
	}
}
