using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;

public class CharacterMarkerVisionCollider : BaseVisionCollider
{
	public CharacterMarker parentMarker;

	private bool isApplicationQuitting;

	private void OnDisable()
	{
		if (!isApplicationQuitting && !(LevelLoaderManager.Instance == null) && !LevelLoaderManager.Instance.isLoadingNewScene && !(SchedulingManager.Instance == null))
		{
			if (parentMarker.inVisionPOIs != null)
			{
				parentMarker.ClearPOIsInVisionRange();
			}
			if (parentMarker.character?.combatComponent.hostilesInRange != null)
			{
				parentMarker.character.combatComponent.ClearHostilesInRange();
			}
			if (parentMarker.character?.combatComponent.avoidInRange != null)
			{
				parentMarker.character.combatComponent.ClearAvoidInRange();
			}
		}
	}

	private void OnApplicationQuit()
	{
		isApplicationQuitting = true;
	}

	public void Initialize()
	{
		VoteToFilterVision();
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
	}

	public override void Reset()
	{
		base.Reset();
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		OnDisable();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		parentMarker = null;
	}

	protected void OnTriggerEnter2D(Collider2D collision)
	{
		if ((bool)parentMarker && parentMarker.character != null && !parentMarker.character.hasBeenCleanedUp && parentMarker.character.carryComponent.IsNotBeingCarried() && CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision) is POIVisionTrigger collidedWith)
		{
			OnDetectPoiVisionTrigger(collidedWith);
		}
	}

	protected void OnTriggerExit2D(Collider2D collision)
	{
		if (!(SchedulingManager.Instance == null) && parentMarker.character != null && !parentMarker.character.hasBeenCleanedUp && CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision) is POIVisionTrigger collidedWith)
		{
			OnUndetectPoiVisionTrigger(collidedWith);
		}
	}

	public void OnDetectPoiVisionTrigger(POIVisionTrigger collidedWith)
	{
		if (!(collidedWith != null) || collidedWith.damageable == null || collidedWith.damageable == parentMarker.character || (collidedWith.damageable is Character character && !character.carryComponent.IsNotBeingCarried()) || (collidedWith.damageable.gridTileLocation == null && !(collidedWith.damageable is FeebleSpirit) && !(collidedWith.damageable is RavenousSpirit) && !(collidedWith.damageable is ForlornSpirit)))
		{
			return;
		}
		List<Trait> traitOverrideFunctions = collidedWith.poi.traitContainer.GetTraitOverrideFunctions("Collision_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].OnCollideWith(parentMarker.character, collidedWith.poi);
			}
		}
		TryAddPOIToVision(collidedWith.poi, collidedWith);
	}

	public void OnUndetectPoiVisionTrigger(POIVisionTrigger collidedWith)
	{
		if (collidedWith != null && collidedWith.poi != null && collidedWith.poi != parentMarker.character && !parentMarker.character.hasBeenCleanedUp)
		{
			parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
			parentMarker.RemovePOIAsInRangeButDifferentStructure(collidedWith.poi);
		}
	}

	private void NormalEnterHandling(IPointOfInterest poi)
	{
		parentMarker.AddPOIAsInVisionRange(poi);
	}

	public void TransferAllDifferentStructureCharacters()
	{
		for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++)
		{
			IPointOfInterest pointOfInterest = parentMarker.inVisionPOIsButDiffStructure[i];
			if (pointOfInterest.gridTileLocation != null && TryAddPOIToVision(pointOfInterest, pointOfInterest.mapObjectVisual?.visionTrigger as POIVisionTrigger) && parentMarker.RemovePOIAsInRangeButDifferentStructure(pointOfInterest))
			{
				i--;
			}
		}
	}

	public void ReCategorizeVision()
	{
		for (int i = 0; i < parentMarker.inVisionPOIs.Count; i++)
		{
			IPointOfInterest pointOfInterest = parentMarker.inVisionPOIs[i];
			if (pointOfInterest.gridTileLocation != null && !TryAddPOIToVision(pointOfInterest, pointOfInterest.mapObjectVisual?.visionTrigger as POIVisionTrigger) && parentMarker.RemovePOIFromInVisionRange(pointOfInterest))
			{
				i--;
			}
		}
	}

	private bool TryAddPOIToVision(IPointOfInterest poi, POIVisionTrigger p_visionTrigger)
	{
		LocationStructure locationStructure = parentMarker.character.gridTileLocation?.structure;
		LocationStructure locationStructure2 = poi.gridTileLocation?.structure;
		if ((bool)parentMarker && locationStructure != null && locationStructure2 != null && locationStructure.region == locationStructure2.region)
		{
			if ((ShouldBeConsideredInVision(poi) || (p_visionTrigger != null && p_visionTrigger.IgnoresStructureDifference())) && (ShouldAddToVisionBasedOnRoom(poi) || (p_visionTrigger != null && p_visionTrigger.IgnoresRoomDifference())))
			{
				NormalEnterHandling(poi);
				return true;
			}
			parentMarker.AddPOIAsInRangeButDifferentStructure(poi);
			return false;
		}
		return false;
	}

	private bool ShouldAddToVisionBasedOnRoom(IPointOfInterest seenObject)
	{
		bool result = true;
		if (seenObject.gridTileLocation != null && seenObject.gridTileLocation.structure.IsTilePartOfARoom(seenObject.gridTileLocation, out var room))
		{
			result = parentMarker.character.gridTileLocation.structure.IsTilePartOfARoom(parentMarker.character.gridTileLocation, out var room2) && room2 == room;
		}
		else if (parentMarker.character.gridTileLocation != null && parentMarker.character.gridTileLocation.structure.IsTilePartOfARoom(parentMarker.character.gridTileLocation, out room))
		{
			result = false;
		}
		return result;
	}

	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (parentMarker.character == null || parentMarker.character.hasBeenCleanedUp)
		{
			return;
		}
		if (parentMarker.inVisionPOIsButDiffStructure.Contains(character) && ShouldBeConsideredInVision(structure, character))
		{
			if (ShouldAddToVisionBasedOnRoom(character))
			{
				NormalEnterHandling(character);
				parentMarker.RemovePOIAsInRangeButDifferentStructure(character);
			}
		}
		else if (parentMarker.IsPOIInVision(character) && !ShouldBeConsideredInVision(structure, character))
		{
			parentMarker.RemovePOIFromInVisionRange(character);
			parentMarker.AddPOIAsInRangeButDifferentStructure(character);
		}
		else
		{
			if (!(character.persistentID == parentMarker.character.persistentID))
			{
				return;
			}
			for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++)
			{
				IPointOfInterest pointOfInterest = parentMarker.inVisionPOIsButDiffStructure[i];
				if (pointOfInterest.gridTileLocation == null || pointOfInterest.gridTileLocation.structure == null)
				{
					if (parentMarker.RemovePOIAsInRangeButDifferentStructure(pointOfInterest))
					{
						i--;
					}
				}
				else if (ShouldBeConsideredInVision(pointOfInterest) && ShouldAddToVisionBasedOnRoom(pointOfInterest))
				{
					NormalEnterHandling(pointOfInterest);
					if (parentMarker.RemovePOIAsInRangeButDifferentStructure(pointOfInterest))
					{
						i--;
					}
				}
			}
			for (int j = 0; j < parentMarker.inVisionPOIs.Count; j++)
			{
				IPointOfInterest pointOfInterest2 = parentMarker.inVisionPOIs[j];
				if (pointOfInterest2.gridTileLocation == null || pointOfInterest2.gridTileLocation.structure == null)
				{
					if (parentMarker.RemovePOIFromInVisionRange(pointOfInterest2))
					{
						j--;
					}
				}
				else if (!ShouldBeConsideredInVision(pointOfInterest2))
				{
					if (parentMarker.RemovePOIFromInVisionRange(pointOfInterest2))
					{
						j--;
					}
					parentMarker.AddPOIAsInRangeButDifferentStructure(pointOfInterest2);
				}
			}
			if (character.currentActionNode != null && !character.currentActionNode.hasBeenReset && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL && structure.structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				bool flag = false;
				if (character.currentActionNode.targetStructure == structure)
				{
					flag = true;
				}
				else if (character.currentActionNode.targetStructure is DemonicStructure demonicStructure)
				{
					flag = demonicStructure.CanSeeObjectLocatedHere(character);
				}
				if (flag)
				{
					parentMarker.pathfindingAI.ClearAllCurrentPathData();
					character.PerformGoapAction();
				}
			}
		}
	}

	private bool ShouldBeConsideredInVision(IPointOfInterest target)
	{
		LocationStructure targetStructure = target?.gridTileLocation?.structure;
		return ShouldBeConsideredInVision(targetStructure, target);
	}

	private bool ShouldBeConsideredInVision(LocationStructure targetStructure, IPointOfInterest target)
	{
		LocationStructure locationStructure = parentMarker.character?.currentStructure;
		if (locationStructure != null && targetStructure != null)
		{
			if (!IsTheSameStructureOrSameOpenSpace(locationStructure, targetStructure))
			{
				return parentMarker.IsCharacterInLineOfSightWith(target);
			}
			return true;
		}
		return false;
	}

	public bool IsTheSameStructureOrSameOpenSpaceWithPOI(IPointOfInterest poi)
	{
		LocationStructure structure = parentMarker.character?.currentStructure;
		LocationStructure structure2 = poi?.gridTileLocation?.structure;
		return IsTheSameStructureOrSameOpenSpace(structure, structure2);
	}

	private bool IsTheSameStructureOrSameOpenSpace(LocationStructure structure1, LocationStructure structure2)
	{
		if (structure1 != null && structure2 != null)
		{
			if (structure1 != structure2)
			{
				if (structure1.structureType.IsOpenSpace())
				{
					return structure2.structureType.IsOpenSpace();
				}
				return false;
			}
			return true;
		}
		return false;
	}

	[ContextMenu("Log Diff Struct")]
	public void LogCharactersInDifferentStructures()
	{
	}

	public void OnDeath()
	{
		parentMarker.inVisionPOIsButDiffStructure.Clear();
		OnDisable();
	}
}
