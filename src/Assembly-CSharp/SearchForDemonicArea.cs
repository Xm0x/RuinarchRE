public class SearchForDemonicArea : GoapAction
{
	public SearchForDemonicArea()
		: base(INTERACTION_TYPE.SEARCH_FOR_DEMONIC_AREA)
	{
		base.actionIconString = GoapActionStateDB.Inspect_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.ON_REACH_CORRUPTION;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Search Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		base.OnActionStarted(node);
		if (GameManager.Instance.gameHasStarted && node.actor.previousCharacterDataComponent.previousActionNodeType != base.goapType)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Search For Demonic Area notification", LOG_TAG.Major);
			AddFillersToLog(log, node);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}
}
