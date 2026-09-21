using System.Collections.Generic;
using Locations.Settlements;
using Object_Pools;
using UtilityScripts;

public class RebellionData : SchemeData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REBELLION;

	public override string name => "Rebellion";

	public override string description => "Convince a Settlement Ruler to split off their entire Village from current Faction.";

	public override string verbName => "Rebel";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SCHEME;

	public RebellionData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character p_targetCharacter)
		{
			UIManager.Instance.ShowSchemeUI(p_targetCharacter, null, this);
		}
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (WorldSettings.Instance.worldSettingsData.factionSettings.disableNewFactions)
		{
			return false;
		}
		return base.IsValid(target);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions)
			{
				return false;
			}
			if (targetCharacter.faction != null && !targetCharacter.isFactionLeader && targetCharacter.isSettlementRuler && targetCharacter.faction.HasOwnedSettlementThatHasAliveResidentAndIsNotHomeOf(targetCharacter))
			{
				return true;
			}
			return false;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.isFactionLeader || !targetCharacter.isSettlementRuler)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Rebellion_Not_Faction_Leader") + "|";
		}
		if (targetCharacter.faction == null || !targetCharacter.faction.HasOwnedSettlementThatHasAliveResidentAndIsNotHomeOf(targetCharacter))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Rebellion_No_Faction") + "|";
		}
		if (FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Max_Factions") + "|";
		}
		return text;
	}

	protected override void OnSuccessScheme(Character character, object target)
	{
		base.OnSuccessScheme(character, target);
		Faction faction = character.faction;
		if (faction.leader is Character character2)
		{
			character2.traitContainer.AddTrait(character2, "Betrayed", character);
			character2.relationshipContainer.AdjustOpinion(character2, character, "Rebellion", -100);
			character.relationshipContainer.AdjustOpinion(character, character2, "Rebellion", -100);
			if (!character2.relationshipContainer.HasGrudgeAgainst(character))
			{
				character2.relationshipContainer.SetHasGrudgeAgainst(character2, character, p_state: true);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Relationships", "Relationships_Table", "Grudge_Rebellion", LOG_TAG.Life_Changes);
				log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
		}
		BaseSettlement homeSettlement = character.homeSettlement;
		if (character.traitContainer.IsReligiousCultist(out var p_religion))
		{
			bool flag = false;
			if (FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion) == null)
			{
				flag = true;
				FactionManager.Instance.CreateReligiousCultFactionForReligion(p_religion);
			}
			Faction religiousCultFactionForReligion = FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion);
			if (flag || (religiousCultFactionForReligion.CanCharacterJoinFactionBasedOnNonReligionIdeologiesAndBanning(character) && faction != religiousCultFactionForReligion))
			{
				if (homeSettlement != null)
				{
					LandmarkManager.Instance.OwnSettlement(religiousCultFactionForReligion, homeSettlement);
				}
				character.ChangeFactionTo(religiousCultFactionForReligion, bypassIdeologyChecking: true);
				if (flag || !religiousCultFactionForReligion.HasAliveMemberExcept(character))
				{
					religiousCultFactionForReligion.SetLeader(character);
				}
				if (homeSettlement != null)
				{
					List<Character> list = RuinarchListPool<Character>.Claim();
					if (homeSettlement.residents.Count > 0)
					{
						list.AddRange(homeSettlement.residents);
					}
					for (int i = 0; i < list.Count; i++)
					{
						Character character3 = list[i];
						if (character3.faction != character.faction && character != character3 && character3.petComponent.petOwner == null)
						{
							character3.interruptComponent.TriggerInterrupt(INTERRUPT.Evaluate_Cultist_Affiliation, character, p_religion.ToStringEnum());
						}
					}
					RuinarchListPool<Character>.Release(list);
				}
				religiousCultFactionForReligion.SetRelationshipFor(faction, FACTION_RELATIONSHIP_STATUS.Hostile);
				if (flag)
				{
					Messenger.Broadcast(FactionSignals.CREATE_FACTION_INTERRUPT, religiousCultFactionForReligion, character);
				}
			}
			else
			{
				NormalRebellionProcessing(character, homeSettlement, faction);
			}
		}
		else
		{
			NormalRebellionProcessing(character, homeSettlement, faction);
		}
	}

	private void NormalRebellionProcessing(Character p_character, BaseSettlement p_settlement, Faction p_previousFaction)
	{
		p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, p_character, "own_settlement");
		if (p_settlement == null)
		{
			return;
		}
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(p_settlement.residents);
		for (int i = 0; i < list.Count; i++)
		{
			Character character = list[i];
			if (character.faction == p_character.faction || p_character == character)
			{
				continue;
			}
			if (p_character.faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(character))
			{
				character.ChangeFactionTo(p_character.faction);
				continue;
			}
			BaseSettlement randomElement = CollectionUtilities.GetRandomElement(p_previousFaction.ownedSettlements);
			if (randomElement != null)
			{
				character.MigrateHomeTo(randomElement);
			}
			else
			{
				character.MigrateHomeTo(null);
			}
		}
		RuinarchListPool<Character>.Release(list);
	}

	protected override void PopulateSchemeConversation(List<ConversationData> conversationList, Character character, object target, bool isSuccessful)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Rebellion_Message");
		log.AddToFillers(character.faction, character.faction.name, LOG_IDENTIFIER.FACTION_1);
		string logText = log.logText;
		LogPool.Release(log);
		ConversationData item = ObjectPoolManager.Instance.CreateNewConversationData(logText, null, DialogItem.Position.Right);
		conversationList.Add(item);
		base.PopulateSchemeConversation(conversationList, character, target, isSuccessful);
	}

	public override void ProcessSuccessRateWithMultipliers(Character p_targetCharacter, object p_otherTarget, ref float p_newSuccessRate)
	{
		p_newSuccessRate *= 0.5f;
		if (p_targetCharacter.faction != null && p_targetCharacter.faction.leader != null && p_targetCharacter.faction.leader is Character character && p_targetCharacter != character)
		{
			if (p_targetCharacter.relationshipContainer.IsFriendsWith(character))
			{
				p_newSuccessRate *= 0.2f;
			}
			else if (p_targetCharacter.relationshipContainer.IsEnemiesWith(character))
			{
				p_newSuccessRate *= 3f;
			}
		}
		if (p_targetCharacter.traitContainer.HasTrait("Treacherous"))
		{
			p_newSuccessRate *= 2f;
		}
		base.ProcessSuccessRateWithMultipliers(p_targetCharacter, p_otherTarget, ref p_newSuccessRate);
	}

	public override string GetSuccessRateMultiplierText(Character p_targetCharacter, object p_otherTarget)
	{
		string empty = string.Empty;
		empty = empty + LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Rebellion") + ": <color=white>x0.5</color>";
		if (p_targetCharacter.faction != null && p_targetCharacter.faction.leader != null && p_targetCharacter.faction.leader is Character character && p_targetCharacter != character)
		{
			if (p_targetCharacter.relationshipContainer.IsFriendsWith(character))
			{
				if (empty != string.Empty)
				{
					empty += "\n";
				}
				empty = empty + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Friends_With_Leader") + ": <color=white>x0.2</color>";
			}
			else if (p_targetCharacter.relationshipContainer.IsEnemiesWith(character))
			{
				if (empty != string.Empty)
				{
					empty += "\n";
				}
				empty = empty + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", "Enemies_With_Leader") + ": <color=white>x3</color>";
			}
		}
		if (p_targetCharacter.traitContainer.HasTrait("Treacherous"))
		{
			if (empty != string.Empty)
			{
				empty += "\n";
			}
			empty = empty + p_targetCharacter.visuals.GetCharacterNameWithIconAndColor() + " - " + TraitManager.Instance.GetLocalizedNameOfTrait("Treacherous") + ": <color=white>x2</color>";
		}
		if (empty != string.Empty)
		{
			return empty;
		}
		return base.GetSuccessRateMultiplierText(p_targetCharacter, p_otherTarget);
	}
}
