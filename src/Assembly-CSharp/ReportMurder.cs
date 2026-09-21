using UtilityScripts;

public class ReportMurder : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public ReportMurder()
		: base(INTERACTION_TYPE.REPORT_MURDER)
	{
		base.actionIconString = GoapActionStateDB.Report_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Crimes,
			LOG_TAG.Major
		};
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Report Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		_ = node.actor;
		_ = node.poiTarget;
		OtherData[] otherData = node.otherData;
		if (otherData.Length == 1 && otherData[0].obj is ActualGoapNode actualGoapNode)
		{
			Character character = actualGoapNode.target as Character;
			if (actualGoapNode.disguisedTarget != null)
			{
				character = actualGoapNode.disguisedTarget;
			}
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.CHARACTER_3);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = poiTarget as Character;
			if (!character.carryComponent.IsNotBeingCarried())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_carried";
			}
			else if (!character.limiterComponent.canWitness)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_inactive";
			}
		}
		return goapActionInvalidity;
	}

	public void AfterReportSuccess(ActualGoapNode goapNode)
	{
		ActualGoapNode crime = goapNode.otherData[0].obj as ActualGoapNode;
		Character recipient = goapNode.poiTarget as Character;
		ProcessInformation(recipient, crime);
	}

	private void ProcessInformation(Character recipient, ActualGoapNode crime)
	{
		recipient.reactionComponent.ReactTo(crime, REACTION_STATUS.INFORMED, addLog: false);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			Character character = poiTarget as Character;
			if (actor != character)
			{
				return !GameUtilities.IsRaceBeast(character.race);
			}
			return false;
		}
		return false;
	}
}
