using System.Collections.Generic;
using UtilityScripts;

public class DarkRitual : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public override bool isTargetSelf => true;

	public DarkRitual()
		: base(INTERACTION_TYPE.DARK_RITUAL)
	{
		base.actionIconString = GoapActionStateDB.Cult_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Crimes
		};
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasCultistKit);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Ritual Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int result = 10;
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target) && !actor.partyComponent.hasParty)
		{
			return 2000;
		}
		return result;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Demon_Worship;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Demon_Worship;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		bool flag = false;
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Demon_Worship).IsConsideredACrime())
		{
			flag = true;
		}
		if (!witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && flag)
		{
			reactions.Add(EMOTION.Repulsed);
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				if (witness.traitContainer.HasTrait("Psychopath"))
				{
					return;
				}
				if (witness.classComponent.IsTargetRecognizedAsCultistByStalker(actor))
				{
					reactions.Add(EMOTION.Disapproval);
					return;
				}
				reactions.Add(EMOTION.Threatened);
				string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
				if (opinionLabel == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else if (opinionLabel == "Friend")
				{
					reactions.Add(EMOTION.Shock);
				}
			}
		}
		else
		{
			reactions.Add(EMOTION.Approval);
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
			{
				reactions.Add(EMOTION.Arousal);
			}
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return target.gridTileLocation != null;
		}
		return false;
	}

	private bool HasCultistKit(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItem("Cultist Kit");
	}

	public void AfterRitualSuccess(ActualGoapNode goapNode)
	{
		Messenger.Broadcast(JobSignals.ON_FINISH_PRAYING, goapNode);
		goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
	}
}
