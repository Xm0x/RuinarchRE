namespace Traits;

public class PlagueCared : Status
{
	public override bool isSingleton => true;

	public PlagueCared()
	{
		name = "Plague Cared";
		description = "This has been cared for.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isHidden = true;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6);
	}
}
