using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class Steal : GoapAction
{
	public Steal()
		: base(INTERACTION_TYPE.STEAL)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
		AddBaseExpectedEffectsToList(list);
		TileObject tileObject = target as TileObject;
		list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		if (actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		}
		isOverridden = true;
		return list;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Steal Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		int num = Utilities.Rng.Next(300, 351);
		if (actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			num = Utilities.Rng.Next(90, 151);
		}
		else
		{
			TileObject tileObject = null;
			if (target is TileObject tileObject2)
			{
				tileObject = tileObject2;
			}
			if (tileObject?.characterOwner != null)
			{
				string opinionLabel = actor.relationshipContainer.GetOpinionLabel(tileObject.characterOwner);
				if (actor.moodComponent.moodState != MOOD_STATE.Normal)
				{
					switch (opinionLabel)
					{
					case "Acquaintance":
					case "Friend":
					case "Close Friend":
						break;
					default:
						goto IL_00db;
					}
				}
				num += 2000;
			}
		}
		goto IL_0124;
		IL_0124:
		return num;
		IL_00db:
		if (actor.moodComponent.moodState == MOOD_STATE.Bad)
		{
			num += Utilities.Rng.Next(500, 601);
		}
		else if (actor.moodComponent.moodState == MOOD_STATE.Critical)
		{
			num += Utilities.Rng.Next(120, 201);
		}
		goto IL_0124;
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is TileObject)
		{
			TileObject tileObject = goapNode.poiTarget as TileObject;
			if (tileObject.isBeingCarriedBy != null)
			{
				return tileObject.isBeingCarriedBy;
			}
		}
		return base.GetTargetToGoTo(goapNode);
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		TileObject tileObject = node.poiTarget as TileObject;
		if (tileObject.isBeingCarriedBy != null)
		{
			return tileObject.isBeingCarriedBy.currentStructure;
		}
		return base.GetTargetStructure(node);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissingOverride(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unreachable";
		return invalidity;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!witness.traitContainer.HasTrait("Demon Cultist"))
		{
			reactions.Add(EMOTION.Disapproval);
			if (witness.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Disappointment);
				reactions.Add(EMOTION.Shock);
			}
		}
		else if (witness == target || (target is TileObject tileObject && tileObject.IsOwnedBy(witness)))
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
		return CRIME_TYPE.Theft;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Theft;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			TileObject tileObject = poiTarget as TileObject;
			if (tileObject.characterOwner != null)
			{
				if (tileObject.gridTileLocation != null)
				{
					return !tileObject.IsOwnedBy(actor);
				}
				if (tileObject.isBeingCarriedBy != null)
				{
					return !tileObject.IsOwnedBy(actor);
				}
				return false;
			}
		}
		return false;
	}

	public void AfterStealSuccess(ActualGoapNode goapNode)
	{
		TileObject tileObject = goapNode.poiTarget as TileObject;
		goapNode.actor.PickUpItem(tileObject);
		tileObject.SetLastStolenBy(goapNode.actor);
		if (goapNode.actor.traitContainer.HasTrait("Kleptomaniac"))
		{
			goapNode.actor.needsComponent.AdjustHappiness(10f);
		}
	}

	private bool IsTargetMissingOverride(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		bool num = poiTarget.IsAvailable();
		LocationGridTile locationGridTile = poiTarget.gridTileLocation;
		if (locationGridTile == null)
		{
			locationGridTile = poiTarget.isBeingCarriedBy?.gridTileLocation;
		}
		if ((!num && !base.canBeAdvertisedEvenIfTargetIsUnavailable) || (locationGridTile == null && !base.canBePerformedEvenIfTargetHasNoTileLocation))
		{
			return true;
		}
		if (actor.gridTileLocation != locationGridTile && !actor.gridTileLocation.IsNeighbour(locationGridTile, sameStructureOnly: true))
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
