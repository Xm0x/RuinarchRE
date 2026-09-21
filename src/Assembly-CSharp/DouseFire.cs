using System.Collections.Generic;

public class DouseFire : GoapAction
{
	private Precondition _waterPrecondition;

	public DouseFire()
		: base(INTERACTION_TYPE.DOUSE_FIRE)
	{
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Douse_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		_waterPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Water Flask", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burning", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (!actor.traitContainer.HasTrait("Fire Master"))
		{
			Precondition waterPrecondition = _waterPrecondition;
			isOverridden = true;
			return waterPrecondition;
		}
		return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Douse Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissing(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		if (!invalidity.isInvalid && !poiTarget.traitContainer.HasTrait("Burning"))
		{
			invalidity.isInvalid = true;
			invalidity.reason = "not_burning";
		}
		return invalidity;
	}

	private bool IsTargetMissing(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget.gridTileLocation == null)
		{
			return true;
		}
		if (actor.currentRegion != poiTarget.gridTileLocation.structure.region)
		{
			return true;
		}
		if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET)
		{
			if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true))
			{
				if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
				{
					return false;
				}
				return true;
			}
		}
		else if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET && actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
		{
			if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool ShouldAddLogs(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character)
		{
			return true;
		}
		return base.ShouldAddLogs(goapNode);
	}

	public void AfterDouseSuccess(ActualGoapNode goapNode)
	{
		TileObject tileObject = null;
		if (!goapNode.actor.traitContainer.HasTrait("Fire Master"))
		{
			tileObject = goapNode.actor.GetItem(TILE_OBJECT_TYPE.WATER_FLASK);
		}
		if (tileObject != null && tileObject.traitContainer.HasTrait("Poisoned"))
		{
			tileObject.traitContainer.AddTrait(tileObject, "Burning", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Fire);
		}
		else
		{
			if (goapNode.poiTarget is Cinder cinder)
			{
				cinder.OnHitByWater();
			}
			if (goapNode.poiTarget.gridTileLocation != null)
			{
				goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Burning", goapNode.actor);
			}
			if (goapNode.poiTarget.gridTileLocation != null)
			{
				goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Wet", goapNode.actor, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
			}
		}
		if (tileObject != null)
		{
			goapNode.actor.UnobtainItem(tileObject);
		}
	}

	private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItem(TILE_OBJECT_TYPE.WATER_FLASK);
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (target is Character)
		{
			reactions.Add(EMOTION.Gratefulness);
		}
	}
}
