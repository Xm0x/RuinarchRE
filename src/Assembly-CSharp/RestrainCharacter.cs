using System.Collections.Generic;
using Traits;
using UnityEngine;

public class RestrainCharacter : GoapAction
{
	public RestrainCharacter()
		: base(INTERACTION_TYPE.RESTRAIN_CHARACTER)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Restrain_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[3]
		{
			LOG_TAG.Work,
			LOG_TAG.Crimes,
			LOG_TAG.Combat
		};
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		if (node.associatedJobType == JOB_TYPE.BLOOD_SACRIFICE)
		{
			return false;
		}
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), TargetUnconsciousOrParalyzed);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Restrain Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && !(poiTarget as Character).carryComponent.IsNotBeingCarried())
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "target_carried";
		}
		return goapActionInvalidity;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (node.associatedJobType == JOB_TYPE.BLOOD_SACRIFICE || !(target is Character character))
		{
			return;
		}
		if (character.traitContainer.HasTrait("Criminal"))
		{
			if (witness.relationshipContainer.IsFriendsWith(character))
			{
				reactions.Add(EMOTION.Resentment);
			}
			else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
			{
				reactions.Add(EMOTION.Resentment);
			}
			else
			{
				reactions.Add(EMOTION.Approval);
			}
		}
		else
		{
			if (witness.relationshipContainer.IsEnemiesWith(character) || witness.IsHostileWith(character))
			{
				return;
			}
			if (witness.relationshipContainer.IsFriendsWith(character))
			{
				if (!witness.traitContainer.HasTrait("Psychopath"))
				{
					reactions.Add(EMOTION.Resentment);
				}
				if (Random.Range(0, 100) < 35 && !witness.traitContainer.HasTrait("Diplomatic"))
				{
					reactions.Add(EMOTION.Anger);
				}
			}
			else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
			{
				if (!witness.traitContainer.HasTrait("Psychopath"))
				{
					reactions.Add(EMOTION.Resentment);
				}
				if (Random.Range(0, 100) < 35 && !witness.traitContainer.HasTrait("Diplomatic"))
				{
					reactions.Add(EMOTION.Anger);
				}
			}
			else if (witness.relationshipContainer.IsEnemiesWith(character))
			{
				reactions.Add(EMOTION.Approval);
			}
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (!(target is Character character))
		{
			return;
		}
		if (node.associatedJobType.IsApprehendTypeJob())
		{
			if (witness.relationshipContainer.IsFriendsWith(character))
			{
				if (!witness.traitContainer.HasTrait("Psychopath"))
				{
					reactions.Add(EMOTION.Concern);
					reactions.Add(EMOTION.Sadness);
				}
			}
			else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
			{
				if (!witness.traitContainer.HasTrait("Psychopath"))
				{
					reactions.Add(EMOTION.Concern);
					reactions.Add(EMOTION.Sadness);
				}
			}
			else if (Random.Range(0, 100) < 30 && !witness.traitContainer.HasTrait("Diplomatic"))
			{
				reactions.Add(EMOTION.Scorn);
			}
			return;
		}
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
		if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Distraught);
			}
		}
		else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Distraught);
			}
		}
		else if (opinionLabel == "Acquaintance")
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Concern);
			}
		}
		else if (((witness.faction != null && witness.faction.leader == character) || (witness.homeSettlement != null && witness.homeSettlement.ruler == character)) && opinionLabel != "Rival")
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Distraught);
			}
		}
		else if (opinionLabel == "Enemy" || opinionLabel == "Rival")
		{
			if (!witness.traitContainer.HasTrait("Diplomatic"))
			{
				reactions.Add(EMOTION.Scorn);
			}
			else
			{
				reactions.Add(EMOTION.Concern);
			}
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (node.associatedJobType != JOB_TYPE.BLOOD_SACRIFICE && target is Character character && !character.IsHostileWith(actor))
		{
			reactions.Add(EMOTION.Resentment);
			if (character.traitContainer.HasTrait("Hothead") || Random.Range(0, 100) < 35)
			{
				reactions.Add(EMOTION.Anger);
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.poiTarget is Character otherCharacter && (node.poiTarget.traitContainer.HasTrait("Criminal") || witness.IsHostileWith(otherCharacter) || node.associatedJobType == JOB_TYPE.BLOOD_SACRIFICE))
		{
			return REACTABLE_EFFECT.Positive;
		}
		return REACTABLE_EFFECT.Negative;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget && poiTarget is Character character)
			{
				if (!character.isDead)
				{
					if (character.traitContainer.HasTrait("Restrained"))
					{
						Prisoner traitOrStatus = character.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
						if (traitOrStatus != null)
						{
							if (ShouldBePersonalPrisoner(job.jobType, actor))
							{
								if (!traitOrStatus.IsPersonalPrisonerOf(actor))
								{
									return true;
								}
							}
							else if (actor.faction != null && !traitOrStatus.IsFactionPrisonerOf(actor.faction))
							{
								return true;
							}
						}
						return false;
					}
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public void AfterRestrainSuccess(ActualGoapNode goapNode)
	{
		Faction factionThatImprisoned = null;
		Character characterThatImprisoned = null;
		if (ShouldBePersonalPrisoner(goapNode.associatedJobType, goapNode.actor))
		{
			characterThatImprisoned = goapNode.actor;
		}
		else
		{
			factionThatImprisoned = goapNode.actor.faction;
		}
		goapNode.poiTarget.traitContainer.RestrainAndImprison(goapNode.poiTarget, goapNode.actor, factionThatImprisoned, characterThatImprisoned);
	}

	private bool TargetUnconsciousOrParalyzed(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType)
	{
		return target.traitContainer.HasTrait("Unconscious", "Paralyzed", "Stoned");
	}

	private bool ShouldBePersonalPrisoner(JOB_TYPE jobType, Character p_actor)
	{
		if (jobType != JOB_TYPE.APPREHEND && jobType != JOB_TYPE.APPREHEND_RESTRAINED && jobType != JOB_TYPE.KIDNAP_RAID)
		{
			Faction faction = p_actor.faction;
			if ((faction == null || faction.factionType.type != FACTION_TYPE.Ratmen || jobType != JOB_TYPE.MONSTER_ABDUCT) && jobType != JOB_TYPE.RESTRAIN && jobType != JOB_TYPE.SNATCH && jobType != JOB_TYPE.SNATCH_RESTRAIN && (jobType != JOB_TYPE.MONSTER_ABDUCT || !(p_actor is Tarantula)) && (jobType != JOB_TYPE.CAPTURE_CHARACTER || (p_actor.race != RACE.CENTAUR && p_actor.race != RACE.KOBOLD)) && jobType != JOB_TYPE.IMPRISON_BLOOD_SOURCE && jobType != JOB_TYPE.FACTION_KIDNAP)
			{
				return true;
			}
		}
		return false;
	}
}
