using Inner_Maps;

public class SummonBoneGolem : GoapAction
{
	public SummonBoneGolem()
		: base(INTERACTION_TYPE.SUMMON_BONE_GOLEM)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Major,
			LOG_TAG.Work
		};
		base.showNotification = true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Summon Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation != null)
			{
				return poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.CULT_TEMPLE;
			}
			return false;
		}
		return false;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			IPointOfInterest poiTarget = node.poiTarget;
			if (poiTarget.gridTileLocation == null || poiTarget.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CULT_TEMPLE)
			{
				goapActionInvalidity.isInvalid = true;
			}
			else
			{
				OtherData[] otherData = node.otherData;
				if (otherData == null || otherData.Length != 3)
				{
					goapActionInvalidity.isInvalid = true;
				}
				else
				{
					for (int i = 0; i < otherData.Length; i++)
					{
						if (otherData[i].obj == null)
						{
							goapActionInvalidity.isInvalid = true;
							break;
						}
						if (!(otherData[i].obj is Character character))
						{
							goapActionInvalidity.isInvalid = true;
							break;
						}
						if (!character.isDead)
						{
							goapActionInvalidity.isInvalid = true;
							break;
						}
						IPointOfInterest pointOfInterest = character;
						if (character.grave != null)
						{
							pointOfInterest = character.grave;
						}
						if (pointOfInterest.gridTileLocation == null || pointOfInterest.mapObjectVisual == null || pointOfInterest.isBeingCarriedBy != null || pointOfInterest.isBeingSeized || pointOfInterest.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CULT_TEMPLE)
						{
							goapActionInvalidity.isInvalid = true;
							break;
						}
					}
				}
			}
		}
		return goapActionInvalidity;
	}

	public void AfterSummonSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 3)
		{
			for (int i = 0; i < otherData.Length; i++)
			{
				if (!(otherData[i].obj is Character character) || character == null || !character.isDead)
				{
					continue;
				}
				IPointOfInterest pointOfInterest = character;
				if (character.grave != null)
				{
					pointOfInterest = character.grave;
				}
				if (pointOfInterest.gridTileLocation != null && pointOfInterest.mapObjectVisual != null && pointOfInterest.isBeingCarriedBy == null && !pointOfInterest.isBeingSeized)
				{
					if (character.grave != null)
					{
						character.grave.SetRespawnCorpseOnDestroy(state: false);
						character.grave.gridTileLocation.structure.RemovePOI(character.grave);
					}
					else
					{
						character.DestroyMarker();
					}
				}
			}
		}
		LocationGridTile locationGridTile = goapNode.actor.gridTileLocation.GetFirstNeighbor();
		if (locationGridTile == null)
		{
			locationGridTile = goapNode.actor.gridTileLocation;
		}
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Bone_Golem, goapNode.actor.faction, goapNode.actor.homeSettlement, locationGridTile.parentMap.region, null, "", bypassIdeologyChecking: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
		GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
		Messenger.Broadcast(MonsterSignals.BONE_GOLEM_SPAWNED_FROM_TEMPLE, summon);
	}
}
