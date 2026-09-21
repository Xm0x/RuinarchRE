namespace Traits;

public class Pessimist : Trait
{
	public override bool isSingleton => true;

	public Pessimist()
	{
		name = "Pessimist";
		description = "Usually expects the worst. Loses Entertainment more quickly than normal.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		mutuallyExclusive = new string[1] { "Optimist" };
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (character.needsComponent.isSulking)
		{
			character.needsComponent.AdjustHappiness(-5f);
		}
		else
		{
			character.needsComponent.SetHappiness(20f);
		}
		return base.TriggerFlaw(character);
	}
}
