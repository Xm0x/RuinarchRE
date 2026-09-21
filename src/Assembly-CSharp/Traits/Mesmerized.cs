namespace Traits;

public class Mesmerized : Status
{
	public override bool isSingleton => true;

	public Mesmerized()
	{
		name = "Mesmerized";
		description = "Under a mysterious enchantment.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(72);
		hindersFullnessRecovery = true;
		hindersHappinessRecovery = true;
		hindersTirednessRecovery = true;
		hindersSocials = true;
	}
}
