using System.Collections.Generic;

namespace Traits;

public class IceResistant : Trait
{
	public override bool isSingleton => true;

	public IceResistant()
	{
		name = "Ice Resistant";
		description = "Receives significantly reduced damage from Ice attacks.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		resistancesType = new List<RESISTANCE> { RESISTANCE.Ice };
		resistancesValue = new List<float> { 100f };
	}
}
