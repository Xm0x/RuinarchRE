using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class Execute : GoapAction
{
	public Execute()
		: base(INTERACTION_TYPE.EXECUTE)
	{
		base.actionIconString = GoapActionStateDB.Death_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Life_Changes
		};
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Execute Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (character.crimeComponent.HasWantedCrime() && character.crimeComponent.IsTargetOfACrime(witness))
		{
			reactions.Add(EMOTION.Approval);
			return;
		}
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
		if (opinionLabel == "Close Friend")
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Rage);
			}
		}
		else if (opinionLabel == "Friend" && !witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Resentment);
		}
		if (witness.traitContainer.HasTrait("Psychopath"))
		{
			return;
		}
		if (witness.traitContainer.HasTrait("Coward"))
		{
			if (GameUtilities.RollChance(75))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				reactions.Add(EMOTION.Shock);
			}
		}
		else if (GameUtilities.RollChance(15))
		{
			reactions.Add(EMOTION.Fear);
		}
		else
		{
			reactions.Add(EMOTION.Shock);
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		Character target2 = target as Character;
		switch (witness.relationshipContainer.GetOpinionLabel(target2))
		{
		case "Acquaintance":
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Sadness);
			}
			break;
		case "Friend":
		case "Close Friend":
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Sadness);
			}
			break;
		case "Rival":
			if (!witness.traitContainer.HasTrait("Diplomatic"))
			{
				reactions.Add(EMOTION.Scorn);
			}
			break;
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		Character character = target as Character;
		if (character.relationshipContainer.IsFriendsWith(actor) || character.relationshipContainer.IsFamilyMember(actor))
		{
			reactions.Add(EMOTION.Betrayal);
		}
		else
		{
			reactions.Add(EMOTION.Resentment);
		}
		if (target.traitContainer.HasTrait("Hothead") || GameUtilities.RollChance(20))
		{
			reactions.Add(EMOTION.Anger);
		}
	}

	public void AfterExecuteSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.target as Character;
		if (character.traitContainer.HasTrait("Criminal"))
		{
			character.traitContainer.GetTraitOrStatus<Criminal>("Criminal").SetIsImprisoned(state: false);
		}
		character.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(character.faction, CRIME_STATUS.Executed, goapNode.actor);
		character.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
		character.traitContainer.RemoveTrait(character, "Criminal", goapNode.actor);
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
		character.Death("executed", goapNode, goapNode.actor, null, null, null, null, isPlayerSource: false, goapNode.actor);
	}

	private bool IsTargetRestrained(Character actor, IPointOfInterest poiTarget, object[] otherData)
	{
		return poiTarget.traitContainer.HasTrait("Restrained");
	}
}
