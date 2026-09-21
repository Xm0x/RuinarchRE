using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Kleptomaniac : Trait
{
	private Character traitOwner;

	private const string Steal_Coins = "Steal Coins";

	private const string Pickpocket_Item = "Pickpocket Item";

	private const string Steal_Item_On_Floor = "Steal Item On Floor";

	public Kleptomaniac()
	{
		name = "Kleptomaniac";
		description = "Cannot stop itself from stealing things.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		canBeTriggered = true;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		traitOwner = sourceCharacter as Character;
		traitOwner.behaviourComponent.AddBehaviourComponent(typeof(KleptomaniacBehaviour));
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		traitOwner.behaviourComponent.RemoveBehaviourComponent(typeof(KleptomaniacBehaviour));
		traitOwner = null;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		traitOwner = addTo as Character;
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character character)
		{
			PLAYER_SKILL_TYPE afflictionSkillType = GetAfflictionSkillType();
			if (afflictionSkillType != PLAYER_SKILL_TYPE.NONE && characterThatWillDoJob.HasAfflictedByPlayerWith(afflictionSkillType))
			{
				PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(afflictionSkillType);
				SkillData skillData = PlayerSkillManager.Instance.GetSkillData(afflictionSkillType);
				if (skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Do_Pick_Pocket) && ChanceData.RollChance((skillData.currentLevel != 1) ? CHANCE_TYPE.Kleptomania_Pickpocket_Level_2 : CHANCE_TYPE.Kleptomania_Pickpocket_Level_1) && !characterThatWillDoJob.IsHostileWith(character) && characterThatWillDoJob.relationshipContainer.GetOpinionLabel(character) != "Close Friend")
				{
					GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KLEPTOMANIAC_STEAL, INTERACTION_TYPE.PICKPOCKET, character, characterThatWillDoJob);
					characterThatWillDoJob.jobQueue.AddJobInQueue(job);
				}
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override string GetTriggerFlawEffectDescription(Character character, string key)
	{
		if (character.afflictionsSkillsInflictedByPlayer.Contains(PLAYER_SKILL_TYPE.KLEPTOMANIA))
		{
			PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.KLEPTOMANIA);
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.KLEPTOMANIA);
			bool flag = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Do_Pick_Pocket);
			bool flag2 = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_From_House);
			bool flag3 = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_Any_Place);
			key = "flaw_effect_base";
			if (flag)
			{
				key = "flaw_effect_pickpocket";
			}
			if (flag2 || flag3)
			{
				key = "flaw_effect_steal_item";
			}
		}
		else
		{
			key = "flaw_effect_base";
		}
		return base.GetTriggerFlawEffectDescription(character, key);
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
		{
			bool flag = false;
			Heartbroken traitOrStatus = character.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
			if (traitOrStatus != null)
			{
				flag = Random.Range(0, 100) < 25 * character.traitContainer.stacks[traitOrStatus.name];
			}
			if (!flag)
			{
				if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY))
				{
					character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
				}
				List<Character> list = RuinarchListPool<Character>.Claim();
				BaseSettlement currentSettlement = character.currentSettlement;
				if (currentSettlement != null)
				{
					for (int i = 0; i < currentSettlement.areas.Count; i++)
					{
						Area area = currentSettlement.areas[i];
						for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
						{
							Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
							if (character != character2 && character2.hasMarker && !character2.isBeingSeized && !character2.isDead && IsCharacterValidTargetGivenCurrentKleptomaniaLevel(character, character2))
							{
								list.Add(character2);
							}
						}
					}
				}
				if (list.Count <= 0)
				{
					return "no_target";
				}
				PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.KLEPTOMANIA);
				SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.KLEPTOMANIA);
				Character randomElement = CollectionUtilities.GetRandomElement(list);
				LocationGridTile gridTileLocation = randomElement.gridTileLocation;
				if (!character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation))
				{
					return "no_path_to_target";
				}
				List<string> list2 = RuinarchListPool<string>.Claim();
				list2.Add("Steal Coins");
				if (character.afflictionsSkillsInflictedByPlayer.Contains(PLAYER_SKILL_TYPE.KLEPTOMANIA) && skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Do_Pick_Pocket) && randomElement.HasItem())
				{
					list2.Add("Pickpocket Item");
				}
				if (character.afflictionsSkillsInflictedByPlayer.Contains(PLAYER_SKILL_TYPE.KLEPTOMANIA) && (skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_From_House) || skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_Any_Place)) && randomElement.HasOwnedItemInHomeStructure())
				{
					list2.Add("Steal Item On Floor");
				}
				if (list2.Count <= 0)
				{
					return "no_target";
				}
				switch (CollectionUtilities.GetRandomElement(list2))
				{
				case "Steal Coins":
				{
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEAL_COINS, randomElement, character);
					goapPlanJob.SetIsTriggeredByPlayer(isTriggeredByPlayer);
					character.jobQueue.AddJobInQueue(goapPlanJob);
					break;
				}
				case "Pickpocket Item":
				{
					GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.PICKPOCKET, randomElement, character);
					goapPlanJob2.AddPriorityLocation(INTERACTION_TYPE.PICKPOCKET, character.currentSettlement);
					goapPlanJob2.SetIsTriggeredByPlayer(isTriggeredByPlayer);
					character.jobQueue.AddJobInQueue(goapPlanJob2);
					break;
				}
				case "Steal Item On Floor":
				{
					TileObject randomObjectAtHomeToStealFromCharacter = GetRandomObjectAtHomeToStealFromCharacter(character, randomElement);
					character.jobComponent.CreateStealItemJob(JOB_TYPE.TRIGGER_FLAW, randomObjectAtHomeToStealFromCharacter)?.SetIsTriggeredByPlayer(isTriggeredByPlayer);
					break;
				}
				}
				RuinarchListPool<Character>.Release(list);
			}
			else
			{
				traitOrStatus.TriggerBrokenhearted();
			}
		}
		return base.TriggerFlaw(character);
	}

	public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
		switch (action)
		{
		case INTERACTION_TYPE.STEAL:
		case INTERACTION_TYPE.PICKPOCKET:
		case INTERACTION_TYPE.STEAL_ANYTHING:
		case INTERACTION_TYPE.STEAL_COINS:
			cost = 0;
			break;
		case INTERACTION_TYPE.PICK_UP:
			cost = 10000;
			break;
		}
	}

	private bool IsCharacterValidTargetGivenCurrentKleptomaniaLevel(Character p_actor, Character p_target)
	{
		if (p_actor.afflictionsSkillsInflictedByPlayer.Contains(PLAYER_SKILL_TYPE.KLEPTOMANIA))
		{
			PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.KLEPTOMANIA);
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.KLEPTOMANIA);
			bool flag = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Do_Pick_Pocket);
			bool flag2 = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_From_House);
			bool flag3 = skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Rob_Any_Place);
			if (flag && p_target.HasItem())
			{
				return true;
			}
			if ((flag2 || flag3) && p_target.HasOwnedItemInHomeStructure())
			{
				return true;
			}
			return p_target.moneyComponent.HasCoins();
		}
		return p_target.moneyComponent.HasCoins();
	}

	private TileObject GetRandomObjectAtHomeToStealFromCharacter(Character p_actor, Character p_character)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_character.ownedItems.Count; i++)
		{
			TileObject tileObject = p_character.ownedItems[i];
			if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure == p_character.homeStructure && p_actor.villagerWantsComponent.CanItemSatisfyWant(tileObject) && !p_actor.HasItem(tileObject.tileObjectType))
			{
				list.Add(tileObject);
			}
		}
		if (list.Count <= 0)
		{
			for (int j = 0; j < p_character.ownedItems.Count; j++)
			{
				TileObject tileObject2 = p_character.ownedItems[j];
				if (tileObject2.gridTileLocation != null && tileObject2.gridTileLocation.structure == p_character.homeStructure)
				{
					list.Add(tileObject2);
				}
			}
		}
		if (list.Count > 0)
		{
			TileObject randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<TileObject>.Release(list);
			return randomElement;
		}
		RuinarchListPool<TileObject>.Release(list);
		return null;
	}

	private TileObject GetRandomCarriedObjectToStealFromCharacter(Character p_actor, Character p_character)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_character.items.Count; i++)
		{
			TileObject tileObject = p_character.items[i];
			if (p_actor.villagerWantsComponent.CanItemSatisfyWant(tileObject) && !p_actor.HasItem(tileObject.tileObjectType))
			{
				list.Add(tileObject);
			}
		}
		if (list.Count <= 0)
		{
			list.AddRange(p_character.items);
		}
		if (list.Count > 0)
		{
			TileObject randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<TileObject>.Release(list);
			return randomElement;
		}
		RuinarchListPool<TileObject>.Release(list);
		return null;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = traitOwner;
	}
}
