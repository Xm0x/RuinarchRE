using System.Collections.Generic;
using Traits;

public class Exile : GoapAction
{
	public Exile()
		: base(INTERACTION_TYPE.EXILE)
	{
		base.actionIconString = GoapActionStateDB.Judge_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Life_Changes
		};
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Criminal", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Exile Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (!witness.traitContainer.HasTrait("Psychopath"))
		{
			if (witness.relationshipContainer.IsFriendsWith(character) || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
			{
				reactions.Add(EMOTION.Resentment);
			}
			else if (witness.relationshipContainer.IsEnemiesWith(character))
			{
				reactions.Add(EMOTION.Gratefulness);
			}
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
	}

	public void AfterExileSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.target as Character;
		if (character.traitContainer.HasTrait("Criminal"))
		{
			character.traitContainer.GetTraitOrStatus<Criminal>("Criminal").SetIsImprisoned(state: false);
		}
		character.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(character.faction, CRIME_STATUS.Exiled, goapNode.actor);
		Faction faction = character.faction;
		faction.KickOutCharacterAndRollForGrudge(character, out var p_wasGrudgeAdded);
		if (!p_wasGrudgeAdded)
		{
			OtherData[] otherData = goapNode.otherData;
			if (otherData != null && otherData[0].obj is CrimeData crimeData && crimeData.IsCrimeFabricated())
			{
				crimeData.TryTriggerGrudgeAgainstJudgeOrReporter();
			}
		}
		character.crimeComponent.RemoveAllCrimesWantedBy(faction);
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
		character.MigrateHomeStructureTo(null);
		character.ClearTerritory();
	}
}
