using System.Collections.Generic;
using UtilityScripts;

namespace Traits;

public class Restrained : Status
{
	private Character owner;

	public Restrained()
	{
		name = "Restrained";
		description = "Tied up to prevent it from moving.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.FEED,
			INTERACTION_TYPE.REMOVE_RESTRAINED
		};
		ticksDuration = 0;
		hindersMovement = true;
		hindersAttackTarget = true;
		hindersPerform = true;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
		AddTraitOverrideFunctionIdentifier("Hour_Started_Trait");
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
		if (sourceCharacter is Character character)
		{
			owner = character;
			PlayerManager.Instance?.player?.retaliationComponent.OnCharacterDisabled(owner);
			Character character2 = base.responsibleCharacter;
			if (character2 != null)
			{
				owner.classComponent.OnCharacterRestrainedBy(character2);
			}
			UpdateCharacterAwarenessStateOnAddTrait(owner);
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (sourceCharacter is Character character)
		{
			character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.FEED);
			character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.JUDGE_PRISONER);
			character.traitContainer.RemoveTrait(character, "Webbed");
			character.defaultCharacterTrait.SetHasBeenAbductedByWildMonster(state: false);
			character.defaultCharacterTrait.SetHasBeenAbductedByPlayerMonster(state: false);
			UpdateCharacterAwarenessStateOnRemoveTrait(character, removedBy);
			owner = null;
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}

	public override bool OnDeath(Character character)
	{
		return character.traitContainer.RemoveTrait(character, this);
	}

	public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob)
	{
		if (traitOwner is Character)
		{
			Character character = traitOwner as Character;
			if (character.isDead)
			{
				return false;
			}
			if (!character.traitContainer.HasTrait("Criminal") && !characterThatWillDoJob.IsHostileWith(character))
			{
				if (characterThatWillDoJob.traitContainer.HasTrait("Psychopath"))
				{
					return false;
				}
				if (characterThatWillDoJob.isAlliedWithPlayer && character.limiterComponent.isTargetedByDemonicSnatch)
				{
					return false;
				}
				if (character.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_STATUS, name) == null && !IsResponsibleForTrait(characterThatWillDoJob) && InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, character))
				{
					GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
					GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffectData, character, characterThatWillDoJob);
					JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(characterThatWillDoJob, job, INTERACTION_TYPE.NONE);
					characterThatWillDoJob.jobQueue.AddJobInQueue(job);
					return true;
				}
			}
		}
		return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
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
		if (character.isLycanthrope && !character.lycanData.isMaster && character.lycanData.activeForm == character.lycanData.lycanthropeForm && character.carryComponent.isBeingCarriedBy == null && ChanceData.RollChance(CHANCE_TYPE.Lycanthrope_Transform_Chance))
		{
			character.lycanData.Transform(character);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
