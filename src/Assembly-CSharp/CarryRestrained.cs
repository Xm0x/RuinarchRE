using Inner_Maps.Location_Structures;
using Traits;

public class CarryRestrained : GoapAction
{
	private readonly Precondition _carryPrecondition;

	public CarryRestrained()
		: base(INTERACTION_TYPE.CARRY_RESTRAINED)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		_carryPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), TargetIsRestrainedOrDead);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (!target.traitContainer.HasTrait("Hibernating"))
		{
			Precondition carryPrecondition = _carryPrecondition;
			isOverridden = true;
			return carryPrecondition;
		}
		return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Carry Success", goapNode);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = TargetMissingForCarry(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unreachable";
		if (!invalidity.isInvalid && poiTarget is Character && !(poiTarget as Character).carryComponent.IsNotBeingCarried())
		{
			invalidity.isInvalid = true;
		}
		if (!invalidity.isInvalid && node.associatedJobType == JOB_TYPE.MONSTER_ABDUCT && node.poiTarget is Character { isDead: not false })
		{
			invalidity.isInvalid = true;
			invalidity.reason = "target_dead";
		}
		if (!invalidity.isInvalid && node.associatedJob is GoapPlanJob goapPlanJob && goapPlanJob.jobType.IsApprehendTypeJob())
		{
			OtherData[] otherDataSpecific = goapPlanJob.GetOtherDataSpecific(INTERACTION_TYPE.DROP_RESTRAINED);
			if (otherDataSpecific != null && otherDataSpecific.Length == 1 && otherDataSpecific[0].obj is LocationStructure locationStructure && poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure == locationStructure)
			{
				invalidity.isInvalid = true;
				invalidity.reason = "at_location";
			}
		}
		return invalidity;
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && actor.gridTileLocation != null && poiTarget.gridTileLocation != null)
		{
			if (poiTarget is Character character)
			{
				if (actor != poiTarget && (bool)poiTarget.mapObjectVisual && poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 0)
				{
					return character.carryComponent.IsNotBeingCarried();
				}
				return false;
			}
			if (actor != poiTarget && (bool)poiTarget.mapObjectVisual)
			{
				return poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 0;
			}
			return false;
		}
		return false;
	}

	public void AfterCarrySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.CarryPOI(goapNode.poiTarget, changeOwnership: false, setOwnership: false);
		if (goapNode.associatedJobType == JOB_TYPE.FACTION_KIDNAP)
		{
			Prisoner traitOrStatus = goapNode.poiTarget.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus != null && !traitOrStatus.IsFactionPrisonerOf(goapNode.actor.faction))
			{
				goapNode.poiTarget.traitContainer.RestrainAndImprison(goapNode.poiTarget, goapNode.actor, goapNode.actor.faction);
			}
		}
	}

	private bool TargetIsRestrainedOrDead(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType)
	{
		if (target is Character)
		{
			if (!target.traitContainer.HasTrait("Restrained"))
			{
				return target.isDead;
			}
			return true;
		}
		return true;
	}

	private bool TargetMissingForCarry(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion || !poiTarget.mapObjectVisual)
		{
			return true;
		}
		if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true))
		{
			if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
