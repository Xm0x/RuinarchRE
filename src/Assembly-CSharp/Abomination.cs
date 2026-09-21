using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class Abomination : Summon
{
	public Abomination()
		: base(SUMMON_TYPE.Abomination, "Abomination", RACE.ABOMINATION, Utilities.GetRandomGender())
	{
	}

	public Abomination(string className)
		: base(SUMMON_TYPE.Abomination, className, RACE.ABOMINATION, Utilities.GetRandomGender())
	{
	}

	public Abomination(SaveDataSummon data)
		: base(data)
	{
	}

	public override void PerTickDuringMovement()
	{
		base.PerTickDuringMovement();
		if (base.gridTileLocation?.tileObjectComponent.objHere != null && Random.Range(0, 100) < 5)
		{
			IPointOfInterest objHere = base.gridTileLocation.tileObjectComponent.objHere;
			objHere.traitContainer.AddTrait(objHere, "Abomination Germ", this);
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		LocationStructure locationStructure = base.currentStructure;
		LocationGridTile locationGridTile = null;
		if (locationStructure != null)
		{
			if (locationStructure.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				Area area = base.areaLocation;
				if (area != null)
				{
					locationGridTile = area.gridTileComponent.GetRandomPassableTileThatIsNotOccupied();
				}
			}
			else
			{
				locationGridTile = locationStructure.GetRandomPassableTileThatIsNotOccupied();
			}
		}
		if (locationGridTile != null)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.AGITATED, INTERACTION_TYPE.ABOMINATION_GERM_MUSHROOM, locationGridTile.tileObjectComponent.genericTileObject, this);
			goapPlanJob.SetIsAgitateJob(p_state: true);
			p_agitateJob = goapPlanJob;
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
			return true;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_1);
		return false;
	}
}
