using Traits;

namespace Plague.Fatality;

public abstract class Fatality : Plagued.IPlaguedListener
{
	public abstract PLAGUE_FATALITY fatalityType { get; }

	protected abstract void ActivateFatality(Character p_character);

	protected void ActivateFatalityOn(Character p_character)
	{
		if (CanActivateFatalityOn(p_character))
		{
			p_character.causeOfDeath = INTERACTION_TYPE.PLAGUE_FATALITY;
			ActivateFatality(p_character);
			Messenger.Broadcast(CharacterSignals.PLAGUE_FATALITY_ACTIVATED, this, p_character);
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

	protected virtual bool CanActivateFatalityOn(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Plague Reservoir") || p_character.characterClass.IsZombie() || p_character.raceSetting.category == CHARACTER_CATEGORY.Undead)
		{
			return false;
		}
		return true;
	}
}
