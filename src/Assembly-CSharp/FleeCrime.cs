public class FleeCrime : GoapAction
{
	public FleeCrime()
		: base(INTERACTION_TYPE.FLEE_CRIME)
	{
		base.actionIconString = GoapActionStateDB.Flee_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Flee Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterFleeSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (!actor.isVagrantOrFactionless)
		{
			if (actor.faction != null)
			{
				actor.faction.AddBannedCharacter(actor);
			}
			actor.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
		}
		actor.MigrateHomeStructureTo(null);
	}
}
