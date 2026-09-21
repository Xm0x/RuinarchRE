using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Interrupts;
using Traits;
using UnityEngine.Localization.Settings;
using UtilityScripts;

public class InterruptTriggerBehaviour : CharacterBehaviour
{
	private int _currentInterruptIndex;

	private readonly List<INTERRUPT> _interrupts;

	public InterruptTriggerBehaviour()
	{
		base.priority = 42;
		_currentInterruptIndex = 0;
		_interrupts = CollectionUtilities.GetEnumValues<INTERRUPT>().ToList();
		_interrupts.Remove(INTERRUPT.None);
		_interrupts.Remove(INTERRUPT.Being_Tortured);
		_interrupts.Remove(INTERRUPT.Being_Brainwashed);
		_interrupts.Remove(INTERRUPT.Create_Party);
		_interrupts.Remove(INTERRUPT.Join_Party);
		_interrupts.Remove(INTERRUPT.Removed_From_Party);
		_interrupts.Remove(INTERRUPT.Set_Home_Ratman);
		_interrupts.Remove(INTERRUPT.Left_Party);
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.interruptComponent.isInterrupted)
		{
			producedJob = null;
			return true;
		}
		if (!_interrupts.IsIndexInList(_currentInterruptIndex))
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(InterruptTriggerBehaviour));
			UIManager.Instance.Pause();
			producedJob = null;
			return false;
		}
		INTERRUPT iNTERRUPT = _interrupts[_currentInterruptIndex];
		switch (iNTERRUPT)
		{
		case INTERRUPT.Accident:
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Accident, character, "", null, InteractionManager.Instance.goapActionData[INTERACTION_TYPE.CRAFT_EQUIPMENT].localizedName);
			_currentInterruptIndex++;
			break;
		case INTERRUPT.Cowering:
		{
			string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", "Got_Scared");
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, localizedValue3);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Join_Faction:
		{
			List<Faction> list = FactionManager.Instance.allFactions.Where((Faction f) => f.isMajorNonPlayer && f != character.faction).ToList();
			if (list.Count > 0)
			{
				Character randomElement6 = CollectionUtilities.GetRandomElement(CollectionUtilities.GetRandomElement(list).characters);
				if (randomElement6 != null)
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, randomElement6, "join_faction_normal");
					_currentInterruptIndex++;
				}
			}
			break;
		}
		case INTERRUPT.Leave_Faction:
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
			_currentInterruptIndex++;
			break;
		case INTERRUPT.Puke:
		{
			Character randomElement4 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character c) => c != character));
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, randomElement4, "", null, "Saw_Dead");
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Shocked:
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, character, "", null, "Shocked_Witness_Reason");
			_currentInterruptIndex++;
			break;
		case INTERRUPT.Stopped:
		{
			Character randomElement5 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character c) => c != character));
			GoapAction goapAction = InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BURY_CHARACTER];
			Log log7 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Stopped effect_with_action", LOG_TAG.Social);
			log7.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log7.AddToFillers(randomElement5, randomElement5.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log7.AddToFillers(null, goapAction.localizedName, LOG_IDENTIFIER.STRING_1);
			log7.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Cry:
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", "Cry_Suicidal");
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Cry cry_self", LOG_TAG.Social);
			log2.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			log2.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Surprised:
		{
			Character randomElement3 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character c) => c != character));
			Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Surprised effect", LOG_TAG.Social);
			log4.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log4.AddToFillers(randomElement3, randomElement3.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", "Shocked_Copycat_Reason");
			log4.AddToFillers(null, localizedValue2, LOG_IDENTIFIER.STRING_1);
			log4.AddLogToDatabase();
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Panicking:
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Panicking, character, "", null, "Panicking_Fire");
			_currentInterruptIndex++;
			break;
		case INTERRUPT.Evaluate_Cultist_Affiliation:
		{
			Faction faction = FactionManager.Instance.CreateReligiousCultFactionForReligion(RELIGION.Demon_Worship);
			Log log15 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Evaluate Cultist Affiliation leave", LOG_TAG.Life_Changes);
			log15.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log15.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			log15.AddLogToDatabase(releaseLogAfter: true);
			Log log16 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Evaluate Cultist Affiliation join", LOG_TAG.Life_Changes);
			log16.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log16.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			log16.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Declare_War:
		{
			List<Faction> list2 = FactionManager.Instance.allFactions.Where((Faction f) => f.isMajorNonPlayer && f != character.faction).ToList();
			if (list2.Count > 0)
			{
				Faction randomElement7 = CollectionUtilities.GetRandomElement(list2);
				Log log9 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Declare War effect", LOG_TAG.Major);
				log9.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log9.AddToFillers(character.faction, character.faction.name, LOG_IDENTIFIER.FACTION_1);
				log9.AddToFillers(randomElement7, randomElement7.name, LOG_IDENTIFIER.FACTION_2);
				log9.AddLogToDatabase(releaseLogAfter: true);
				_currentInterruptIndex++;
			}
			break;
		}
		case INTERRUPT.Pass_Out:
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Pass_Out, character, "", null, "Pass_Out_Coward");
			_currentInterruptIndex++;
			break;
		case INTERRUPT.Buy_Home:
		{
			LocationStructure randomStructureOfType = InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.DWELLING);
			Log log8 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Buy Home buy_new_home_structure", LOG_TAG.Life_Changes);
			log8.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log8.AddToFillers(randomStructureOfType, randomStructureOfType.name, LOG_IDENTIFIER.LANDMARK_1);
			log8.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Hunter_Specialization:
		{
			Log log5 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Hunter Specialization specialized", LOG_TAG.Combat);
			log5.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string localizedValue4;
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Latin America) (es)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)"))
			{
				localizedValue4 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Humanoid");
				log5.AddToFillers(null, Utilities.ForcePluralizeString(localizedValue4), LOG_IDENTIFIER.STRING_1);
			}
			else
			{
				localizedValue4 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Humanoid_Plural");
				if (string.IsNullOrEmpty(localizedValue4))
				{
					localizedValue4 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Humanoid");
				}
				log5.AddToFillers(null, localizedValue4, LOG_IDENTIFIER.STRING_1);
			}
			log5.AddToFillers(null, Utilities.PluralizeString(localizedValue4), LOG_IDENTIFIER.STRING_1);
			log5.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Resign:
		{
			NPCSettlement randomElement11 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements);
			Faction randomElement12 = CollectionUtilities.GetRandomElement(FactionManager.Instance.allFactions.Where((Faction f) => f.isMajorNonPlayer));
			Log log20 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Resign resign_both", LOG_TAG.Major);
			log20.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log20.AddToFillers(randomElement12, randomElement12.name, LOG_IDENTIFIER.FACTION_1);
			log20.AddToFillers(randomElement11, randomElement11.name, LOG_IDENTIFIER.LANDMARK_1);
			log20.AddLogToDatabase(releaseLogAfter: true);
			Log log21 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Resign resign_faction_leader", LOG_TAG.Major);
			log21.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log21.AddToFillers(randomElement12, randomElement12.name, LOG_IDENTIFIER.FACTION_1);
			log21.AddLogToDatabase(releaseLogAfter: true);
			Log log22 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Resign resign_ruler", LOG_TAG.Major);
			log22.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log22.AddToFillers(randomElement11, randomElement11.name, LOG_IDENTIFIER.LANDMARK_1);
			log22.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Claim_Work_Structure:
		{
			LocationStructure randomStructureOfType2 = InnerMapManager.Instance.currentlyShowingLocation.GetRandomStructureOfType(STRUCTURE_TYPE.MINE);
			if (randomStructureOfType2 != null)
			{
				Log log19 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Claim Work Structure set_work_structure", LOG_TAG.Life_Changes);
				log19.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log19.AddToFillers(randomStructureOfType2, randomStructureOfType2.name, LOG_IDENTIFIER.LANDMARK_1);
				log19.AddLogToDatabase(releaseLogAfter: true);
				_currentInterruptIndex++;
			}
			break;
		}
		case INTERRUPT.Become_Settlement_Ruler:
		{
			NPCSettlement randomElement10 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements);
			Log log18 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Become Settlement Ruler became_ruler", LOG_TAG.Major);
			log18.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log18.AddToFillers(randomElement10, randomElement10.name, LOG_IDENTIFIER.LANDMARK_1);
			log18.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Order_Attack:
		{
			NPCSettlement randomElement9 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements);
			Log log17 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Order Attack effect", LOG_TAG.Combat, LOG_TAG.Work);
			log17.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log17.AddToFillers(randomElement9, randomElement9.name, LOG_IDENTIFIER.LANDMARK_1);
			log17.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Become_Faction_Leader:
		{
			Faction randomElement8 = CollectionUtilities.GetRandomElement(FactionManager.Instance.allFactions.Where((Faction f) => f.isMajorNonPlayer));
			FactionManager.Instance.CreateReligiousCultFactionForReligion(RELIGION.Demon_Worship);
			Faction religiousCultFactionForReligion = FactionManager.Instance.GetReligiousCultFactionForReligion(RELIGION.Demon_Worship);
			Log log10 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Become Faction Leader convert_to_cult", LOG_TAG.Major);
			log10.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log10.AddToFillers(randomElement8, randomElement8.name, LOG_IDENTIFIER.FACTION_1);
			log10.AddToFillers(religiousCultFactionForReligion, religiousCultFactionForReligion.name, LOG_IDENTIFIER.FACTION_2);
			log10.AddToFillers(null, RELIGION.Demon_Worship.GetCultistTraitNameForReligion(), LOG_IDENTIFIER.STRING_1);
			log10.AddLogToDatabase(releaseLogAfter: true);
			Log log11 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Become Faction Leader merged_with_cult", LOG_TAG.Major);
			log11.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log11.AddToFillers(randomElement8, randomElement8.name, LOG_IDENTIFIER.FACTION_1);
			log11.AddToFillers(religiousCultFactionForReligion, religiousCultFactionForReligion.name, LOG_IDENTIFIER.FACTION_2);
			log11.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait(RELIGION.Demon_Worship.GetCultistTraitNameForReligion()), LOG_IDENTIFIER.STRING_1);
			log11.AddLogToDatabase(releaseLogAfter: true);
			Log log12 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "cannot_join_demon_cult", LOG_TAG.Major);
			log12.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log12.AddToFillers(randomElement8, randomElement8.name, LOG_IDENTIFIER.FACTION_1);
			log12.AddToFillers(religiousCultFactionForReligion, religiousCultFactionForReligion.name, LOG_IDENTIFIER.STRING_1);
			log12.AddLogToDatabase(releaseLogAfter: true);
			Log log13 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "ideology_change", LOG_TAG.Life_Changes);
			log13.AddToFillers(randomElement8, randomElement8.name, LOG_IDENTIFIER.FACTION_1);
			log13.AddLogToDatabase(releaseLogAfter: true);
			Log log14 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Become Faction Leader became_leader", LOG_TAG.Major);
			log14.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log14.AddToFillers(randomElement8, randomElement8.name, LOG_IDENTIFIER.FACTION_1);
			log14.AddLogToDatabase(releaseLogAfter: true);
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Become_Lycanthrope:
		{
			Interrupt interruptData3 = InteractionManager.Instance.GetInterruptData(iNTERRUPT);
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WEREWOLF_PELT);
			character.gridTileLocation.structure.AddPOI(tileObject, character.gridTileLocation);
			interruptData3.CreateEffectLog(character, tileObject)?.AddLogToDatabase();
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Septic_Shock:
		case INTERRUPT.Zombie_Death:
		case INTERRUPT.Ingested_Poison:
		case INTERRUPT.Abomination_Death:
		case INTERRUPT.Necromantic_Transformation:
		case INTERRUPT.Set_Lair:
		case INTERRUPT.Recall_Attack:
		case INTERRUPT.Heatstroke_Death:
		case INTERRUPT.Seizure:
		case INTERRUPT.Become_Vampire_Lord:
		case INTERRUPT.Burning_At_Stake:
		case INTERRUPT.Transform_To_Werewolf:
		case INTERRUPT.Revert_From_Werewolf:
		case INTERRUPT.Heart_Attack:
		case INTERRUPT.Stroke:
		case INTERRUPT.Total_Organ_Failure:
		case INTERRUPT.Pneumonia:
		case INTERRUPT.Hypothermia_Death:
		{
			Interrupt interruptData2 = InteractionManager.Instance.GetInterruptData(iNTERRUPT);
			Log log6 = interruptData2.CreateEffectLog(character, character);
			if (interruptData2.shouldAddLogs || interruptData2.shouldShowNotif)
			{
				log6?.AddLogToDatabase();
			}
			_currentInterruptIndex++;
			break;
		}
		case INTERRUPT.Flirt:
		case INTERRUPT.Laugh_At:
		case INTERRUPT.Mock:
		case INTERRUPT.Angered:
		case INTERRUPT.Inspired:
		case INTERRUPT.Feared:
		case INTERRUPT.Worried:
		case INTERRUPT.Feeling_Angry:
		case INTERRUPT.Wary:
		case INTERRUPT.Pulled_Down:
		case INTERRUPT.Taunted:
		case INTERRUPT.Angry_Stare:
		case INTERRUPT.Tame_Beast:
		case INTERRUPT.Heal_Other:
		case INTERRUPT.Feeling_Anxious:
		{
			Character randomElement2 = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character c) => c != character));
			if (randomElement2 != null)
			{
				Interrupt interruptData = InteractionManager.Instance.GetInterruptData(iNTERRUPT);
				Log log3 = interruptData.CreateEffectLog(character, randomElement2);
				if (interruptData.shouldAddLogs || interruptData.shouldShowNotif)
				{
					log3?.AddLogToDatabase();
				}
				_currentInterruptIndex++;
			}
			break;
		}
		case INTERRUPT.Break_Up:
		case INTERRUPT.Chat:
		case INTERRUPT.Reduce_Conflict:
		{
			Character randomElement = CollectionUtilities.GetRandomElement(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Where((Character c) => c != character));
			if (randomElement != null)
			{
				character.interruptComponent.TriggerInterrupt(iNTERRUPT, randomElement);
				_currentInterruptIndex++;
			}
			break;
		}
		default:
			character.interruptComponent.TriggerInterrupt(iNTERRUPT, character);
			_currentInterruptIndex++;
			break;
		}
		producedJob = null;
		return true;
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
	}

	private void OnCharacterGainedTrait(Character p_character, Trait p_trait)
	{
	}
}
