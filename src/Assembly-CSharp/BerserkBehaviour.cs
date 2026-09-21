using UtilityScripts;

public class BerserkBehaviour : CharacterBehaviour
{
	public BerserkBehaviour()
	{
		base.priority = 1085;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!character.combatComponent.isInCombat)
		{
			bool flag = false;
			for (int i = 0; i < character.marker.inVisionPOIs.Count; i++)
			{
				IPointOfInterest pointOfInterest = character.marker.inVisionPOIs[i];
				if (pointOfInterest is MovingTileObject || pointOfInterest is GenericTileObject || pointOfInterest is StructureTileObject || (pointOfInterest.mapObjectVisual != null && pointOfInterest.mapObjectVisual.IsInvisibleToPlayer()) || pointOfInterest.gridTileLocation == null || pointOfInterest.traitContainer.HasTrait("Hibernating", "Indestructible") || !PathfindingManager.Instance.HasPathEvenDiffRegion(character.gridTileLocation, pointOfInterest.gridTileLocation) || character.combatComponent.IsHostileInRange(pointOfInterest) || character.combatComponent.IsAvoidInRange(pointOfInterest))
				{
					continue;
				}
				if (pointOfInterest is Character character2)
				{
					if (!character2.isDead && !character.combatComponent.bannedFromHostileList.Contains(character2) && !character2.traitContainer.HasTrait("Unconscious", "Paralyzed", "Restrained"))
					{
						producedJob = CreateBerserkAttackJob(character, character2);
						if (producedJob != null)
						{
							flag = true;
							break;
						}
					}
				}
				else if (pointOfInterest is TileObject { mapObjectState: MAP_OBJECT_STATE.BUILT } tileObject && GameUtilities.RollChance(35))
				{
					producedJob = CreateBerserkAttackJob(character, tileObject);
					if (producedJob != null)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				character.jobComponent.PlanIdleBerserkStrollOutside(out producedJob);
			}
		}
		return true;
	}

	private GoapPlanJob CreateBerserkAttackJob(Character character, IPointOfInterest targetPOI)
	{
		if (!character.jobQueue.HasJob(JOB_TYPE.BERSERK_ATTACK, targetPOI))
		{
			return JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BERSERK_ATTACK, INTERACTION_TYPE.ASSAULT, targetPOI, character);
		}
		return null;
	}
}
