using System.Collections.Generic;

namespace Traits;

public class PoisonResistant : Trait
{
	public override bool isSingleton => true;

	public PoisonResistant()
	{
		name = "Poison Resistant";
		description = "Receives significantly reduced damage from Poison attacks.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		resistancesType = new List<RESISTANCE> { RESISTANCE.Poison };
		resistancesValue = new List<float> { 100f };
	}
}
