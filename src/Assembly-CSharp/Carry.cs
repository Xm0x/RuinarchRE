using Traits;

public class Carry : GoapAction
{
	private Precondition _carryPrecondition;

	public Carry()
		: base(INTERACTION_TYPE.CARRY)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		_carryPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), TargetCannotMove);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (jobType != JOB_TYPE.MOVE_CHARACTER)
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
			invalidity.reason = "target_carried";
		}
		if (!invalidity.isInvalid && node.associatedJobType == JOB_TYPE.MONSTER_ABDUCT && node.poiTarget is Character { isDead: not false })
		{
			invalidity.isInvalid = true;
			invalidity.reason = "target_dead";
		}
		return invalidity;
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (job.jobType == JOB_TYPE.MOVE_CHARACTER && target is Character character && character.limiterComponent.canMove)
		{
			return 2000;
		}
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (job.jobType == JOB_TYPE.MOVE_CHARACTER && poiTarget is Character character && character.limiterComponent.canMove)
			{
				return false;
			}
			if (actor.gridTileLocation != null && poiTarget.gridTileLocation != null)
			{
				if (poiTarget is Character character2)
				{
					if (actor != poiTarget && (bool)poiTarget.mapObjectVisual && poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 0)
					{
						return character2.carryComponent.IsNotBeingCarried();
					}
					return false;
				}
				if (actor != poiTarget && (bool)poiTarget.mapObjectVisual)
				{
					return poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 0;
				}
				return false;
			}
		}
		return false;
	}

	public void AfterCarrySuccess(ActualGoapNode goapNode)
	{
		bool setOwnership = true;
		if (goapNode.associatedJobType == JOB_TYPE.HAUL || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT || goapNode.associatedJobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD)
		{
			setOwnership = false;
		}
		if (goapNode.associatedJobType == JOB_TYPE.SNATCH)
		{
			goapNode.poiTarget.traitContainer.RestrainAndImprison(goapNode.poiTarget, goapNode.actor, goapNode.actor.faction);
		}
		else if (goapNode.associatedJobType == JOB_TYPE.FACTION_KIDNAP)
		{
			Prisoner traitOrStatus = goapNode.poiTarget.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus != null && !traitOrStatus.IsFactionPrisonerOf(goapNode.actor.faction))
			{
				goapNode.poiTarget.traitContainer.RestrainAndImprison(goapNode.poiTarget, goapNode.actor, goapNode.actor.faction);
			}
		}
		goapNode.actor.CarryPOI(goapNode.poiTarget, changeOwnership: false, setOwnership);
		if (goapNode.associatedJobType == JOB_TYPE.DEMON_STEAL && goapNode.poiTarget is TileObject tileObject)
		{
			tileObject.SetCharacterOwner(null);
		}
	}

	private bool TargetCannotMove(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType)
	{
		if (target is Character)
		{
			return !(target as Character).limiterComponent.canMove;
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
