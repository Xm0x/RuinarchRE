using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using UnityEngine;
using UtilityScripts;

public class FoundCultData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FOUND_CULT;

	public override string name => "Found Cult";

	public override string description => "This Ability instructs the Cult Leader to start a new Demon Cult faction. Available only on Cult Leaders.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public override bool canBeCastOnBlessed => true;

	public FoundCultData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (!(targetPOI is Character character))
		{
			return;
		}
		character.MigrateHomeStructureTo(null);
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character, "create_religious_cult");
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, targetPOI as IPlayerActionTarget);
		if (WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages)
		{
			return;
		}
		if (!character.currentRegion.IsRegionVillageCapacityReached())
		{
			VillageSpot firstUnoccupiedVillageSpotThatCanAccomodateFaction = character.currentRegion.GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(FACTION_TYPE.Demon_Cult);
			if (firstUnoccupiedVillageSpotThatCanAccomodateFaction != null)
			{
				Area coreSpot = firstUnoccupiedVillageSpotThatCanAccomodateFaction.coreSpot;
				StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, character.faction.factionType.mainResource);
				GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(character.faction.factionType.type, structureSetting));
				if (LandmarkManager.Instance.HasEnoughSpaceForStructure(randomElement.name, coreSpot.gridTileComponent.centerGridTile))
				{
					character.jobComponent.TriggerFindNewVillage(coreSpot.gridTileComponent.centerGridTile, randomElement.name);
				}
			}
		}
		else
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Flaw_Failed");
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("actorName", character.visuals.GetCharacterNameWithIconAndColor());
			dictionary.Add("factionName", character.faction.nameWithColor);
			dictionary.Add("regionName", character.faction.nameWithColor);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Village_Capacity_Reached_Description", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, localizedValue2);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.isFactionLeader)
			{
				return false;
			}
			if (targetCharacter.faction != null && targetCharacter.faction.factionType.type == FACTION_TYPE.Demon_Cult)
			{
				return false;
			}
			if (targetCharacter.traitContainer.HasTrait("Enslaved"))
			{
				return false;
			}
			if (FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.isFactionLeader)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Faction_Leader") + "|";
		}
		if (targetCharacter.faction != null && targetCharacter.faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Demon_Cult") + "|";
		}
		if (targetCharacter.traitContainer.HasTrait("Enslaved"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Slave_Cannot_Perform") + "|";
		}
		if (FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Max_Factions") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (character.isDead)
			{
				return false;
			}
			if (character.characterClass.className != "Demon Cult Leader")
			{
				return false;
			}
			if (FactionManager.Instance.demonCultFaction != null)
			{
				return false;
			}
			if (WorldSettings.Instance.worldSettingsData.factionSettings.disableNewFactions)
			{
				return false;
			}
		}
		return base.IsValid(target);
	}
}
