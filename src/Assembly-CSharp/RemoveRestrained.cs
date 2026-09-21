using System.Collections.Generic;

public class RemoveRestrained : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public RemoveRestrained()
		: base(INTERACTION_TYPE.REMOVE_RESTRAINED)
	{
		base.actionIconString = GoapActionStateDB.FirstAid_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1];
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && poiTarget.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.REMOVE_STATUS, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET)))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "already_being_removed";
			goapActionInvalidity.shouldLogInvalidity = false;
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Remove Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target))
		{
			return 2000;
		}
		return 10;
	}

	public override void OnStoppedInterrupt(ActualGoapNode node)
	{
		base.OnStoppedInterrupt(node);
		if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescuePartyQuest && rescuePartyQuest.targetCharacter == node.poiTarget)
		{
			rescuePartyQuest.SetIsReleasing(state: false);
		}
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescuePartyQuest && rescuePartyQuest.targetCharacter == node.poiTarget)
		{
			rescuePartyQuest.SetIsReleasing(state: false);
		}
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescuePartyQuest && rescuePartyQuest.targetCharacter == node.poiTarget)
		{
			rescuePartyQuest.SetIsReleasing(state: false);
		}
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescuePartyQuest && rescuePartyQuest.targetCharacter == node.poiTarget)
		{
			rescuePartyQuest.SetIsReleasing(state: false);
		}
	}

	public void AfterRemoveSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		goapNode.poiTarget.traitContainer.RemoveRestrainAndImprison(goapNode.poiTarget, goapNode.actor);
		if (actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescuePartyQuest && rescuePartyQuest.targetCharacter == goapNode.poiTarget)
		{
			rescuePartyQuest.SetIsSuccessful(state: true);
			rescuePartyQuest.SetIsReleasing(state: false);
			actor.partyComponent.currentParty.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
			if (character.traitContainer.HasTrait("Paralyzed") && !character.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER))
			{
				actor.jobComponent.TryTriggerMoveCharacter(character);
			}
		}
		character.combatComponent.RemoveHostileInRange(actor);
		character.combatComponent.RemoveAvoidInRange(actor);
		if (RelationshipManager.IsSexuallyCompatibleOneSided(character, actor))
		{
			MOOD_STATE moodState = character.moodComponent.moodState;
			bool flag = false;
			if (moodState == MOOD_STATE.Bad && ChanceData.RollChance(CHANCE_TYPE.Obsess_Bad_Mood))
			{
				character.traitContainer.BecomeObsessWith(character, actor);
				flag = true;
			}
			else if (moodState == MOOD_STATE.Critical && ChanceData.RollChance(CHANCE_TYPE.Obsess_Critical_Mood))
			{
				character.traitContainer.BecomeObsessWith(character, actor);
				flag = true;
			}
			if (flag)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Release_Obsession", LOG_TAG.Social);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is Character character)
		{
			if (!character.traitContainer.HasTrait("Restrained"))
			{
				return false;
			}
			Faction faction = actor.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Bandits)
			{
				Faction faction2 = character.faction;
				if (faction2 == null || faction2.factionType.type != FACTION_TYPE.Bandits)
				{
					return false;
				}
			}
			return actor != poiTarget;
		}
		return false;
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		reactions.Add(EMOTION.Gratefulness);
	}
}
