public class AbsorbPowerCrystal : GoapAction
{
	public AbsorbPowerCrystal()
		: base(INTERACTION_TYPE.ABSORB_POWER_CRYSTAL)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Absorb Crystal Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public void AfterAbsorbCrystalSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		TileObject tileObject = goapNode.target as TileObject;
		actor.AbsorbCrystal(tileObject as PowerCrystal);
		tileObject.currentStructure?.RemovePOI(tileObject);
	}
}
