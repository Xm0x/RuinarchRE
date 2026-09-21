using System;

public class NewcomerOpinionModifier : SharedOpinionModifier
{
	public override Type serializedData => typeof(SaveDataNewcomerOpinionModifier);

	public override string modifierName => "Newcomer";

	public NewcomerOpinionModifier(Character p_targetCharacter)
		: base(p_targetCharacter)
	{
		Messenger.AddListener(Signals.DAY_STARTED, RemoveNewcomerStackPerDay);
	}

	public NewcomerOpinionModifier(SaveDataNewcomerOpinionModifier data)
		: base(data)
	{
		Messenger.AddListener(Signals.DAY_STARTED, RemoveNewcomerStackPerDay);
	}

	protected override void OnIncreaseModifierValue()
	{
		base.OnIncreaseModifierValue();
		if (base.modifierValue >= 0)
		{
			base.eventDispatcher.ExecuteModifierExpired(this);
			Messenger.RemoveListener(Signals.HOUR_STARTED, RemoveNewcomerStackPerDay);
		}
	}

	private void RemoveNewcomerStackPerDay()
	{
		base.targetCharacter.traitContainer.RemoveTrait(base.targetCharacter, "Newcomer");
	}

	public override bool CanAddModifierToCharacter(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Newcomer") || p_character.homeSettlement != base.targetCharacter.homeSettlement)
		{
			return false;
		}
		return true;
	}

	public override void OnModifierRemovedFromDatabase()
	{
		base.OnModifierRemovedFromDatabase();
		Messenger.RemoveListener(Signals.HOUR_STARTED, RemoveNewcomerStackPerDay);
	}
}
