using UtilityScripts;

public class EnhanceRelationship : GoapAction
{
	public EnhanceRelationship()
		: base(INTERACTION_TYPE.ENHANCE_RELATIONSHIP)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Social_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && !(node.poiTarget as Character).isSettlementRuler)
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "not_leader";
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		Character character = goapNode.poiTarget as Character;
		int num = 0;
		switch (goapNode.actor.TryGetTalentLevel(CHARACTER_TALENT.Social))
		{
		case 1:
			num = 50;
			break;
		case 2:
			num = 60;
			break;
		case 3:
			num = 70;
			break;
		case 4:
			num = 80;
			break;
		case 5:
			num = 90;
			break;
		}
		int totalOpinion = character.relationshipContainer.GetTotalOpinion(goapNode.actor);
		if (totalOpinion < 0)
		{
			num += totalOpinion;
		}
		else if (totalOpinion > 0)
		{
			int num2 = totalOpinion / 5;
			num += num2;
		}
		SetState(GameUtilities.RollChance(num) ? "Enhance Success" : "Enhance Fail", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		Character character = node.poiTarget as Character;
		log.AddToFillers(node.actor.faction, node.actor.faction.name, LOG_IDENTIFIER.FACTION_1);
		log.AddToFillers(character.faction, character.faction.name, LOG_IDENTIFIER.FACTION_2);
	}

	public void PreEnhanceSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		FactionRelationship relationshipWith = actor.faction.GetRelationshipWith(character.faction);
		relationshipWith?.ImproveRelationshipByAStep();
		actor.relationshipContainer.AdjustOpinion(actor, character, "Enhanced_Diplomatic_Relations", 20);
		character.relationshipContainer.AdjustOpinion(character, actor, "Enhanced_Diplomatic_Relations", 20);
		goapNode.descriptionLog.AddToFillers(null, relationshipWith?.localizedRelationshipStatus, LOG_IDENTIFIER.STRING_1);
	}

	public void PreEnhanceFail(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character target = goapNode.poiTarget as Character;
		actor.relationshipContainer.AdjustOpinion(actor, target, "Failed_Diplomatic_Relations", -20);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Character { faction: not null } character)
			{
				return character.faction != actor.faction;
			}
			return false;
		}
		return false;
	}
}
