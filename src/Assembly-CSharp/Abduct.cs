using System.Collections.Generic;

public class Abduct : GoapAction
{
	public Abduct()
		: base(INTERACTION_TYPE.ABDUCT)
	{
		base.actionIconString = GoapActionStateDB.Restrain_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Abduct Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (witness.relationshipContainer.IsFriendsWith(character) || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Resentment);
			}
			if (!witness.traitContainer.HasTrait("Diplomatic"))
			{
				reactions.Add(EMOTION.Anger);
			}
		}
		else if (witness.relationshipContainer.IsEnemiesWith(character))
		{
			reactions.Add(EMOTION.Approval);
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
		bool flag = false;
		if (witness.faction != null && witness.faction.leader == character)
		{
			flag = true;
		}
		else if (witness.homeSettlement != null && witness.homeSettlement.ruler == character)
		{
			flag = true;
		}
		if (opinionLabel == "Friend" || opinionLabel == "Close Friend" || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character) || (flag && opinionLabel != "Rival"))
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Distraught);
			}
			return;
		}
		switch (opinionLabel)
		{
		case "Acquaintance":
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Concern);
			}
			break;
		case "Enemy":
		case "Rival":
			if (!witness.traitContainer.HasTrait("Diplomatic"))
			{
				reactions.Add(EMOTION.Scorn);
			}
			else
			{
				reactions.Add(EMOTION.Concern);
			}
			break;
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		Character character = target as Character;
		if (!character.IsHostileWith(actor))
		{
			reactions.Add(EMOTION.Resentment);
			if (character.traitContainer.HasTrait("Hothead"))
			{
				reactions.Add(EMOTION.Rage);
			}
			else
			{
				reactions.Add(EMOTION.Anger);
			}
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		if (target is Character character)
		{
			if (actor.faction != null && witness.faction != null && witness.faction.isMajorNonPlayerOrBandits && actor.faction.isMajorNonPlayerOrBandits && actor.faction != witness.faction && witness.faction == character.faction)
			{
				if (witness.isFactionLeader || witness.isSettlementRuler)
				{
					witness.faction.FactionProcessingAbductionOrMurder(actor, character, node);
					return result;
				}
				if (witness.faction.leader is Character || (witness.homeSettlement != null && witness.homeSettlement.ruler != null))
				{
					witness.jobComponent.TryCreateReportMurderOrAbduct(node);
					return result;
				}
			}
			else if (witness.faction == character.faction && witness.faction != null && witness.faction.ShouldRescueCharacter(node) && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, character) && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, character))
			{
				witness.faction.partyQuestBoard.CreateRescuePartyQuest(witness, null, character);
			}
		}
		return result;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (target.factionOwner == null || actor.faction == null)
		{
			return CRIME_TYPE.Assault;
		}
		if (actor.faction.IsHostileWith(target.factionOwner) && target is Character character && !character.crimeComponent.IsWantedBy(actor.faction))
		{
			return CRIME_TYPE.Assault;
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Assault;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public void AfterAbductSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character && (character.isFactionLeader || character.isSettlementRuler) && character.faction != null && character.faction.isMajorNonPlayerOrBandits && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, character) && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, character))
		{
			character.faction.partyQuestBoard.CreateRescuePartyQuest(null, character.homeSettlement, character);
		}
	}
}
