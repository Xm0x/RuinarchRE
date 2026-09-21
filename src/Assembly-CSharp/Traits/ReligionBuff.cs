namespace Traits;

public class ReligionBuff : Status
{
	public ReligionBuff()
	{
		name = "Religion Buff";
		description = "2x damage against characters of a different religion.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
	}
}
