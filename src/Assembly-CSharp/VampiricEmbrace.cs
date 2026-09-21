using System.Collections.Generic;
using Object_Pools;
using Traits;

public class VampiricEmbrace : GoapAction
{
	public VampiricEmbrace()
		: base(INTERACTION_TYPE.VAMPIRIC_EMBRACE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Vampire_Turn_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Embrace Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire).IsConsideredACrime())
		{
			if (witness.traitContainer.HasTrait("Coward", "Hemophobic"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Threatened);
				if (witness.relationshipContainer.GetOpinionLabel(actor) == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else
				{
					reactions.Add(EMOTION.Shock);
				}
			}
			if (target is Character character)
			{
				if (witness.relationshipContainer.IsFriendsWith(character) || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
				{
					reactions.Add(EMOTION.Anger);
				}
				else if (!witness.relationshipContainer.IsEnemiesWith(character) && (witness.relationshipContainer.GetOpinionLabel(character) == "Acquaintance" || (witness.faction != null && witness.faction == character.faction) || (witness.homeSettlement != null && witness.homeSettlement == character.homeSettlement)))
				{
					reactions.Add(EMOTION.Anger);
				}
			}
		}
		else if (witness.traitContainer.HasTrait("Hemophiliac"))
		{
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor))
			{
				reactions.Add(EMOTION.Arousal);
			}
			else
			{
				reactions.Add(EMOTION.Approval);
			}
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (!(target is Character character))
		{
			return;
		}
		if (CrimeManager.Instance.GetCrimeSeverity(character, actor, target, CRIME_TYPE.Vampire).IsConsideredACrime())
		{
			reactions.Add(EMOTION.Shock);
			string opinionLabel = character.relationshipContainer.GetOpinionLabel(actor);
			if (character.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				reactions.Add(EMOTION.Threatened);
			}
			if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
		else if (character.traitContainer.HasTrait("Hemophiliac"))
		{
			if (RelationshipManager.IsSexuallyCompatibleOneSided(actor, character))
			{
				reactions.Add(EMOTION.Arousal);
			}
			else
			{
				reactions.Add(EMOTION.Approval);
			}
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AddAwareCharacter(witness);
		return base.ReactionToActor(actor, target, witness, node, status);
	}

	public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		if (target is Character character)
		{
			actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AddAwareCharacter(character);
		}
		return base.ReactionOfTarget(actor, target, node, status);
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Vampire;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Vampire;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Character character)
			{
				if (actor != character && actor.traitContainer.HasTrait("Vampire"))
				{
					return character.carryComponent.IsNotBeingCarried();
				}
				return false;
			}
			if (actor != poiTarget)
			{
				return actor.traitContainer.HasTrait("Vampire");
			}
			return false;
		}
		return false;
	}

	public void AfterEmbraceSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (!(goapNode.poiTarget is Character character))
		{
			return;
		}
		if (character.HasItem("Phylactery"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " activate_phylactery", LOG_TAG.Social, goapNode);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			character.UnobtainItem("Phylactery");
			actor.AdjustHP(-500, ELEMENTAL_TYPE.Normal);
			if (!actor.HasHealth())
			{
				actor.Death("normal", goapNode, character, log, null, null, null, isPlayerSource: false, character);
			}
			else
			{
				actor.traitContainer.AddTrait(actor, "Unconscious", character);
				actor.traitContainer.GetTraitOrStatus<Trait>("Unconscious")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
			}
			LogPool.Release(log);
			return;
		}
		if (character.isDead)
		{
			character.ReturnToLife();
		}
		character.traitContainer.RemoveStatusAndStacks(character, "Injured");
		character.traitContainer.RemoveStatusAndStacks(character, "Plagued");
		if (!character.classComponent.IsStalkerCannotBeTurned() && character.traitContainer.AddTrait(character, "Vampire", actor))
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_BECAME_VAMPIRE, character);
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " contracted", LOG_TAG.Life_Changes, goapNode);
			log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log2.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(actor, log2, releaseLogAfter: true);
		}
		if (character.isNormalCharacter)
		{
			actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AdjustNumOfConvertedVillagers(1);
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.isActiveMember && actor.partyComponent.currentParty.currentQuest is RecruitVampiresPartyQuest)
		{
			character.ChangeFactionTo(actor.faction, bypassIdeologyChecking: true);
			character.MigrateHomeTo(actor.homeSettlement);
			if (character is FireElemental)
			{
				character.MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement: false);
			}
			else if (character is VengefulGhost)
			{
				character.behaviourComponent.SetIsAttackingDemonicStructure(state: false, null);
				character.behaviourComponent.UpdateDefaultBehaviourSet();
			}
			character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
			if (character is Summon)
			{
				character.AdjustHP(character.maxHP, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true);
			}
			Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " recruited", LOG_TAG.Life_Changes, goapNode);
			log3.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log3.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log3.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(actor, log3, releaseLogAfter: true);
			character.jobComponent.PlanReturnToVillageCenter(JOB_TYPE.RETURN_HOME_URGENT);
		}
	}
}
