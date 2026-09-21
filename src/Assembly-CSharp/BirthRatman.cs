public class BirthRatman : GoapAction
{
	public BirthRatman()
		: base(INTERACTION_TYPE.BIRTH_RATMAN)
	{
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Birth Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor.gridTileLocation != null;
		}
		return false;
	}

	public void AfterBirthSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (actor.gridTileLocation != null)
		{
			Character character = CharacterManager.Instance.CreateNewCharacter("Ratman", RACE.RATMAN, actor.gender, actor.faction, actor.homeSettlement, actor.homeRegion, actor.homeStructure);
			character.CreateMarker();
			character.InitialCharacterPlacement(actor.gridTileLocation);
		}
	}
}
