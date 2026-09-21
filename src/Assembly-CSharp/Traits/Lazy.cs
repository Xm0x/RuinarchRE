using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Lazy : Trait
{
	public Character owner { get; private set; }

	public Lazy()
	{
		name = "Lazy";
		description = "Would rather loaf around than work. If afflicted by the player, will produce a Chaos Orb each time it starts feeling lazy.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			owner = character;
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
		}
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (p_character.HasAfflictedByPlayerWith(this) && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.LAZINESS).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Likes_To_Sleep) && ChanceData.RollChance((PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.LAZINESS).currentLevel == 2) ? CHANCE_TYPE.Laziness_Nap_Level_2 : CHANCE_TYPE.Laziness_Nap_Level_3) && !p_character.jobQueue.HasJob(JOB_TYPE.LAZY_NAP))
		{
			if (p_character.homeStructure != null)
			{
				if (p_character.tileObjectComponent.primaryBed != null && p_character.tileObjectComponent.primaryBed.gridTileLocation != null && p_character.tileObjectComponent.primaryBed.structureLocation == p_character.homeStructure && p_character.movementComponent.HasPathToEvenIfDiffRegion(p_character.homeStructure))
				{
					p_character.PlanFixedJob(JOB_TYPE.LAZY_NAP, INTERACTION_TYPE.NAP, p_character.tileObjectComponent.primaryBed);
					return true;
				}
				if (!p_character.movementComponent.HasPathToEvenIfDiffRegion(p_character.homeStructure))
				{
					TileObject tileObject = p_character.areaLocation?.tileObjectComponent.GetAvailableTileObject(TILE_OBJECT_TYPE.BED);
					if (tileObject != null)
					{
						p_character.PlanFixedJob(JOB_TYPE.LAZY_NAP, INTERACTION_TYPE.NAP, tileObject);
						return true;
					}
				}
			}
			p_character.PlanFixedJob(JOB_TYPE.LAZY_NAP, INTERACTION_TYPE.SLEEP_OUTSIDE, p_character);
			return true;
		}
		return base.PerTickWhileStationaryOrUnoccupied(p_character);
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
		{
			if (character.currentActionNode != null)
			{
				character.StopCurrentActionNode();
			}
			if (character.stateComponent.currentState != null)
			{
				character.stateComponent.ExitCurrentState();
			}
			bool flag = false;
			Heartbroken traitOrStatus = character.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
			if (traitOrStatus != null)
			{
				flag = Random.Range(0, 100) < 25 * owner.traitContainer.stacks[traitOrStatus.name];
			}
			if (!flag)
			{
				if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY))
				{
					character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
				}
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), character, character);
				JobUtilities.PopulatePriorityLocationsForHappinessRecovery(character, goapPlanJob);
				goapPlanJob.SetIsTriggeredByPlayer(isTriggeredByPlayer);
				character.jobQueue.AddJobInQueue(goapPlanJob);
			}
			else
			{
				traitOrStatus.TriggerBrokenhearted();
			}
			return base.TriggerFlaw(character);
		}
		return "has_trigger_flaw";
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
	}

	public bool TriggerLazy()
	{
		if (owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Lazy, owner))
		{
			if (owner.HasAfflictedByPlayerWith(this))
			{
				DispenseChaosOrbsForAffliction(owner, PLAYER_SKILL_TYPE.LAZINESS, 1);
			}
			return true;
		}
		return false;
	}

	public bool TryIgnoreUrgentTask(JOB_TYPE job)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Ignore_Urgent_Task) && owner.HasAfflictedByPlayerWith(this) && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.LAZINESS).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Ignore_Urgent_Tasks))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", "Lazy ignore_urgent_job", LOG_TAG.Work, LOG_TAG.Player);
			log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, job.ToStringEnumWithSpaceNormalized(), LOG_IDENTIFIER.STRING_1);
			owner.logComponent.RegisterLog(log);
			DispenseChaosOrbsForAffliction(owner, PLAYER_SKILL_TYPE.LAZINESS, 1);
			return true;
		}
		return false;
	}

	public float GetTriggerChance(Character p_character)
	{
		if (p_character.HasAfflictedByPlayerWith(this))
		{
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.LAZINESS);
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.LAZINESS);
			return scriptableObjPlayerSkillData.afflictionUpgradeData.GetRateChancePerLevel(skillData.currentLevel);
		}
		return PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.LAZINESS).afflictionUpgradeData.GetRateChancePerLevel(0);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
