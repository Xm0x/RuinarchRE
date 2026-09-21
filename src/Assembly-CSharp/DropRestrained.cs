using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class DropRestrained : GoapAction
{
	public DropRestrained()
		: base(INTERACTION_TYPE.DROP_RESTRAINED)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Carry Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory);
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
			if (otherData.Length == 1 && otherData[0].obj is LocationStructure)
			{
				return otherData[0].obj as LocationStructure;
			}
			if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
			{
				return otherData[0].obj as LocationStructure;
			}
		}
		return base.GetTargetStructure(node);
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile)
		{
			return otherData[1].obj as LocationGridTile;
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
			if (goapNode.associatedJobType == JOB_TYPE.TRITON_KIDNAP)
			{
				if (!character.isDead)
				{
					character.SetDestroyMarkerOnDeath(state: true);
					character.Death("drowned", null, goapNode.actor, null, null, null, null, isPlayerSource: false, goapNode.actor);
				}
			}
			else if (goapNode.associatedJobType.IsApprehendTypeJob() && currentSettlement != null && currentSettlement is NPCSettlement nPCSettlement && character.currentStructure == nPCSettlement.prison)
			{
				if (character.traitContainer.HasTrait("Criminal"))
				{
					character.traitContainer.GetTraitOrStatus<Criminal>("Criminal").SetIsImprisoned(state: true);
				}
				character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND);
				character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND_RESTRAINED);
			}
			if (goapNode.associatedJobType == JOB_TYPE.SNATCH)
			{
				LocationStructure targetStructure = GetTargetStructure(goapNode);
				if (goapNode.actor.deployedAtStructure != null && !goapNode.actor.deployedAtStructure.hasBeenDestroyed && targetStructure == goapNode.actor.deployedAtStructure)
				{
					LocationGridTile locationGridTile = ((targetStructure.rooms == null || targetStructure.rooms.Length == 0) ? goapNode.actor.deployedAtStructure.GetRandomPassableTile() : CollectionUtilities.GetRandomElement(targetStructure.rooms.First().tilesInRoom));
					if (locationGridTile != null)
					{
						CharacterManager.Instance.Teleport(character, locationGridTile);
						if (character.hasMarker)
						{
							character.marker.UpdatePosition();
						}
						if (locationGridTile.structure is Kennel kennel)
						{
							kennel.OnSnatchedCharacterDroppedHere(character);
						}
						else if (locationGridTile.structure is TortureChambers tortureChambers)
						{
							tortureChambers.OnSnatchedCharacterDroppedHere(character);
						}
					}
				}
				Party currentParty = goapNode.actor.partyComponent.currentParty;
				if (currentParty != null && currentParty.isActive && currentParty.currentQuest is DemonSnatchPartyQuest demonSnatchPartyQuest && demonSnatchPartyQuest.targetCharacter == goapNode.poiTarget)
				{
					currentParty.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
				}
			}
			else if (goapNode.associatedJobType == JOB_TYPE.CAPTURE_CHARACTER && goapNode.actor is Harpy && goapNode.poiTarget.gridTileLocation != null)
			{
				LocationStructure structure = goapNode.poiTarget.gridTileLocation.structure;
				if (goapNode.poiTarget is Character character2)
				{
					if (structure is Kennel kennel2 && kennel2.rooms.ElementAtOrDefault(0) is KennelCell kennelCell)
					{
						if (!kennel2.IsTilePartOfARoom(goapNode.poiTarget.gridTileLocation, out var _))
						{
							LocationGridTile locationGridTile2 = kennelCell.tilesInRoom.FirstOrDefault((LocationGridTile t) => t.charactersHere.Count <= 0) ?? CollectionUtilities.GetRandomElement(kennelCell.tilesInRoom);
							if (locationGridTile2 != null)
							{
								CharacterManager.Instance.Teleport(character2, locationGridTile2);
								GameManager.Instance.CreateParticleEffectAt(locationGridTile2, PARTICLE_EFFECT.Minion_Dissipate);
							}
						}
						kennelCell.OnHarpyDroppedCharacterHere(character2);
					}
					else if (structure is TortureChambers tortureChambers2 && tortureChambers2.rooms.ElementAtOrDefault(0) is PrisonCell prisonCell)
					{
						if (!tortureChambers2.IsTilePartOfARoom(goapNode.poiTarget.gridTileLocation, out var _))
						{
							LocationGridTile locationGridTile3 = prisonCell.tilesInRoom.FirstOrDefault((LocationGridTile t) => t.charactersHere.Count <= 0) ?? CollectionUtilities.GetRandomElement(prisonCell.tilesInRoom);
							if (locationGridTile3 != null)
							{
								CharacterManager.Instance.Teleport(character2, locationGridTile3);
								GameManager.Instance.CreateParticleEffectAt(locationGridTile3, PARTICLE_EFFECT.Minion_Dissipate);
							}
						}
						prisonCell.OnHarpyDroppedCharacterHere(character2);
					}
				}
			}
		}
		if ((goapNode.associatedJobType == JOB_TYPE.KIDNAP_RAID || goapNode.associatedJobType == JOB_TYPE.FACTION_KIDNAP) && goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.isActive)
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
		if (goapNode.associatedJobType.IsJobAbduction() && goapNode.actor.faction != goapNode.target.factionOwner && goapNode.target.factionOwner != null && goapNode.target.factionOwner.isMajorNonPlayerOrBandits)
		{
			InteractionManager.Instance.CreateNewIllusionAction(goapNode.actor, goapNode.target, INTERACTION_TYPE.ABDUCT, shouldLog: true, executeAfterEffect: true);
		}
		if (goapNode.associatedJobType == JOB_TYPE.SNATCH)
		{
			Party currentParty2 = goapNode.actor.partyComponent.currentParty;
			if (currentParty2 != null && currentParty2.isActive && currentParty2.currentQuest is DemonSnatchPartyQuest demonSnatchPartyQuest2 && demonSnatchPartyQuest2.targetCharacter == goapNode.poiTarget)
			{
				currentParty2.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
			}
		}
	}
}
