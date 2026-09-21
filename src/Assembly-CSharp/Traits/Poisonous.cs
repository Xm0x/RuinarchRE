namespace Traits;

public class Poisonous : Trait
{
	public override bool isSingleton => true;

	public Poisonous()
	{
		name = "Poisonous";
		description = "Continually produces Poison.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Hour_Started_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		for (int i = 0; i < 3; i++)
		{
			addedTo.traitContainer.AddTrait(addedTo, "Poisoned", null, bypassElementalChance: true, 0, 0f, ELEMENTAL_TYPE.Poison);
		}
	}

	public override void OnHourStarted(ITraitable traitable)
	{
		base.OnHourStarted(traitable);
		traitable.traitContainer.AddTrait(traitable, "Poisoned", null, bypassElementalChance: true, 0, 0f, ELEMENTAL_TYPE.Poison);
	}
}
