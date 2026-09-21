namespace Traits;

public class Nocturnal : Trait
{
	public override bool isSingleton => true;

	public Nocturnal()
	{
		name = "Nocturnal";
		description = "Awake at night and asleep during the day.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		if (sourcePOI is Character)
		{
			Character character = sourcePOI as Character;
			character.dailyScheduleComponent.OnCharacterGainedNocturnal(character);
		}
	}

	public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy)
	{
		base.OnRemoveTrait(sourcePOI, removedBy);
		if (sourcePOI is Character)
		{
			Character character = sourcePOI as Character;
			character.dailyScheduleComponent.OnCharacterLostNocturnal(character);
		}
	}
}
