using UnityEngine;

namespace Traits;

public class Sick : Status
{
	private Character owner;

	private readonly float pukeChance;

	public Sick()
	{
		name = "Sick";
		description = "Has a mild illness.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
		mutuallyExclusive = new string[1] { "Robust" };
		moodEffect = -4;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.5f;
		hindersSocials = true;
		pukeChance = 5f;
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character)
		{
			owner = addTo as Character;
		}
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character)
		{
			owner = sourceCharacter as Character;
			owner.movementComponent.AdjustSpeedModifier(-0.1f);
			if (base.gainedFromDoingType != INTERACTION_TYPE.EAT)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "add_trait", LOG_TAG.Needs);
				log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		owner.movementComponent.AdjustSpeedModifier(0.1f);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "remove_trait", LOG_TAG.Needs);
		log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
		owner = null;
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (Random.Range(0f, 100f) < pukeChance)
		{
			if (owner.characterClass.IsZombie())
			{
				return false;
			}
			return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, owner, "", null, "Sick");
		}
		return false;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
