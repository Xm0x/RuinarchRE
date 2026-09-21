namespace Traits;

public class DangerRemnant : RemnantTrait
{
	public DangerRemnant()
	{
		name = "Danger Remnant";
		description = "Danger Remnant";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 1;
		canBeTriggered = false;
		isHidden = true;
		isTangible = true;
	}
}
