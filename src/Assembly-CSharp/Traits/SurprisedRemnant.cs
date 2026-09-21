namespace Traits;

public class SurprisedRemnant : RemnantTrait
{
	public SurprisedRemnant()
	{
		name = "Surprised Remnant";
		description = "Surprised Remnant";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 1;
		canBeTriggered = false;
		isHidden = true;
		isTangible = true;
	}
}
