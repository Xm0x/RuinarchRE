using UtilityScripts;

namespace Traits;

public class Lustful : Trait
{
	public override bool isSingleton => true;

	public Lustful()
	{
		name = "Lustful";
		description = "Enjoys frequent lovemaking.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		mutuallyExclusive = new string[1] { "Chaste" };
	}

	public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
		base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
		if (action != INTERACTION_TYPE.MAKE_LOVE)
		{
			return;
		}
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick(actor);
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT)
		{
			if (poiTarget is Character)
			{
				Character relatable = poiTarget as Character;
				if (actor.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful") != null && actor.relationshipContainer.HasRelationshipWith(relatable, RELATIONSHIP_TYPE.AFFAIR))
				{
					cost = Utilities.Rng.Next(15, 37);
				}
			}
			cost = Utilities.Rng.Next(5, 26);
		}
		cost = Utilities.Rng.Next(15, 26);
	}
}
