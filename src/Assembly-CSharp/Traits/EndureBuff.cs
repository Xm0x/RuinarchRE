namespace Traits;

public class EndureBuff : Status
{
	public EndureBuff()
	{
		name = "Endure Buff";
		description = "Damage taken is reduced";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = false;
		ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
		isHidden = true;
	}
}
