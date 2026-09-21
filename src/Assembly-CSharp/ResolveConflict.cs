using System;
using UnityEngine;

public class ResolveConflict : GoapAction
{
	public ResolveConflict()
		: base(INTERACTION_TYPE.RESOLVE_CONFLICT)
	{
		base.actionIconString = GoapActionStateDB.Agree_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Resolve Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 4;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = poiTarget as Character;
			if ((character.traitContainer.HasTrait("Hothead") && UnityEngine.Random.Range(0, 2) == 0) || character.combatComponent.isInCombat)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.stateName = "Resolve Fail";
			}
		}
		return goapActionInvalidity;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			bool flag = false;
			if (poiTarget is Character)
			{
				flag = (poiTarget as Character).relationshipContainer.GetFirstAliveEnemyCharacter() != null;
			}
			if (actor != poiTarget && flag)
			{
				return actor.traitContainer.HasTrait("Diplomatic");
			}
			return false;
		}
		return false;
	}

	public void PreResolveSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character)
		{
			Character character = goapNode.poiTarget as Character;
			Character firstAliveEnemyCharacter = character.relationshipContainer.GetFirstAliveEnemyCharacter();
			if (firstAliveEnemyCharacter == null)
			{
				throw new Exception("Cannot resolve conflict for " + character.name + " because he/she does not have enemies!");
			}
			goapNode.descriptionLog.AddToFillers(firstAliveEnemyCharacter, firstAliveEnemyCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
		}
	}

	public void AfterResolveSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character)
		{
			_ = goapNode.poiTarget;
		}
	}
}
