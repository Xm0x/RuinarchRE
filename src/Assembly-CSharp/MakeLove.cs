using System.Collections.Generic;
using JetBrains.Annotations;
using Traits;
using UtilityScripts;

public class MakeLove : GoapAction
{
	public MakeLove()
		: base(INTERACTION_TYPE.MAKE_LOVE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Flirt_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Needs,
			LOG_TAG.Social
		};
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.INVITED, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsTargetInvited);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Make Love Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.isActiveMember)
		{
			return 2000;
		}
		Character character = target as Character;
		if (character != null && character.partyComponent.hasParty && character.partyComponent.currentParty.isActive && character.partyComponent.isActiveMember)
		{
			return 2000;
		}
		int num = Utilities.Rng.Next(90, 131);
		if (job.jobType != JOB_TYPE.TRIGGER_FLAW && job.jobType != JOB_TYPE.TRIGGER_AROUSAL)
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
			if (actor.race.IsSapient() && currentTimeInWordsOfTick != TIME_IN_WORDS.EARLY_NIGHT && currentTimeInWordsOfTick != TIME_IN_WORDS.LATE_NIGHT && currentTimeInWordsOfTick != TIME_IN_WORDS.AFTER_MIDNIGHT)
			{
				num += 2000;
			}
		}
		if (job.jobType != JOB_TYPE.TRIGGER_AROUSAL)
		{
			Angry traitOrStatus = actor.traitContainer.GetTraitOrStatus<Angry>("Angry");
			if (actor.traitContainer.HasTrait("Chaste") || (traitOrStatus != null && traitOrStatus.IsResponsibleForTrait(character)))
			{
				num += 2000;
			}
			if (actor.traitContainer.HasTrait("Lustful"))
			{
				num -= 40;
			}
			else
			{
				int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
				if (numOfTimesActionDone > 5)
				{
					num += 2000;
				}
				else
				{
					int num2 = 10 * numOfTimesActionDone;
					num += num2;
				}
			}
		}
		return num;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		Character character = node.poiTarget as Character;
		actor.UncarryPOI(character);
		List<TileObject> tileObjectsOfType = actor.gridTileLocation.structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED);
		if (tileObjectsOfType != null && tileObjectsOfType.Count > 0)
		{
			(tileObjectsOfType[0] as Bed)?.OnDoneActionToObject(actor.currentActionNode);
		}
		if (character.currentActionNode != null && character.currentActionNode.action == this)
		{
			character.SetCurrentActionNode(null, null, null);
		}
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		Character character = node.poiTarget as Character;
		actor.UncarryPOI(character);
		if (character.currentActionNode != null && character.currentActionNode.action == this)
		{
			character.SetCurrentActionNode(null, null, null);
		}
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		Character target = goapNode.poiTarget as Character;
		return GetValidBedForActor(goapNode.actor, target, goapNode.associatedJob);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			_ = node.poiTarget;
			Bed bed = node.actor.gridTileLocation.tileObjectComponent.objHere as Bed;
			if (bed == null)
			{
				for (int i = 0; i < node.actor.gridTileLocation.neighbourList.Count; i++)
				{
					if (node.actor.gridTileLocation.neighbourList[i].tileObjectComponent.objHere is Bed bed2)
					{
						bed = bed2;
					}
				}
			}
			if (bed == null || !bed.IsAvailable() || bed.GetActiveUserCount() > 0)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.stateName = "Make Love Fail";
			}
			else if (bed != null && bed.traitContainer.HasTrait("Wet"))
			{
				if (node.associatedJob == null)
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "wet_bed";
				}
				else if (!(node.associatedJob is GoapPlanJob { isAgitateJob: not false }))
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "wet_bed";
				}
			}
		}
		return goapActionInvalidity;
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		Character actor = node.actor;
		Character character = node.poiTarget as Character;
		actor.UncarryPOI(character);
		if (character.currentActionNode != null && character.currentActionNode.action == this)
		{
			character.SetCurrentActionNode(null, null, null);
		}
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (status == REACTION_STATUS.WITNESSED)
		{
			reactions.Add(EMOTION.Shock);
		}
		if (!(target is Character character))
		{
			return;
		}
		if (!actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER))
		{
			Character firstCharacterWithRelationship = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
			if (firstCharacterWithRelationship != null && firstCharacterWithRelationship == witness)
			{
				reactions.Add(EMOTION.Betrayal);
				reactions.Add(EMOTION.Disapproval);
			}
			Character firstCharacterWithRelationship2 = character.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
			if (firstCharacterWithRelationship2 != null)
			{
				if (witness == firstCharacterWithRelationship2)
				{
					reactions.Add(EMOTION.Rage);
				}
				if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor))
				{
					reactions.Add(EMOTION.Betrayal);
				}
				else
				{
					reactions.Add(EMOTION.Resentment);
				}
			}
		}
		else if (witness.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.AFFAIR))
		{
			reactions.Add(EMOTION.Resentment);
		}
		if (witness != actor && witness != character && witness.traitContainer.HasTrait("Obsessed") && witness.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == character && !reactions.Contains(EMOTION.Rage))
		{
			reactions.Add(EMOTION.Rage);
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (status == REACTION_STATUS.WITNESSED)
		{
			reactions.Add(EMOTION.Shock);
		}
		if (!(target is Character character))
		{
			return;
		}
		if (!actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER))
		{
			Character characterByID = CharacterManager.Instance.GetCharacterByID(character.relationshipContainer.GetFirstAliveOrUnspawnedRelationshipID(RELATIONSHIP_TYPE.LOVER));
			if (characterByID != null && characterByID == actor && characterByID == witness)
			{
				reactions.Add(EMOTION.Betrayal);
				reactions.Add(EMOTION.Disapproval);
			}
			if (CharacterManager.Instance.GetCharacterByID(actor.relationshipContainer.GetFirstAliveOrUnspawnedRelationshipID(RELATIONSHIP_TYPE.LOVER)) != null)
			{
				if (witness == characterByID)
				{
					reactions.Add(EMOTION.Rage);
				}
				if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor))
				{
					reactions.Add(EMOTION.Betrayal);
				}
				else
				{
					reactions.Add(EMOTION.Resentment);
				}
			}
		}
		else if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR))
		{
			reactions.Add(EMOTION.Resentment);
		}
		if (witness != actor && witness != character && witness.traitContainer.HasTrait("Obsessed") && witness.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == actor && !reactions.Contains(EMOTION.Rage))
		{
			reactions.Add(EMOTION.Rage);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (target.traitContainer.HasTrait("Obsessed") && target.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == actor)
		{
			reactions.Add(EMOTION.Arousal);
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		bool flag = false;
		Character character = target as Character;
		if (character != null && witness != actor && witness != character)
		{
			flag = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER);
		}
		string result = base.ReactionToActor(actor, target, witness, node, status);
		if (character != null && witness != actor && witness != character)
		{
			if (flag)
			{
				result = "Infidelity_Make_Love_Lover_Reaction";
			}
			if (witness.traitContainer.HasTrait("Obsessed") && witness.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == character)
			{
				witness.relationshipContainer.SetHasGrudgeAgainst(witness, actor, p_state: true);
			}
		}
		return result;
	}

	public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToTarget(actor, target, witness, node, status);
		if (target is Character character && witness != actor && witness != character && witness.traitContainer.HasTrait("Obsessed") && witness.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == actor)
		{
			witness.relationshipContainer.SetHasGrudgeAgainst(witness, character, p_state: true);
		}
		return result;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.poiTarget is Character relatable && !node.actor.relationshipContainer.HasRelationshipWith(relatable, RELATIONSHIP_TYPE.LOVER))
		{
			return REACTABLE_EFFECT.Negative;
		}
		return REACTABLE_EFFECT.Neutral;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (target is Character character && ((!actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER) && actor.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER)) || (!character.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER) && character.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))))
		{
			return CRIME_TYPE.Infidelity;
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Infidelity;
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public void PreMakeLoveSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		Bed bed = null;
		if (actor.tileObjectComponent.primaryBed != null)
		{
			if (actor.tileObjectComponent.primaryBed.gridTileLocation != null && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation, sameStructureOnly: true)))
			{
				bed = actor.tileObjectComponent.primaryBed;
			}
		}
		else if (character.tileObjectComponent.primaryBed != null && character.tileObjectComponent.primaryBed.gridTileLocation != null && (actor.gridTileLocation == character.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(character.tileObjectComponent.primaryBed.gridTileLocation, sameStructureOnly: true)))
		{
			bed = character.tileObjectComponent.primaryBed;
		}
		if (bed != null)
		{
			goapNode.actor.UncarryPOI(character, bringBackToInventory: false, addToLocation: true, bed.gridTileLocation);
			bed.OnDoActionToObject(goapNode);
			goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
			character.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
			character.SetCurrentActionNode(goapNode.actor.currentActionNode, goapNode.actor.currentJob, goapNode.actor.currentPlan);
		}
		if (actor.traitContainer.HasTrait("Obsessed") && actor.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").targetCharacter == character)
		{
			CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, actor, character);
		}
		goapNode.descriptionLog.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	}

	public void PerTickMakeLoveSuccess(ActualGoapNode goapNode)
	{
		Character obj = goapNode.poiTarget as Character;
		goapNode.actor.needsComponent.AdjustHappiness(6f);
		obj.needsComponent.AdjustHappiness(6f);
	}

	public void AfterMakeLoveSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		Bed bed = null;
		if (actor.tileObjectComponent.primaryBed != null)
		{
			if (actor.tileObjectComponent.primaryBed.gridTileLocation != null && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation, sameStructureOnly: true)))
			{
				bed = actor.tileObjectComponent.primaryBed;
			}
		}
		else if (character.tileObjectComponent.primaryBed != null && character.tileObjectComponent.primaryBed.gridTileLocation != null && (actor.gridTileLocation == character.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(character.tileObjectComponent.primaryBed.gridTileLocation, sameStructureOnly: true)))
		{
			bed = character.tileObjectComponent.primaryBed;
		}
		bed?.OnDoneActionToObject(goapNode);
		if (actor is SeducerSummon)
		{
			character.Death("seduced", goapNode, actor, null, null, null, null, isPlayerSource: false, actor);
		}
		if (character.currentActionNode == goapNode)
		{
			character.SetCurrentActionNode(null, null, null);
		}
	}

	private bool IsTargetInvited(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.carryComponent.IsPOICarried(poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			Character character = poiTarget as Character;
			if (character == actor)
			{
				return false;
			}
			if (character.stateComponent.currentState is CombatState)
			{
				return false;
			}
			if (character.hasBeenRaisedFromDead)
			{
				return false;
			}
			if (character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld || character.currentRegion != actor.currentRegion)
			{
				return false;
			}
			if (GetValidBedForActor(actor, character, job) == null)
			{
				return false;
			}
			if (!(actor is SeducerSummon))
			{
				if (job.jobType == JOB_TYPE.TRIGGER_AROUSAL)
				{
					return true;
				}
				if (!actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER) && !actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.AFFAIR))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private Bed GetValidBedForActor(Character actor, [NotNull] Character target, JobQueueItem p_job)
	{
		Bed result = null;
		if (actor.tileObjectComponent.primaryBed != null && actor.tileObjectComponent.primaryBed.gridTileLocation != null)
		{
			if (!actor.tileObjectComponent.primaryBed.traitContainer.HasTrait("Wet"))
			{
				result = actor.tileObjectComponent.primaryBed;
			}
			else if (p_job != null && p_job is GoapPlanJob { isAgitateJob: not false })
			{
				result = actor.tileObjectComponent.primaryBed;
			}
		}
		else if (target.tileObjectComponent.primaryBed != null && target.tileObjectComponent.primaryBed.gridTileLocation != null)
		{
			if (!target.tileObjectComponent.primaryBed.traitContainer.HasTrait("Wet"))
			{
				result = target.tileObjectComponent.primaryBed;
			}
			else if (p_job != null && p_job is GoapPlanJob { isAgitateJob: not false })
			{
				result = target.tileObjectComponent.primaryBed;
			}
		}
		return result;
	}
}
