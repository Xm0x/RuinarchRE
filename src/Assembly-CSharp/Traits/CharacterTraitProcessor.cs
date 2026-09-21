using UnityEngine;

namespace Traits;

public class CharacterTraitProcessor : TraitProcessor
{
	public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, int overrideDuration)
	{
		Character character = traitable as Character;
		if (trait is Status status)
		{
			ApplyStatusEffects(character, status);
		}
		ApplyTraitEffects(character, trait);
		Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)character);
		if (GameManager.Instance.gameHasStarted)
		{
			if (trait.name == "Starving")
			{
				character.needsComponent.PlanFullnessRecoveryActions();
			}
			else if (!(trait.name == "Sulking") && !(trait.name == "Lonely") && trait.name == "Exhausted")
			{
				character.needsComponent.PlanTirednessRecoveryActions();
			}
		}
		DefaultProcessOnAddTrait(traitable, trait, characterResponsible, overrideDuration);
		if (trait.affectsNameIcon)
		{
			character.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(character);
			if (character.hasMarker)
			{
				character.marker.UpdateName();
			}
		}
		Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_ADDED, character, trait);
	}

	public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy)
	{
		Character character = traitable as Character;
		if (trait is Status status)
		{
			UnapplyStatusEffects(character, status);
		}
		UnapplyTraitEffects(character, trait);
		Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)character);
		DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
		Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_REMOVED, character, trait);
	}

	public override void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, int overrideDuration)
	{
		Character arg = traitable as Character;
		if (DefaultProcessOnStackStatus(traitable, status, characterResponsible, overrideDuration))
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_STACKED, arg, status.GetBase());
		}
	}

	public override void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null, bool bySchedule = false)
	{
		Character arg = traitable as Character;
		DefaultProcessOnUnstackStatus(traitable, status, removedBy, bySchedule);
		Messenger.Broadcast(CharacterSignals.CHARACTER_TRAIT_UNSTACKED, arg, status.GetBase());
	}

	private void ApplyStatusEffects(Character character, Status status)
	{
		if (status.hindersWitness)
		{
			character.limiterComponent.DecreaseCanWitness();
		}
		if (status.hindersMovement)
		{
			character.limiterComponent.DecreaseCanMove();
		}
		if (status.hindersAttackTarget)
		{
			character.limiterComponent.DecreaseCanBeAttacked();
		}
		if (status.hindersPerform)
		{
			character.limiterComponent.DecreaseCanPerform();
		}
		if (status.hindersSocials)
		{
			character.limiterComponent.DecreaseSociable();
		}
		if (status.hindersFullnessRecovery)
		{
			character.limiterComponent.DecreaseCanDoFullnessRecovery();
		}
		if (status.hindersHappinessRecovery)
		{
			character.limiterComponent.DecreaseCanDoHappinessRecovery();
		}
		if (status.hindersTirednessRecovery)
		{
			character.limiterComponent.DecreaseCanDoTirednessRecovery();
		}
	}

	private void ApplyTraitEffects(Character character, Trait trait)
	{
		if (trait.resistancesType != null && trait.resistancesValue != null)
		{
			if (trait.resistancesType.Count != trait.resistancesValue.Count)
			{
				Debug.LogError("Resistances does not match for " + trait.name);
			}
			else
			{
				for (int i = 0; i < trait.resistancesType.Count; i++)
				{
					character.piercingAndResistancesComponent.AdjustResistance(trait.resistancesType[i], trait.resistancesValue[i]);
				}
			}
		}
		if (trait.name == "Abducted" || trait.name == "Restrained")
		{
			character.needsComponent.AdjustDoNotGetTired(1);
		}
		else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Resting" || trait.name == "Unconscious" || trait.name == "Recuperating")
		{
			character.needsComponent.AdjustDoNotGetTired(1);
			character.needsComponent.AdjustDoNotGetHungry(1);
			character.needsComponent.AdjustDoNotGetBored(1);
		}
		else if (trait.name == "Charmed")
		{
			character.needsComponent.AdjustDoNotGetBored(1);
		}
		else if (trait.name == "Eating")
		{
			character.needsComponent.AdjustDoNotGetHungry(1);
		}
		else if (trait.name == "Daydreaming")
		{
			character.needsComponent.AdjustDoNotGetTired(1);
			character.needsComponent.AdjustDoNotGetBored(1);
		}
		else if (trait.name == "Optimist")
		{
			character.needsComponent.AdjustHappinessDecreaseRateDivisor(2f);
		}
		else if (trait.name == "Pessimist")
		{
			character.needsComponent.AdjustHappinessDecreaseRateMultiplier(0.5f);
		}
		else if (trait.name == "Fast")
		{
			character.movementComponent.AdjustSpeedModifier(0.25f);
		}
	}

	private void UnapplyStatusEffects(Character character, Status status)
	{
		if (status.hindersWitness)
		{
			character.limiterComponent.IncreaseCanWitness();
		}
		if (status.hindersMovement)
		{
			character.limiterComponent.IncreaseCanMove();
		}
		if (status.hindersAttackTarget)
		{
			character.limiterComponent.IncreaseCanBeAttacked();
		}
		if (status.hindersPerform)
		{
			character.limiterComponent.IncreaseCanPerform();
		}
		if (status.hindersSocials)
		{
			character.limiterComponent.IncreaseSociable();
		}
		if (status.hindersFullnessRecovery)
		{
			character.limiterComponent.IncreaseCanDoFullnessRecovery();
		}
		if (status.hindersHappinessRecovery)
		{
			character.limiterComponent.IncreaseCanDoHappinessRecovery();
		}
		if (status.hindersTirednessRecovery)
		{
			character.limiterComponent.IncreaseCanDoTirednessRecovery();
		}
	}

	public void UnapplyTraitEffects(Character character, Trait trait)
	{
		if (trait.resistancesType != null && trait.resistancesValue != null)
		{
			if (trait.resistancesType.Count != trait.resistancesValue.Count)
			{
				Debug.LogError("Resistances does not match for " + trait.name);
			}
			else
			{
				for (int i = 0; i < trait.resistancesType.Count; i++)
				{
					character.piercingAndResistancesComponent.AdjustResistance(trait.resistancesType[i], 0f - trait.resistancesValue[i]);
				}
			}
		}
		if (trait.name == "Abducted" || trait.name == "Restrained")
		{
			character.needsComponent.AdjustDoNotGetTired(-1);
		}
		else if (trait.name == "Packaged" || trait.name == "Hibernating" || trait.name == "Resting" || trait.name == "Unconscious")
		{
			character.needsComponent.AdjustDoNotGetTired(-1);
			character.needsComponent.AdjustDoNotGetHungry(-1);
			character.needsComponent.AdjustDoNotGetBored(-1);
		}
		else if (trait.name == "Charmed")
		{
			character.needsComponent.AdjustDoNotGetBored(-1);
		}
		else if (trait.name == "Eating")
		{
			character.needsComponent.AdjustDoNotGetHungry(-1);
		}
		else if (trait.name == "Daydreaming")
		{
			character.needsComponent.AdjustDoNotGetTired(-1);
			character.needsComponent.AdjustDoNotGetBored(-1);
		}
		else if (trait.name == "Optimist")
		{
			character.needsComponent.AdjustHappinessDecreaseRateDivisor(-2f);
		}
		else if (trait.name == "Pessimist")
		{
			character.needsComponent.AdjustHappinessDecreaseRateMultiplier(-0.5f);
		}
		else if (trait.name == "Fast")
		{
			character.movementComponent.AdjustSpeedModifier(-0.25f);
		}
	}
}
