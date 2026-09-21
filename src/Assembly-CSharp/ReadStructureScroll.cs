using UnityEngine.Localization.Settings;
using UtilityScripts;

public class ReadStructureScroll : GoapAction
{
	public override bool shouldShowNotifRegardlessOfWatcher => true;

	public ReadStructureScroll()
		: base(INTERACTION_TYPE.READ_STRUCTURE_SCROLL)
	{
		base.actionIconString = GoapActionStateDB.Read_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.showNotification = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Read Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string text = (node.poiTarget as StructureScroll).structureType.LocalizedStructureName();
		string articleForWord = Utilities.GetArticleForWord(text);
		if (LocalizationSettings.SelectedLocale.Identifier.Code == "tr-TR")
		{
			log.AddToFillers(null, text ?? "", LOG_IDENTIFIER.STRING_1);
		}
		else
		{
			log.AddToFillers(null, articleForWord + " " + text, LOG_IDENTIFIER.STRING_1);
		}
		log.AddToFillers(null, Utilities.PluralizeString(text) ?? "", LOG_IDENTIFIER.STRING_2);
		log.AddToFillers(node.actor.faction, node.actor.faction?.name, LOG_IDENTIFIER.FACTION_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 1;
		}
		return false;
	}

	public void AfterReadSuccess(ActualGoapNode goapNode)
	{
		goapNode.poiTarget.gridTileLocation?.structure.RemovePOI(goapNode.poiTarget);
	}
}
