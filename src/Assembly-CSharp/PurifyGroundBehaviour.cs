using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class PurifyGroundBehaviour : CharacterBehaviour
{
	public PurifyGroundBehaviour()
	{
		base.priority = 630;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (CanStillPurifyGround(character))
		{
			producedJob = PurifyNearestTile(character);
			if (producedJob == null)
			{
				character.behaviourComponent.RemoveBehaviourComponent(typeof(PurifyGroundBehaviour));
			}
		}
		else
		{
			producedJob = null;
			character.behaviourComponent.RemoveBehaviourComponent(typeof(PurifyGroundBehaviour));
		}
		return true;
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.ResetNumberOfPurifiedGrounds();
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.behaviourComponent.SetSettlementTargetForPurifyingGround(null);
	}

	private bool CanStillPurifyGround(Character p_character)
	{
		return p_character.behaviourComponent.numberOfPurifiedGrounds < 20;
	}

	private bool StillHasCorruptedTile(Character character)
	{
		return false;
	}

	private JobQueueItem PurifyNearestTile(Character character)
	{
		LocationGridTile nearestCorruptedTile = GetNearestCorruptedTile(character);
		if (nearestCorruptedTile != null)
		{
			character.PlanFixedJob(JOB_TYPE.PURIFY_GROUND, INTERACTION_TYPE.PURIFY_GROUND, nearestCorruptedTile.tileObjectComponent.genericTileObject, out var producedJob);
			return producedJob;
		}
		return null;
	}

	private LocationGridTile GetNearestCorruptedTile(Character p_character)
	{
		LocationGridTile gridTileLocation = p_character.gridTileLocation;
		if (gridTileLocation != null)
		{
			for (int i = 0; i < gridTileLocation.neighbourList.Count; i++)
			{
				LocationGridTile locationGridTile = gridTileLocation.neighbourList[i];
				if (!locationGridTile.HasNeighbourStructure(STRUCTURE_TYPE.THE_PORTAL) && locationGridTile.structure.structureType != STRUCTURE_TYPE.THE_PORTAL && locationGridTile.IsPassable() && p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile) && locationGridTile.corruptionComponent.isCorrupted)
				{
					return locationGridTile;
				}
			}
		}
		LocationGridTile locationGridTile2 = null;
		float num = 0f;
		locationGridTile2 = (DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(p_character.behaviourComponent.settlementTargetForPurifyGround) as NPCSettlement).settlementJobTriggerComponent.GetFirstTileToPurify(p_character);
		if (locationGridTile2 != null)
		{
			return locationGridTile2;
		}
		List<LocationGridTile> corruptedTiles = PlayerManager.Instance.player.playerSettlement.corruptedTiles;
		for (int j = 0; j < corruptedTiles.Count; j++)
		{
			LocationGridTile locationGridTile3 = corruptedTiles[j];
			if (locationGridTile3.corruptionComponent.isCorrupted && !locationGridTile3.HasNeighbourStructure(STRUCTURE_TYPE.THE_PORTAL) && locationGridTile3.structure.structureType != STRUCTURE_TYPE.THE_PORTAL && locationGridTile3.IsPassable() && p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile3))
			{
				float num2 = Vector2.Distance(p_character.worldObject.transform.position, locationGridTile3.worldLocation);
				if (locationGridTile2 == null || num2 < num)
				{
					locationGridTile2 = locationGridTile3;
					num = num2;
				}
			}
		}
		if (locationGridTile2 != null)
		{
			return locationGridTile2;
		}
		return null;
	}
}
