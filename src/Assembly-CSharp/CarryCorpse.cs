public class CarryCorpse : GoapAction
{
	public CarryCorpse()
		: base(INTERACTION_TYPE.CARRY_CORPSE)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), TargetIsDead);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Corpse", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Carry Success", goapNode);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = TargetMissingForCarry(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unreachable";
		if (!invalidity.isInvalid)
		{
			if (poiTarget.isBeingCarriedBy != null && poiTarget.isBeingCarriedBy != actor)
			{
				invalidity.isInvalid = true;
				invalidity.reason = "target_carried";
			}
			else if (poiTarget.numOfNonSecretActionsBeingPerformedOnThis > 0)
			{
				invalidity.isInvalid = true;
				invalidity.reason = "target_unavailable";
			}
			else if (poiTarget is Tombstone { character: not null } tombstone && tombstone.character.numOfNonSecretActionsBeingPerformedOnThis > 0)
			{
				invalidity.isInvalid = true;
				invalidity.reason = "target_unavailable";
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
			Character character = null;
			if (poiTarget is Character character2)
			{
				character = character2;
			}
			else if (poiTarget is Tombstone tombstone)
			{
				character = tombstone.character;
			}
			if (character != null && character.isDead)
			{
				if (actor != poiTarget && (bool)poiTarget.mapObjectVisual && poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 0)
				{
					return poiTarget.isBeingCarriedBy == null;
				}
				return false;
			}
		}
		return false;
	}

	public void AfterCarrySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.CarryPOI(goapNode.poiTarget, changeOwnership: false, setOwnership: false);
	}

	private bool TargetIsDead(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType)
	{
		if (target is Character character)
		{
			return character.isDead;
		}
		if (target is Tombstone tombstone)
		{
			return tombstone.character.isDead;
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
