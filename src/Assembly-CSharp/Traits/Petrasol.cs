namespace Traits;

public class Petrasol : Trait
{
	public override bool isSingleton => true;

	public Petrasol()
	{
		name = "Petrasol";
		description = "Turns to stone when hit by sunlight.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		if (traitable is Character character)
		{
			CheckStonedStatus(character);
		}
	}

	private void CheckStonedStatus(Character character)
	{
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
		{
			character.traitContainer.RemoveTrait(character, "Stoned");
		}
		else if (!character.currentStructure.isInterior)
		{
			if (!character.traitContainer.HasTrait("Stoned"))
			{
				character.traitContainer.AddTrait(character, "Stoned");
			}
		}
		else
		{
			character.traitContainer.RemoveTrait(character, "Stoned");
		}
	}
}
