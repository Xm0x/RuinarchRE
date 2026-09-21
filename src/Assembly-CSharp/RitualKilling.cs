using System.Collections.Generic;

public class RitualKilling : GoapAction
{
	private Precondition atHomePrecondition;

	private Precondition notAtHomePrecondition;

	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public RitualKilling()
		: base(INTERACTION_TYPE.RITUAL_KILLING)
	{
		base.actionIconString = GoapActionStateDB.Death_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Crimes,
			LOG_TAG.Life_Changes
		};
		atHomePrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), HasRestrained);
		notAtHomePrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsTargetInWildernessOrHome);
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Killing Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (target is Character)
		{
			Precondition precondition = null;
			Character character = target as Character;
			precondition = ((actor.homeStructure != character.currentStructure) ? notAtHomePrecondition : atHomePrecondition);
			isOverridden = true;
			return precondition;
		}
		return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		if (target is Character character && actor.faction != null && witness.faction != null && witness.faction.isMajorNonPlayerOrBandits && actor.faction.isMajorNonPlayerOrBandits && actor.faction != witness.faction && witness.faction == character.faction)
		{
			if (witness.isFactionLeader || witness.isSettlementRuler)
			{
				witness.faction.FactionProcessingAbductionOrMurder(actor, character, node);
				return result;
			}
			if (witness.faction.leader is Character || (witness.homeSettlement != null && witness.homeSettlement.ruler != null))
			{
				witness.jobComponent.TryCreateReportMurderOrAbduct(node);
			}
		}
		return result;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is Character target2))
		{
			return;
		}
		if (witness.traitContainer.HasTrait("Coward"))
		{
			reactions.Add(EMOTION.Fear);
			reactions.Add(EMOTION.Shock);
			return;
		}
		if (witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Approval);
			return;
		}
		reactions.Add(EMOTION.Threatened);
		reactions.Add(EMOTION.Disgust);
		reactions.Add(EMOTION.Shock);
		if (witness.relationshipContainer.IsFriendsWith(actor))
		{
			reactions.Add(EMOTION.Disappointment);
		}
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(target2);
		if (opinionLabel == "Close Friend")
		{
			reactions.Add(EMOTION.Despair);
		}
		else if (opinionLabel == "Friend")
		{
			reactions.Add(EMOTION.Anger);
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (!(target is Character))
		{
			return;
		}
		Character target2 = target as Character;
		if (!witness.traitContainer.HasTrait("Psychopath"))
		{
			switch (witness.relationshipContainer.GetOpinionLabel(target2))
			{
			case "Acquaintance":
			case "Friend":
			case "Close Friend":
				reactions.Add(EMOTION.Concern);
				break;
			}
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (!(target is Character))
		{
			return;
		}
		Character character = target as Character;
		if (character.traitContainer.HasTrait("Coward"))
		{
			reactions.Add(EMOTION.Fear);
			reactions.Add(EMOTION.Shock);
			return;
		}
		reactions.Add(EMOTION.Threatened);
		if (character.relationshipContainer.IsFriendsWith(actor) && !character.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Betrayal);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Murder;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Murder;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		if (node.poiTarget is Character poi)
		{
			actor.UncarryPOI(poi, bringBackToInventory: false, addToLocation: false);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor != poiTarget)
			{
				return actor.traitContainer.HasTrait("Psychopath");
			}
			return false;
		}
		return false;
	}

	private bool IsTargetInWildernessOrHome(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (target is Character && otherData != null)
		{
			Character character = target as Character;
			bool flag = false;
			if (otherData.Length == 1)
			{
				if (otherData[0] is AreaOtherData areaOtherData)
				{
					flag = target.gridTileLocation.area == areaOtherData.area;
				}
				else if (otherData[0] is LocationStructureOtherData locationStructureOtherData)
				{
					flag = character.currentStructure == locationStructureOtherData.locationStructure;
				}
				else if (otherData[0] is LocationGridTileOtherData locationGridTileOtherData)
				{
					flag = character.gridTileLocation == locationGridTileOtherData.tile;
				}
			}
			return character.carryComponent.IsNotBeingCarried() && character.traitContainer.HasTrait("Restrained") && flag;
		}
		return false;
	}

	private bool HasRestrained(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType)
	{
		return target.traitContainer.HasTrait("Restrained");
	}

	public void PreKillingSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character poi && goapNode.actor.carryComponent.IsPOICarried(poi))
		{
			goapNode.actor.carryComponent.UncarryPOI(poi);
		}
	}

	public void AfterKillingSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character { homeSettlement: var homeSettlement } character)
		{
			character.Death("normal", goapNode, goapNode.actor, deathSource: goapNode.actor, _deathLog: goapNode.descriptionLog);
			goapNode.actor.jobComponent.TriggerBuryPsychopathVictim(character, homeSettlement);
		}
	}
}
