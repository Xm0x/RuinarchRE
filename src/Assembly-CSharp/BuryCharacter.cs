using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BuryCharacter : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public BuryCharacter()
		: base(INTERACTION_TYPE.BURY_CHARACTER)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
		base.actionIconString = GoapActionStateDB.Bury_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		Character actor = node.actor;
		OtherData[] otherData = node.otherData;
		if (node.associatedJobType == JOB_TYPE.BURY_IN_ACTIVE_PARTY)
		{
			return actor.currentStructure;
		}
		if (otherData != null && otherData.Length >= 1 && otherData[0].obj is LocationStructure)
		{
			return otherData[0].obj as LocationStructure;
		}
		return actor.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) ?? actor.currentRegion.wilderness;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJobType == JOB_TYPE.BURY_IN_ACTIVE_PARTY)
		{
			Character actor = goapNode.actor;
			if (actor.limiterComponent.canMove && !actor.movementComponent.isStationary)
			{
				List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
				actor.gridTileLocation.PopulateTilesInRadius(list, 3, 0, includeCenterTile: false, includeTilesInDifferentStructure: false, includeImpassable: false);
				LocationGridTile result = actor.gridTileLocation;
				if (list.Count > 0)
				{
					result = list[Utilities.Rng.Next(0, list.Count)];
				}
				RuinarchListPool<LocationGridTile>.Release(list);
				return result;
			}
			return actor.gridTileLocation;
		}
		if (goapNode.otherData != null && goapNode.otherData.Length == 2 && goapNode.otherData[1].obj is LocationGridTile)
		{
			return goapNode.otherData[1].obj as LocationGridTile;
		}
		LocationStructure targetStructure = GetTargetStructure(goapNode);
		if (targetStructure.structureType == STRUCTURE_TYPE.WILDERNESS)
		{
			if (goapNode.actor.homeSettlement != null)
			{
				List<Area> list2 = RuinarchListPool<Area>.Claim();
				List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
				goapNode.actor.homeSettlement.PopulateSurroundingAreas(list2);
				CollectionUtilities.Shuffle(list2);
				for (int i = 0; i < list2.Count; i++)
				{
					Area area = list2[i];
					for (int j = 0; j < area.gridTileComponent.gridTiles.Count; j++)
					{
						LocationGridTile locationGridTile = area.gridTileComponent.gridTiles[j];
						if (!locationGridTile.isOccupied && locationGridTile.IsNextToSettlement(goapNode.actor.homeSettlement) && locationGridTile.structure is Wilderness)
						{
							list3.Add(locationGridTile);
						}
					}
				}
				if (list3.Count <= 0)
				{
					for (int k = 0; k < targetStructure.unoccupiedTiles.Count; k++)
					{
						LocationGridTile locationGridTile2 = targetStructure.unoccupiedTiles[k];
						if (locationGridTile2.IsNextToSettlement(goapNode.actor.homeSettlement))
						{
							list3.Add(locationGridTile2);
						}
					}
				}
				LocationGridTile result2 = null;
				if (list3.Count > 0)
				{
					result2 = CollectionUtilities.GetRandomElement(list3);
				}
				RuinarchListPool<Area>.Release(list2);
				RuinarchListPool<LocationGridTile>.Release(list3);
				return result2;
			}
			if (goapNode.poiTarget.gridTileLocation != null)
			{
				return goapNode.poiTarget.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(targetStructure.structureType);
			}
			if (goapNode.actor.gridTileLocation != null)
			{
				return goapNode.actor.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(targetStructure.structureType);
			}
		}
		else if (targetStructure.structureType == STRUCTURE_TYPE.CEMETERY)
		{
			List<LocationGridTile> list4 = RuinarchListPool<LocationGridTile>.Claim();
			for (int l = 0; l < targetStructure.unoccupiedTiles.Count; l++)
			{
				LocationGridTile locationGridTile3 = targetStructure.unoccupiedTiles[l];
				if (locationGridTile3.groundType != LocationGridTile.Ground_Type.Ruined_Stone)
				{
					list4.Add(locationGridTile3);
				}
			}
			if (list4.Count <= 0)
			{
				list4.AddRange(targetStructure.unoccupiedTiles);
			}
			LocationGridTile result3 = null;
			if (list4.Count > 0)
			{
				result3 = CollectionUtilities.GetRandomElement(list4);
			}
			RuinarchListPool<LocationGridTile>.Release(list4);
			return result3;
		}
		return null;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarried);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Bury Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
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

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = false;
		invalidity.stateName = stateName;
		invalidity.reason = string.Empty;
		return invalidity;
	}

	public void PreBurySuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterBurySuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		LocationGridTile locationGridTile = goapNode.actor.gridTileLocation;
		if (locationGridTile.isOccupied)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			goapNode.actor.gridTileLocation.PopulateUnoccupiedNeighboursThatIsSameStructureAs(list, goapNode.actor.currentStructure);
			if (list.Count > 0)
			{
				locationGridTile = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		bool flag = true;
		if (!character.race.IsSapient())
		{
			flag = false;
			if (locationGridTile != null && locationGridTile.structure is CultTemple)
			{
				flag = true;
			}
			else
			{
				List<int> list2 = RuinarchListPool<int>.Claim();
				character.relationshipContainer.PopulateAllRelatableIDWithRelationship(list2, RELATIONSHIP_TYPE.MASTER);
				for (int i = 0; i < list2.Count; i++)
				{
					Character characterByID = CharacterManager.Instance.GetCharacterByID(list2[i]);
					if (characterByID != null && characterByID.faction == actor.faction && !characterByID.isDead)
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag && locationGridTile != null)
		{
			Tombstone tombstone = new Tombstone();
			tombstone.SetCharacter(character);
			goapNode.actor.currentStructure.AddPOI(tombstone, locationGridTile);
			actor.UncarryPOI(character, bringBackToInventory: false, addToLocation: false);
			if (character.hasMarker)
			{
				character.DisableMarker();
			}
		}
		else
		{
			actor.UncarryPOI(character, bringBackToInventory: false, addToLocation: false);
			if (character.hasMarker)
			{
				character.DestroyMarker();
			}
		}
		character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.BURY);
	}

	private bool IsCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.IsPOICarriedOrInInventory(poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is Character character)
		{
			if (!character.isDead)
			{
				return false;
			}
			if (character.grave != null)
			{
				return false;
			}
			if (character.numOfNonSecretActionsBeingPerformedOnThis > 0)
			{
				return false;
			}
			if (character.marker == null)
			{
				return false;
			}
			if (otherData != null && otherData.Length >= 1 && otherData[0].obj is LocationStructure)
			{
				return true;
			}
			return actor.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) != null;
		}
		return false;
	}
}
