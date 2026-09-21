using System.Collections.Generic;
using UnityEngine;

namespace Traits;

public class Unconscious : Status
{
	public Unconscious()
	{
		name = "Unconscious";
		description = "Knocked out!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.REMOVE_UNCONSCIOUS };
		hindersWitness = true;
		hindersPerform = true;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
		AddTraitOverrideFunctionIdentifier("Hour_Started_Trait");
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character character)
		{
			character.needsComponent.AdjustDoNotGetTired(1);
			if (!character.HasHealth())
			{
				int hP = Mathf.CeilToInt((float)character.maxHP * 0.1f);
				character.SetHP(hP);
			}
			if (base.gainedFromDoingType == INTERACTION_TYPE.NONE)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "add_trait", LOG_TAG.Needs);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
			Messenger.Broadcast(CharacterSignals.REPROCESS_POI, (IPointOfInterest)character);
			PlayerManager.Instance?.player?.retaliationComponent.OnCharacterDisabled(character);
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (sourceCharacter is Character character)
		{
			if (!character.isDead)
			{
				character.AdjustHP(1, ELEMENTAL_TYPE.Normal);
			}
			character.needsComponent.AdjustDoNotGetTired(-1);
			if (!character.traitContainer.HasTrait("Restrained"))
			{
				character.traitContainer.RemoveTrait(character, "Webbed");
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "remove_trait", LOG_TAG.Needs);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}

	public override void OnRemoveStatusBySchedule(ITraitable removedFrom)
	{
		base.OnRemoveStatusBySchedule(removedFrom);
		removedFrom.traitContainer.AddTrait(removedFrom, "Injured");
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		if (traitable is Character character && character.needsComponent.HasNeeds())
		{
			character.needsComponent.AdjustTiredness(1.4f);
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

	private void CheckForLycanthropy(Character character)
	{
		if (character.isLycanthrope && !character.lycanData.isMaster && character.carryComponent.isBeingCarriedBy == null && ChanceData.RollChance(CHANCE_TYPE.Lycanthrope_Transform_Chance))
		{
			character.lycanData.Transform(character);
		}
	}
}
