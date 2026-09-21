using System.Collections.Generic;
using UtilityScripts;

public class Liberate : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Liberate()
		: base(INTERACTION_TYPE.LIBERATE)
	{
		base.actionIconString = GoapActionStateDB.Cult_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Player,
			LOG_TAG.Crimes
		};
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && poiTarget is Character character)
		{
			if (character.traitContainer.HasTrait("Berserked"))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_unavailable";
			}
			else if (!character.carryComponent.IsNotBeingCarried())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_carried";
			}
		}
		return goapActionInvalidity;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Liberate Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 0;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (witness.religionComponent.religion != actor.religionComponent.religion)
		{
			reactions.Add(EMOTION.Shock);
			if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType).IsConsideredACrime())
			{
				reactions.Add(EMOTION.Disapproval);
			}
		}
		else
		{
			reactions.Add(EMOTION.Approval);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (actor != target)
		{
			reactions.Add(EMOTION.Gratefulness);
		}
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string value = node.actor.religionComponent.religion.LocalizedName();
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return GetCrimeTypeBasedOnActorReligion(actor);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return GetCrimeTypeBasedOnActorReligion(actor);
	}

	private CRIME_TYPE GetCrimeTypeBasedOnActorReligion(Character p_actor)
	{
		return p_actor.religionComponent.religion.GetCrimeTypeByReligion();
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			string cultistTraitNameForReligion = actor.religionComponent.religion.GetCultistTraitNameForReligion();
			if (target != actor && !target.traitContainer.HasTrait(cultistTraitNameForReligion))
			{
				return target.traitContainer.HasTrait("Restrained", "Ensnared");
			}
			return false;
		}
		return false;
	}

	public void AfterLiberateSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
		character.traitContainer.RemoveStatusAndStacks(character, "Ensnared", goapNode.actor);
		character.combatComponent.RemoveHostileInRange(goapNode.actor);
		character.combatComponent.RemoveAvoidInRange(goapNode.actor);
		RELIGION religion = goapNode.actor.religionComponent.religion;
		RELIGION religion2 = character.religionComponent.religion;
		if (religion2 != religion)
		{
			character.religionComponent.DecreaseBeliefPoints(religion2, 10);
		}
		character.religionComponent.IncreaseBeliefPointsFromReligiousActions(religion, base.goapType, GameUtilities.RandomBetweenTwoNumbers(20, 40));
		RELIGION religion3 = character.religionComponent.religion;
		Log log = null;
		if (!character.traitContainer.IsReligiousCultist(religion))
		{
			log = ((religion3 != religion) ? GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Evangelize success_no_convert", LOG_TAG.Life_Changes, goapNode) : ((religion3 != religion2) ? GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Evangelize success_convert", LOG_TAG.Life_Changes, goapNode) : GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Evangelize success_no_convert", LOG_TAG.Life_Changes, goapNode)));
		}
		else
		{
			log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Evangelize success_cultist", LOG_TAG.Life_Changes, goapNode);
			log.AddToFillers(null, goapNode.actor.religionComponent.religion.GetCultistTraitNameForReligion(), LOG_IDENTIFIER.STRING_1);
		}
		log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
	}
}
