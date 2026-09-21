using Inner_Maps;
using Traits;
using UnityEngine;

public class CleanseTileBehaviour : CharacterBehaviour
{
	public CleanseTileBehaviour()
	{
		base.priority = 630;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (StillHasPoisonedTile(character))
		{
			producedJob = CleanseNearestTile(character);
			if (producedJob == null)
			{
				character.traitContainer.RemoveTrait(character, "Cleansing");
			}
		}
		else
		{
			producedJob = null;
			character.traitContainer.RemoveTrait(character, "Cleansing");
		}
		return true;
	}

	private bool StillHasPoisonedTile(Character character)
	{
		return character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles.Count > 0;
	}

	private JobQueueItem CleanseNearestTile(Character character)
	{
		LocationGridTile locationGridTile = null;
		float num = 99999f;
		for (int i = 0; i < character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles.Count; i++)
		{
			LocationGridTile locationGridTile2 = character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles[i];
			Poisoned traitOrStatus = locationGridTile2.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
			if (traitOrStatus != null)
			{
				if (traitOrStatus.cleanser == null)
				{
					float num2 = Vector2.Distance(character.worldObject.transform.position, locationGridTile2.worldLocation);
					if (num2 < num)
					{
						locationGridTile = locationGridTile2;
						num = num2;
					}
				}
			}
			else
			{
				character.behaviourComponent.cleansingTilesForSettlement.settlementJobTriggerComponent.poisonedTiles.RemoveAt(i);
				i--;
			}
		}
		if (locationGridTile != null)
		{
			locationGridTile.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned").SetCleanser(character);
			return JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLEANSE_TILES, INTERACTION_TYPE.CLEANSE_TILE, locationGridTile.tileObjectComponent.genericTileObject, character);
		}
		return null;
	}
}
