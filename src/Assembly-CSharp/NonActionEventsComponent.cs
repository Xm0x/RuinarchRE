using Inner_Maps.Location_Structures;
using Interrupts;
using Object_Pools;
using Plague.Transmission;
using Traits;
using UnityEngine;
using UtilityScripts;

public class NonActionEventsComponent : CharacterComponent
{
	private const string Warm_Chat = "Warm Chat";

	private const string Awkward_Chat = "Awkward Chat";

	private const string Argument = "Argument";

	private const string Insult = "Insult";

	private const string Praise = "Praise";

	private readonly WeightedDictionary<string> chatWeights;

	public GameDate lastConversationDate { get; private set; }

	public bool canChatOrFlirt { get; private set; }

	public GameDate chatOrFlirtReenableDate { get; private set; }

	public NonActionEventsComponent()
	{
		chatWeights = new WeightedDictionary<string>();
		lastConversationDate = GameManager.Instance.Today();
	}

	public NonActionEventsComponent(SaveDataNonActionEventsComponent data)
	{
		chatWeights = new WeightedDictionary<string>();
		lastConversationDate = data.lastConversationDate;
		canChatOrFlirt = data.canChatOrFlirt;
		chatOrFlirtReenableDate = data.chatOrFlirtReenableDate;
	}

	public bool CanChat(Character target)
	{
		Character disguisedCharacter = base.owner;
		Character character = target;
		if (base.owner.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = base.owner.reactionComponent.disguisedCharacter;
		}
		if (target.reactionComponent.disguisedCharacter != null)
		{
			character = target.reactionComponent.disguisedCharacter;
		}
		if (target.isDead || !disguisedCharacter.limiterComponent.canWitness || !character.limiterComponent.canWitness || disguisedCharacter is Summon || character is Summon)
		{
			return false;
		}
		if (target.traitContainer.HasTrait("Burning", "Burning At Stake"))
		{
			return false;
		}
		if (base.owner.traitContainer.HasTrait("Burning", "Burning At Stake"))
		{
			return false;
		}
		return true;
	}

	private bool CanFlirt(Character p_character1, Character p_character2)
	{
		if (p_character2.traitContainer.HasTrait("Burning", "Burning At Stake"))
		{
			return false;
		}
		if (p_character1.traitContainer.HasTrait("Burning", "Burning At Stake"))
		{
			return false;
		}
		if (p_character1.relationshipContainer.IsLoverOrAffair(p_character2))
		{
			return true;
		}
		Unfaithful traitOrStatus = p_character1.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
		if (traitOrStatus != null)
		{
			return traitOrStatus.CanBeLoverOrAffairBasedOnPersonalConstraints(p_character1, p_character2);
		}
		if (p_character1.raceSetting.category == CHARACTER_CATEGORY.Beast || p_character1.raceSetting.category == CHARACTER_CATEGORY.Undead)
		{
			return false;
		}
		if (p_character2.raceSetting.category == CHARACTER_CATEGORY.Beast || p_character2.raceSetting.category == CHARACTER_CATEGORY.Undead)
		{
			return false;
		}
		if (!p_character1.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER))
		{
			return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
		}
		return true;
	}

	private void DisableChatAndFlirt()
	{
		canChatOrFlirt = false;
	}

	private void EnableChatAndFlirt()
	{
		canChatOrFlirt = true;
		chatOrFlirtReenableDate = default(GameDate);
	}

	private void ScheduleChatAndFlirtEnable(int p_ticks)
	{
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.AddTicks(p_ticks);
		chatOrFlirtReenableDate = gameDate;
		SchedulingManager.Instance.AddEntry(gameDate, EnableChatAndFlirt, base.owner);
	}

	public bool ForceChatCharacter(Character target, ref Log overrideLog)
	{
		Character disguisedCharacter = base.owner;
		Character otherCharacter = target;
		if (base.owner.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = base.owner.reactionComponent.disguisedCharacter;
		}
		if (target.reactionComponent.disguisedCharacter != null)
		{
			otherCharacter = target.reactionComponent.disguisedCharacter;
		}
		if (!disguisedCharacter.IsHostileWith(otherCharacter))
		{
			TriggerChatCharacter(target, ref overrideLog);
			return true;
		}
		return false;
	}

	private void TriggerChatCharacter(Character target, ref Log overrideLog)
	{
		_ = string.Empty;
		Character disguisedCharacter = base.owner;
		Character character = target;
		if (base.owner.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = base.owner.reactionComponent.disguisedCharacter;
		}
		if (target.reactionComponent.disguisedCharacter != null)
		{
			character = target.reactionComponent.disguisedCharacter;
		}
		chatWeights.Clear();
		chatWeights.AddElement("Warm Chat", 100);
		chatWeights.AddElement("Awkward Chat", 30);
		chatWeights.AddElement("Argument", 20);
		chatWeights.AddElement("Insult", 20);
		chatWeights.AddElement("Praise", 20);
		MOOD_STATE moodState = disguisedCharacter.moodComponent.moodState;
		MOOD_STATE moodState2 = character.moodComponent.moodState;
		string opinionLabel = disguisedCharacter.relationshipContainer.GetOpinionLabel(character);
		string opinionLabel2 = character.relationshipContainer.GetOpinionLabel(disguisedCharacter);
		int compatibilityBetween = RelationshipManager.Instance.GetCompatibilityBetween(disguisedCharacter, character);
		switch (moodState)
		{
		case MOOD_STATE.Bad:
			chatWeights.AddWeightToElement("Warm Chat", -20);
			chatWeights.AddWeightToElement("Argument", 15);
			chatWeights.AddWeightToElement("Insult", 20);
			break;
		case MOOD_STATE.Critical:
			chatWeights.AddWeightToElement("Warm Chat", -40);
			chatWeights.AddWeightToElement("Argument", 30);
			chatWeights.AddWeightToElement("Insult", 50);
			break;
		}
		switch (moodState2)
		{
		case MOOD_STATE.Bad:
			chatWeights.AddWeightToElement("Warm Chat", -20);
			chatWeights.AddWeightToElement("Argument", 15);
			break;
		case MOOD_STATE.Critical:
			chatWeights.AddWeightToElement("Warm Chat", -40);
			chatWeights.AddWeightToElement("Argument", 30);
			break;
		}
		switch (opinionLabel)
		{
		case "Close Friend":
		case "Friend":
			chatWeights.AddWeightToElement("Awkward Chat", -15);
			break;
		case "Enemy":
		case "Rival":
			chatWeights.AddWeightToElement("Awkward Chat", 15);
			break;
		}
		if (opinionLabel == "Enemy")
		{
			chatWeights.AddWeightToElement("Argument", 15);
			chatWeights.AddWeightToElement("Insult", 15);
			chatWeights.AddWeightToElement("Praise", -15);
		}
		else if (opinionLabel == "Rival")
		{
			chatWeights.AddWeightToElement("Argument", 30);
			chatWeights.AddWeightToElement("Insult", 30);
			chatWeights.AddWeightToElement("Praise", -30);
		}
		switch (opinionLabel2)
		{
		case "Close Friend":
		case "Friend":
			chatWeights.AddWeightToElement("Awkward Chat", -15);
			break;
		case "Enemy":
		case "Rival":
			chatWeights.AddWeightToElement("Awkward Chat", 15);
			break;
		}
		if (opinionLabel2 == "Enemy")
		{
			chatWeights.AddWeightToElement("Argument", 15);
			chatWeights.AddWeightToElement("Insult", 15);
			chatWeights.AddWeightToElement("Praise", -15);
		}
		else if (opinionLabel2 == "Rival")
		{
			chatWeights.AddWeightToElement("Argument", 30);
			chatWeights.AddWeightToElement("Insult", 30);
			chatWeights.AddWeightToElement("Praise", -30);
		}
		switch (compatibilityBetween)
		{
		case 0:
			chatWeights.AddWeightToElement("Awkward Chat", 15);
			chatWeights.AddWeightToElement("Argument", 20);
			chatWeights.AddWeightToElement("Insult", 15);
			break;
		case 1:
			chatWeights.AddWeightToElement("Awkward Chat", 10);
			chatWeights.AddWeightToElement("Argument", 10);
			chatWeights.AddWeightToElement("Insult", 10);
			break;
		case 2:
			chatWeights.AddWeightToElement("Awkward Chat", 5);
			chatWeights.AddWeightToElement("Argument", 5);
			chatWeights.AddWeightToElement("Insult", 5);
			break;
		case 3:
			chatWeights.AddWeightToElement("Praise", 5);
			break;
		case 4:
			chatWeights.AddWeightToElement("Praise", 10);
			break;
		case 5:
			chatWeights.AddWeightToElement("Praise", 20);
			break;
		}
		if (disguisedCharacter.traitContainer.HasTrait("Hothead"))
		{
			chatWeights.AddWeightToElement("Argument", 15);
		}
		if (character.traitContainer.HasTrait("Hothead"))
		{
			chatWeights.AddWeightToElement("Argument", 15);
		}
		if (disguisedCharacter.traitContainer.HasTrait("Diplomatic"))
		{
			chatWeights.AddWeightToElement("Insult", -30);
			chatWeights.AddWeightToElement("Praise", 30);
		}
		if (!disguisedCharacter.limiterComponent.isSociable)
		{
			chatWeights.AddWeightToElement("Warm Chat", -20);
			chatWeights.AddWeightToElement("Awkward Chat", 20);
			chatWeights.AddWeightToElement("Argument", 20);
			chatWeights.AddWeightToElement("Insult", 50);
			chatWeights.AddWeightToElement("Praise", -20);
		}
		if (!character.limiterComponent.isSociable)
		{
			chatWeights.AddWeightToElement("Warm Chat", -20);
			chatWeights.AddWeightToElement("Awkward Chat", 20);
			chatWeights.AddWeightToElement("Argument", 20);
		}
		Trait traitOrStatus = disguisedCharacter.traitContainer.GetTraitOrStatus<Trait>("Angry");
		Trait traitOrStatus2 = character.traitContainer.GetTraitOrStatus<Trait>("Angry");
		if (traitOrStatus != null && traitOrStatus.responsibleCharacters != null && traitOrStatus.responsibleCharacters.Contains(character))
		{
			chatWeights.AddWeightToElement("Warm Chat", -50);
			chatWeights.AddWeightToElement("Awkward Chat", 20);
			chatWeights.AddWeightToElement("Argument", 50);
			chatWeights.AddWeightToElement("Insult", 100);
			chatWeights.AddWeightToElement("Praise", -50);
		}
		if (traitOrStatus2 != null && traitOrStatus2.responsibleCharacters != null && traitOrStatus2.responsibleCharacters.Contains(disguisedCharacter))
		{
			chatWeights.AddWeightToElement("Warm Chat", -50);
			chatWeights.AddWeightToElement("Awkward Chat", 20);
			chatWeights.AddWeightToElement("Argument", 50);
		}
		if (disguisedCharacter.traitContainer.HasTrait("Hemophobic"))
		{
			Vampire traitOrStatus3 = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus3 != null && traitOrStatus3.DoesCharacterKnowThisVampire(disguisedCharacter))
			{
				chatWeights.AddWeightToElement("Warm Chat", -50);
				chatWeights.AddWeightToElement("Insult", 50);
			}
		}
		else if (disguisedCharacter.traitContainer.HasTrait("Hemophiliac"))
		{
			Vampire traitOrStatus4 = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus4 != null && traitOrStatus4.DoesCharacterKnowThisVampire(disguisedCharacter))
			{
				chatWeights.AddWeightToElement("Warm Chat", 50);
				chatWeights.AddWeightToElement("Praise", 50);
			}
		}
		if (character.traitContainer.HasTrait("Hemophobic"))
		{
			Vampire traitOrStatus5 = disguisedCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus5 != null && traitOrStatus5.DoesCharacterKnowThisVampire(character))
			{
				chatWeights.AddWeightToElement("Warm Chat", -50);
			}
		}
		else if (character.traitContainer.HasTrait("Hemophiliac"))
		{
			Vampire traitOrStatus6 = disguisedCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus6 != null && traitOrStatus6.DoesCharacterKnowThisVampire(character))
			{
				chatWeights.AddWeightToElement("Warm Chat", 50);
			}
		}
		if (disguisedCharacter.traitContainer.HasTrait("Lycanphobic"))
		{
			if (character.isLycanthrope && character.lycanData.DoesCharacterKnowThisLycan(disguisedCharacter))
			{
				chatWeights.AddWeightToElement("Warm Chat", -50);
				chatWeights.AddWeightToElement("Insult", 50);
			}
		}
		else if (disguisedCharacter.traitContainer.HasTrait("Lycanphiliac") && character.isLycanthrope && character.lycanData.DoesCharacterKnowThisLycan(disguisedCharacter))
		{
			chatWeights.AddWeightToElement("Warm Chat", 50);
			chatWeights.AddWeightToElement("Praise", 50);
		}
		if (character.traitContainer.HasTrait("Lycanphobic"))
		{
			if (disguisedCharacter.isLycanthrope && disguisedCharacter.lycanData.DoesCharacterKnowThisLycan(character))
			{
				chatWeights.AddWeightToElement("Warm Chat", -50);
			}
		}
		else if (character.traitContainer.HasTrait("Lycanphiliac") && disguisedCharacter.isLycanthrope && disguisedCharacter.lycanData.DoesCharacterKnowThisLycan(character))
		{
			chatWeights.AddWeightToElement("Warm Chat", 50);
		}
		if (disguisedCharacter.traitContainer.HasTrait("Hero") || character.traitContainer.HasTrait("Hero"))
		{
			chatWeights.RemoveElement("Argument");
		}
		string text = chatWeights.PickRandomElementGivenWeights();
		if (!string.IsNullOrEmpty(text))
		{
			bool flag = false;
			int opinionValue = 0;
			switch (text)
			{
			case "Warm Chat":
				opinionValue = 6;
				flag = true;
				break;
			case "Awkward Chat":
				opinionValue = -3;
				flag = true;
				break;
			case "Argument":
				opinionValue = -5;
				flag = true;
				break;
			case "Insult":
				opinionValue = -6;
				break;
			case "Praise":
				opinionValue = 6;
				break;
			}
			if (flag)
			{
				base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Conversations", opinionValue, "Disastrous_Conversation");
				target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", opinionValue, "Disastrous_Conversation");
			}
			else
			{
				target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", opinionValue);
			}
			if (base.owner.traitContainer.HasTrait("Plagued"))
			{
				Transmission<AirborneTransmission>.Instance.Transmit(base.owner, target, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
			}
			if (target.traitContainer.HasTrait("Plagued"))
			{
				Transmission<AirborneTransmission>.Instance.Transmit(target, base.owner, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
			}
			GameDate gameDate = GameManager.Instance.Today();
			if (overrideLog != null)
			{
				LogPool.Release(overrideLog);
			}
			overrideLog = GameManager.CreateNewLogUsingNewLocalization(gameDate, "Interrupt", "Interrupts_Table", "Chat " + text, LOG_TAG.Social);
			overrideLog.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			base.owner.SetIsConversing(state: true);
			target.SetIsConversing(state: true);
			gameDate.AddTicks(2);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				base.owner.SetIsConversing(state: false);
			}, base.owner);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				target.SetIsConversing(state: false);
			}, target);
			DisableChatAndFlirt();
			ScheduleChatAndFlirtEnable(4);
			target.nonActionEventsComponent.DisableChatAndFlirt();
			target.nonActionEventsComponent.ScheduleChatAndFlirtEnable(4);
		}
	}

	public void SetLastConversationDate(GameDate date)
	{
		lastConversationDate = date;
	}

	public void NormalBreakUp(Character p_target, string p_reasonKey)
	{
		RELATIONSHIP_TYPE relationshipFromParametersWith = base.owner.relationshipContainer.GetRelationshipFromParametersWith(p_target, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
		TriggerBreakUp(p_target, relationshipFromParametersWith, p_reasonKey);
	}

	private void TriggerBreakUp(Character target, RELATIONSHIP_TYPE relationship, string reasonKey)
	{
		RelationshipManager.Instance.RemoveRelationshipBetween(base.owner, target, relationship);
		if (!base.owner.traitContainer.HasTrait("Psychopath"))
		{
			base.owner.traitContainer.AddTrait(base.owner, "Heartbroken", target);
		}
		if (!target.traitContainer.HasTrait("Psychopath"))
		{
			target.traitContainer.AddTrait(target, "Heartbroken", base.owner);
		}
		RelationshipManager.Instance.CreateNewRelationshipBetween(base.owner, target, RELATIONSHIP_TYPE.EX_LOVER);
		Log log;
		if (!string.IsNullOrEmpty(reasonKey))
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", reasonKey);
			log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Break Up break_up_reason", LOG_TAG.Social, LOG_TAG.Life_Changes);
			if (!string.IsNullOrEmpty(localizedValue))
			{
				log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			}
			else
			{
				log.AddToFillers(null, reasonKey, LOG_IDENTIFIER.STRING_1);
			}
		}
		else
		{
			log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Break Up break_up", LOG_TAG.Social, LOG_TAG.Life_Changes);
		}
		log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		base.owner.logComponent.RegisterLog(log, releaseAfter: true);
		if (relationship == RELATIONSHIP_TYPE.LOVER)
		{
			base.owner.MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement: false);
		}
	}

	public bool CheckForFlirtTrigger(Character p_actor, Character p_target, bool isOnSight, ref string debugLog, out JobQueueItem producedJob)
	{
		if (p_actor.moodComponent.moodState == MOOD_STATE.Normal && RelationshipManager.Instance.IsCompatibleBasedOnSexualityAndOpinion(p_actor, p_target) && p_actor.limiterComponent.isSociable && p_actor.nonActionEventsComponent.CanFlirt(p_actor, p_target))
		{
			int compatibilityBetween = RelationshipManager.Instance.GetCompatibilityBetween(p_actor, p_target);
			int chance = ChanceData.GetChance(CHANCE_TYPE.Flirt_Base_Chance);
			float num = ((compatibilityBetween != -1) ? ((float)(chance * compatibilityBetween)) : ((p_target.isNormalCharacter && p_target.race != RACE.RATMAN) ? ((float)(chance * 2)) : ((!p_actor.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.UNFAITHFULNESS) || !PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.UNFAITHFULNESS).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Wild_Multiple_Affair)) ? 0.5f : ((float)chance))));
			if (p_actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER) == p_target)
			{
				num *= 0.2f;
			}
			else if (p_actor.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))
			{
				num /= 2f;
			}
			else if (!isOnSight)
			{
				num *= 10f;
			}
			if (GameUtilities.RollChance(num, ref debugLog))
			{
				if (p_actor.behaviourComponent.canFlirtOnActivePartyQuest && p_actor.partyComponent.isMemberThatJoinedQuest)
				{
					p_actor.behaviourComponent.SetCanFlirtOnActivePartyQuest(p_state: false);
				}
				p_actor.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, p_target);
			}
		}
		producedJob = null;
		return false;
	}

	public bool NormalFlirtCharacter(Character target, InterruptHolder p_interrupt, ref Log overrideLog)
	{
		Character disguisedCharacter = base.owner;
		Character character = target;
		if (base.owner.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = base.owner.reactionComponent.disguisedCharacter;
		}
		if (target.reactionComponent.disguisedCharacter != null)
		{
			character = target.reactionComponent.disguisedCharacter;
		}
		if (!disguisedCharacter.IsHostileWith(character) || character.combatComponent.combatMode == COMBAT_MODE.Passive)
		{
			string text = TriggerFlirtCharacter(target);
			p_interrupt.SetIdentifier(text);
			if (base.owner.traitContainer.HasTrait("Plagued"))
			{
				Transmission<AirborneTransmission>.Instance.Transmit(base.owner, target, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
			}
			if (target.traitContainer.HasTrait("Plagued"))
			{
				Transmission<AirborneTransmission>.Instance.Transmit(target, base.owner, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
			}
			GameDate gameDate = GameManager.Instance.Today();
			if (overrideLog != null)
			{
				LogPool.Release(overrideLog);
			}
			if (character.raceSetting.category == CHARACTER_CATEGORY.Beast || character.raceSetting.category == CHARACTER_CATEGORY.Undead)
			{
				overrideLog = GameManager.CreateNewLogUsingNewLocalization(gameDate, "Interrupt", "Interrupts_Table", "Flirt flirted_back_beast_or_undead", LOG_TAG.Social);
			}
			else
			{
				overrideLog = GameManager.CreateNewLogUsingNewLocalization(gameDate, "Interrupt", "Interrupts_Table", "Flirt " + text, LOG_TAG.Social);
			}
			overrideLog.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			base.owner.SetIsConversing(state: true);
			target.SetIsConversing(state: true);
			gameDate.AddTicks(2);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				base.owner.SetIsConversing(state: false);
			}, base.owner);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				target.SetIsConversing(state: false);
			}, target);
			DisableChatAndFlirt();
			ScheduleChatAndFlirtEnable(4);
			target.nonActionEventsComponent.DisableChatAndFlirt();
			target.nonActionEventsComponent.ScheduleChatAndFlirtEnable(4);
			return true;
		}
		return false;
	}

	private string TriggerFlirtCharacter(Character target)
	{
		Character disguisedCharacter = base.owner;
		Character character = target;
		if (base.owner.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = base.owner.reactionComponent.disguisedCharacter;
		}
		if (target.reactionComponent.disguisedCharacter != null)
		{
			character = target.reactionComponent.disguisedCharacter;
		}
		bool flag = false;
		Obsessed traitOrStatus = character.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
		if (traitOrStatus != null && traitOrStatus.targetCharacter == disguisedCharacter)
		{
			flag = true;
		}
		bool flag2 = character.raceSetting.category == CHARACTER_CATEGORY.Beast || character.raceSetting.category == CHARACTER_CATEGORY.Undead;
		if (!flag && !flag2)
		{
			bool flag3 = false;
			Obsessed traitOrStatus2 = disguisedCharacter.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
			if (traitOrStatus2 != null && traitOrStatus2.targetCharacter == character)
			{
				flag3 = true;
			}
			int num = Random.Range(0, 100);
			if (num < 50 && disguisedCharacter.traitContainer.HasTrait("Unattractive"))
			{
				int p_totalOpinionReduction = -6;
				string p_lastStrawReasonKey = "Disastrous_Flirting";
				if (flag3)
				{
					CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, disguisedCharacter, character, ref p_totalOpinionReduction, ref p_lastStrawReasonKey);
				}
				base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Rebuffed_Courtship", p_totalOpinionReduction, p_lastStrawReasonKey);
				target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", -3, "Disastrous_Flirting");
				return "ugly";
			}
			if (!character.limiterComponent.isSociable)
			{
				int p_totalOpinionReduction2 = -6;
				string p_lastStrawReasonKey2 = "Disastrous_Flirting";
				if (flag3)
				{
					CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, disguisedCharacter, character, ref p_totalOpinionReduction2, ref p_lastStrawReasonKey2);
				}
				base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Rebuffed_Courtship", p_totalOpinionReduction2, p_lastStrawReasonKey2);
				target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", -3, "Disastrous_Flirting");
				return "unsociable";
			}
			if (character.traitContainer.HasTrait("Hemophobic"))
			{
				Vampire traitOrStatus3 = disguisedCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus3 != null && traitOrStatus3.DoesCharacterKnowThisVampire(character))
				{
					int p_totalOpinionReduction3 = -6;
					string p_lastStrawReasonKey3 = "Disastrous_Flirting";
					if (flag3)
					{
						CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, disguisedCharacter, character, ref p_totalOpinionReduction3, ref p_lastStrawReasonKey3);
					}
					base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Rebuffed_Courtship", p_totalOpinionReduction3, p_lastStrawReasonKey3);
					target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", -3, "Disastrous_Flirting");
					return "vampire";
				}
			}
			if (character.traitContainer.HasTrait("Lycanphobic") && disguisedCharacter.isLycanthrope && disguisedCharacter.lycanData.DoesCharacterKnowThisLycan(character))
			{
				int p_totalOpinionReduction4 = -6;
				string p_lastStrawReasonKey4 = "Disastrous_Flirting";
				if (flag3)
				{
					CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, disguisedCharacter, character, ref p_totalOpinionReduction4, ref p_lastStrawReasonKey4);
				}
				base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Rebuffed_Courtship", p_totalOpinionReduction4, p_lastStrawReasonKey4);
				target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", -3, "Disastrous_Flirting");
				return "werewolf";
			}
			if (num < 70)
			{
				Trait traitOrStatus4 = character.traitContainer.GetTraitOrStatus<Trait>("Angry");
				if (traitOrStatus4?.responsibleCharacters != null && traitOrStatus4.responsibleCharacters.Contains(disguisedCharacter))
				{
					int p_totalOpinionReduction5 = -6;
					string p_lastStrawReasonKey5 = "Disastrous_Flirting";
					if (flag3)
					{
						CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, disguisedCharacter, character, ref p_totalOpinionReduction5, ref p_lastStrawReasonKey5);
					}
					base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Rebuffed_Courtship", p_totalOpinionReduction5, p_lastStrawReasonKey5);
					target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", -3, "Disastrous_Flirting");
					return "angry";
				}
			}
			if (num < 90 && !(disguisedCharacter.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful")?.IsCompatibleBasedOnSexualityAndOpinions(disguisedCharacter, character) ?? RelationshipManager.IsSexuallyCompatibleOneSided(character, disguisedCharacter)))
			{
				int p_totalOpinionReduction6 = -6;
				string p_lastStrawReasonKey6 = "Disastrous_Flirting";
				if (flag3)
				{
					CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, disguisedCharacter, character, ref p_totalOpinionReduction6, ref p_lastStrawReasonKey6);
				}
				base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Rebuffed_Courtship", p_totalOpinionReduction6, p_lastStrawReasonKey6);
				target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", -3, "Disastrous_Flirting");
				return "incompatible";
			}
		}
		base.owner.relationshipContainer.AdjustOpinion(base.owner, character, "Reciprocated_Courtship", 6);
		target.relationshipContainer.AdjustOpinion(target, disguisedCharacter, "Conversations", 10);
		CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, disguisedCharacter, character);
		if (flag)
		{
			CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, character, disguisedCharacter);
		}
		return "flirted_back";
	}

	public void CreateRelationshipBasedOnFlirtResult(string p_result, Character target)
	{
		if (!p_result.Equals("flirted_back"))
		{
			return;
		}
		Character disguisedCharacter = base.owner;
		Character character = target;
		bool flag = false;
		bool flag2 = false;
		if (base.owner.reactionComponent.disguisedCharacter != null)
		{
			flag = true;
			disguisedCharacter = base.owner.reactionComponent.disguisedCharacter;
		}
		if (target.reactionComponent.disguisedCharacter != null)
		{
			flag2 = true;
			character = target.reactionComponent.disguisedCharacter;
		}
		string opinionLabel = disguisedCharacter.relationshipContainer.GetOpinionLabel(character);
		if (flag || flag2 || disguisedCharacter.faction != character.faction)
		{
			return;
		}
		if (!string.IsNullOrEmpty(opinionLabel))
		{
			switch (opinionLabel)
			{
			default:
				return;
			case "Acquaintance":
				break;
			case "Friend":
			case "Close Friend":
				if (disguisedCharacter.relationshipValidator.CanHaveRelationship(disguisedCharacter, character, RELATIONSHIP_TYPE.LOVER) && character.relationshipValidator.CanHaveRelationship(character, disguisedCharacter, RELATIONSHIP_TYPE.LOVER))
				{
					if (ChanceData.RollChance(CHANCE_TYPE.Flirt_Friend_Become_Lover_Chance))
					{
						RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedCharacter, character, RELATIONSHIP_TYPE.LOVER);
					}
				}
				else if (disguisedCharacter.relationshipValidator.CanHaveRelationship(disguisedCharacter, character, RELATIONSHIP_TYPE.AFFAIR) && character.relationshipValidator.CanHaveRelationship(character, disguisedCharacter, RELATIONSHIP_TYPE.AFFAIR) && ChanceData.RollChance(CHANCE_TYPE.Flirt_Friend_Become_Affair_Chance))
				{
					RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedCharacter, character, RELATIONSHIP_TYPE.AFFAIR);
				}
				return;
			}
		}
		if (disguisedCharacter.relationshipValidator.CanHaveRelationship(disguisedCharacter, character, RELATIONSHIP_TYPE.LOVER) && character.relationshipValidator.CanHaveRelationship(character, disguisedCharacter, RELATIONSHIP_TYPE.LOVER))
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Flirt_Acquaintance_Become_Lover_Chance))
			{
				RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedCharacter, character, RELATIONSHIP_TYPE.LOVER);
			}
		}
		else if (disguisedCharacter.relationshipValidator.CanHaveRelationship(disguisedCharacter, character, RELATIONSHIP_TYPE.AFFAIR) && character.relationshipValidator.CanHaveRelationship(character, disguisedCharacter, RELATIONSHIP_TYPE.AFFAIR) && ChanceData.RollChance(CHANCE_TYPE.Flirt_Acquaintance_Become_Affair_Chance))
		{
			RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedCharacter, character, RELATIONSHIP_TYPE.AFFAIR);
		}
	}

	public void LoadReferences(SaveDataNonActionEventsComponent data)
	{
		if (chatOrFlirtReenableDate.hasValue)
		{
			SchedulingManager.Instance.AddEntry(chatOrFlirtReenableDate, EnableChatAndFlirt, base.owner);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
