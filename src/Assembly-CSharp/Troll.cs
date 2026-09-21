using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class Troll : Summon
{
	public override bool defaultDigMode => true;

	public Troll()
		: base(SUMMON_TYPE.Troll, "Troll", RACE.TROLL, Utilities.GetRandomGender())
	{
	}

	public Troll(string className)
		: base(SUMMON_TYPE.Troll, className, RACE.TROLL, Utilities.GetRandomGender())
	{
	}

	public Troll(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetAvoidSettlements(state: true);
		base.movementComponent.SetEnableDigging(state: true);
		base.traitContainer.AddTrait(this, "Petrasol");
	}

	public override void OnSummonAsPlayerMonster()
	{
		base.OnSummonAsPlayerMonster();
		base.traitContainer.RemoveTrait(this, "Petrasol");
		base.movementComponent.SetAvoidSettlements(state: false);
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
			Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
			Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.homeStructure == null)
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Abduct_No_Home);
			return false;
		}
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
		if (currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
		{
			Area area = base.areaLocation;
			if (area != null)
			{
				Character character = null;
				List<Area> list = RuinarchListPool<Area>.Claim();
				List<Character> list2 = RuinarchListPool<Character>.Claim();
				List<Character> list3 = RuinarchListPool<Character>.Claim();
				area.PopulateAreasInRange(list, 6, includeCenterTile: true);
				for (int i = 0; i < list.Count; i++)
				{
					Area area2 = list[i];
					Character randomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel = area2.locationCharacterTracker.GetRandomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel();
					if (randomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel != null)
					{
						list3.Add(randomCharacterInsideHexThatIsAliveAnimalAndIsNotInKennel);
						continue;
					}
					Character randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel = area2.locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel();
					if (randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel != null)
					{
						list2.Add(randomCharacterInsideAreaThatIsAliveAndSleepingVillagerAndIsNotInPrisonOrKennel);
					}
				}
				if (list3.Count > 0)
				{
					character = list3[GameUtilities.RandomBetweenTwoNumbers(0, list3.Count - 1)];
				}
				else if (list2.Count > 0)
				{
					character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
				}
				RuinarchListPool<Area>.Release(list);
				RuinarchListPool<Character>.Release(list2);
				RuinarchListPool<Character>.Release(list3);
				if (character == null)
				{
					CreateAgitateLog(AGITATE_MESSAGE_TYPE.Abduct_Character_No_Target);
					return false;
				}
				base.jobComponent.TryTriggerAbductCharacter(JOB_TYPE.AGITATED, character, base.homeStructure, out p_agitateJob, doNotRecalculate: true);
				if (p_agitateJob is GoapPlanJob goapPlanJob)
				{
					goapPlanJob.SetIsAgitateJob(p_state: true);
					CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
					return true;
				}
			}
			return false;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.Abduct_Wrong_Time);
		return false;
	}

	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (!base.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)) && (base.faction == null || base.faction == defaultFaction) && character != this && base.currentStructure != null && !structure.isInterior && base.currentStructure.isInterior)
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
			if (currentTimeInWordsOfTick != TIME_IN_WORDS.EARLY_NIGHT && currentTimeInWordsOfTick != TIME_IN_WORDS.LATE_NIGHT && currentTimeInWordsOfTick != TIME_IN_WORDS.AFTER_MIDNIGHT)
			{
				base.combatComponent.RemoveHostileInRange(character);
				ForceCancelAllJobsTargetingPOI(character, string.Empty);
			}
		}
	}

	private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom)
	{
		if (base.currentActionNode != null && base.currentActionNode.action.goapType == INTERACTION_TYPE.COOK && base.currentActionNode.otherData != null && base.currentActionNode.otherData.Length == 1 && base.currentActionNode.otherData[0].obj == p_tileObject)
		{
			if (base.currentActionNode.associatedJob != null)
			{
				base.currentActionNode.associatedJob.CancelJob("Cauldron_Not_Available");
			}
			else
			{
				StopCurrentActionNode("Cauldron_Not_Available");
			}
		}
	}
}
