using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class Centaur : Summon
{
	public const string ClassName = "Centaur";

	public override bool defaultDigMode => true;

	public Centaur()
		: base(SUMMON_TYPE.Centaur, "Centaur", RACE.CENTAUR, Utilities.GetRandomGender())
	{
	}

	public Centaur(string className)
		: base(SUMMON_TYPE.Centaur, className, RACE.CENTAUR, Utilities.GetRandomGender())
	{
	}

	public Centaur(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetEnableDigging(state: true);
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		Area area = null;
		List<Area> list = RuinarchListPool<Area>.Claim();
		PopulateValidHexTilesNextToHome(list);
		if (list.Count > 0)
		{
			area = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Area>.Release(list);
		if (area != null)
		{
			LocationGridTile randomUnoccupiedNoFreezingTrap = area.gridTileComponent.GetRandomUnoccupiedNoFreezingTrap();
			if (randomUnoccupiedNoFreezingTrap != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, INTERACTION_TYPE.PLACE_SNARE_TRAP, randomUnoccupiedNoFreezingTrap.tileObjectComponent.genericTileObject, this);
				goapPlanJob.SetIsAgitateJob(p_state: true);
				p_agitateJob = goapPlanJob;
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
				return true;
			}
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_2);
			return false;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_1);
		return false;
	}

	private void PopulateValidHexTilesNextToHome(List<Area> areas)
	{
		Area area = null;
		if (base.homeSettlement != null)
		{
			base.homeSettlement.PopulateSurroundingAreasInSameRegionWithLessThanNumOfSnareTraps(areas, base.homeRegion, 5);
		}
		else if (base.homeStructure != null)
		{
			area = ((!(base.homeStructure is Cave cave)) ? base.homeStructure.occupiedArea : CollectionUtilities.GetRandomElement(cave.occupiedAreas.Keys));
		}
		else if (HasTerritory())
		{
			area = base.territory;
		}
		if (area == null)
		{
			return;
		}
		for (int i = 0; i < area.neighbourComponent.neighbours.Count; i++)
		{
			Area area2 = area.neighbourComponent.neighbours[i];
			if (area2.region == area.region && area2.snareTraps < 5)
			{
				areas.Add(area2);
			}
		}
	}
}
