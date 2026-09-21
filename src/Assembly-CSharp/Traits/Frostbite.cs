using UtilityScripts;

namespace Traits;

public class Frostbite : Status
{
	private ITraitable _owner;

	public Frostbite()
	{
		name = "Frostbite";
		description = "May eventually cause Hypothermia.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		isStacking = true;
		moodEffect = -6;
		stackLimit = 3;
		stackModifier = 0.25f;
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		_owner = addTo;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		_owner = addedTo;
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		_owner = null;
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (GameUtilities.RollChance(0.35f * (float)_owner.traitContainer.GetStacks(name)))
		{
			return FrostbiteEffects();
		}
		return false;
	}

	private bool FrostbiteEffects()
	{
		if (_owner is Character { isDead: false } character)
		{
			if (GameUtilities.RollChance(35))
			{
				return character.interruptComponent.TriggerInterrupt(INTERRUPT.Hypothermia_Death, character);
			}
			if (!character.traitContainer.HasTrait("Injured") && character.traitContainer.AddTrait(character, "Injured"))
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Frostbite_Injured", LOG_TAG.Needs);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
				return true;
			}
		}
		return false;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
