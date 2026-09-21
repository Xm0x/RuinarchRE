public class GatherHerb : GoapAction
{
	public int m_amountProducedPerTick = 1;

	public GatherHerb()
		: base(INTERACTION_TYPE.GATHER_HERB)
	{
		base.actionIconString = GoapActionStateDB.Harvest_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Gather Herb Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		if (!node.poiTarget.isBeingSeized && node.ticksPerformingCurrentState > 0)
		{
			ProduceHerbPlant(node);
		}
	}

	public void AfterGatherHerbSuccess(ActualGoapNode p_node)
	{
		p_node.actor.jobComponent.CreateDropItemJob(JOB_TYPE.GATHER_HERB, ProduceHerbPlant(p_node), p_node.actor.structureComponent.workPlaceStructure);
	}

	private HerbPlant ProduceHerbPlant(ActualGoapNode p_node)
	{
		HerbPlant herbPlant = p_node.target as HerbPlant;
		p_node.actor.PickUpItem(herbPlant);
		return herbPlant;
	}
}
