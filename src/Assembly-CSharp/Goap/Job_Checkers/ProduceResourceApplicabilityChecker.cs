using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Goap.Job_Checkers;

public class ProduceResourceApplicabilityChecker : JobApplicabilityChecker
{
	public const int MinimumFood = 100;

	public const int MinimumMetal = 100;

	public const int MinimumStone = 100;

	public const int MinimumWood = 100;

	public override string key => "IsProduceResourceApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		NPCSettlement settlement = job.originalOwner as NPCSettlement;
		RESOURCE rESOURCE = ((job.jobType != JOB_TYPE.PRODUCE_FOOD) ? ((job.jobType == JOB_TYPE.PRODUCE_WOOD) ? RESOURCE.WOOD : ((job.jobType != JOB_TYPE.PRODUCE_STONE) ? RESOURCE.METAL : RESOURCE.STONE)) : RESOURCE.FOOD);
		return GetTotalResource(rESOURCE, settlement) < GetMinimumResource(rESOURCE);
	}

	private int GetTotalResource(RESOURCE resourceType, NPCSettlement settlement)
	{
		int num = 0;
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		settlement.mainStorage.PopulateTileObjectsOfType<ResourcePile>(list);
		for (int i = 0; i < list.Count; i++)
		{
			ResourcePile resourcePile = list[i] as ResourcePile;
			if (resourcePile.providedResource == resourceType)
			{
				num += resourcePile.resourceInPile;
			}
		}
		RuinarchListPool<TileObject>.Release(list);
		return num;
	}

	private int GetMinimumResource(RESOURCE resource)
	{
		return resource switch
		{
			RESOURCE.FOOD => 100, 
			RESOURCE.WOOD => 100, 
			RESOURCE.METAL => 100, 
			RESOURCE.STONE => 100, 
			_ => throw new Exception("There is no minimum resource for " + resource.ToStringEnum()), 
		};
	}
}
