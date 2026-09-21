namespace Traits;

public class Devout : Trait
{
	public Devout()
	{
		name = "Devout";
		description = "Very religious.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
	}
}
