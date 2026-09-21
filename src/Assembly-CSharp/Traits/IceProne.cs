using System.Collections.Generic;

namespace Traits;

public class IceProne : Trait
{
	public override bool isSingleton => true;

	public IceProne()
	{
		name = "Ice Prone";
		description = "Ice is especially painful for this one.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		mutuallyExclusive = new string[1] { "Ice Resistant" };
		resistancesType = new List<RESISTANCE> { RESISTANCE.Ice };
		resistancesValue = new List<float> { -100f };
	}
}
