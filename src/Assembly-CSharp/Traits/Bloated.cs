namespace Traits;

public class Bloated : Status
{
	public Bloated()
	{
		name = "Bloated";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		moodEffect = -7;
		isStacking = false;
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character character)
		{
			character.movementComponent.AdjustSpeedModifier(-0.5f);
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (sourceCharacter is Character character)
		{
			character.movementComponent.AdjustSpeedModifier(0.5f);
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}
}
