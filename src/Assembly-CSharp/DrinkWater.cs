public class DrinkWater : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

	public DrinkWater()
		: base(INTERACTION_TYPE.DRINK_WATER)
	{
		base.actionIconString = GoapActionStateDB.Drink_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Drink Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void PreDrinkSuccess(ActualGoapNode goapNode)
	{
	}

	public void PerTickDrinkSuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterDrinkSuccess(ActualGoapNode goapNode)
	{
	}
}
