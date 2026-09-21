namespace Traits;

public class Consumable : Trait
{
	public Consumable()
	{
		name = "Consumable";
		description = "This is consumable.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override bool OnDeath(Character character)
	{
		if (character.traitContainer.HasTrait("Burning", "Burnt"))
		{
			CharacterManager.Instance.CreateFoodPileForPOI(character);
			character.SetDestroyMarkerOnDeath(state: true);
		}
		return base.OnDeath(character);
	}
}
