using UtilityScripts;

namespace Traits;

public class Coward : Trait
{
	public override bool isSingleton => true;

	public Coward()
	{
		name = "Coward";
		description = "A scaredy-cat. Will often flee from combat. If afflicted by the player, will produce a Chaos Orb each time it flees from combat.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character character && !(character is Animal))
		{
			bool num = character.faction != characterThatWillDoJob.faction || character.faction == null;
			bool flag = !characterThatWillDoJob.relationshipContainer.HasRelationshipWith(character);
			if (num && flag && characterThatWillDoJob.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.COWARDICE) && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.COWARDICE).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Flees_From_Anyone))
			{
				characterThatWillDoJob.combatComponent.hostilesInRange.Remove(character);
				characterThatWillDoJob.combatComponent.avoidInRange.Remove(character);
				if (characterThatWillDoJob.combatComponent.Flight(character, "Coward"))
				{
					characterThatWillDoJob.ForceCancelAllJobsTargetingPOI(character, "Actor_Fled");
				}
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		string result = base.TriggerFlaw(character);
		if (character.homeStructure != null && !character.homeStructure.hasBeenDestroyed && character.homeStructure.tiles.Count > 0)
		{
			if (character.currentStructure != character.homeStructure)
			{
				if (character.currentActionNode != null)
				{
					character.StopCurrentActionNode();
				}
				if (character.stateComponent.currentState != null)
				{
					character.stateComponent.ExitCurrentState();
				}
				ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], character, character, null, 0);
				GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, character);
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.RETURN_HOME, character, character);
				goapPlan.SetDoNotRecalculate(state: true);
				goapPlanJob.SetCannotBePushedBack(state: true);
				goapPlanJob.SetAssignedPlan(goapPlan);
				goapPlanJob.SetIsTriggeredByPlayer(isTriggeredByPlayer);
				character.jobQueue.AddJobInQueue(goapPlanJob);
				return result;
			}
			return "fail_at_home";
		}
		return "fail_no_home";
	}

	public bool TryActivatePassOut(Character p_character)
	{
		if (GameUtilities.RollChance(20) && p_character.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.COWARDICE) && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.COWARDICE).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Pass_Out_From_Fright))
		{
			return p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Pass_Out, p_character, "", null, "Pass_Out_Coward");
		}
		return false;
	}
}
