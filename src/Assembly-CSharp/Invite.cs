using System.Collections.Generic;

public class Invite : GoapAction
{
	public Invite()
		: base(INTERACTION_TYPE.INVITE)
	{
		base.actionIconString = GoapActionStateDB.Flirt_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Needs,
			LOG_TAG.Social
		};
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.INVITED, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Invite Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		Character character = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			Character character2 = poiTarget as Character;
			if (character2.traitContainer.HasTrait("Unconscious") || !character2.limiterComponent.canPerform || character2.combatComponent.isInCombat || (character2.interruptComponent.isInterrupted && character2.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Cowering))
			{
				goapActionInvalidity.isInvalid = true;
				if (character2.limiterComponent.canPerform)
				{
					goapActionInvalidity.stateName = "Invite Fail";
				}
				else
				{
					goapActionInvalidity.stateName = "Invite Fail Cannot Perform";
				}
			}
			else
			{
				if (character.reactionComponent.disguisedCharacter != null)
				{
					character = character.reactionComponent.disguisedCharacter;
				}
				if (character2.reactionComponent.disguisedCharacter != null)
				{
					character2 = character2.reactionComponent.disguisedCharacter;
				}
				if (!character2.traitContainer.HasTrait("Unconscious"))
				{
					WeightedDictionary<string> weightedDictionary = new WeightedDictionary<string>();
					int num = 20;
					int num2 = 10;
					Character firstCharacterWithRelationship = character2.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
					if (firstCharacterWithRelationship != null && firstCharacterWithRelationship != character)
					{
						num = 0;
						num2 = 50;
						if (character2.traitContainer.HasTrait("Unfaithful"))
						{
							num += 200;
							if (character2.traitContainer.HasTrait("Drunk"))
							{
								num += 100;
							}
						}
						else
						{
							if (character2.traitContainer.HasTrait("Treacherous", "Psychopath"))
							{
								num += 50;
							}
							else
							{
								num2 += 100;
							}
							if (character2.traitContainer.HasTrait("Drunk"))
							{
								num += 50;
							}
						}
					}
					else
					{
						int compatibilityBetween = RelationshipManager.Instance.GetCompatibilityBetween(character2, character);
						num += 10 * compatibilityBetween;
						if (character2.traitContainer.HasTrait("Drunk"))
						{
							num += 100;
						}
					}
					if (character2.traitContainer.HasTrait("Lustful"))
					{
						num += 100;
					}
					else if (character2.traitContainer.HasTrait("Chaste"))
					{
						num2 += 300;
					}
					if (character2.moodComponent.moodState == MOOD_STATE.Bad)
					{
						num2 += 50;
					}
					else if (character2.moodComponent.moodState == MOOD_STATE.Critical)
					{
						num2 += 200;
					}
					weightedDictionary.AddElement("Accept", num);
					weightedDictionary.AddElement("Reject", num2);
					weightedDictionary.PickRandomElementGivenWeights();
				}
				if ("Accept" == "Reject")
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.stateName = "Invite Rejected";
					character.relationshipContainer.AdjustOpinion(character, character2, "Base", -3, "Rejected_Sexual_Advances");
					character.traitContainer.AddTrait(character, "Annoyed", character2);
					Faction faction = character.faction;
					if (faction != null && faction.factionType.type == FACTION_TYPE.Disguised)
					{
						character.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
						if ((bool)character2.marker && !character2.marker.HasUnprocessedPOI(character))
						{
							character2.marker.AddUnprocessedPOI(character);
						}
					}
				}
			}
		}
		return goapActionInvalidity;
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		if (node.actor is SeducerSummon)
		{
			(node.poiTarget as Character).combatComponent.Fight(node.actor, "Hostility");
		}
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (target == witness || !(target is Character character))
		{
			return;
		}
		bool num = witness.relationshipContainer.IsLoverOrAffair(actor);
		bool flag = witness.relationshipContainer.IsLoverOrAffair(character);
		if (num)
		{
			reactions.Add(EMOTION.Rage);
			reactions.Add(EMOTION.Betrayal);
			return;
		}
		if (flag)
		{
			reactions.Add(EMOTION.Rage);
			if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor))
			{
				reactions.Add(EMOTION.Betrayal);
			}
			return;
		}
		reactions.Add(EMOTION.Embarassment);
		Character firstCharacterWithRelationship = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
		if (firstCharacterWithRelationship != null && firstCharacterWithRelationship != character)
		{
			reactions.Add(EMOTION.Disapproval);
			reactions.Add(EMOTION.Disgust);
		}
		else if (witness.relationshipContainer.IsFriendsWith(actor))
		{
			reactions.Add(EMOTION.Scorn);
		}
	}

	public void PreInviteSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.CarryPOI(goapNode.poiTarget);
	}

	public void AfterInviteFail(ActualGoapNode goapNode)
	{
		if (goapNode.actor is SeducerSummon)
		{
			(goapNode.poiTarget as Character).combatComponent.Fight(goapNode.actor, "Hostility");
		}
		else
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Annoyed", goapNode.poiTarget as Character);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			Character character = poiTarget as Character;
			if (character == actor)
			{
				return false;
			}
			if (character.stateComponent.currentState is CombatState)
			{
				return false;
			}
			if (character.hasBeenRaisedFromDead)
			{
				return false;
			}
			if (character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld || character.currentRegion != actor.currentRegion)
			{
				return false;
			}
			return character.carryComponent.IsNotBeingCarried();
		}
		return false;
	}
}
