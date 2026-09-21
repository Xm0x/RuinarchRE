using Traits;

public class ReleaseCharacter : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public ReleaseCharacter()
		: base(INTERACTION_TYPE.RELEASE_CHARACTER)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Release Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && !(poiTarget as Character).carryComponent.IsNotBeingCarried())
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "target_carried";
		}
		return goapActionInvalidity;
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

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string text = string.Empty;
		Character obj = node.poiTarget as Character;
		Restrained traitOrStatus = obj.traitContainer.GetTraitOrStatus<Restrained>("Restrained");
		Unconscious traitOrStatus2 = obj.traitContainer.GetTraitOrStatus<Unconscious>("Unconscious");
		Frozen traitOrStatus3 = obj.traitContainer.GetTraitOrStatus<Frozen>("Frozen");
		Ensnared traitOrStatus4 = obj.traitContainer.GetTraitOrStatus<Ensnared>("Ensnared");
		Enslaved traitOrStatus5 = obj.traitContainer.GetTraitOrStatus<Enslaved>("Enslaved");
		if (traitOrStatus != null)
		{
			text += traitOrStatus.localizedName;
		}
		if (traitOrStatus2 != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += traitOrStatus2.localizedName;
		}
		if (traitOrStatus3 != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += traitOrStatus3.localizedName;
		}
		if (traitOrStatus4 != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += traitOrStatus4.localizedName;
		}
		if (traitOrStatus5 != null)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += traitOrStatus5.localizedName;
		}
		if (!string.IsNullOrEmpty(text))
		{
			log.AddToFillers(null, text, LOG_IDENTIFIER.STRING_1);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return (poiTarget as Character).traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved");
		}
		return false;
	}

	public void AfterReleaseSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		bool flag = character.traitContainer.HasTrait("Enslaved");
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
		character.traitContainer.RemoveStatusAndStacks(character, "Unconscious", goapNode.actor);
		character.traitContainer.RemoveStatusAndStacks(character, "Frozen", goapNode.actor);
		character.traitContainer.RemoveStatusAndStacks(character, "Ensnared", goapNode.actor);
		character.traitContainer.RemoveTrait(character, "Enslaved", goapNode.actor);
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
		if (flag && actor.faction != null && actor.faction.isMajorNonPlayer)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, actor, "join_faction_normal");
		}
		character.combatComponent.RemoveHostileInRange(actor);
		character.combatComponent.RemoveAvoidInRange(actor);
		if (RelationshipManager.IsSexuallyCompatibleOneSided(character, actor))
		{
			MOOD_STATE moodState = character.moodComponent.moodState;
			bool flag2 = false;
			if (moodState == MOOD_STATE.Bad && ChanceData.RollChance(CHANCE_TYPE.Obsess_Bad_Mood))
			{
				character.traitContainer.BecomeObsessWith(character, actor);
				flag2 = true;
			}
			else if (moodState == MOOD_STATE.Critical && ChanceData.RollChance(CHANCE_TYPE.Obsess_Critical_Mood))
			{
				character.traitContainer.BecomeObsessWith(character, actor);
				flag2 = true;
			}
			if (flag2)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Release_Obsession", LOG_TAG.Social);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
		}
	}
}
