using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class Drop : GoapAction
{
	public Drop()
		: base(INTERACTION_TYPE.DROP)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode actionNode)
	{
		base.Perform(actionNode);
		SetState("Drop Success", actionNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null)
		{
			if (otherData.Length == 1)
			{
				if (otherData[0].obj is LocationStructure result)
				{
					return result;
				}
				if (otherData[0].obj is BaseSettlement baseSettlement)
				{
					return baseSettlement.region.wilderness;
				}
			}
			else if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
			{
				return otherData[0].obj as LocationStructure;
			}
		}
		return base.GetTargetStructure(node);
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null)
		{
			if (otherData.Length == 1 && otherData[0].obj is BaseSettlement baseSettlement)
			{
				return baseSettlement.GetRandomPassableTileFromAreas(10);
			}
			if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
			{
				return otherData[1].obj as LocationGridTile;
			}
		}
		return null;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		Character poi = node.poiTarget as Character;
		actor.UncarryPOI(poi);
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		Character poi = node.poiTarget as Character;
		actor.UncarryPOI(poi);
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		Character actor = node.actor;
		Character poi = node.poiTarget as Character;
		actor.UncarryPOI(poi);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		_ = node.poiTarget;
		string stateName = "Target Missing";
		string p_targetMissingLog;
		bool isInvalid = IsDropTargetMissing(node) || IsTargetMissing(node, out p_targetMissingLog);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "unable_to_do";
		return invalidity;
	}

	private bool IsDropTargetMissing(ActualGoapNode node)
	{
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget.gridTileLocation == null && !node.actor.IsPOICarriedOrInInventory(poiTarget))
		{
			return true;
		}
		return false;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor == poiTarget)
			{
				return false;
			}
			if (otherData != null)
			{
				if (otherData.Length == 1 && otherData[0].obj is LocationStructure locationStructure)
				{
					return actor.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(locationStructure.passableTiles));
				}
				if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile toTile)
				{
					return actor.movementComponent.HasPathToEvenIfDiffRegion(toTile);
				}
			}
			return true;
		}
		return false;
	}

	private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.IsPOICarriedOrInInventory(poiTarget);
	}

	public void PreDropSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJobType == JOB_TYPE.TRITON_KIDNAP)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " triton_kidnap", base.logTags, goapNode);
			log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			goapNode.OverrideDescriptionLog(log);
		}
	}

	public void AfterDropSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		LocationGridTile dropLocation = null;
		if (otherData != null && otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
		{
			dropLocation = otherData[1].obj as LocationGridTile;
		}
		goapNode.actor.UncarryPOI(goapNode.poiTarget, bringBackToInventory: false, addToLocation: true, dropLocation);
		if (goapNode.poiTarget is Character character)
		{
			BaseSettlement currentSettlement = goapNode.actor.currentSettlement;
			if (goapNode.associatedJobType.IsApprehendTypeJob() && currentSettlement != null && currentSettlement is NPCSettlement nPCSettlement && character.currentStructure == nPCSettlement.prison)
			{
				if (character.traitContainer.HasTrait("Criminal"))
				{
					character.traitContainer.GetTraitOrStatus<Criminal>("Criminal").SetIsImprisoned(state: true);
				}
				character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND);
				character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND_RESTRAINED);
			}
		}
		if (goapNode.associatedJobType == JOB_TYPE.KIDNAP_RAID || goapNode.associatedJobType == JOB_TYPE.FACTION_KIDNAP)
		{
			if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.isActive)
			{
				if (goapNode.actor.partyComponent.currentParty.currentQuest is RaidPartyQuest raidPartyQuest)
				{
					raidPartyQuest.SetIsSuccessful(state: true);
					if (!raidPartyQuest.TryTriggerRetreat(PartyQuest.GetLocalizedEndQuestReason("Raid_Successful")))
					{
						goapNode.actor.partyComponent.currentParty.RemoveMemberThatJoinedQuest(goapNode.actor, broadcastSignal: true, shouldDropQuest: true, shouldGainRewards: true);
					}
				}
				else
				{
					goapNode.actor.partyComponent.currentParty.RemoveMemberThatJoinedQuest(goapNode.actor, broadcastSignal: true, shouldDropQuest: true, shouldGainRewards: true);
				}
			}
		}
		else if (goapNode.associatedJobType == JOB_TYPE.HAUL_ANIMAL_CORPSE && goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.isActive && goapNode.actor.partyComponent.currentParty.currentQuest is HuntBeastPartyQuest)
		{
			goapNode.actor.partyComponent.currentParty.RemoveMemberThatJoinedQuest(goapNode.actor, broadcastSignal: true, shouldDropQuest: true, shouldGainRewards: true);
		}
		if (goapNode.associatedJobType.IsJobAbduction() && goapNode.actor.faction != goapNode.target.factionOwner && goapNode.target.factionOwner != null && goapNode.target.factionOwner.isMajorNonPlayerOrBandits)
		{
			InteractionManager.Instance.CreateNewIllusionAction(goapNode.actor, goapNode.target, INTERACTION_TYPE.ABDUCT, shouldLog: true, executeAfterEffect: true);
		}
	}
}
