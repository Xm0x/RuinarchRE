namespace Traits;

public class NegativeNancy : Trait
{
	public override bool isSingleton => true;

	public NegativeNancy()
	{
		name = "Negative Nancy";
		description = "Brings the mood down of other Villagers they encounter.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character)
		{
			Character character = targetPOI as Character;
			if (characterThatWillDoJob.isNormalCharacter && character.isNormalCharacter && !characterThatWillDoJob.isDead && !character.isDead && character.limiterComponent.canWitness)
			{
				character.traitContainer.AddTrait(character, "Downcast", characterThatWillDoJob);
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}
}
