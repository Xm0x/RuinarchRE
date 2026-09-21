using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class KnockoutCharacter : GoapAction
{
	private Precondition _ritualPrecondition;

	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public KnockoutCharacter()
		: base(INTERACTION_TYPE.KNOCKOUT_CHARACTER)
	{
		base.doesNotStopTargetCharacter = true;
		base.canBePerformedEvenIfTargetInCombat = true;
		base.actionIconString = GoapActionStateDB.Stealth_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
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
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Knockout Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = 0;
		if (target is Character)
		{
			if (job.jobType == JOB_TYPE.BLOOD_SACRIFICE)
			{
				num = 1;
			}
			else
			{
				Character character = target as Character;
				switch (actor.relationshipContainer.GetOpinionLabel(character))
				{
				case "Friend":
				case "Close Friend":
					num += 35;
					break;
				case "Enemy":
				case "Rival":
					num = num;
					break;
				default:
					if (actor.faction != character.faction)
					{
						num += 10;
						break;
					}
					goto case "Acquaintance";
				case "Acquaintance":
					num += 20;
					break;
				}
			}
		}
		return num;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (node.associatedJobType == JOB_TYPE.BLOOD_SACRIFICE || !(target is Character character))
		{
			return;
		}
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
		if (node.crimeType == CRIME_TYPE.Vampire)
		{
			if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType).IsConsideredACrime())
			{
				if (witness.characterClass.className == "Stalker")
				{
					reactions.Add(EMOTION.Disapproval);
					return;
				}
				if (witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && actor.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
				{
					reactions.Add(EMOTION.Approval);
					if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
					{
						reactions.Add(EMOTION.Arousal);
					}
					return;
				}
				reactions.Add(EMOTION.Shock);
				if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(actor))
				{
					reactions.Add(EMOTION.Despair);
				}
				if (witness.traitContainer.HasTrait("Coward", "Hemophobic"))
				{
					reactions.Add(EMOTION.Fear);
				}
				else if (!witness.traitContainer.HasTrait("Psychopath"))
				{
					reactions.Add(EMOTION.Threatened);
				}
				if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(character))
				{
					reactions.Add(EMOTION.Anger);
				}
				else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
				{
					reactions.Add(EMOTION.Disapproval);
					reactions.Add(EMOTION.Anger);
				}
			}
			else if (witness.traitContainer.HasTrait("Hemophiliac"))
			{
				if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor))
				{
					reactions.Add(EMOTION.Arousal);
				}
				else
				{
					reactions.Add(EMOTION.Approval);
				}
			}
			else if (witness.traitContainer.HasTrait("Coward", "Hemophobic"))
			{
				reactions.Add(EMOTION.Fear);
			}
		}
		else
		{
			if (node.crimeType != CRIME_TYPE.Assault)
			{
				return;
			}
			if (opinionLabel == "Rival")
			{
				reactions.Add(EMOTION.Approval);
				return;
			}
			if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
			{
				reactions.Add(EMOTION.Rage);
				reactions.Add(EMOTION.Threatened);
				return;
			}
			switch (opinionLabel)
			{
			case "Friend":
			case "Close Friend":
				reactions.Add(EMOTION.Disapproval);
				reactions.Add(EMOTION.Anger);
				reactions.Add(EMOTION.Threatened);
				break;
			case "Acquaintance":
				reactions.Add(EMOTION.Disapproval);
				reactions.Add(EMOTION.Threatened);
				break;
			}
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (node.associatedJobType == JOB_TYPE.BLOOD_SACRIFICE || !(target is Character character))
		{
			return;
		}
		if (GetCrimeType(actor, target, node) == CRIME_TYPE.Vampire)
		{
			if (target.traitContainer.HasTrait("Hemophobic"))
			{
				reactions.Add(EMOTION.Rage);
			}
			else
			{
				reactions.Add(EMOTION.Threatened);
			}
			return;
		}
		reactions.Add(EMOTION.Threatened);
		if (character.traitContainer.HasTrait("Hothead"))
		{
			reactions.Add(EMOTION.Rage);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.associatedJobType == JOB_TYPE.BLOOD_SACRIFICE)
		{
			return REACTABLE_EFFECT.Positive;
		}
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (crime.associatedJobType == JOB_TYPE.BLOOD_SACRIFICE)
		{
			return CRIME_TYPE.None;
		}
		if (crime.associatedJobType == JOB_TYPE.SNATCH)
		{
			return CRIME_TYPE.Assault;
		}
		if (target is Character character && character.race.IsSapient() && !crime.associatedJobType.IsApprehendTypeJob() && crime.associatedJobType != JOB_TYPE.RESTRAIN)
		{
			if (crime.associatedJobType.IsFullnessRecoveryTypeJob() || crime.associatedJobType == JOB_TYPE.IMPRISON_BLOOD_SOURCE)
			{
				return CRIME_TYPE.Vampire;
			}
			return CRIME_TYPE.Assault;
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Assault;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor == poiTarget)
			{
				return false;
			}
			if (poiTarget.traitContainer.HasTrait("Sturdy"))
			{
				return false;
			}
			if (job.jobType == JOB_TYPE.BLOOD_SACRIFICE)
			{
				return true;
			}
			if (actor.race == RACE.TRITON)
			{
				return true;
			}
			if (!actor.isNormalCharacter)
			{
				return false;
			}
			return actor.traitContainer.HasTrait("Psychopath", "Vampire");
		}
		return false;
	}

	public void AfterKnockoutSuccess(ActualGoapNode goapNode)
	{
		goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Unconscious", goapNode.actor);
		goapNode.poiTarget.traitContainer.GetTraitOrStatus<Trait>("Unconscious")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
	}
}
