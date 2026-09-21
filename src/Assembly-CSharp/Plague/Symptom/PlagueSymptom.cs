using Traits;

namespace Plague.Symptom;

public abstract class PlagueSymptom : Plagued.IPlaguedListener
{
	public abstract PLAGUE_SYMPTOM symptomType { get; }

	protected abstract void ActivateSymptom(Character p_character);

	protected void ActivateSymptomOn(Character p_character)
	{
		if (CanActivateSymptomOn(p_character))
		{
			ActivateSymptom(p_character);
		}
	}

	public virtual void PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
	}

	public virtual void CharacterGainedTrait(Character p_character, Trait p_gainedTrait)
	{
	}

	public virtual void CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action)
	{
	}

	public virtual void CharacterDonePerformingAction(Character p_character, INTERACTION_TYPE p_actionPerformed)
	{
	}

	public virtual void HourStarted(Character p_character, int p_numOfHoursPassed)
	{
	}

	protected virtual bool CanActivateSymptomOn(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Plague Reservoir") || p_character.characterClass.IsZombie() || p_character.raceSetting.category == CHARACTER_CATEGORY.Undead)
		{
			return false;
		}
		return true;
	}
}
