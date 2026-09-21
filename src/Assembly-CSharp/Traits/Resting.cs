namespace Traits;

public class Resting : Status
{
	public override bool isSingleton => true;

	public Resting()
	{
		name = "Resting";
		description = "Sleeping. May or may not be snoring.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		hindersWitness = true;
		hindersPerform = true;
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
		AddTraitOverrideFunctionIdentifier("Hour_Started_Trait");
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		if (traitable is Character character)
		{
			RecoverHP(character);
		}
	}

	public override void OnHourStarted(ITraitable traitable)
	{
		base.OnHourStarted(traitable);
		if (traitable is Character character)
		{
			CheckForLycanthropy(character);
		}
	}

	private void RecoverHP(Character character)
	{
		character.PassiveHPRecovery(0.01f);
	}

	private void CheckForLycanthropy(Character character)
	{
		if (character.isLycanthrope && !character.lycanData.isMaster && character.carryComponent.isBeingCarriedBy == null && ChanceData.RollChance(CHANCE_TYPE.Lycanthrope_Transform_Chance))
		{
			character.lycanData.Transform(character);
		}
	}
}
