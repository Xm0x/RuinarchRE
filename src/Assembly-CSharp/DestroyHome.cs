using System.Linq;
using Inner_Maps.Location_Structures;

public class DestroyHome : GoapAction
{
	public DestroyHome()
		: base(INTERACTION_TYPE.DESTROY_HOME)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Destroy Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Assault;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Assault;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode goapNode)
	{
		base.AddFillersToLog(log, goapNode);
		if (goapNode.poiTarget is StructureTileObject { structureParent: { } structureParent })
		{
			log.AddToFillers(structureParent, structureParent.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		}
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		if (node.poiTarget is StructureTileObject { gridTileLocation: not null, structureParent: { } structureParent })
		{
			DestroySmokeEffectOn(structureParent);
		}
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		if (node.poiTarget is StructureTileObject { gridTileLocation: not null, structureParent: { } structureParent })
		{
			DestroySmokeEffectOn(structureParent);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return poiTarget.gridTileLocation != null;
		}
		return false;
	}

	public void PreDestroySuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is StructureTileObject { gridTileLocation: not null, structureParent: { } structureParent })
		{
			CreateSmokeEffectOn(structureParent);
		}
	}

	public void AfterDestroySuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is StructureTileObject { gridTileLocation: not null, structureParent: { settlementLocation: var settlementLocation } structureParent })
		{
			DestroySmokeEffectOn(structureParent);
			structureParent.AdjustHP(-structureParent.maxHP);
			if (structureParent.hasBeenDestroyed && structureParent.structureType.IsVillageStructure() && goapNode.associatedJobType == JOB_TYPE.GRUDGE)
			{
				Messenger.Broadcast(StructureSignals.STRUCTURE_DESTROYED_BY_PLAYER, structureParent, settlementLocation);
			}
		}
	}

	private void CreateSmokeEffectOn(LocationStructure p_structure)
	{
		if (p_structure.objectsThatContributeToDamage.Count > 0)
		{
			for (int i = 0; i < p_structure.objectsThatContributeToDamage.Count; i++)
			{
				if (p_structure.objectsThatContributeToDamage.ElementAt(i) is IPointOfInterest { gridTileLocation: not null } pointOfInterest && (bool)pointOfInterest.mapObjectVisual)
				{
					GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Construction_Dust);
				}
			}
		}
		else
		{
			if (!(p_structure is ManMadeStructure manMadeStructure))
			{
				return;
			}
			for (int j = 0; j < manMadeStructure.structureWalls.Count; j++)
			{
				ThinWall thinWall = manMadeStructure.structureWalls[j];
				if (thinWall.gridTileLocation != null && (bool)thinWall.mapObjectVisual)
				{
					GameManager.Instance.CreateParticleEffectAt(thinWall, PARTICLE_EFFECT.Construction_Dust);
				}
			}
		}
	}

	private void DestroySmokeEffectOn(LocationStructure p_structure)
	{
		if (p_structure.objectsThatContributeToDamage.Count > 0)
		{
			for (int i = 0; i < p_structure.objectsThatContributeToDamage.Count; i++)
			{
				if (p_structure.objectsThatContributeToDamage.ElementAt(i) is IPointOfInterest { gridTileLocation: not null } pointOfInterest && (bool)pointOfInterest.mapObjectVisual)
				{
					pointOfInterest.mapObjectVisual.DestroyParticlesByName("ConstructionDust");
				}
			}
		}
		else
		{
			if (!(p_structure is ManMadeStructure manMadeStructure))
			{
				return;
			}
			for (int j = 0; j < manMadeStructure.structureWalls.Count; j++)
			{
				ThinWall thinWall = manMadeStructure.structureWalls[j];
				if (thinWall.gridTileLocation != null && (bool)thinWall.mapObjectVisual)
				{
					thinWall.mapObjectVisual.DestroyParticlesByName("ConstructionDust");
				}
			}
		}
	}
}
