namespace Traits;

public class LightningRemnant : RemnantTrait
{
	public LightningRemnant()
	{
		name = "Lightning Remnant";
		description = "Lightning Remnant";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 1;
		canBeTriggered = false;
		isHidden = true;
		isTangible = true;
	}
}
