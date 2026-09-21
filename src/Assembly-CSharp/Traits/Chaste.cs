using UtilityScripts;

namespace Traits;

public class Chaste : Trait
{
	public override bool isSingleton => true;

	public Chaste()
	{
		name = "Chaste";
		description = "Not very interested in fooling around.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		mutuallyExclusive = new string[1] { "Lustful" };
	}

	public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
		base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
		if (action == INTERACTION_TYPE.MAKE_LOVE)
		{
			cost = Utilities.Rng.Next(40, 67);
		}
	}
}
