using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CharacterJobTriggerComponent : JobTriggerComponent
{
	private DailySchedule _defaultSchedule;

	public Character owner { get; private set; }

	public List<JOB_TYPE> ableJobs { get; private set; }

	public Dictionary<INTERACTION_TYPE, ActionTrackingData> actionTracker { get; private set; }

	public List<string> obtainPersonalItemUnownedRandomList { get; private set; }

	public bool hasStartedScreamCheck { get; private set; }

	public bool doNotDoRecoverHPJob { get; private set; }

	public int producedFish { get; set; }

	public FishPile fishPile { get; set; }

	public CharacterJobTriggerComponent()
	{
		actionTracker = new Dictionary<INTERACTION_TYPE, ActionTrackingData>();
		ableJobs = new List<JOB_TYPE>();
		AddDefaultAbleJobs();
	}

	public CharacterJobTriggerComponent(SaveDataCharacterJobTriggerComponent data)
	{
		ableJobs = new List<JOB_TYPE>(data.ableJobs);
		actionTracker = new Dictionary<INTERACTION_TYPE, ActionTrackingData>(data.numOfTimesActionDone);
		if (data.obtainPersonalItemUnownedRandomList != null && data.obtainPersonalItemUnownedRandomList.Count > 0)
		{
			obtainPersonalItemUnownedRandomList = new List<string>(data.obtainPersonalItemUnownedRandomList);
		}
		else
		{
			obtainPersonalItemUnownedRandomList = new List<string>();
		}
		hasStartedScreamCheck = data.hasStartedScreamCheck;
		doNotDoRecoverHPJob = data.doNotDoRecoverHPJob;
		producedFish = data.producedFish;
	}

	public void SetOwner(Character owner)
	{
		this.owner = owner;
	}

	public void SubscribeToListeners()
	{
		Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
	}

	public void UnsubscribeListeners()
	{
		Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnJobAddedToQueue);
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
		TryStopScreamCheck();
	}

	public void OnTraitableGainedTrait(ITraitable traitable, Trait trait)
	{
		if (traitable == owner)
		{
			if (TraitManager.Instance.removeStatusTraits.Contains(trait.name))
			{
				TryCreateSettlementRemoveStatusJob(trait);
			}
			if (trait is Burning || trait is Poisoned)
			{
				TriggerRemoveStatusSelf(trait);
			}
			TryStartScreamCheck();
		}
	}

	public void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy)
	{
		if (traitable == owner)
		{
			TryStopScreamCheck();
			if (TraitManager.Instance.removeStatusTraits.Contains("trait"))
			{
				owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_STATUS, trait.name, removedBy);
			}
		}
	}

	public void OnCharacterEnteredArea(Area p_area)
	{
		TryCreateRemoveStatusJob();
	}

	private void OnCharacterExitedArea(Character character, Area p_area)
	{
		if (character == owner)
		{
			Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RESTRAIN, (IPointOfInterest)owner);
		}
		else if (PlayerManager.Instance.player != null && owner.faction == PlayerManager.Instance.player.playerFaction && PlayerManager.Instance.player.playerSettlement.HasArea(p_area) && owner.behaviourComponent.HasBehaviour(typeof(DemonDefendBehaviour)))
		{
			owner.combatComponent.RemoveHostileInRange(character);
		}
	}

	private void OnSeizePOI(IPointOfInterest poi)
	{
		if (poi is Character)
		{
			OnSeizedCharacter(poi as Character);
		}
	}

	private void OnUnseizePOI(IPointOfInterest poi)
	{
		if (poi is Character)
		{
			OnUnseizeCharacter(poi as Character);
		}
	}

	private void OnSeizedCharacter(Character character)
	{
		if (character == owner)
		{
			TryStopScreamCheck();
		}
	}

	private void OnUnseizeCharacter(Character character)
	{
		if (character == owner)
		{
			TryStartScreamCheck();
		}
	}

	private void OnJobRemovedFromQueue(JobQueueItem jobQueueItem, Character character)
	{
		if (character == owner && (jobQueueItem.jobType == JOB_TYPE.CRAFT_MISSING_FURNITURE || jobQueueItem.jobType != JOB_TYPE.HAUL))
		{
			Messenger.Broadcast(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY);
		}
	}

	private void OnJobAddedToQueue(JobQueueItem jobQueueItem, Character character)
	{
	}

	public string GetAbleJobs()
	{
		string text = string.Empty;
		if (owner.characterClass.ableJobs != null && owner.characterClass.ableJobs.Length != 0)
		{
			for (int i = 0; i < owner.characterClass.ableJobs.Length; i++)
			{
				if (i > 0)
				{
					text += ",";
				}
				text += owner.characterClass.ableJobs[i].ToStringEnum();
			}
		}
		if (owner.jobComponent.ableJobs.Count > 0)
		{
			if (text != string.Empty)
			{
				text += ",";
			}
			for (int j = 0; j < owner.jobComponent.ableJobs.Count; j++)
			{
				if (j > 0)
				{
					text += ",";
				}
				text += owner.jobComponent.ableJobs[j].ToStringEnum();
			}
		}
		return text;
	}

	private void AddDefaultAbleJobs()
	{
		AddAbleJob(JOB_TYPE.TEND_WYVERN_COOP);
		AddAbleJob(JOB_TYPE.PURIFY_GROUND);
		AddAbleJob(JOB_TYPE.PLACE_BLUEPRINT);
		AddAbleJob(JOB_TYPE.CREATE_GOLEM);
		AddAbleJob(JOB_TYPE.BLOOD_SACRIFICE);
	}

	public void AddAbleJob(JOB_TYPE jobType)
	{
		if (!ableJobs.Contains(jobType))
		{
			ableJobs.Add(jobType);
		}
	}

	public bool RemoveAbleJob(JOB_TYPE jobType)
	{
		return ableJobs.Remove(jobType);
	}

	public bool HasHigherPriorityJobThan(JOB_TYPE jobType)
	{
		if (owner.jobQueue.jobsInQueue.Count == 0)
		{
			return false;
		}
		return owner.jobQueue.jobsInQueue[0].priority >= jobType.GetJobTypePriority();
	}

	public bool CanDoJob(JOB_TYPE jobType)
	{
		if (!owner.characterClass.CanDoJob(jobType))
		{
			return ableJobs.Contains(jobType);
		}
		return true;
	}

	public bool PlanIdleLongStandStill(out JobQueueItem p_producedJob)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.LONG_STAND_STILL], owner, owner, null, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.LONG_STAND_STILL, owner, owner);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		p_producedJob = goapPlanJob;
		return true;
	}

	public bool PlanIdleStrollOutside(out JobQueueItem producedJob)
	{
		CharacterStateJob characterStateJob = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL_OUTSIDE, owner);
		producedJob = characterStateJob;
		return true;
	}

	public bool PlanZombieStrollOutside(out JobQueueItem producedJob)
	{
		CharacterStateJob characterStateJob = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.ZOMBIE_STROLL, CHARACTER_STATE.STROLL_OUTSIDE, owner);
		producedJob = characterStateJob;
		return true;
	}

	public bool PlanIdleBerserkStrollOutside(out JobQueueItem producedJob)
	{
		CharacterStateJob characterStateJob = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.BERSERK_STROLL, CHARACTER_STATE.STROLL_OUTSIDE, owner);
		producedJob = characterStateJob;
		return true;
	}

	private void TriggerScreamJob()
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SCREAM))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, owner, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}

	public void TriggerBuryPsychopathVictim(Character target, NPCSettlement settlementOfTarget)
	{
		if (owner.traitComponent.IsCharacterTargetOfObsession(target))
		{
			return;
		}
		settlementOfTarget?.GetJob(JOB_TYPE.BURY, target)?.ForceCancelJob();
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY_SERIAL_KILLER_VICTIM, INTERACTION_TYPE.BURY_CHARACTER, target, owner);
		bool flag = false;
		Area area = null;
		List<Area> list = RuinarchListPool<Area>.Claim();
		if (owner.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			settlement.PopulateSurroundingAreasWithPathTo(list, owner);
		}
		else
		{
			owner.currentRegion.PopulateAreasThatAreNextToAVillageButNotMountainAndWaterAndNoSettlementAndWithPathTo(list, owner);
		}
		if (list.Count > 0)
		{
			Area area2 = null;
			float num = float.MaxValue;
			for (int i = 0; i < list.Count; i++)
			{
				Area area3 = list[i];
				float distanceTo = area3.gridTileComponent.centerGridTile.GetDistanceTo(owner.gridTileLocation);
				if (distanceTo < num)
				{
					area2 = area3;
					num = distanceTo;
				}
			}
			area = area2;
		}
		RuinarchListPool<Area>.Release(list);
		if (area != null)
		{
			List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
			for (int j = 0; j < area.gridTileComponent.passableTiles.Count; j++)
			{
				LocationGridTile locationGridTile = area.gridTileComponent.passableTiles[j];
				if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS && owner.movementComponent.HasPathTo(locationGridTile))
				{
					list2.Add(locationGridTile);
				}
			}
			LocationGridTile locationGridTile2 = null;
			if (list2.Count > 0)
			{
				locationGridTile2 = CollectionUtilities.GetRandomElement(list2);
			}
			RuinarchListPool<LocationGridTile>.Release(list2);
			if (locationGridTile2 != null)
			{
				flag = true;
				goapPlanJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[2] { locationGridTile2.structure, locationGridTile2 });
			}
		}
		if (!flag)
		{
			LocationStructure wilderness = owner.currentRegion.wilderness;
			List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
			for (int k = 0; k < wilderness.unoccupiedTiles.Count; k++)
			{
				LocationGridTile locationGridTile3 = wilderness.unoccupiedTiles[k];
				if (!locationGridTile3.IsPartOfSettlement(owner.homeSettlement))
				{
					list3.Add(locationGridTile3);
				}
			}
			LocationGridTile locationGridTile4 = null;
			if (list3.Count > 0)
			{
				locationGridTile4 = CollectionUtilities.GetRandomElement(list3);
			}
			RuinarchListPool<LocationGridTile>.Release(list3);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[2] { wilderness, locationGridTile4 });
		}
		owner.jobQueue.AddJobInQueue(goapPlanJob);
	}

	public bool TriggerDestroy(IPointOfInterest target, string reason)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.DESTROY, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.ASSAULT, target, owner);
			if (!string.IsNullOrEmpty(reason))
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.ASSAULT, new object[1] { reason });
			}
			goapPlanJob.SetStillApplicableChecker("IsDestroyApplicable");
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerDestroy(IPointOfInterest target, out JobQueueItem producedJob, string reason)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.DESTROY, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.ASSAULT, target, owner);
			if (!string.IsNullOrEmpty(reason))
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.ASSAULT, new object[1] { reason });
			}
			goapPlanJob.SetStillApplicableChecker("IsDestroyApplicable");
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerAngryDestroy(IPointOfInterest target, string reason)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ANGRY_DESTROY, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ANGRY_DESTROY, INTERACTION_TYPE.ASSAULT, target, owner);
			if (!string.IsNullOrEmpty(reason))
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.ASSAULT, new object[1] { reason });
			}
			goapPlanJob.SetStillApplicableChecker("IsDestroyApplicable");
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	private void TriggerSettlementRemoveStatusJob(Trait trait)
	{
		if (!owner.isDead && !trait.isGainedFromDoingStealth)
		{
			GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, trait.name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
			if (!owner.homeSettlement.HasJob(goapEffectData, owner))
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffectData, owner, owner.homeSettlement);
				JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, goapPlanJob, INTERACTION_TYPE.NONE);
				goapPlanJob.SetCanTakeThisJobChecker("CanTakeRemoveStatus");
				goapPlanJob.SetStillApplicableChecker("IsRemoveStatusApplicable");
				owner.homeSettlement.AddToAvailableJobs(goapPlanJob);
			}
		}
	}

	private void TriggerRemoveStatusSelf(Trait trait)
	{
		if (owner is Summon || trait.isGainedFromDoingStealth)
		{
			return;
		}
		GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, trait.name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
		if (!owner.jobQueue.HasJob(goapEffectData, owner))
		{
			JOB_TYPE jobType = JOB_TYPE.REMOVE_STATUS;
			if (trait is Burning)
			{
				jobType = JOB_TYPE.DOUSE_FIRE_SELF;
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, goapEffectData, owner, owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, goapPlanJob, INTERACTION_TYPE.NONE);
			goapPlanJob.SetStillApplicableChecker("IsRemoveStatusSelfApplicable");
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool TriggerRemoveStatusTarget(IPointOfInterest target, string traitName)
	{
		if (owner is Summon && owner.petComponent.petOwner != target)
		{
			return false;
		}
		if (target is Character character && (owner.relationshipContainer.HasGrudgeAgainst(character) || owner.crimeComponent.IsWantedBy(character.faction)))
		{
			return false;
		}
		GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, traitName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
		if (!owner.jobQueue.HasJob(goapEffectData, owner))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffectData, target, owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, goapPlanJob, INTERACTION_TYPE.NONE);
			goapPlanJob.SetStillApplicableChecker("IsRemoveStatusTargetApplicable");
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerRemoveStatusTarget(IPointOfInterest target, string traitName, out JobQueueItem p_producedJob)
	{
		if (owner is Summon && owner.petComponent.petOwner != target)
		{
			p_producedJob = null;
			return false;
		}
		if (target is Character character && (owner.relationshipContainer.HasGrudgeAgainst(character) || owner.crimeComponent.IsWantedBy(character.faction)))
		{
			p_producedJob = null;
			return false;
		}
		GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, traitName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
		if (!owner.jobQueue.HasJob(goapEffectData, owner))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffectData, target, owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, goapPlanJob, INTERACTION_TYPE.NONE);
			goapPlanJob.SetStillApplicableChecker("IsRemoveStatusTargetApplicable");
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerRemoveStatusTargetForMonster(IPointOfInterest target, string traitName)
	{
		if (target is Character p_target && owner.relationshipContainer.HasGrudgeAgainst(p_target))
		{
			return false;
		}
		GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, traitName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
		if (!owner.jobQueue.HasJob(goapEffectData, owner))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffectData, target, owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, job, INTERACTION_TYPE.NONE);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool TriggerRemovePlayerTrap(Character actor, IPointOfInterest target, string trapName)
	{
		if (actor.traitContainer.HasTrait("Demon Cultist"))
		{
			return false;
		}
		GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, trapName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAP, goapEffectData, target, owner);
		JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, goapPlanJob, INTERACTION_TYPE.NONE);
		goapPlanJob.SetStillApplicableChecker("IsRemoveTrapApplicable");
		return owner.jobQueue.AddJobInQueue(goapPlanJob);
	}

	public void TryStartScreamCheck()
	{
		if (!hasStartedScreamCheck && owner.isNormalCharacter && !owner.isDead && ((!owner.limiterComponent.canMove && owner.traitContainer.HasTrait("Exhausted", "Starving", "Sulking")) || (owner.traitContainer.HasTrait("Restrained") && owner.currentStructure.structureType != STRUCTURE_TYPE.PRISON)))
		{
			hasStartedScreamCheck = true;
			Messenger.AddListener(Signals.HOUR_STARTED, HourlyScreamCheck);
		}
	}

	public void TryStopScreamCheck()
	{
		if (!hasStartedScreamCheck)
		{
			return;
		}
		if (owner.isDead || owner.currentStructure == null || !owner.hasMarker)
		{
			hasStartedScreamCheck = false;
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyScreamCheck);
			return;
		}
		bool flag = !owner.traitContainer.HasTrait("Exhausted", "Starving", "Sulking");
		bool flag2 = !owner.traitContainer.HasTrait("Restrained");
		bool flag3 = owner.traitContainer.HasTrait("Restrained") && owner.currentStructure.structureType == STRUCTURE_TYPE.PRISON;
		if (((owner.limiterComponent.canMove || flag) && (flag2 || flag3)) || owner.gridTileLocation == null || owner.isDead)
		{
			hasStartedScreamCheck = false;
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyScreamCheck);
		}
	}

	private void HourlyScreamCheck()
	{
		if (owner.isDead || owner.currentStructure == null || !owner.hasMarker)
		{
			hasStartedScreamCheck = false;
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyScreamCheck);
		}
		else
		{
			if (owner.limiterComponent.canPerform)
			{
				return;
			}
			if (owner.needsComponent.isExhausted)
			{
				owner.needsComponent.PlanExtremeTirednessRecoveryActionsForCannotPerform();
				return;
			}
			int num = 50;
			if (!owner.limiterComponent.canMove && owner.traitContainer.HasTrait("Starving", "Sulking"))
			{
				num = 75;
			}
			if (Random.Range(0, 100) < num)
			{
				TriggerScreamJob();
			}
		}
	}

	private bool ShouldCreateRemoveStatusJobFor(Character p_character)
	{
		if (_defaultSchedule == null)
		{
			_defaultSchedule = CharacterManager.Instance.GetDailySchedule<NonPartyMemberSchedule>();
		}
		if (_defaultSchedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Work)
		{
			return true;
		}
		return p_character.homeSettlement.tileObjectComponent.IsCharacterInProximityOfWardLight(p_character);
	}

	private void TryCreateSettlementRemoveStatusJob(Trait trait)
	{
		if (owner.homeSettlement != null && owner.gridTileLocation != null && owner.gridTileLocation.IsNextToOrPartOfSettlement(owner.homeSettlement) && !owner.traitContainer.HasTrait("Criminal", "Berserked") && ShouldCreateRemoveStatusJobFor(owner))
		{
			TriggerSettlementRemoveStatusJob(trait);
		}
	}

	private void TryCreateRemoveStatusJob()
	{
		if (owner.homeSettlement == null || !owner.gridTileLocation.IsNextToOrPartOfSettlement(owner.homeSettlement) || owner.traitContainer.HasTrait("Criminal", "Berserked") || !ShouldCreateRemoveStatusJobFor(owner))
		{
			return;
		}
		for (int i = 0; i < TraitManager.Instance.removeStatusTraits.Count; i++)
		{
			string traitName = TraitManager.Instance.removeStatusTraits[i];
			Trait traitOrStatus = owner.traitContainer.GetTraitOrStatus<Trait>(traitName);
			if (traitOrStatus != null)
			{
				TryCreateSettlementRemoveStatusJob(traitOrStatus);
			}
		}
	}

	public bool TryTriggerFeed(Character targetCharacter)
	{
		TryTriggerFeed(targetCharacter, out var producedJob);
		if (producedJob != null)
		{
			return owner.jobQueue.AddJobInQueue(producedJob);
		}
		return false;
	}

	public bool TryTriggerFeed(Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.FEED) && !targetCharacter.traitContainer.HasTrait("Abstain Fullness"))
		{
			GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffectData, targetCharacter, owner);
			if (owner.homeStructure != null)
			{
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, owner.homeStructure);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { 20 });
			}
			if (owner.homeSettlement != null)
			{
				for (int i = 0; i < owner.homeSettlement.allStructures.Count; i++)
				{
					LocationStructure locationStructure = owner.homeSettlement.allStructures[i];
					if (locationStructure.structureType.IsFoodProducingStructure())
					{
						goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.BUY_FOOD, locationStructure);
					}
				}
				goapPlanJob.AddOtherData(INTERACTION_TYPE.BUY_FOOD, new object[1] { 20 });
			}
			goapPlanJob.SetStillApplicableChecker("IsFeedStillApplicable");
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryTriggerMoveCharacter(Character targetCharacter, LocationStructure dropLocationStructure, bool doNotRecalculate = false)
	{
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new OtherData[1]
			{
				new LocationStructureOtherData(dropLocationStructure)
			});
			goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TryTriggerMoveCharacter(Character targetCharacter, BaseSettlement p_settlement, bool doNotRecalculate = false)
	{
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new OtherData[1]
			{
				new SettlementOtherData(p_settlement)
			});
			goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TryTriggerMoveCharacter(Character targetCharacter, bool doNotRecalculate = false)
	{
		TryTriggerMoveCharacter(targetCharacter, out var producedJob, doNotRecalculate);
		if (producedJob != null)
		{
			return owner.jobQueue.AddJobInQueue(producedJob);
		}
		return false;
	}

	public bool TryTriggerMoveCharacter(Character targetCharacter, out JobQueueItem producedJob, bool doNotRecalculate = false)
	{
		producedJob = null;
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER))
		{
			LocationStructure locationStructure = owner.homeSettlement?.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (locationStructure == null)
			{
				locationStructure = owner.homeSettlement?.GetRandomStructure();
			}
			if (locationStructure == null)
			{
				locationStructure = owner.homeStructure;
			}
			if (locationStructure != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { locationStructure });
				goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
				producedJob = goapPlanJob;
				return true;
			}
		}
		return false;
	}

	public bool TryTriggerRescueMoveCharacter(Character targetCharacter, out JobQueueItem producedJob, bool doNotRecalculate = false)
	{
		producedJob = null;
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.RESCUE_MOVE_CHARACTER))
		{
			LocationStructure locationStructure = owner.homeSettlement?.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (locationStructure == null)
			{
				locationStructure = owner.homeSettlement?.GetRandomStructure();
			}
			if (locationStructure == null)
			{
				locationStructure = owner.homeStructure;
			}
			if (locationStructure != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESCUE_MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { locationStructure });
				goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
				producedJob = goapPlanJob;
				return true;
			}
		}
		return false;
	}

	public bool TryTriggerHaulAnimalCorpse(Character targetCharacter, out JobQueueItem producedJob, bool doNotRecalculate = false)
	{
		producedJob = null;
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.HAUL_ANIMAL_CORPSE))
		{
			LocationStructure locationStructure = owner.homeSettlement?.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (locationStructure == null)
			{
				locationStructure = owner.homeSettlement?.GetRandomStructure();
			}
			if (locationStructure == null)
			{
				locationStructure = owner.homeStructure;
			}
			if (locationStructure != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL_ANIMAL_CORPSE, INTERACTION_TYPE.DROP, targetCharacter, owner);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { locationStructure });
				goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
				producedJob = goapPlanJob;
				return true;
			}
		}
		return false;
	}

	public bool TryTriggerMoveCharacter(Character targetCharacter, LocationStructure dropLocationStructure, LocationGridTile dropGridTile)
	{
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[2] { dropLocationStructure, dropGridTile });
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TryTriggerCaptureCharacter(Character targetCharacter, LocationStructure dropLocationStructure, bool doNotRecalculate = false)
	{
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CAPTURE_CHARACTER, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { dropLocationStructure });
			goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TryTriggerCaptureCharacter(JOB_TYPE jobType, Character targetCharacter, LocationStructure dropLocationStructure, out JobQueueItem producedJob, bool doNotRecalculate = false)
	{
		producedJob = null;
		if (!targetCharacter.HasJobTargetingThis(jobType) && !owner.jobQueue.HasJob(jobType, targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { dropLocationStructure });
			goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerSuicideJob(out JobQueueItem producedJob, string reason)
	{
		producedJob = null;
		if (owner.traitContainer.HasTrait("Paralyzed"))
		{
			return false;
		}
		if (!owner.jobQueue.HasJob(JOB_TYPE.COMMIT_SUICIDE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMMIT_SUICIDE, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
			JobUtilities.PopulatePriorityLocationsForSuicide(owner, goapPlanJob);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.NONE, new object[1] { reason });
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerSuicideJobForCriticalBreak(out JobQueueItem producedJob, string reason)
	{
		producedJob = null;
		if (owner.traitContainer.HasTrait("Paralyzed"))
		{
			return false;
		}
		if (!owner.jobQueue.HasJob(JOB_TYPE.COMMIT_SUICIDE) && !owner.jobQueue.HasJob(JOB_TYPE.CRITICAL_BREAK))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRITICAL_BREAK, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
			JobUtilities.PopulatePriorityLocationsForSuicide(owner, goapPlanJob);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.NONE, new object[1] { reason });
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	private void ScheduleActionCounterDecrease(GameDate p_dueDate, INTERACTION_TYPE p_action)
	{
		SchedulingManager.Instance.AddEntry(p_dueDate, delegate
		{
			DecreaseNumOfTimesActionDone(p_action);
		}, owner);
	}

	public void IncreaseNumOfTimesActionDone(INTERACTION_TYPE action)
	{
		if (!actionTracker.ContainsKey(action))
		{
			actionTracker.Add(action, RuinarchActionTrackingDataPool.Claim());
		}
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.AddDays(3);
		actionTracker[action].IncreaseActionCounter(gameDate);
		ScheduleActionCounterDecrease(gameDate, action);
	}

	private void DecreaseNumOfTimesActionDone(INTERACTION_TYPE action)
	{
		if (actionTracker.ContainsKey(action))
		{
			actionTracker[action].DecreaseActionCounter();
			if (actionTracker[action].numberOfTimesActionDone <= 0)
			{
				ActionTrackingData p_data = actionTracker[action];
				actionTracker.Remove(action);
				RuinarchActionTrackingDataPool.Release(p_data);
			}
		}
	}

	public int GetNumOfTimesActionDone(GoapAction action)
	{
		if (actionTracker.ContainsKey(action.goapType))
		{
			return actionTracker[action.goapType].numberOfTimesActionDone;
		}
		return 0;
	}

	public bool TriggerRoamAroundTerritory(out JobQueueItem producedJob, bool checkIfPathPossibleWithoutDigging = false)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TERRITORY, JOB_TYPE.IDLE_RETURN_HOME))
		{
			bool flag = false;
			LocationGridTile locationGridTile = null;
			if (owner.homeSettlement != null && owner.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				if (owner.IsInHomeSettlement())
				{
					flag = true;
					List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
					list.AddRange(owner.areaLocation.gridTileComponent.passableTiles);
					for (int i = 0; i < owner.areaLocation.neighbourComponent.neighbours.Count; i++)
					{
						Area area = owner.areaLocation.neighbourComponent.neighbours[i];
						if (owner.homeSettlement.areas.Contains(area))
						{
							list.AddRange(area.gridTileComponent.passableTiles);
						}
					}
					list.Shuffle();
					locationGridTile = list.GetFirstTileCharacterCanGoTo(owner);
					RuinarchListPool<LocationGridTile>.Release(list);
				}
				else
				{
					List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
					owner.homeSettlement.PopulatePassableTilesList(list2);
					list2.Shuffle();
					locationGridTile = list2.GetFirstTileCharacterCanGoTo(owner);
					RuinarchListPool<LocationGridTile>.Release(list2);
				}
			}
			else if (owner.homeStructure != null)
			{
				if (owner.isAtHomeStructure)
				{
					flag = true;
				}
				if (checkIfPathPossibleWithoutDigging)
				{
					List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
					list3.AddRange(owner.homeStructure.passableTiles);
					list3.Shuffle();
					locationGridTile = ((list3.Count > 0) ? list3.GetFirstTileCharacterCanGoTo(owner) : CollectionUtilities.GetRandomElement(owner.homeStructure.tiles));
					RuinarchListPool<LocationGridTile>.Release(list3);
				}
				else
				{
					locationGridTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
				}
			}
			else if (owner.HasTerritory())
			{
				if (owner.IsInTerritory())
				{
					flag = true;
				}
				Area territory = owner.territory;
				if (checkIfPathPossibleWithoutDigging)
				{
					List<LocationGridTile> list4 = RuinarchListPool<LocationGridTile>.Claim();
					territory.gridTileComponent.gridTiles.PopulateListWithTilesCharacterCanGoTo(owner, list4);
					locationGridTile = CollectionUtilities.GetRandomElement((list4.Count > 0) ? list4 : territory.gridTileComponent.gridTiles);
					RuinarchListPool<LocationGridTile>.Release(list4);
				}
				else
				{
					locationGridTile = CollectionUtilities.GetRandomElement(territory.gridTileComponent.gridTiles);
				}
			}
			if (locationGridTile == null)
			{
				locationGridTile = ((owner.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS) ? CollectionUtilities.GetRandomElement(owner.currentStructure.passableTiles) : CollectionUtilities.GetRandomElement(owner.areaLocation.gridTileComponent.gridTiles));
			}
			JOB_TYPE jobType = JOB_TYPE.ROAM_AROUND_TERRITORY;
			if (!flag)
			{
				jobType = JOB_TYPE.IDLE_RETURN_HOME;
			}
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[1]
			{
				new LocationGridTileOtherData(locationGridTile)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ROAM, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerRoamAroundTile(JOB_TYPE jobType, LocationGridTile tile = null)
	{
		JobQueueItem producedJob = null;
		if (TriggerRoamAroundTile(jobType, out producedJob, tile))
		{
			return owner.jobQueue.AddJobInQueue(producedJob);
		}
		return false;
	}

	public bool TriggerRoamAroundTile(JOB_TYPE jobType, out JobQueueItem producedJob, LocationGridTile tile = null)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(jobType))
		{
			LocationGridTile locationGridTile = tile;
			if (locationGridTile == null)
			{
				if (owner.IsInHomeSettlement() && owner.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
				{
					locationGridTile = owner.homeSettlement.GetRandomPassableGridTileInSettlementStructuresThatCharacterHasPathTo(owner);
				}
				else if (owner.isAtHomeStructure)
				{
					locationGridTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
				}
				else if (owner.IsInTerritory())
				{
					locationGridTile = owner.territory.GetRandomPassableTile();
				}
			}
			if (locationGridTile == null)
			{
				locationGridTile = owner.areaLocation.gridTileComponent.GetRandomPassableTile();
			}
			if (locationGridTile == null)
			{
				producedJob = null;
				return false;
			}
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[1]
			{
				new LocationGridTileOtherData(locationGridTile)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ROAM, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerRoamAroundTile(out JobQueueItem producedJob, LocationGridTile tile = null)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE, JOB_TYPE.IDLE_RETURN_HOME))
		{
			LocationGridTile locationGridTile = tile;
			bool flag = false;
			if (locationGridTile == null)
			{
				if (owner.IsInHomeSettlement() && owner.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
				{
					flag = true;
					locationGridTile = owner.homeSettlement.GetRandomPassableGridTileInSettlementStructuresThatCharacterHasPathTo(owner);
				}
				else if (owner.isAtHomeStructure)
				{
					flag = true;
					locationGridTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
				}
				else if (owner.IsInTerritory())
				{
					flag = true;
					locationGridTile = owner.territory.GetRandomPassableTile();
				}
			}
			if (locationGridTile == null)
			{
				locationGridTile = owner.areaLocation.gridTileComponent.GetRandomPassableTile();
			}
			if (locationGridTile == null)
			{
				producedJob = null;
				return false;
			}
			JOB_TYPE jobType = JOB_TYPE.ROAM_AROUND_TILE;
			if (!flag)
			{
				jobType = JOB_TYPE.IDLE_RETURN_HOME;
			}
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[1]
			{
				new LocationGridTileOtherData(locationGridTile)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ROAM, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerRoamAroundTileAvoidVillageStructures(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE, JOB_TYPE.IDLE_RETURN_HOME))
		{
			LocationGridTile randomTileThatIsPassableAndIsNotInVillageStructure = owner.areaLocation.gridTileComponent.GetRandomTileThatIsPassableAndIsNotInVillageStructure();
			if (randomTileThatIsPassableAndIsNotInVillageStructure == null)
			{
				producedJob = null;
				return false;
			}
			JOB_TYPE jobType = JOB_TYPE.ROAM_AROUND_TILE;
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[1]
			{
				new LocationGridTileOtherData(randomTileThatIsPassableAndIsNotInVillageStructure)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ROAM, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerRoamAroundStructure(out JobQueueItem producedJob, LocationGridTile tile = null)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_STRUCTURE, JOB_TYPE.IDLE_RETURN_HOME_HIGHER))
		{
			LocationGridTile locationGridTile = tile;
			if (locationGridTile == null && owner.currentStructure != null)
			{
				if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS)
				{
					Area nearestAreaWithinRegion = owner.gridTileLocation.GetNearestAreaWithinRegion();
					if (nearestAreaWithinRegion != null)
					{
						locationGridTile = nearestAreaWithinRegion.gridTileComponent.GetRandomTile();
					}
				}
				else
				{
					locationGridTile = CollectionUtilities.GetRandomElement(owner.currentStructure.passableTiles);
				}
			}
			JOB_TYPE jobType = JOB_TYPE.ROAM_AROUND_STRUCTURE;
			if (!owner.IsAtHome())
			{
				jobType = JOB_TYPE.IDLE_RETURN_HOME_HIGHER;
			}
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[1]
			{
				new LocationGridTileOtherData(locationGridTile)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ROAM, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerRoamAroundStructure(JOB_TYPE jobType, out JobQueueItem producedJob, LocationGridTile tile = null)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			LocationGridTile locationGridTile = tile;
			if (locationGridTile == null && owner.currentStructure != null)
			{
				if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS)
				{
					Area nearestAreaWithinRegion = owner.gridTileLocation.GetNearestAreaWithinRegion();
					if (nearestAreaWithinRegion != null)
					{
						locationGridTile = nearestAreaWithinRegion.gridTileComponent.GetRandomTile();
					}
				}
				else
				{
					locationGridTile = CollectionUtilities.GetRandomElement(owner.currentStructure.passableTiles);
				}
			}
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[1]
			{
				new LocationGridTileOtherData(locationGridTile)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ROAM, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerMoveToArea(JOB_TYPE p_jobType, out JobQueueItem producedJob, Area p_area)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(p_area.gridTileComponent.gridTiles);
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[1]
			{
				new LocationGridTileOtherData(randomElement)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.ROAM, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerStand(JOB_TYPE jobType)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.STAND, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerStand(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.STAND))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND, INTERACTION_TYPE.STAND, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool PlanReturnHome(JOB_TYPE jobType)
	{
		JobQueueItem producedJob = null;
		if (PlanReturnHome(jobType, out producedJob))
		{
			return owner.jobQueue.AddJobInQueue(producedJob);
		}
		return false;
	}

	public bool PlanReturnHome(JOB_TYPE jobType, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(jobType))
		{
			LocationGridTile locationGridTile = null;
			if (owner.homeStructure != null && !owner.homeStructure.hasBeenDestroyed)
			{
				locationGridTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
			}
			else if (owner.homeSettlement != null)
			{
				LocationStructure randomStructure = owner.homeSettlement.GetRandomStructure();
				if (randomStructure != null)
				{
					locationGridTile = CollectionUtilities.GetRandomElement(randomStructure.passableTiles);
				}
			}
			else
			{
				if (!owner.HasTerritory())
				{
					return TriggerRoamAroundTile(out producedJob);
				}
				locationGridTile = owner.territory.GetRandomPassableTile();
			}
			if (locationGridTile == null)
			{
				return TriggerRoamAroundTile(out producedJob);
			}
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.GO_TO_TILE], owner, locationGridTile.tileObjectComponent.genericTileObject, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.GO_TO_TILE, locationGridTile.tileObjectComponent.genericTileObject, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool PlanReturnToVillageCenter(JOB_TYPE jobType)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			LocationStructure cityCenter = owner.homeSettlement.cityCenter;
			if (cityCenter != null && owner.currentStructure != cityCenter)
			{
				LocationGridTile locationGridTile = ((cityCenter.passableTiles.Count > 0) ? CollectionUtilities.GetRandomElement(cityCenter.passableTiles) : CollectionUtilities.GetRandomElement(cityCenter.tiles));
				ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.GO_TO_TILE], owner, locationGridTile.tileObjectComponent.genericTileObject, null, 0);
				GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.GO_TO_TILE, locationGridTile.tileObjectComponent.genericTileObject, owner);
				goapPlan.SetDoNotRecalculate(state: true);
				goapPlanJob.SetCannotBePushedBack(state: true);
				goapPlanJob.SetAssignedPlan(goapPlan);
				owner.jobQueue.AddJobInQueue(goapPlanJob);
				return true;
			}
		}
		return false;
	}

	public bool TriggerMonsterSleep(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SLEEP_OUTSIDE], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, INTERACTION_TYPE.SLEEP_OUTSIDE, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void CreateOpenChestJob(TileObject target)
	{
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OPEN_CHEST, INTERACTION_TYPE.OPEN, target, owner);
		owner.jobQueue.AddJobInQueue(job);
	}

	public void CreateAbductJob(Character target, LocationStructure targetStructure = null)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABDUCT, INTERACTION_TYPE.DROP, target, owner);
		if (targetStructure == null)
		{
			targetStructure = PlayerManager.Instance.player.portalArea.region.GetRandomStructureOfType(STRUCTURE_TYPE.TORTURE_CHAMBERS);
			if (targetStructure == null)
			{
				targetStructure = PlayerManager.Instance.player.portalArea.structureComponent.GetMostImportantStructureOnTile();
			}
		}
		if (targetStructure != null)
		{
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { targetStructure });
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool CreateBrawlJob(Character targetCharacter)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.BRAWL, targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BRAWL, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetDoNotRecalculate(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool CreateSlayTargetJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SLAY_TARGET, targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SLAY_TARGET, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateSlayCharacterJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SLAY_TARGET, targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SLAY_TARGET, INTERACTION_TYPE.SLAY_CHARACTER, targetCharacter, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void CreateProduceFoodJob()
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
			if (owner.faction != null && owner.faction.factionType.type != FACTION_TYPE.Vagrants && owner.currentStructure != null)
			{
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, owner.currentStructure);
			}
			else if (owner.traitContainer.HasTrait("Enslaved"))
			{
				SlaveProduceFoodPriorityLocations(goapPlanJob);
			}
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool CreateProduceFoodJob(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
			if (goapPlanJob != null && owner.traitContainer.HasTrait("Enslaved"))
			{
				SlaveProduceFoodPriorityLocations(goapPlanJob);
			}
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateProduceFoodForCampJob(out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD_FOR_CAMP))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD_FOR_CAMP, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	private void SlaveProduceFoodPriorityLocations(GoapPlanJob p_job)
	{
		if (owner.gridTileLocation == null)
		{
			return;
		}
		List<Area> list = RuinarchListPool<Area>.Claim();
		owner.gridTileLocation.area.PopulateAreasInRange(list, 6, includeCenterTile: true);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				List<LocationStructure> structures = list[i].structureComponent.structures;
				if (structures == null || structures.Count <= 0)
				{
					continue;
				}
				for (int j = 0; j < structures.Count; j++)
				{
					LocationStructure locationStructure = structures[j];
					if (!locationStructure.hasBeenDestroyed && locationStructure.structureType.IsForageStructure())
					{
						p_job.AddPriorityLocation(INTERACTION_TYPE.NONE, locationStructure);
					}
				}
			}
		}
		RuinarchListPool<Area>.Release(list);
	}

	public bool CreateButcherJob(Character target, JOB_TYPE jobType, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BUTCHER, target, owner);
			goapPlanJob.SetCancelOnDeath(state: false);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool CreateFullnessRecoveryOnSight(IPointOfInterest target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT))
		{
			CreateFullnessRecoveryOnSight(target, cancelOtherFullnessRecoveryJobs: false, out var producedJob);
			if (producedJob != null && owner.jobQueue.AddJobInQueue(producedJob))
			{
				owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
				return true;
			}
		}
		return false;
	}

	public bool CreateFullnessRecoveryOnSight(IPointOfInterest target, bool cancelOtherFullnessRecoveryJobs, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT))
		{
			if (owner.partyComponent.isActiveMember)
			{
				return false;
			}
			if (!owner.limiterComponent.canDoFullnessRecovery && (owner.limiterComponent.canDoFullnessRecoveryValue != -1 || !owner.traitContainer.HasTrait("Abstain Fullness")))
			{
				return false;
			}
			if (target is Table && target.gridTileLocation != null && target.gridTileLocation.structure == owner.homeStructure && owner.currentJob != null && owner.currentJob.jobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD)
			{
				return false;
			}
			if (owner.currentJob != null && owner.currentJob.jobType.IsFullnessRecoveryTypeJob() && owner.currentActionNode != null && owner.currentActionNode.poiTarget != null && owner.marker != null && owner.marker.IsPOIInVision(owner.currentActionNode.poiTarget))
			{
				return false;
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, INTERACTION_TYPE.EAT, target, owner);
			JobUtilities.PopulatePriorityLocationsForFullnessRecovery(owner, goapPlanJob);
			if (target is Table)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { 20 });
			}
			producedJob = goapPlanJob;
			if (cancelOtherFullnessRecoveryJobs)
			{
				owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
			}
			return true;
		}
		return false;
	}

	public GoapPlanJob CreateDrinkBloodJob(JOB_TYPE jobType, IPointOfInterest target)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			if (owner.partyComponent.isActiveMember)
			{
				return null;
			}
			if (!owner.limiterComponent.canDoFullnessRecovery)
			{
				return null;
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DRINK_BLOOD, target, owner);
			if (owner.jobQueue.AddJobInQueue(goapPlanJob))
			{
				owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
			}
			return goapPlanJob;
		}
		return null;
	}

	public bool CreateDrinkBloodJob(JOB_TYPE jobType, IPointOfInterest target, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			if (owner.partyComponent.isActiveMember)
			{
				producedJob = null;
				return false;
			}
			if (!owner.limiterComponent.canDoFullnessRecovery)
			{
				producedJob = null;
				return false;
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DRINK_BLOOD, target, owner);
			owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateDrinkBloodJobForBloodHunt(JOB_TYPE jobType, IPointOfInterest target, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			if (!owner.limiterComponent.canDoFullnessRecovery)
			{
				producedJob = null;
				return false;
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DRINK_BLOOD, target, owner);
			owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public GoapPlanJob CreateVampiricEmbraceJob(JOB_TYPE jobType, IPointOfInterest target)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.VAMPIRIC_EMBRACE, target, owner);
			owner.jobQueue.AddJobInQueue(goapPlanJob);
			return goapPlanJob;
		}
		return null;
	}

	public bool CreateVampiricEmbraceJob(JOB_TYPE jobType, IPointOfInterest target, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.VAMPIRIC_EMBRACE, target, owner);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool CreateFeedSelfToVampireJob(Character vampire)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.OFFER_BLOOD))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OFFER_BLOOD, INTERACTION_TYPE.FEED_SELF, vampire, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public void CreateTakeItemOnSightJob(TileObject targetItem, JOB_TYPE jobType = JOB_TYPE.TAKE_ITEM_ON_SIGHT)
	{
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.PICK_UP, targetItem, owner);
		owner.jobQueue.AddJobInQueue(job);
	}

	public GoapPlanJob CreateStealItemJob(JOB_TYPE jobType, TileObject targetItem)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.STEAL, targetItem, owner);
		owner.jobQueue.AddJobInQueue(goapPlanJob);
		return goapPlanJob;
	}

	public bool CreateDemonStealJob(JOB_TYPE jobType, TileObject targetItem, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType, targetItem))
		{
			producedJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DEMON_STEAL, targetItem, owner);
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateDemonStealKnockoutJob(JOB_TYPE jobType, Character target, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ASSAULT, target, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateObtainPersonalItemJob(string chosenItemName, out JobQueueItem producedJob)
	{
		if (!owner.IsInventoryAtFullCapacity() && !owner.jobQueue.HasJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM))
		{
			GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffectData, owner, owner);
			JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(owner, goapPlanJob, INTERACTION_TYPE.PICK_UP);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void CreateDropItemJob(JOB_TYPE jobType, TileObject target, LocationStructure dropLocation, bool doNotRecalculate = false, bool cannotBePushedBack = false)
	{
		if (!owner.jobQueue.HasJob(jobType, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DROP_ITEM, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[1] { dropLocation });
			goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
			goapPlanJob.SetCannotBePushedBack(cannotBePushedBack);
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool CreateDropItemJob(JOB_TYPE jobType, TileObject target, LocationStructure dropLocation, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DROP_ITEM, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[1] { dropLocation });
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void CreateHoardItemJob(TileObject target, LocationStructure dropLocation, bool doNotRecalculate = false)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.HOARD, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HOARD, INTERACTION_TYPE.DROP_ITEM, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[1] { dropLocation });
			goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool CreateHideAtHomeJob()
	{
		if (owner.homeStructure != null && !owner.homeStructure.hasBeenDestroyed && owner.homeStructure.tiles.Count > 0)
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HIDE_AT_HOME, INTERACTION_TYPE.RETURN_HOME, owner, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool TriggerStandStill(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.STAND_STILL))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND_STILL], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND_STILL, INTERACTION_TYPE.STAND_STILL, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerIdleJob(INTERACTION_TYPE actionType, IPointOfInterest p_target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, actionType, p_target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	private bool CreatePlaceTrapPOIJob(IPointOfInterest target, JOB_TYPE jobType = JOB_TYPE.PLACE_TRAP)
	{
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), target, owner);
		return owner.jobQueue.AddJobInQueue(job);
	}

	private bool CreatePlaceTrapPOIJob(IPointOfInterest target, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), target, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool CreatePlaceTrapJob(Character targetCharacter, JOB_TYPE jobType = JOB_TYPE.PLACE_TRAP)
	{
		IPointOfInterest pointOfInterest = null;
		for (int i = 0; i < targetCharacter.ownedItems.Count; i++)
		{
			TileObject tileObject = targetCharacter.ownedItems[i];
			if (tileObject.gridTileLocation != null && (bool)tileObject.mapVisual && !tileObject.traitContainer.HasTrait("Booby Trapped") && tileObject.tileObjectType != TILE_OBJECT_TYPE.TORCH)
			{
				pointOfInterest = tileObject;
				break;
			}
		}
		if (pointOfInterest != null)
		{
			return CreatePlaceTrapPOIJob(pointOfInterest, jobType);
		}
		return false;
	}

	public bool CreatePlaceTrapOnOwnedHomeItemJob(Character targetCharacter, JOB_TYPE jobType = JOB_TYPE.PLACE_TRAP)
	{
		if (targetCharacter.homeStructure != null)
		{
			IPointOfInterest pointOfInterest = null;
			for (int i = 0; i < targetCharacter.ownedItems.Count; i++)
			{
				TileObject tileObject = targetCharacter.ownedItems[i];
				if (tileObject.gridTileLocation != null && (bool)tileObject.mapVisual && tileObject.gridTileLocation.structure == targetCharacter.homeStructure && !tileObject.traitContainer.HasTrait("Booby Trapped") && tileObject.tileObjectType != TILE_OBJECT_TYPE.TORCH)
				{
					pointOfInterest = tileObject;
					break;
				}
			}
			if (pointOfInterest != null)
			{
				return CreatePlaceTrapPOIJob(pointOfInterest, jobType);
			}
		}
		return false;
	}

	public bool CreatePlaceTrapOnAnyHomeItemJob(Character targetCharacter, JOB_TYPE jobType = JOB_TYPE.PLACE_TRAP)
	{
		if (targetCharacter.homeStructure != null)
		{
			IPointOfInterest pointOfInterest = null;
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			targetCharacter.homeStructure.PopulateBuiltTileObjects(list);
			for (int i = 0; i < list.Count; i++)
			{
				TileObject tileObject = list[i];
				if (!tileObject.traitContainer.HasTrait("Booby Trapped") && tileObject.tileObjectType != TILE_OBJECT_TYPE.BLOCK_WALL && tileObject.tileObjectType != TILE_OBJECT_TYPE.ICE_BLOCK_WALL && tileObject.tileObjectType != TILE_OBJECT_TYPE.THIN_WALL && tileObject.tileObjectType != TILE_OBJECT_TYPE.TORCH)
				{
					pointOfInterest = tileObject;
					break;
				}
			}
			RuinarchListPool<TileObject>.Release(list);
			if (pointOfInterest != null)
			{
				return CreatePlaceTrapPOIJob(pointOfInterest, jobType);
			}
		}
		return false;
	}

	public bool CreatePlaceTrapJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		IPointOfInterest pointOfInterest = null;
		for (int i = 0; i < targetCharacter.ownedItems.Count; i++)
		{
			TileObject tileObject = targetCharacter.ownedItems[i];
			if (tileObject.gridTileLocation != null && (bool)tileObject.mapVisual && !tileObject.traitContainer.HasTrait("Booby Trapped"))
			{
				pointOfInterest = tileObject;
				break;
			}
		}
		if (pointOfInterest != null)
		{
			return CreatePlaceTrapPOIJob(pointOfInterest, out producedJob);
		}
		producedJob = null;
		return false;
	}

	public bool CreateReportDemonicStructure(LocationStructure structureToReport)
	{
		NPCSettlement homeSettlement = owner.homeSettlement;
		if (homeSettlement != null && homeSettlement.mainStorage != null && !owner.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE))
		{
			if (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.SEARCH_FOR_DEMONIC_AREA)
			{
				if (owner.hasMarker)
				{
					owner.marker.pathfindingAI.ClearAllCurrentPathData();
				}
				owner.PerformGoapAction();
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE, INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE, new object[2] { structureToReport, homeSettlement.mainStorage });
			if (owner.jobQueue.AddJobInQueue(goapPlanJob))
			{
				Messenger.Broadcast(JobSignals.DEMONIC_STRUCTURE_DISCOVERED, structureToReport, owner, goapPlanJob);
			}
			return true;
		}
		return false;
	}

	public void SetDoNotDoRecoverHPJob(bool state)
	{
		if (doNotDoRecoverHPJob == state)
		{
			return;
		}
		doNotDoRecoverHPJob = state;
		if (doNotDoRecoverHPJob)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SetDoNotDoRecoverHPJob(state: false);
			}, owner);
		}
	}

	public void OnHPReduced()
	{
		if (!doNotDoRecoverHPJob && !owner.jobQueue.HasJob(JOB_TYPE.RECOVER_HP) && owner.isNormalCharacter && owner.HasHealth() && owner.currentHP < Mathf.FloorToInt((float)owner.maxHP * 0.5f) && !owner.partyComponent.isMemberThatJoinedQuest)
		{
			CreateHealSelfJob();
		}
	}

	private void CreateHealSelfJob()
	{
		if (owner.race.IsSapient() && !owner.traitContainer.HasTrait("Paralyzed") && !owner.jobQueue.HasJob(JOB_TYPE.RECOVER_HP))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECOVER_HP, INTERACTION_TYPE.HEAL_SELF, owner, owner);
			JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(owner, goapPlanJob, INTERACTION_TYPE.PICK_UP);
			goapPlanJob.SetStillApplicableChecker("IsHealSelfApplicable");
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	private GoapPlanJob CreatePoisonFood(Character targetCharacter, JOB_TYPE jobType)
	{
		if (owner.jobQueue.HasJob(jobType, targetCharacter))
		{
			return null;
		}
		if (targetCharacter.isDead)
		{
			return null;
		}
		if (targetCharacter.homeRegion == null)
		{
			return null;
		}
		IPointOfInterest pointOfInterest = null;
		for (int i = 0; i < targetCharacter.ownedItems.Count; i++)
		{
			TileObject tileObject = targetCharacter.ownedItems[i];
			if (tileObject.gridTileLocation != null && (bool)tileObject.mapVisual && tileObject.advertisedActions.Contains(INTERACTION_TYPE.EAT) && !tileObject.gridTileLocation.structure.IsResident(owner))
			{
				Poisoned traitOrStatus = tileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
				if (traitOrStatus?.responsibleCharacters == null || !traitOrStatus.responsibleCharacters.Contains(owner))
				{
					pointOfInterest = tileObject;
					break;
				}
			}
		}
		if (pointOfInterest == null)
		{
			if (targetCharacter.homeStructure != null && owner.homeStructure != targetCharacter.homeStructure)
			{
				List<TileObject> list = RuinarchListPool<TileObject>.Claim();
				targetCharacter.homeStructure.PopulateValidTileObjectsForPoisonNeighbour<TileObject>(list, owner);
				if (list.Count > 0)
				{
					pointOfInterest = CollectionUtilities.GetRandomElement(list);
				}
				RuinarchListPool<TileObject>.Release(list);
			}
			if (pointOfInterest == null)
			{
				return null;
			}
		}
		return JobManager.Instance.CreateNewGoapPlanJob(jobType, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Poisoned", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), pointOfInterest, owner);
	}

	public bool CreatePoisonFoodJob(Character targetCharacter, JOB_TYPE jobType = JOB_TYPE.POISON_FOOD)
	{
		GoapPlanJob goapPlanJob = CreatePoisonFood(targetCharacter, jobType);
		if (goapPlanJob != null)
		{
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool CreatePoisonFoodJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		producedJob = CreatePoisonFood(targetCharacter, JOB_TYPE.POISON_FOOD);
		return producedJob != null;
	}

	public bool CreateSpreadNegativeInfoJob(JOB_TYPE p_jobType, Character targetCharacter, ActualGoapNode negativeInfo)
	{
		if (targetCharacter.isDead)
		{
			return false;
		}
		if (negativeInfo == null)
		{
			return false;
		}
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[1] { negativeInfo });
		owner.jobQueue.AddJobInQueue(goapPlanJob);
		return true;
	}

	public bool CreateSpreadRumorJob(Character targetCharacter, Rumor rumor, JOB_TYPE jobType = JOB_TYPE.SPREAD_RUMOR)
	{
		if (targetCharacter.isDead)
		{
			return false;
		}
		if (rumor == null)
		{
			return false;
		}
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[1] { rumor });
		owner.jobQueue.AddJobInQueue(goapPlanJob);
		return true;
	}

	public bool CreateSpreadRumorJob(Character targetCharacter, Rumor rumor, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (targetCharacter.isDead)
		{
			return false;
		}
		if (rumor == null)
		{
			return false;
		}
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPREAD_RUMOR, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[1] { rumor });
		producedJob = goapPlanJob;
		return true;
	}

	public bool CreateConfirmRumorJob(Character targetCharacter, ActualGoapNode action)
	{
		if (targetCharacter.isDead)
		{
			return false;
		}
		if (action == null)
		{
			return false;
		}
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CONFIRM_RUMOR, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[1] { action });
		owner.jobQueue.AddJobInQueue(goapPlanJob);
		return true;
	}

	public bool TryCreateReportCrimeJob(Character actor, IPointOfInterest target, CrimeData crimeData, ICrimeable crime)
	{
		if (owner.crimeComponent.CanCreateReportCrimeJob(actor, target, crimeData, crime))
		{
			GoapPlanJob goapPlanJob = CreateReportCrimeJob(crimeData, crime);
			if (goapPlanJob != null)
			{
				owner.jobQueue.AddJobInQueue(goapPlanJob);
				return true;
			}
		}
		else
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "report_do_nothing", LOG_TAG.Crimes);
			log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddToFillers(null, crimeData.crimeTypeObj.name, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase(releaseLogAfter: true);
			owner.crimeComponent.AddReportedCrime(crimeData);
			if (owner.characterClass.className == "Shaman" && owner.relationshipContainer.IsFriendsWith(actor) && (crimeData.crimeType == CRIME_TYPE.Vampire || crimeData.crimeType == CRIME_TYPE.Werewolf))
			{
				string text = string.Empty;
				if (actor.traitContainer.HasTrait("Vampire"))
				{
					text = "Vampire";
				}
				else if (actor.traitContainer.HasTrait("Lycanthrope"))
				{
					text = "Lycanthrope";
				}
				if (!string.IsNullOrEmpty(text))
				{
					owner.jobComponent.TriggerCureMagicalAffliction(actor, text);
				}
			}
		}
		return false;
	}

	public bool TryCreateReportCrimeJob(Character actor, IPointOfInterest target, CrimeData crimeData, ICrimeable crime, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (owner.crimeComponent.CanCreateReportCrimeJob(actor, target, crimeData, crime))
		{
			producedJob = CreateReportCrimeJob(crimeData, crime);
			if (producedJob != null)
			{
				return true;
			}
		}
		else
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "report_do_nothing", LOG_TAG.Crimes);
			log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddToFillers(null, crimeData.crimeTypeObj.name, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase(releaseLogAfter: true);
			owner.crimeComponent.AddReportedCrime(crimeData);
			if (owner.characterClass.className == "Shaman" && owner.relationshipContainer.IsFriendsWith(actor) && (crimeData.crimeType == CRIME_TYPE.Vampire || crimeData.crimeType == CRIME_TYPE.Werewolf))
			{
				string text = string.Empty;
				if (actor.traitContainer.HasTrait("Vampire"))
				{
					text = "Vampire";
				}
				else if (actor.traitContainer.HasTrait("Lycanthrope"))
				{
					text = "Lycanthrope";
				}
				if (!string.IsNullOrEmpty(text))
				{
					return TriggerCureMagicalAffliction(actor, text, out producedJob);
				}
			}
		}
		return false;
	}

	private GoapPlanJob CreateReportCrimeJob(CrimeData crimeData, ICrimeable crime)
	{
		if (owner.faction != null && owner.faction.isMajorNonPlayer && crimeData.IsWantedBy(owner.faction))
		{
			return null;
		}
		Character character = null;
		if (owner.homeSettlement != null)
		{
			Character ruler = owner.homeSettlement.ruler;
			if (ruler != null && !ruler.isInLimbo && !ruler.isDead && ruler != owner && ruler != crimeData.criminal && ruler != crimeData.target && !ruler.traitContainer.HasTrait("Travelling") && !ruler.partyComponent.isActiveMember && !crimeData.IsWitness(ruler) && ruler.currentSettlement == owner.currentSettlement)
			{
				character = ruler;
			}
		}
		if (character == null && owner.faction != null)
		{
			if (owner.faction.leader is Character { isInLimbo: false, isDead: false } character2 && character2 != owner && character2 != crimeData.criminal && character2 != crimeData.target && !character2.traitContainer.HasTrait("Travelling") && !character2.partyComponent.isActiveMember && !crimeData.IsWitness(character2) && character2.currentSettlement == owner.currentSettlement)
			{
				character = character2;
			}
			if (character == null)
			{
				Character character3 = null;
				float num = float.MaxValue;
				for (int i = 0; i < owner.faction.characters.Count; i++)
				{
					Character character4 = owner.faction.characters[i];
					if ((character4.isFactionLeader || character4.isSettlementRuler) && !character4.isDead && !character4.isInLimbo && character4 != owner && character4 != crimeData.criminal && character4 != crimeData.target && !character4.traitContainer.HasTrait("Travelling") && !character4.partyComponent.isActiveMember && !crimeData.IsWitness(character4))
					{
						float num2 = Vector2.Distance(owner.worldPosition, character4.worldPosition);
						if (num2 < num)
						{
							num = num2;
							character3 = character4;
						}
					}
				}
				if (character3 != null)
				{
					character = character3;
				}
			}
		}
		if (character != null && !owner.jobQueue.HasJob(JOB_TYPE.REPORT_CRIME, character))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPORT_CRIME, INTERACTION_TYPE.REPORT_CRIME, character, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.REPORT_CRIME, new object[2] { crime, crimeData });
			return goapPlanJob;
		}
		return null;
	}

	public bool IsCharacterGhost(Character p_character)
	{
		if (p_character is Summon summon && (summon.summonType == SUMMON_TYPE.Ghost || summon.summonType == SUMMON_TYPE.Vengeful_Ghost))
		{
			return true;
		}
		return false;
	}

	public void TriggerBuryMe()
	{
		if (owner.minion == null && !(owner is Animal) && owner.gridTileLocation != null && owner.gridTileLocation.IsNextToOrPartOfSettlement(out var settlement) && settlement is NPCSettlement nPCSettlement && !nPCSettlement.HasJob(JOB_TYPE.BURY, owner) && owner.grave == null && !IsCharacterGhost(owner) && !owner.traitContainer.HasTrait("Mummified") && (!owner.race.IsSkinnable() || !nPCSettlement.HasStructureOfTypeThatIsAssigned(STRUCTURE_TYPE.HUNTER_LODGE)) && (nPCSettlement.HasStructure(STRUCTURE_TYPE.CEMETERY) || nPCSettlement.HasStructure(STRUCTURE_TYPE.CULT_TEMPLE) || owner.previousCharacterDataComponent.homeSettlementOnDeath == nPCSettlement))
		{
			LocationStructure locationStructure = nPCSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CULT_TEMPLE) ?? nPCSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) ?? nPCSettlement.region.wilderness;
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, owner, nPCSettlement);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeBury");
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[1] { locationStructure });
			goapPlanJob.SetStillApplicableChecker("IsBurySettlementApplicable");
			nPCSettlement.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void TriggerPersonalOutsideVillageBuryJob(Character targetCharacter)
	{
		if (owner.gridTileLocation == null || owner.jobQueue.HasJob(JOB_TYPE.BURY, targetCharacter) || targetCharacter.HasJobTargetingThis(JOB_TYPE.BURY, JOB_TYPE.BURY_IN_ACTIVE_PARTY))
		{
			return;
		}
		LocationStructure locationStructure = null;
		if (!IsCharacterGhost(owner) && !owner.traitComponent.IsCharacterTargetOfObsession(targetCharacter))
		{
			if (owner.homeSettlement != null)
			{
				locationStructure = owner.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CULT_TEMPLE) ?? owner.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
			}
			if (locationStructure == null)
			{
				locationStructure = owner.currentRegion.wilderness;
			}
			if (owner.movementComponent.HasPathToEvenIfDiffRegion(locationStructure.GetRandomPassableTile()))
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter, owner);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[1] { locationStructure });
				goapPlanJob.SetStillApplicableChecker("IsBuryApplicable");
				owner.jobQueue.AddJobInQueue(goapPlanJob);
			}
		}
	}

	public void TriggerPersonalBuryInActivePartyJob(Character targetCharacter)
	{
		if (owner.gridTileLocation != null && !owner.jobQueue.HasJob(JOB_TYPE.BURY_IN_ACTIVE_PARTY, targetCharacter) && !targetCharacter.HasJobTargetingThis(JOB_TYPE.BURY, JOB_TYPE.BURY_IN_ACTIVE_PARTY) && !IsCharacterGhost(owner) && !owner.traitComponent.IsCharacterTargetOfObsession(targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY_IN_ACTIVE_PARTY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter, owner);
			goapPlanJob.SetStillApplicableChecker("IsBuryApplicable");
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool CreateGoToJob(IPointOfInterest target)
	{
		CreateGoToJob(target, out var producedJob);
		if (producedJob != null)
		{
			return owner.jobQueue.AddJobInQueue(producedJob);
		}
		return false;
	}

	public bool CreateGoToJob(IPointOfInterest target, out JobQueueItem producedJob, JOB_TYPE p_jobType = JOB_TYPE.GO_TO)
	{
		if (!owner.jobQueue.HasJob(p_jobType, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.GO_TO, target, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateGoToJob(LocationGridTile tile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateGoToJob(JOB_TYPE jobType, LocationGridTile tile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.GO_TO_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateGoToJob(LocationGridTile tile)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool CreatePartyGoToJob(LocationGridTile tile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PARTY_GO_TO))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTY_GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreatePartyGoToJob(LocationGridTile tile)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PARTY_GO_TO))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTY_GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool CreateGoToSpecificTileJob(LocationGridTile tile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO_SPECIFIC_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateGoToSpecificTileJob(LocationGridTile tile)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO_SPECIFIC_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool CreatePartyGoToSpecificTileJob(LocationGridTile tile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PARTY_GO_TO))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTY_GO_TO, INTERACTION_TYPE.GO_TO_SPECIFIC_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateGoToWaitingJob(LocationGridTile tile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO_WAITING))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO_WAITING, INTERACTION_TYPE.GO_TO_TILE, tile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetDoNotRecalculate(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateFleeCrimeJob()
	{
		LocationGridTile locationGridTile = null;
		Area area = null;
		List<Area> list = RuinarchListPool<Area>.Claim();
		if (owner.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			settlement.PopulateSurroundingAreas(list);
			if (list.Count > 0)
			{
				List<Area> list2 = RuinarchListPool<Area>.Claim(8);
				while (list.Count > 0)
				{
					int index = GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1);
					Area area2 = list[index];
					list2.Clear();
					area2.neighbourComponent.PopulateNeighboursNotInSettlementAndWithPathTo(list2, settlement, owner);
					if (list2.Count > 0)
					{
						area = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
						break;
					}
					list.RemoveAt(index);
				}
				RuinarchListPool<Area>.Release(list2);
			}
		}
		else
		{
			owner.currentRegion.PopulateAreasThatAreNextToAVillageButNotMountainAndWaterAndNoSettlementAndWithPathTo(list, owner);
			if (list.Count > 0)
			{
				Area area3 = null;
				float num = float.MaxValue;
				for (int i = 0; i < list.Count; i++)
				{
					Area area4 = list[i];
					float distanceTo = area4.gridTileComponent.centerGridTile.GetDistanceTo(owner.gridTileLocation);
					if (distanceTo < num)
					{
						area3 = area4;
						num = distanceTo;
					}
				}
				area = area3;
			}
		}
		RuinarchListPool<Area>.Release(list);
		if (area != null)
		{
			List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
			for (int j = 0; j < area.gridTileComponent.passableTiles.Count; j++)
			{
				LocationGridTile locationGridTile2 = area.gridTileComponent.passableTiles[j];
				if (owner.movementComponent.HasPathTo(locationGridTile2))
				{
					list3.Add(locationGridTile2);
				}
			}
			if (list3.Count > 0)
			{
				locationGridTile = CollectionUtilities.GetRandomElement(list3);
			}
			RuinarchListPool<LocationGridTile>.Release(list3);
		}
		if (locationGridTile != null && !owner.jobQueue.HasJob(JOB_TYPE.FLEE_CRIME))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FLEE_CRIME, INTERACTION_TYPE.FLEE_CRIME, locationGridTile.tileObjectComponent.genericTileObject, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public void TriggerSpawnLair(LocationGridTile targetTile)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_LAIR, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BUILD_LAIR, new object[1] { targetTile });
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public void TriggerSpawnLair(LocationGridTile targetTile, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_LAIR, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BUILD_LAIR, new object[1] { targetTile });
			producedJob = goapPlanJob;
		}
	}

	public bool TriggerBuildBanditCamp(LocationGridTile targetTile, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.BUILD_BANDIT_CAMP))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_BANDIT_CAMP, INTERACTION_TYPE.BUILD_BANDIT_CAMP, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BUILD_BANDIT_CAMP, new object[1] { targetTile });
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerSpawnSkeleton(out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_SKELETON))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.SPAWN_SKELETON, owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerRaiseCorpse(IPointOfInterest target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.RAISE_CORPSE))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RAISE_CORPSE, INTERACTION_TYPE.RAISE_CORPSE, target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public void TriggerRaiseCorpse(JOB_TYPE p_jobType, IPointOfInterest target, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.RAISE_CORPSE, target, owner);
			producedJob = goapPlanJob;
		}
	}

	public bool TriggerReadNecronomicon(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.READ_NECRONOMICON, owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerMeditate(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.MEDITATE, owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerRegainEnergy(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.REGAIN_ENERGY, owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateApprehend(Character p_target, NPCSettlement p_settlement, ref bool p_canApprehend, LocationStructure p_intendedPrison = null)
	{
		if (owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive && owner.partyComponent.currentParty.DidMemberJoinQuest(owner))
		{
			return false;
		}
		LocationStructure locationStructure = p_intendedPrison;
		Prisoner traitOrStatus = p_target.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		if (locationStructure == null)
		{
			locationStructure = ((traitOrStatus != null) ? traitOrStatus.GetIntendedPrisonAccordingTo(owner) : owner.GetSettlementPrisonFor(p_target));
		}
		p_canApprehend = InteractionManager.Instance.CanCharacterTakeApprehendJob(owner, p_target) && locationStructure != null && CanDoJob(JOB_TYPE.APPREHEND);
		if (p_target.currentStructure != locationStructure && p_settlement != null && p_target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(p_settlement))
		{
			if (p_target.traitContainer.HasTrait("Restrained") && traitOrStatus != null && traitOrStatus.IsConsideredPrisonerOf(owner))
			{
				if (p_settlement.HasJob(JOB_TYPE.APPREHEND, p_target, out var p_job))
				{
					if (p_job.assignedCharacter != null)
					{
						return false;
					}
					p_settlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.APPREHEND, p_target);
				}
				if (!p_settlement.HasJob(JOB_TYPE.APPREHEND_RESTRAINED, p_target, out var p_job2))
				{
					p_job2 = p_settlement.settlementJobTriggerComponent.CreateApprehendJob(JOB_TYPE.APPREHEND_RESTRAINED, p_target);
				}
				if (((p_job2 != null && p_job2.assignedCharacter == null) & p_canApprehend) && !owner.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.APPREHEND_RESTRAINED))
				{
					return owner.jobQueue.AddJobInQueue(p_job2);
				}
			}
			else
			{
				if (p_settlement.HasJob(JOB_TYPE.APPREHEND_RESTRAINED, p_target, out var p_job3))
				{
					if (p_job3.assignedCharacter != null)
					{
						return false;
					}
					p_settlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.APPREHEND_RESTRAINED, p_target);
				}
				JobQueueItem jobQueueItem = p_settlement.GetJob(JOB_TYPE.APPREHEND, p_target);
				if (jobQueueItem == null)
				{
					jobQueueItem = p_settlement.settlementJobTriggerComponent.CreateApprehendJob(JOB_TYPE.APPREHEND, p_target);
				}
				if (p_canApprehend && !owner.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.APPREHEND) && jobQueueItem.assignedCharacter == null)
				{
					bool num = p_target.traitContainer.HasTrait("Criminal") && p_target.crimeComponent.IsWantedBy(owner.faction);
					bool flag = traitOrStatus?.IsConsideredPrisonerOf(owner) ?? false;
					if (num || flag)
					{
						return owner.jobQueue.AddJobInQueue(jobQueueItem);
					}
				}
			}
		}
		return false;
	}

	public bool TryCreateSabotageNeighbourJob(Character target, out JobQueueItem producedJob)
	{
		producedJob = null;
		string text = string.Empty;
		List<Trait> allTraitsOf = target.traitContainer.GetAllTraitsOf(TRAIT_TYPE.BUFF);
		if (allTraitsOf != null && allTraitsOf.Count > 0)
		{
			Trait randomElement = CollectionUtilities.GetRandomElement(allTraitsOf);
			if (randomElement != null)
			{
				text = randomElement.name;
			}
		}
		if (text != string.Empty)
		{
			List<JobNode> list = RuinarchListPool<JobNode>.Claim();
			if (!owner.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT))
			{
				TileObject poiTarget = owner.homeStructure?.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT);
				ActualGoapNode actionNode = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PICK_UP], owner, poiTarget, null, 0);
				SingleJobNode singleJobNode = ObjectPoolManager.Instance.CreateNewSingleJobNode();
				singleJobNode.SetActionNode(actionNode);
				list.Add(singleJobNode);
			}
			ActualGoapNode actionNode2 = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.REMOVE_BUFF], owner, target, new OtherData[1]
			{
				new StringOtherData(text)
			}, 0);
			SingleJobNode singleJobNode2 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
			singleJobNode2.SetActionNode(actionNode2);
			list.Add(singleJobNode2);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(list, target);
			goapPlan.SetDoNotRecalculate(state: true);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SABOTAGE_NEIGHBOUR, INTERACTION_TYPE.REMOVE_BUFF, target, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	private bool IsValidSabotageNeighbourTarget(Character character)
	{
		AWARENESS_STATE awarenessState = owner.relationshipContainer.GetAwarenessState(owner, character);
		if (character.isNormalCharacter && !character.traitContainer.HasTrait("Travelling") && character.traitContainer.HasTrait("Resting", "Unconscious") && character.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF) && !character.traitContainer.HasTrait("Demon Cultist") && owner.HasSameHomeAs(character) && awarenessState != AWARENESS_STATE.Missing)
		{
			return awarenessState != AWARENESS_STATE.Presumed_Dead;
		}
		return false;
	}

	public bool TryGetValidSabotageNeighbourTarget(out Character targetCharacter)
	{
		List<Character> list = null;
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (IsValidSabotageNeighbourTarget(character))
			{
				if (list == null)
				{
					list = new List<Character>();
				}
				list.Add(character);
			}
		}
		if (list != null)
		{
			WeightedDictionary<Character> weightedDictionary = new WeightedDictionary<Character>();
			for (int j = 0; j < list.Count; j++)
			{
				Character character2 = list[j];
				int num = 0;
				switch (owner.relationshipContainer.GetOpinionLabel(character2))
				{
				case "Close Friend":
					num += Random.Range(10, 51);
					break;
				case "Friend":
					num += Random.Range(100, 151);
					break;
				case "Acquaintance":
					num += Random.Range(150, 251);
					break;
				case "Enemy":
				case "Rival":
					num += Random.Range(200, 351);
					break;
				}
				weightedDictionary.AddElement(character2, num);
			}
			if (weightedDictionary.GetTotalOfWeights() > 0)
			{
				targetCharacter = weightedDictionary.PickRandomElementGivenWeights();
				return true;
			}
		}
		targetCharacter = null;
		return false;
	}

	public void TriggerPray()
	{
		TriggerPray(out var producedJob);
		if (producedJob != null)
		{
			owner.jobQueue.AddJobInQueue(producedJob);
		}
	}

	public void TriggerPray(out JobQueueItem producedJob, JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.PRAY, owner, owner);
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PRAY], owner, owner, null, 0);
		ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner).SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		producedJob = goapPlanJob;
	}

	public bool TriggerSpawnPoisonCloud(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.SPAWN_POISON_CLOUD, owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerSpawnPoisonCloud(JOB_TYPE jobType, int minStacks, int maxStacks, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SPAWN_POISON_CLOUD, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.SPAWN_POISON_CLOUD, new object[2] { minStacks, maxStacks });
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerDecreaseMood(out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DECREASE_MOOD, INTERACTION_TYPE.DECREASE_MOOD, owner, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TriggerDisable(out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DISABLE, INTERACTION_TYPE.DISABLE, owner, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TryTriggerLayEgg(Character character, int maxResidentCount, TILE_OBJECT_TYPE eggType, out JobQueueItem producedJob)
	{
		int num = 0;
		int num2 = 0;
		if (character.homeSettlement != null)
		{
			num = ((!(owner is Broodmother)) ? character.homeSettlement.residents.Count((Character x) => !x.isDead && x.race == character.race) : character.homeSettlement.residents.Count((Character x) => !x.isDead && (x is GiantSpider || x is SmallSpider)));
			num2 = character.homeSettlement.GetNumberOfTileObjectsInAllStructures(eggType);
		}
		else if (character.homeStructure != null)
		{
			num = character.homeStructure.residents.Count((Character x) => !x.isDead && x.race == character.race);
			num2 = character.homeStructure.GetNumberOfTileObjects(eggType);
		}
		else if (character.HasTerritory())
		{
			num = character.homeRegion.GetCountOfAliveCharacterWithSameTerritoryAndRace(character);
			num2 = character.territory.tileObjectComponent.GetNumberOfTileObjects(eggType);
		}
		if (num < maxResidentCount && num2 < 2)
		{
			return character.jobComponent.TriggerLayEgg(out producedJob);
		}
		producedJob = null;
		return false;
	}

	public bool TriggerLayEgg(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.LAY_EGG, owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerMonsterAbduct(Character targetCharacter, out JobQueueItem producedJob, LocationGridTile targetTile = null)
	{
		LocationStructure locationStructure = ((targetTile == null) ? owner.homeStructure : targetTile.structure);
		if (locationStructure == null)
		{
			producedJob = null;
			return false;
		}
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, (targetTile == null) ? new object[1] { locationStructure } : new object[2] { locationStructure, targetTile });
		goapPlanJob.SetDoNotRecalculate(state: true);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TryTriggerAbductCharacter(JOB_TYPE jobType, Character targetCharacter, LocationStructure dropLocationStructure, out JobQueueItem producedJob, bool doNotRecalculate = false)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(jobType, targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { dropLocationStructure });
			goapPlanJob.SetDoNotRecalculate(doNotRecalculate);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerEatAlive(Character webbedCharacter, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT_ALIVE, webbedCharacter, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TriggerTorture(Character targetCharacter, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TORTURE, INTERACTION_TYPE.TORTURE, targetCharacter, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TriggerImpregnate(Character targetCharacter, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IMPREGNATE, INTERACTION_TYPE.IMPREGNATE, targetCharacter, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TriggerBirthRatman(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.BIRTH_RATMAN, owner, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerEatCorpse(Character targetCharacter)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.MONSTER_EAT_CORPSE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT_CORPSE, INTERACTION_TYPE.EAT_CORPSE, targetCharacter, owner);
			goapPlanJob.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			if (owner.jobQueue.AddJobInQueue(goapPlanJob))
			{
				owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
				return true;
			}
			return false;
		}
		return false;
	}

	public bool TriggerArson(TileObject target, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ARSON, INTERACTION_TYPE.BURN, target, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TriggerArson(JOB_TYPE jobType, TileObject target, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BURN, target, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public GoapPlanJob TriggerArson(TileObject target, JOB_TYPE jobType = JOB_TYPE.ARSON)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BURN, target, owner);
		if (owner.jobQueue.AddJobInQueue(goapPlanJob))
		{
			return goapPlanJob;
		}
		return null;
	}

	public bool TriggerArsonRaid(TileObject target, ref JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ARSON_RAID))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ARSON_RAID, INTERACTION_TYPE.BURN, target, owner);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerSeekShelterJob()
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SEEK_SHELTER) && owner.gridTileLocation != null)
		{
			List<LocationStructure> exclusions = null;
			string text = string.Empty;
			if (owner.traitContainer.HasTrait("Freezing"))
			{
				exclusions = owner.traitContainer.GetTraitOrStatus<Freezing>("Freezing").excludedStructuresInSeekingShelter;
				text = "Freezing";
			}
			else if (owner.traitContainer.HasTrait("Overheating"))
			{
				exclusions = owner.traitContainer.GetTraitOrStatus<Overheating>("Overheating").excludedStructuresInSeekingShelter;
				text = "Overheating";
			}
			LocationStructure nearestInteriorStructureFromThisExcept = owner.gridTileLocation.GetNearestInteriorStructureFromThisExcept(exclusions);
			if (nearestInteriorStructureFromThisExcept != null && !string.IsNullOrEmpty(text))
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SEEK_SHELTER, INTERACTION_TYPE.TAKE_SHELTER, owner, owner);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_SHELTER, new object[2] { nearestInteriorStructureFromThisExcept, text });
				owner.jobQueue.AddJobInQueue(goapPlanJob);
				return true;
			}
		}
		return false;
	}

	public bool TryCreateDarkRitualJob(out JobQueueItem producedJob)
	{
		if (owner.currentRegion != null)
		{
			LocationGridTile locationGridTile = null;
			bool flag = false;
			if (owner.currentRegion.HasTileObjectOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE) && owner.gridTileLocation != null)
			{
				List<TileObject> list = RuinarchListPool<TileObject>.Claim();
				owner.currentRegion.PopulateBuiltTileObjectsOfTypeWithAreaDistanceFrom(list, TILE_OBJECT_TYPE.MAGIC_CIRCLE, owner.gridTileLocation.area, 4);
				for (int i = 0; i < list.Count; i++)
				{
					TileObject tileObject = list[i];
					if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsNextToOrPartOfSettlement(out var settlement) && settlement.owner != null && (settlement.owner.IsHostileWith(owner.faction) || settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Demon_Worship)))
					{
						list.RemoveAt(i);
						i--;
					}
				}
				if (CollectionUtilities.GetRandomElement(list) is MagicCircle magicCircle)
				{
					locationGridTile = magicCircle.gridTileLocation;
					flag = true;
				}
				RuinarchListPool<TileObject>.Release(list);
			}
			if (locationGridTile == null)
			{
				if (owner.homeSettlement != null)
				{
					List<Area> list2 = RuinarchListPool<Area>.Claim();
					List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
					owner.homeSettlement.PopulateSurroundingAreas(list2);
					for (int j = 0; j < list2.Count; j++)
					{
						Area area = list2[j];
						bool flag2 = true;
						for (int k = 0; k < area.settlementsOnArea.Count; k++)
						{
							BaseSettlement baseSettlement = area.settlementsOnArea[k];
							if (baseSettlement.owner != null && (baseSettlement.owner.IsHostileWith(owner.faction) || baseSettlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Demon_Worship)))
							{
								flag2 = false;
								break;
							}
						}
						if (!flag2)
						{
							continue;
						}
						for (int l = 0; l < area.gridTileComponent.passableTiles.Count; l++)
						{
							LocationGridTile locationGridTile2 = area.gridTileComponent.passableTiles[l];
							if (!locationGridTile2.isOccupied && locationGridTile2.tileObjectComponent.objHere == null && locationGridTile2.tileObjectComponent.hiddenObjHere == null && locationGridTile2.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
							{
								list3.Add(locationGridTile2);
							}
						}
					}
					if (list3.Count > 0)
					{
						locationGridTile = CollectionUtilities.GetRandomElement(list3);
					}
					RuinarchListPool<Area>.Release(list2);
					RuinarchListPool<LocationGridTile>.Release(list3);
				}
				if (locationGridTile == null)
				{
					List<LocationGridTile> unoccupiedTiles = owner.currentRegion.wilderness.unoccupiedTiles;
					if (unoccupiedTiles.Count > 0)
					{
						locationGridTile = CollectionUtilities.GetRandomElement(unoccupiedTiles);
					}
				}
			}
			if (locationGridTile != null)
			{
				GenericTileObject genericTileObject = locationGridTile.tileObjectComponent.genericTileObject;
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DARK_RITUAL, INTERACTION_TYPE.DARK_RITUAL, genericTileObject, owner);
				JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
				if (!flag)
				{
					ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRAW_MAGIC_CIRCLE], owner, genericTileObject, null, 0);
					ActualGoapNode p_action2 = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DARK_RITUAL], owner, genericTileObject, null, 0);
					GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, p_action2, genericTileObject);
					goapPlan.SetDoNotRecalculate(state: true);
					goapPlanJob.SetAssignedPlan(goapPlan);
				}
				producedJob = goapPlanJob;
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	public void TriggerCultistSacrifice()
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SACRIFICE_SELF))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SACRIFICE_SELF, INTERACTION_TYPE.SACRIFICE_SELF, owner, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}

	public bool TriggerMonsterInvadeJob(LocationStructure targetStructure, out JobQueueItem producedJob)
	{
		if (!owner.partyComponent.hasParty)
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MONSTER_INVADE], owner, owner, new OtherData[1]
			{
				new LocationStructureOtherData(targetStructure)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_INVADE, INTERACTION_TYPE.MONSTER_INVADE, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerMonsterInvadeJob(Area p_targetArea, out JobQueueItem producedJob)
	{
		if (!owner.partyComponent.hasParty)
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MONSTER_INVADE], owner, owner, new OtherData[1]
			{
				new AreaOtherData(p_targetArea)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_INVADE, INTERACTION_TYPE.MONSTER_INVADE, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerReleaseJob(Character targetCharacter)
	{
		return TriggerReleaseJob(JOB_TYPE.RELEASE_CHARACTER, targetCharacter);
	}

	public bool TriggerReleaseJob(JOB_TYPE p_jobType, Character targetCharacter)
	{
		JobQueueItem producedJob = null;
		TriggerReleaseJob(p_jobType, targetCharacter, out producedJob);
		if (producedJob != null)
		{
			return owner.jobQueue.AddJobInQueue(producedJob);
		}
		return false;
	}

	public bool TriggerReleaseJob(JOB_TYPE p_jobType, Character targetCharacter, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(p_jobType, targetCharacter))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RELEASE_CHARACTER], owner, targetCharacter, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetCharacter);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.RELEASE_CHARACTER, targetCharacter, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool TriggerDisguiseJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DISGUISE], owner, targetCharacter, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetCharacter);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.DISGUISE, targetCharacter, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerMakeLoveJob(JOB_TYPE p_jobType, Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.MAKE_LOVE, targetCharacter, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerSeduce(Character p_target, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SEDUCE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SEDUCE, INTERACTION_TYPE.SEDUCE, p_target, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void TriggerInspect(TileObject item)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, item, owner);
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.INSPECT], owner, item, null, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, item);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		owner.jobQueue.AddJobInQueue(goapPlanJob);
	}

	public bool TriggerFactionKidnapAndRestrainJob(Character target)
	{
		return TriggerFactionKidnapJob(target);
	}

	public bool TriggerFactionKidnapJob(Character target)
	{
		if (owner.homeSettlement != null && owner.homeSettlement.prison != null && target.gridTileLocation.structure != owner.homeSettlement.prison && !owner.jobQueue.HasJob(JOB_TYPE.FACTION_KIDNAP))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FACTION_KIDNAP, INTERACTION_TYPE.DROP_RESTRAINED, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { owner.homeSettlement.prison });
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerKidnapRaidJob(Character target)
	{
		if (owner.homeSettlement != null && owner.homeSettlement.prison != null && !owner.jobQueue.HasJob(JOB_TYPE.KIDNAP_RAID))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KIDNAP_RAID, INTERACTION_TYPE.DROP_RESTRAINED, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { owner.homeSettlement.prison });
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerRecruitJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.RECRUIT))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RECRUIT], owner, targetCharacter, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetCharacter);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECRUIT, INTERACTION_TYPE.RECRUIT, targetCharacter, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerRecruitJob(Character targetCharacter)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.RECRUIT))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RECRUIT], owner, targetCharacter, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetCharacter);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECRUIT, INTERACTION_TYPE.RECRUIT, targetCharacter, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerBuildTrollCauldronJob(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_TROLL_CAULDRON], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.BUILD_TROLL_CAULDRON, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerCookJob(Character targetCharacter, TileObject whereToCook, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, INTERACTION_TYPE.COOK, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.COOK, new object[1] { whereToCook });
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerRestrainJob(Character target, JOB_TYPE jobType)
	{
		if (TriggerRestrainJob(target, jobType, out var producedJob))
		{
			return owner.jobQueue.AddJobInQueue(producedJob);
		}
		return false;
	}

	public bool TriggerRestrainJob(Character target, JOB_TYPE jobType, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(jobType))
		{
			producedJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.RESTRAIN_CHARACTER, target, owner);
			return true;
		}
		return false;
	}

	public bool TriggerSingJob(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SING], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.SING, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerDanceJob(out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DANCE], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.DANCE, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerPartyDrinkJob(Table table, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRINK], owner, table, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.DRINK, table, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerPlayCardsJob(Desk desk, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PLAY_CARDS], owner, desk, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.PLAY_CARDS, desk, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void TriggerSpawnWolfLair(LocationGridTile targetTile, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_WOLF_LAIR, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BUILD_WOLF_LAIR, new object[1] { targetTile });
			producedJob = goapPlanJob;
		}
	}

	public bool TriggerDrinkJob(JOB_TYPE jobType, Table table, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRINK], owner, table, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DRINK, table, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerBuildCampfireJob(JOB_TYPE jobType, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(jobType))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_CAMPFIRE], owner, owner, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BUILD_CAMPFIRE, owner, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerWarmUp(Campfire campfire, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.WARM_UP))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.WARM_UP], owner, campfire, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.WARM_UP, INTERACTION_TYPE.WARM_UP, campfire, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCannotBePushedBack(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	private bool IsValidEvangelizeTarget(Character character, RELIGION p_religion)
	{
		AWARENESS_STATE awarenessState = owner.relationshipContainer.GetAwarenessState(owner, character);
		if (character.traitContainer.HasTrait("Enslaved") || character.traitContainer.HasTrait("Restrained"))
		{
			if (owner.homeSettlement == null)
			{
				return false;
			}
			if (character.currentSettlement != owner.homeSettlement)
			{
				return false;
			}
		}
		if (character.isNormalCharacter && !character.traitContainer.HasTrait("Travelling") && !character.traitContainer.IsReligiousCultist(p_religion) && owner.HasSameHomeAs(character) && awarenessState != AWARENESS_STATE.Missing)
		{
			return awarenessState != AWARENESS_STATE.Presumed_Dead;
		}
		return false;
	}

	private bool IsValidEvangelizeTargetAtDifferentVillage(Character character)
	{
		AWARENESS_STATE awarenessState = owner.relationshipContainer.GetAwarenessState(owner, character);
		if (character.isNormalCharacter && !character.traitContainer.HasTrait("Travelling") && !character.traitContainer.HasTrait("Demon Cultist") && awarenessState != AWARENESS_STATE.Missing)
		{
			return awarenessState != AWARENESS_STATE.Presumed_Dead;
		}
		return false;
	}

	public bool TryGetValidEvangelizeTarget(out Character targetCharacter, RELIGION p_religion)
	{
		List<Character> list = null;
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (IsValidEvangelizeTarget(character, p_religion))
			{
				if (list == null)
				{
					list = new List<Character>();
				}
				list.Add(character);
			}
		}
		if (list != null)
		{
			WeightedDictionary<Character> weightedDictionary = new WeightedDictionary<Character>();
			for (int j = 0; j < list.Count; j++)
			{
				Character character2 = list[j];
				int num = 0;
				switch (owner.relationshipContainer.GetOpinionLabel(character2))
				{
				case "Close Friend":
					num += Random.Range(300, 451);
					break;
				case "Friend":
					num += Random.Range(100, 251);
					break;
				case "Acquaintance":
					num += Random.Range(10, 51);
					break;
				}
				weightedDictionary.AddElement(character2, num);
			}
			if (weightedDictionary.GetTotalOfWeights() > 0)
			{
				targetCharacter = weightedDictionary.PickRandomElementGivenWeights();
				return true;
			}
		}
		targetCharacter = null;
		return false;
	}

	public bool TryGetValidEvangelizeTargetInsideVillage(out Character targetCharacter, NPCSettlement p_village)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < p_village.residents.Count; i++)
		{
			Character character = p_village.residents[i];
			if (character.currentSettlement == p_village && IsValidEvangelizeTargetAtDifferentVillage(character))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			targetCharacter = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<Character>.Release(list);
			return true;
		}
		RuinarchListPool<Character>.Release(list);
		targetCharacter = null;
		return false;
	}

	public bool TryCreateEvangelizeJob(Character p_target, out JobQueueItem producedJob)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PREACH, INTERACTION_TYPE.EVANGELIZE, p_target, owner);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TryCreateLiberateJob(Character target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PREACH, target))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PREACH, INTERACTION_TYPE.LIBERATE, target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool TryCreateEvangelizeJob(Character p_target, JOB_TYPE p_jobType = JOB_TYPE.PREACH)
	{
		if (!owner.jobQueue.HasJob(p_jobType, p_target))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.EVANGELIZE, p_target, owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool CreateSnatchJob(Character targetCharacter, LocationGridTile targetLocation, LocationStructure structure)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SNATCH, targetCharacter))
		{
			owner.behaviourComponent.SetIsSnatching(state: true);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SNATCH, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { structure, targetLocation });
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerStealRaidJob(ResourcePile target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.STEAL_RAID) && target.gridTileLocation.parentMap.region == owner.currentRegion && owner.homeSettlement != null)
		{
			ResourcePile resourcePileObjectWithLowestCount = owner.homeSettlement.mainStorage.GetResourcePileObjectWithLowestCount(target.tileObjectType);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL_RAID, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), target, owner);
			if (resourcePileObjectWithLowestCount != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { resourcePileObjectWithLowestCount });
			}
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerStealRaidJob(EquipmentItem target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.STEAL_RAID) && target.gridTileLocation.parentMap.region == owner.currentRegion && owner.homeSettlement != null)
		{
			LocationStructure locationStructure = owner.homeSettlement?.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (locationStructure == null)
			{
				locationStructure = owner.homeSettlement?.GetRandomStructure();
			}
			if (locationStructure == null)
			{
				locationStructure = owner.homeStructure;
			}
			if (locationStructure != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL_RAID, INTERACTION_TYPE.DROP, target, owner);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { locationStructure });
				return owner.jobQueue.AddJobInQueue(goapPlanJob);
			}
		}
		return false;
	}

	public bool TriggerPlaceBlueprint(string structurePrefabName, StructureSetting structureSetting, LocationGridTile centerTile, LocationGridTile connectorTile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PLACE_BLUEPRINT))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PLACE_BLUEPRINT], owner, centerTile.tileObjectComponent.genericTileObject, new OtherData[3]
			{
				new StringOtherData(structurePrefabName),
				new LocationGridTileOtherData(connectorTile),
				new StructureSettingOtherData(structureSetting)
			}, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, centerTile.tileObjectComponent.genericTileObject);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.PLACE_BLUEPRINT, centerTile.tileObjectComponent.genericTileObject, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerBuildVampireCastle(LocationGridTile targetTile, out JobQueueItem producedJob, string structurePrefabName = "")
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.BUILD_VAMPIRE_CASTLE))
		{
			OtherData[] otherData = new OtherData[1]
			{
				new StringOtherData(structurePrefabName)
			};
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE], owner, targetTile.tileObjectComponent.genericTileObject, otherData, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetTile.tileObjectComponent.genericTileObject);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_VAMPIRE_CASTLE, INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE, targetTile.tileObjectComponent.genericTileObject, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerImprisonBloodSource(LocationStructure dropStructure, out JobQueueItem producedJob, ref string log)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IMPRISON_BLOOD_SOURCE))
		{
			WeightedDictionary<Character> weightedDictionary = new WeightedDictionary<Character>();
			foreach (KeyValuePair<int, IRelationshipData> relationship in owner.relationshipContainer.relationships)
			{
				Character characterByID = CharacterManager.Instance.GetCharacterByID(relationship.Key);
				if (characterByID == null || characterByID.isDead || characterByID.traitContainer.GetTraitOrStatus<Trait>("Vampire") != null)
				{
					continue;
				}
				string opinionLabel = relationship.Value.opinions.GetOpinionLabel();
				switch (opinionLabel)
				{
				case "Acquaintance":
				case "Enemy":
				case "Rival":
				{
					int num = 0;
					switch (opinionLabel)
					{
					case "Acquaintance":
						num += 10;
						break;
					case "Enemy":
						num += 50;
						break;
					case "Rival":
						num += 100;
						break;
					}
					if (characterByID.homeSettlement != owner.homeSettlement)
					{
						num += 200;
					}
					if (characterByID.faction != owner.faction)
					{
						num *= 3;
					}
					if (num > 0)
					{
						weightedDictionary.AddElement(characterByID, num);
					}
					break;
				}
				}
			}
			List<Character> list = owner.currentRegion.charactersAtLocation.Where((Character x) => x is Animal && !x.isDead).ToList();
			for (int num2 = 0; num2 < 3; num2++)
			{
				if (list.Count == 0)
				{
					break;
				}
				Character randomElement = CollectionUtilities.GetRandomElement(list);
				weightedDictionary.AddElement(randomElement, 100);
				list.Remove(randomElement);
			}
			if (weightedDictionary.GetTotalOfWeights() > 0)
			{
				Character target = weightedDictionary.PickRandomElementGivenWeights();
				return TriggerImprisonBloodSource(target, dropStructure, out producedJob);
			}
		}
		producedJob = null;
		return false;
	}

	public bool TriggerImprisonBloodSource(Character target, LocationStructure dropStructure, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IMPRISON_BLOOD_SOURCE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IMPRISON_BLOOD_SOURCE, INTERACTION_TYPE.DROP, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP, new object[1] { dropStructure });
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerFindNewVillage(LocationGridTile targetTile, string structurePrefabName = "")
	{
		if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !owner.jobQueue.HasJob(JOB_TYPE.FIND_NEW_VILLAGE))
		{
			OtherData[] otherData = new OtherData[1]
			{
				new StringOtherData(structurePrefabName)
			};
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_NEW_VILLAGE], owner, targetTile.tileObjectComponent.genericTileObject, otherData, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetTile.tileObjectComponent.genericTileObject);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FIND_NEW_VILLAGE, INTERACTION_TYPE.BUILD_NEW_VILLAGE, targetTile.tileObjectComponent.genericTileObject, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TriggerFindNewVillage(LocationGridTile targetTile, out JobQueueItem producedJob, string structurePrefabName = "")
	{
		if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !owner.jobQueue.HasJob(JOB_TYPE.FIND_NEW_VILLAGE))
		{
			OtherData[] otherData = new OtherData[1]
			{
				new StringOtherData(structurePrefabName)
			};
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_NEW_VILLAGE], owner, targetTile.tileObjectComponent.genericTileObject, otherData, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetTile.tileObjectComponent.genericTileObject);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FIND_NEW_VILLAGE, INTERACTION_TYPE.BUILD_NEW_VILLAGE, targetTile.tileObjectComponent.genericTileObject, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void TriggerCureMagicalAffliction(Character target, string traitName)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION) && !target.isInLimbo)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION, INTERACTION_TYPE.DISPEL, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DISPEL, new object[1] { traitName });
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool TriggerCureMagicalAffliction(Character target, string traitName, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION) && !target.isInLimbo)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION, INTERACTION_TYPE.DISPEL, target, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DISPEL, new object[1] { traitName });
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerHuntPreyJob(Character target, bool p_isFlawTriggered = false)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.LYCAN_HUNT_PREY))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.LYCAN_HUNT_PREY, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), target, owner);
			goapPlanJob.SetCancelOnDeath(state: false);
			if (p_isFlawTriggered)
			{
				goapPlanJob.isTriggeredFlaw = true;
			}
			goapPlanJob.SetDoNotRecalculate(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool CreateRatFullnessRecovery(BaseSettlement targetSettlement, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
			goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.EAT, targetSettlement);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public void TriggerQuarantineJob(Character target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.QUARANTINE, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.QUARANTINE, INTERACTION_TYPE.QUARANTINE, target, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool TriggerPersonalPatrol(out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PATROL) && !owner.traitContainer.HasTrait("Patrolling"))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.START_PATROL, owner, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerIdleBurrow(LocationGridTile p_targetTile, out JobQueueItem p_producedJob)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BURROW], owner, owner, new OtherData[1]
		{
			new LocationGridTileOtherData(p_targetTile)
		}, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.BURROW, owner, owner);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		p_producedJob = goapPlanJob;
		return true;
	}

	public bool TriggerTritonKidnap(Character targetCharacter, LocationStructure dropLocationStructure, LocationGridTile dropTileLocation)
	{
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.TRITON_KIDNAP))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRITON_KIDNAP, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { dropLocationStructure, dropTileLocation });
			goapPlanJob.SetDoNotRecalculate(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public void TryCreateDisposeFoodPileJob(FoodPile target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.DISPOSE_FOOD_PILE, target) && !target.HasJobTargetingThis(JOB_TYPE.DISPOSE_FOOD_PILE) && target.IsAvailable())
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DISPOSE_FOOD_PILE, INTERACTION_TYPE.DISPOSE_FOOD, target, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}

	public bool TriggerRobLocation(LocationStructure p_target, INTERACTION_TYPE p_actionType, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.KLEPTOMANIAC_STEAL))
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			p_target.PopulateTileObjectsListWithAllTileObjects(list);
			list.Shuffle();
			TileObject tileObject = null;
			for (int i = 0; i < list.Count; i++)
			{
				TileObject tileObject2 = list[i];
				if (tileObject2.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject2.OccupiesTile() && !tileObject2.traitContainer.HasTrait("Immovable") && !tileObject2.hiddenComponent.isHidden && owner.villagerWantsComponent.CanItemSatisfyWant(tileObject2) && !owner.HasItem(tileObject2.tileObjectType))
				{
					tileObject = tileObject2;
					break;
				}
			}
			if (tileObject == null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					TileObject tileObject3 = list[j];
					if (tileObject3.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject3.OccupiesTile() && !tileObject3.traitContainer.HasTrait("Immovable") && !tileObject3.hiddenComponent.isHidden)
					{
						tileObject = tileObject3;
						break;
					}
				}
			}
			if (tileObject != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KLEPTOMANIAC_STEAL, p_actionType, tileObject, owner);
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.STEAL_ANYTHING, owner.homeSettlement);
				p_producedJob = goapPlanJob;
				return true;
			}
			RuinarchListPool<TileObject>.Release(list);
		}
		p_producedJob = null;
		return false;
	}

	public void LoadReferences(SaveDataCharacterJobTriggerComponent data)
	{
		foreach (KeyValuePair<INTERACTION_TYPE, ActionTrackingData> item in actionTracker)
		{
			for (int i = 0; i < item.Value.decreaseSchedules.Count; i++)
			{
				GameDate p_dueDate = item.Value.decreaseSchedules[i];
				ScheduleActionCounterDecrease(p_dueDate, item.Key);
			}
		}
	}

	public void TriggerAbsorbPowerCrystal(TileObject p_crystal)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ABSORB_CRYSTAL))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_CRYSTAL, INTERACTION_TYPE.ABSORB_POWER_CRYSTAL, p_crystal, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}

	public bool TriggerCraftEquipmentJob(TILE_OBJECT_TYPE tileObjectType, LocationStructure p_workStructure, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.CRAFT_EQUIPMENT))
		{
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType);
			if (tileObject is EquipmentItem equipmentItem && p_workStructure.AddPOI(tileObject))
			{
				tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_EQUIPMENT, INTERACTION_TYPE.CRAFT_EQUIPMENT, tileObject, owner);
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, p_workStructure);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { equipmentItem.equipmentData.resourceAmount });
				producedJob = goapPlanJob;
				return true;
			}
		}
		return false;
	}

	public bool TriggerCraftLegendaryEquipmentJob(TILE_OBJECT_TYPE p_legendaryEquipmentType, LegendaryForgeTileObject p_legendaryForge, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.CRAFT_EQUIPMENT))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_EQUIPMENT, INTERACTION_TYPE.CRAFT_LEGENDARY_EQUIPMENT, p_legendaryForge, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CRAFT_LEGENDARY_EQUIPMENT, new object[1] { p_legendaryEquipmentType.ToStringEnum() });
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool ResumeCraftEquipment(EquipmentItem p_unfinishedEquipment, LocationStructure p_workStructure, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.CRAFT_EQUIPMENT))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_EQUIPMENT, INTERACTION_TYPE.CRAFT_EQUIPMENT, p_unfinishedEquipment, owner);
			goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, p_workStructure);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { p_unfinishedEquipment.equipmentData.resourceAmount });
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void TriggerCraftHospiceAntidote(TileObject p_tileObject, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.CREATE_HOSPICE_ANTIDOTE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CREATE_HOSPICE_ANTIDOTE, INTERACTION_TYPE.CREATE_HOSPICE_ANTIDOTE, p_tileObject, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public void TriggerCraftWorkplacePotion(TileObject p_tileObject, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.CREATE_WORKPLACE_POTION))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CREATE_WORKPLACE_POTION, INTERACTION_TYPE.CREATE_WORKPLACE_POTION, p_tileObject, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public void TriggerGatherHerb(TileObject p_tileObject, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.GATHER_HERB))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GATHER_HERB, INTERACTION_TYPE.GATHER_HERB, p_tileObject, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public void TriggerHealerCureCharacter(Character p_character, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.HEALER_CURE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HEALER_CURE, INTERACTION_TYPE.HEALER_CURE, p_character, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public void TriggerKickOutOfHospiceCharacter(Character p_character, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.KICK_OUT))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KICK_OUT, INTERACTION_TYPE.KICK_OUT_OF_HOSPICE, p_character, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public void TriggerMineOre(TileObject p_tileObject, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.MINE_ORE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MINE_ORE, INTERACTION_TYPE.MINE_ORE, p_tileObject, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public void TriggerMineStone(TileObject p_tileObject, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.MINE_STONE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MINE_STONE, INTERACTION_TYPE.MINE_STONE, p_tileObject, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public bool TryRecuperate(TileObject p_tileObject, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.RECUPERATE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECUPERATE, INTERACTION_TYPE.RECUPERATE, p_tileObject, owner);
			jobQueueItem = goapPlanJob;
			return true;
		}
		return false;
	}

	public void TriggerFindFish(FishingSpot p_tileObject)
	{
		TriggerFindFish(p_tileObject, out var producedJob);
		if (producedJob != null)
		{
			owner.jobQueue.AddJobInQueue(producedJob);
		}
	}

	public void TriggerFindFish(FishingSpot p_tileObject, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.FIND_FISH))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FIND_FISH, INTERACTION_TYPE.FIND_FISH, p_tileObject, owner);
			producedJob = goapPlanJob;
		}
	}

	public void TriggerHarvestCrops(TileObject p_tileObject)
	{
		TriggerHarvestCrops(p_tileObject, out var producedJob);
		if (producedJob != null)
		{
			owner.jobQueue.AddJobInQueue(producedJob);
		}
	}

	public void TriggerHarvestCrops(TileObject p_tileObject, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.HARVEST_CROPS))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HARVEST_CROPS, INTERACTION_TYPE.HARVEST_CROPS, p_tileObject, owner);
			producedJob = goapPlanJob;
		}
	}

	public void TriggerTillTile(GenericTileObject p_tileObject)
	{
		TriggerTillTile(p_tileObject, out var producedJob);
		if (producedJob != null)
		{
			owner.jobQueue.AddJobInQueue(producedJob);
		}
	}

	public void TriggerTillTile(GenericTileObject p_tileObject, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.TILL_TILE))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TILL_TILE, INTERACTION_TYPE.TILL_TILE, p_tileObject, owner);
			producedJob = goapPlanJob;
		}
	}

	public void TriggerTendCrop(Character p_worker, Crops p_crop, out JobQueueItem p_producedJob)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.TEND], p_worker, p_crop, null, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, p_crop);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TEND_FARM, INTERACTION_TYPE.TEND, p_crop, p_worker);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		p_producedJob = goapPlanJob;
	}

	public void TriggerShearAnimal(Character p_animal, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.SHEAR_ANIMAL))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SHEAR_ANIMAL, INTERACTION_TYPE.SHEAR_ANIMAL, p_animal, owner);
			producedJob = goapPlanJob;
		}
	}

	public void TriggerSkinAnimal(Character p_animal, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.SKIN_ANIMAL))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SKIN_ANIMAL, INTERACTION_TYPE.SKIN_ANIMAL, p_animal, owner);
			producedJob = goapPlanJob;
		}
	}

	public void TriggerChopWood(TileObject p_tree, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.CHOP_WOOD))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHOP_WOOD, INTERACTION_TYPE.CHOP_WOOD, p_tree, owner);
			jobQueueItem = goapPlanJob;
		}
	}

	public void TryCreateHaulJobItem(TileObject target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.HAUL))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, target, owner);
			if (owner.structureComponent.workPlaceStructure != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { owner.structureComponent.workPlaceStructure });
			}
			if (goapPlanJob != null)
			{
				owner.jobQueue.AddJobInQueue(goapPlanJob);
			}
		}
	}

	public void TryCreateHaulJob(ResourcePile target, LocationStructure dropStructure, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.HAUL))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, target, owner);
			if (dropStructure != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { dropStructure });
			}
			jobQueueItem = goapPlanJob;
		}
	}

	public void TryCreateHaulJobForOnSightResourcePile(ResourcePile target, LocationStructure dropStructure)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.HAUL_ON_SIGHT) && !target.HasJobTargetingThis(JOB_TYPE.HAUL_ON_SIGHT) && (target.gridTileLocation == null || target.gridTileLocation.structure != dropStructure) && (dropStructure == null || dropStructure.HasUnoccupiedTile()))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL_ON_SIGHT, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, target, owner);
			if (dropStructure != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { dropStructure });
			}
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public void TryCreateHaulJobForCrafter(ResourcePile target, out JobQueueItem jobQueueItem, int p_amount)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.HAUL) && owner.structureComponent.workPlaceStructure != null && target.structureLocation != null)
		{
			ManMadeStructure workPlaceStructure = owner.structureComponent.workPlaceStructure;
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(target.tileObjectType);
			if (workPlaceStructure.AddPOI(tileObject))
			{
				tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE, tileObject, owner);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { p_amount });
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, target.structureLocation);
				jobQueueItem = goapPlanJob;
			}
		}
	}

	public void TryCreateHaulToWorkplaceJob(ResourcePile target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.HAUL) && owner.structureComponent.workPlaceStructure != null && owner.structureComponent.workPlaceStructure != target.structureLocation)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, target, owner);
			if (owner.structureComponent.workPlaceStructure != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { owner.structureComponent.workPlaceStructure });
			}
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public void TryCreateCombineStockpile(ResourcePile p_pileToDeposit, ResourcePile targetDrop, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.COMBINE_STOCKPILE) && targetDrop != null)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMBINE_STOCKPILE, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, p_pileToDeposit, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { targetDrop });
			jobQueueItem = goapPlanJob;
		}
	}

	public bool TryCreateStockpileFood(Character p_character, LocationStructure p_preferredStore, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.STOCKPILE_FOOD))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STOCKPILE_FOOD, INTERACTION_TYPE.STOCKPILE_FOOD, p_character, p_character);
			goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.BUY_FOOD, p_preferredStore);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateBuyFoodForTavernTable(TileObject p_targetTable, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.BUY_FOOD_FOR_TAVERN))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUY_FOOD_FOR_TAVERN, INTERACTION_TYPE.DROP_RESOURCE, p_targetTable, owner);
			if (owner.homeSettlement != null)
			{
				for (int i = 0; i < owner.homeSettlement.allStructures.Count; i++)
				{
					LocationStructure locationStructure = owner.homeSettlement.allStructures[i];
					if (locationStructure.structureType.IsFoodProducingStructure())
					{
						goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.BUY_FOOD, locationStructure);
					}
				}
			}
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateCraftFurniture(TILE_OBJECT_TYPE tileObjectType, LocationStructure targetStructure, LocationStructure p_preferredStore, out JobQueueItem producedJob)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		targetStructure.PopulateBuildingTileObjectsOfType(list, tileObjectType);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				TileObject tileObject = list[i];
				if (!tileObject.HasJobTargetingThis(JOB_TYPE.CRAFT_MISSING_FURNITURE))
				{
					INTERACTION_TYPE targetInteractionType = ((tileObject.resourceStorageComponent.GetFirstResourceWithValue() == CONCRETE_RESOURCES.Wood) ? INTERACTION_TYPE.CRAFT_FURNITURE_WOOD : INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, targetInteractionType, tileObject, owner);
					producedJob = goapPlanJob;
					return true;
				}
			}
		}
		else if (CreateUnbuiltFurnitureThenCraft(tileObjectType, targetStructure, p_preferredStore, out producedJob))
		{
			return true;
		}
		producedJob = null;
		return false;
	}

	private bool CreateUnbuiltFurnitureThenCraft(TILE_OBJECT_TYPE tileObjectType, LocationStructure targetStructure, LocationStructure p_preferredStructure, out JobQueueItem producedJob)
	{
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType);
		if (targetStructure.AddPOI(tileObject))
		{
			tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
			int num;
			switch (tileObjectType)
			{
			case TILE_OBJECT_TYPE.TABLE:
				num = 10;
				break;
			case TILE_OBJECT_TYPE.BED:
			case TILE_OBJECT_TYPE.BED_CLINIC:
				num = 20;
				break;
			case TILE_OBJECT_TYPE.TORCH:
			case TILE_OBJECT_TYPE.DIVINE_ORB:
				num = 5;
				break;
			case TILE_OBJECT_TYPE.GUITAR:
				num = 30;
				break;
			default:
				num = 10;
				break;
			}
			if (p_preferredStructure is Lumberyard)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, INTERACTION_TYPE.CRAFT_FURNITURE_WOOD, tileObject, owner);
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.BUY_WOOD, p_preferredStructure);
				TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(tileObjectType);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.BUY_WOOD, new object[2] { num, tileObjectData.craftResourceCost });
				producedJob = goapPlanJob;
				return true;
			}
			if (p_preferredStructure is Mine)
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, INTERACTION_TYPE.CRAFT_FURNITURE_STONE, tileObject, owner);
				goapPlanJob2.AddPriorityLocation(INTERACTION_TYPE.BUY_STONE, p_preferredStructure);
				TileObjectData tileObjectData2 = TileObjectDB.GetTileObjectData(tileObjectType);
				goapPlanJob2.AddOtherData(INTERACTION_TYPE.BUY_STONE, new object[2] { num, tileObjectData2.craftResourceCost });
				producedJob = goapPlanJob2;
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	public bool CreateUnbuiltFurnitureThenCraftUsingOwnedResource(TILE_OBJECT_TYPE tileObjectType, LocationStructure targetStructure, TileObject p_ownedResource, out JobQueueItem producedJob)
	{
		if (p_ownedResource is ResourcePile { gridTileLocation: not null } resourcePile)
		{
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType);
			if (targetStructure.AddPOI(tileObject))
			{
				tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				ILocation location = null;
				location = ((p_ownedResource.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS) ? ((ILocation)p_ownedResource.gridTileLocation.area) : ((ILocation)p_ownedResource.gridTileLocation.structure));
				int num;
				switch (tileObjectType)
				{
				case TILE_OBJECT_TYPE.TABLE:
					num = 10;
					break;
				case TILE_OBJECT_TYPE.BED:
				case TILE_OBJECT_TYPE.BED_CLINIC:
					num = 20;
					break;
				case TILE_OBJECT_TYPE.TORCH:
				case TILE_OBJECT_TYPE.DIVINE_ORB:
					num = 5;
					break;
				case TILE_OBJECT_TYPE.GUITAR:
					num = 30;
					break;
				default:
					num = 10;
					break;
				}
				if (resourcePile.providedResource == RESOURCE.WOOD)
				{
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, INTERACTION_TYPE.CRAFT_FURNITURE_WOOD, tileObject, owner);
					goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, location);
					TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(tileObjectType);
					goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { tileObjectData.craftResourceCost });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.BUY_STONE, new object[2] { num, tileObjectData.craftResourceCost });
					producedJob = goapPlanJob;
					return true;
				}
				if (resourcePile.providedResource == RESOURCE.STONE)
				{
					GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, INTERACTION_TYPE.CRAFT_FURNITURE_STONE, tileObject, owner);
					goapPlanJob2.AddPriorityLocation(INTERACTION_TYPE.NONE, location);
					TileObjectData tileObjectData2 = TileObjectDB.GetTileObjectData(tileObjectType);
					goapPlanJob2.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { tileObjectData2.craftResourceCost });
					goapPlanJob2.AddOtherData(INTERACTION_TYPE.BUY_STONE, new object[2] { num, tileObjectData2.craftResourceCost });
					producedJob = goapPlanJob2;
					return true;
				}
			}
		}
		producedJob = null;
		return false;
	}

	public bool CreateCraftHospiceBed(Hospice hospice, LocationStructure p_preferredStore, out JobQueueItem producedJob)
	{
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BED_CLINIC);
		LocationGridTile tileLocation = null;
		StructureTemplateObjectData p_objectTemplate = null;
		if (hospice.TryGetMissingDefaultBedPosition(out p_objectTemplate))
		{
			tileLocation = hospice.structureObj.GetTileLocationOfPreplacedObject(p_objectTemplate, hospice.region.innerMap);
		}
		if (hospice.AddPOI(tileObject, tileLocation))
		{
			if (p_objectTemplate != null)
			{
				tileObject.mapVisual.SetVisual(p_objectTemplate.spriteRenderer.sprite);
				tileObject.mapVisual.SetRotation(p_objectTemplate.transform.localEulerAngles.z);
				tileObject.RevalidateTileObjectSlots();
			}
			tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
			int num = 20;
			if (p_preferredStore is Lumberyard)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, INTERACTION_TYPE.CRAFT_FURNITURE_WOOD, tileObject, owner);
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.BUY_WOOD, p_preferredStore);
				TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.BED_CLINIC);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.BUY_WOOD, new object[2] { num, tileObjectData.craftResourceCost });
				producedJob = goapPlanJob;
				return true;
			}
			if (p_preferredStore is Mine)
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, INTERACTION_TYPE.CRAFT_FURNITURE_STONE, tileObject, owner);
				goapPlanJob2.AddPriorityLocation(INTERACTION_TYPE.BUY_STONE, p_preferredStore);
				TileObjectData tileObjectData2 = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.BED_CLINIC);
				goapPlanJob2.AddOtherData(INTERACTION_TYPE.BUY_STONE, new object[2] { num, tileObjectData2.craftResourceCost });
				producedJob = goapPlanJob2;
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateBuyItem(Character p_character, TileObject p_targetObject, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.BUY_ITEM))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUY_ITEM, INTERACTION_TYPE.BUY_ITEM, p_targetObject, p_character);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerDrinkWaterJob(WaterWell p_waterWell, out JobQueueItem producedJob)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRINK_WATER], owner, p_waterWell, null, 0);
		GoapPlan assignedPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SOCIALIZE, INTERACTION_TYPE.DRINK_WATER, p_waterWell, owner);
		goapPlanJob.SetAssignedPlan(assignedPlan);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TryCreateCleanItemJob(TileObject p_tileObject, out JobQueueItem p_producedJob)
	{
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.CLEAN_UP], owner, p_tileObject, null, 0);
		GoapPlan assignedPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, owner);
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_CLEAN, INTERACTION_TYPE.CLEAN_UP, p_tileObject, owner);
		goapPlanJob.SetAssignedPlan(assignedPlan);
		p_producedJob = goapPlanJob;
		return true;
	}

	public bool TryCreateCleanItemJob(LocationStructure p_structure, out JobQueueItem p_producedJob)
	{
		for (int i = 0; i < p_structure.pointsOfInterest.Count; i++)
		{
			if (p_structure.pointsOfInterest.ElementAt(i) is TileObject { mapObjectState: MAP_OBJECT_STATE.BUILT } tileObject && tileObject.traitContainer.HasTrait("Wet", "Dirty", "Burnt"))
			{
				return TryCreateCleanItemJob(tileObject, out p_producedJob);
			}
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerPersonalChangeClassJob(string className, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.CHANGE_CLASS))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, null, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[1] { className });
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerMonsterAbduct(JOB_TYPE p_jobType, Character targetCharacter, out JobQueueItem producedJob, LocationGridTile targetTile = null)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, (targetTile == null) ? new object[1] { owner.homeStructure } : new object[2] { targetTile.structure, targetTile });
		goapPlanJob.SetDoNotRecalculate(state: true);
		producedJob = goapPlanJob;
		return true;
	}

	public bool TriggerAttackVillager(JOB_TYPE p_jobType, Character chosenTarget, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.ASSAULT, chosenTarget, owner);
			goapPlanJob.SetDoNotRecalculate(state: true);
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerCastMesmerizedToNearbyVillager(JOB_TYPE p_jobType, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			Area areaLocation = owner.areaLocation;
			if (areaLocation != null)
			{
				Character character = null;
				List<Area> list = RuinarchListPool<Area>.Claim();
				List<Character> list2 = RuinarchListPool<Character>.Claim();
				areaLocation.PopulateAreasInRange(list, 6, includeCenterTile: true);
				for (int i = 0; i < list.Count; i++)
				{
					Character randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel = list[i].locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel();
					if (randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel != null && !randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel.traitContainer.HasTrait("Mesmerized"))
					{
						list2.Add(randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel);
					}
				}
				if (list2.Count > 0)
				{
					character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
				}
				RuinarchListPool<Area>.Release(list);
				RuinarchListPool<Character>.Release(list2);
				if (character != null)
				{
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.CAST_MESMERIZED, character, owner);
					p_producedJob = goapPlanJob;
					return true;
				}
			}
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerCastSeducedToNearbyVillager(JOB_TYPE p_jobType, out JobQueueItem p_producedJob, GENDER p_gender)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			Area areaLocation = owner.areaLocation;
			if (areaLocation != null)
			{
				Character character = null;
				List<Area> list = RuinarchListPool<Area>.Claim();
				List<Character> list2 = RuinarchListPool<Character>.Claim();
				areaLocation.PopulateAreasInRange(list, 6, includeCenterTile: true);
				for (int i = 0; i < list.Count; i++)
				{
					Character randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel = list[i].locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel();
					if (randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel != null && randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel.gender == p_gender && randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel.tileObjectComponent.primaryBed != null)
					{
						list2.Add(randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel);
					}
				}
				if (list2.Count > 0)
				{
					character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
				}
				RuinarchListPool<Area>.Release(list);
				RuinarchListPool<Character>.Release(list2);
				if (character != null)
				{
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.MAKE_LOVE, character, owner);
					p_producedJob = goapPlanJob;
					return true;
				}
			}
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerStealTraitCharacter(JOB_TYPE p_jobType, Character chosenTarget, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.STEAL_TRAIT, chosenTarget, owner);
			goapPlanJob.SetDoNotRecalculate(state: true);
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerGiveTraitCharacter(JOB_TYPE p_jobType, Character chosenTarget, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.GIVE_TRAIT, chosenTarget, owner);
			goapPlanJob.SetDoNotRecalculate(state: true);
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerBroodmotherOrderAttack(JOB_TYPE p_jobType, BaseSettlement chosenTarget, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.BROODMOTHER_ORDER_ATTACK, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BROODMOTHER_ORDER_ATTACK, new object[1] { chosenTarget });
			goapPlanJob.SetDoNotRecalculate(state: true);
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public bool TriggerTransformCentaur(JOB_TYPE p_jobType, Character chosenTarget, out JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.TRANSFORM_CENTAUR, chosenTarget, owner);
			goapPlanJob.SetDoNotRecalculate(state: true);
			p_producedJob = goapPlanJob;
			return true;
		}
		p_producedJob = null;
		return false;
	}

	public void TriggerAbsorbWisp(Character target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, INTERACTION_TYPE.ABSORB_WISP, target, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}

	public bool TryCreatePlaceBlueprintJob(FACTION_TYPE p_factionType, STRUCTURE_TYPE p_structureType, Character character, NPCSettlement p_settlement, out JobQueueItem producedJob, ref string log)
	{
		StructureSetting structureSetting = character.faction.factionType.CreateStructureSettingForStructure(p_structureType, p_settlement);
		if (structureSetting.hasValue && LandmarkManager.Instance.CanPlaceStructureBlueprint(p_factionType, character.homeSettlement, structureSetting, out var targetTile, out var structurePrefabName, out var _, out var connectorTile))
		{
			return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, structureSetting, targetTile, connectorTile, out producedJob);
		}
		producedJob = null;
		return false;
	}

	public bool TriggerTrainMagic(Character actor, LocationStructure targetStructure, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.TRAIN))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRAIN, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TRAIN_TALENT, "Magic", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), actor, owner);
			goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, targetStructure);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateFollowJob(IPointOfInterest target, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO, target) && target.gridTileLocation != null && owner.movementComponent.HasPathTo(target.gridTileLocation) && !target.isBeingSeized)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.FOLLOW_ACTION, target, owner);
			goapPlanJob.SetCannotBePushedBack(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateReportMurderOrAbduct(ActualGoapNode actionDone)
	{
		JobQueueItem jobQueueItem = CreateReportMurderAbduct(actionDone);
		if (jobQueueItem != null && owner.jobQueue.AddJobInQueue(jobQueueItem))
		{
			return true;
		}
		return false;
	}

	private GoapPlanJob CreateReportMurderAbduct(ActualGoapNode actionDone)
	{
		Character character = null;
		if (owner.homeSettlement != null)
		{
			Character ruler = owner.homeSettlement.ruler;
			if (ruler != null && !ruler.isInLimbo && !ruler.isDead && ruler != owner && ruler != actionDone.actor && ruler != actionDone.target && !ruler.traitContainer.HasTrait("Travelling") && !ruler.partyComponent.isActiveMember && !actionDone.IsAware(ruler) && ruler.currentSettlement == owner.homeSettlement)
			{
				character = ruler;
			}
		}
		if (character == null && owner.faction != null)
		{
			if (owner.faction.leader is Character { isInLimbo: false, isDead: false } character2 && character2 != owner && character2 != actionDone.actor && character2 != actionDone.target && !character2.traitContainer.HasTrait("Travelling") && !character2.partyComponent.isActiveMember && !actionDone.IsAware(character2) && character2.currentSettlement == owner.homeSettlement)
			{
				character = character2;
			}
			if (character == null)
			{
				Character character3 = null;
				float num = float.MaxValue;
				for (int i = 0; i < owner.faction.characters.Count; i++)
				{
					Character character4 = owner.faction.characters[i];
					if ((character4.isFactionLeader || character4.isSettlementRuler) && !character4.isDead && !character4.isInLimbo && character4 != owner && character4 != actionDone.actor && character4 != actionDone.target && !character4.traitContainer.HasTrait("Travelling") && !character4.partyComponent.isActiveMember && !actionDone.IsAware(character4))
					{
						float num2 = Vector2.Distance(owner.worldPosition, character4.worldPosition);
						if (num2 < num)
						{
							num = num2;
							character3 = character4;
						}
					}
				}
				if (character3 != null)
				{
					character = character3;
				}
			}
		}
		if (character != null)
		{
			INTERACTION_TYPE iNTERACTION_TYPE = INTERACTION_TYPE.REPORT_MURDER;
			if (actionDone.goapType == INTERACTION_TYPE.ABDUCT)
			{
				iNTERACTION_TYPE = INTERACTION_TYPE.REPORT_ABDUCT;
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPORT_CRIME, iNTERACTION_TYPE, character, owner);
			goapPlanJob.AddOtherData(iNTERACTION_TYPE, new object[1] { actionDone });
			return goapPlanJob;
		}
		if (actionDone.poiTarget is Character character5 && character5.faction == owner.faction && (character5.isFactionLeader || character5.isSettlementRuler) && !owner.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, character5) && !owner.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, character5))
		{
			owner.faction.partyQuestBoard.CreateRescuePartyQuest(owner, owner.homeSettlement, character5);
		}
		return null;
	}

	public void CreateDevastationRitualJob(MagicCircle p_magicCircle, LocationStructure p_mageTower, ref JobQueueItem p_producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.DEVASTATION_RITUAL))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEVASTATION_RITUAL, INTERACTION_TYPE.DEVASTATION_RITUAL, p_magicCircle, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DEVASTATION_RITUAL, new object[1] { p_mageTower });
			p_producedJob = goapPlanJob;
		}
	}

	public bool TryCreateForageFoodJob(TileObject target, LocationStructure dropStructure, out JobQueueItem jobQueueItem)
	{
		jobQueueItem = null;
		if (!owner.jobQueue.HasJob(JOB_TYPE.FORAGE_FOOD))
		{
			INTERACTION_TYPE iNTERACTION_TYPE = INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE;
			if (target is BerryShrub)
			{
				iNTERACTION_TYPE = INTERACTION_TYPE.HARVEST_PLANT;
			}
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FORAGE_FOOD, iNTERACTION_TYPE, target, owner);
			if (dropStructure != null && iNTERACTION_TYPE == INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { dropStructure });
			}
			jobQueueItem = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool CreateEnhanceRelationshipJob(Character target, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ENHANCE_RELATIONSHIP))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENHANCE_RELATIONSHIP, INTERACTION_TYPE.ENHANCE_RELATIONSHIP, target, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateShamanRitualJob(INTERACTION_TYPE p_actionType, out JobQueueItem producedJob)
	{
		BaseSettlement homeSettlement = owner.homeSettlement;
		if (homeSettlement != null && homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			LocationGridTile locationGridTile = homeSettlement.GetRandomTileObjectOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE)?.gridTileLocation;
			bool flag = locationGridTile != null;
			if (locationGridTile == null)
			{
				List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
				for (int i = 0; i < homeSettlement.areas.Count; i++)
				{
					Area area = homeSettlement.areas[i];
					for (int j = 0; j < area.gridTileComponent.passableTiles.Count; j++)
					{
						LocationGridTile locationGridTile2 = area.gridTileComponent.passableTiles[j];
						if (!locationGridTile2.isOccupied && locationGridTile2.tileObjectComponent.objHere == null && locationGridTile2.tileObjectComponent.hiddenObjHere == null && locationGridTile2.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
						{
							list.Add(locationGridTile2);
						}
					}
				}
				if (list.Count > 0)
				{
					locationGridTile = CollectionUtilities.GetRandomElement(list);
				}
				RuinarchListPool<LocationGridTile>.Release(list);
			}
			if (locationGridTile != null)
			{
				GenericTileObject genericTileObject = locationGridTile.tileObjectComponent.genericTileObject;
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SHAMAN_RITUAL, p_actionType, genericTileObject, owner);
				JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
				if (!flag)
				{
					ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRAW_MAGIC_CIRCLE], owner, genericTileObject, null, 0);
					ActualGoapNode p_action2 = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[p_actionType], owner, genericTileObject, null, 0);
					GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, p_action2, genericTileObject);
					goapPlan.SetDoNotRecalculate(state: true);
					goapPlanJob.SetAssignedPlan(goapPlan);
				}
				producedJob = goapPlanJob;
				Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, CheckIfShamanRitualJobRemoved);
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	private void CheckIfShamanRitualJobRemoved(JobQueueItem job, Character character)
	{
		if (character == owner && job.jobType == JOB_TYPE.SHAMAN_RITUAL)
		{
			Messenger.Broadcast(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY);
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, CheckIfShamanRitualJobRemoved);
		}
	}

	public bool TriggerTrainPhysicalCombat(Character actor, LocationStructure targetStructure, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.TRAIN))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRAIN, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TRAIN_TALENT, "Martial Arts", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), actor, owner);
			goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, targetStructure);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreatePackFood(Character p_character, TileObject p_targetObject, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.STOCKPILE_FOOD))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STOCKPILE_FOOD, INTERACTION_TYPE.PACK_FOOD, p_targetObject, p_character);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TryCreateStalkerHuntJob(Character p_target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.STALKER_HUNT, p_target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STALKER_HUNT, INTERACTION_TYPE.ASSAULT, p_target, owner);
			goapPlanJob.SetForceCancelOnInvalid(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TryCreateStalkerPurifyJob(Character p_target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PURIFY, p_target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PURIFY, INTERACTION_TYPE.PURIFY, p_target, owner);
			goapPlanJob.SetForceCancelOnInvalid(state: true);
			return owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public bool TryCreatePilgrimageJob(LocationStructure p_hallowedGround, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.PILGRIMAGE))
		{
			HallowedGround tileObjectOfType = p_hallowedGround.GetTileObjectOfType<HallowedGround>();
			if (tileObjectOfType != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PILGRIMAGE, INTERACTION_TYPE.PILGRIMAGE, tileObjectOfType, owner);
				producedJob = goapPlanJob;
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	public bool CreateClaimHallowedGroundJob(TileObject targetItem, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.CLAIM_LOCATION, targetItem))
		{
			producedJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLAIM_LOCATION, INTERACTION_TYPE.CLAIM_HALLOWED_GROUND, targetItem, owner);
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool CreateCleanseHallowedGroundJob(TileObject targetItem, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.CLEANSE_HALLOWED_GROUND, targetItem))
		{
			producedJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLEANSE_HALLOWED_GROUND, INTERACTION_TYPE.CLEANSE_HALLOWED_GROUND, targetItem, owner);
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerSacrificeJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SACRIFICE))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SACRIFICE_FOR_CHAOS_ORBS], owner, targetCharacter, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, targetCharacter);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SACRIFICE, INTERACTION_TYPE.SACRIFICE_FOR_CHAOS_ORBS, targetCharacter, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void CreateReadScrollJob(StructureScroll structureScroll)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.READ_SCROLL))
		{
			ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.READ_STRUCTURE_SCROLL], owner, structureScroll, null, 0);
			GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, structureScroll);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.READ_SCROLL, INTERACTION_TYPE.READ_STRUCTURE_SCROLL, structureScroll, owner);
			goapPlan.SetDoNotRecalculate(state: true);
			goapPlanJob.SetAssignedPlan(goapPlan);
			owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
	}

	public bool TriggerClaimLegendaryForge(TileObject tileObject, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.CLAIM_LOCATION, tileObject))
		{
			producedJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLAIM_LOCATION, INTERACTION_TYPE.CLAIM_LEGENDARY_FORGE, tileObject, owner);
			return true;
		}
		producedJob = null;
		return false;
	}

	public bool TriggerMummifyCorpse(Character p_target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.MUMMIFY))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MUMMIFY, INTERACTION_TYPE.MUMMIFY, p_target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool TriggerCleanMummifiedCorpse(Character p_target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.CLEAN_MUMMIFIED_CORPSE, p_target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool TriggerAdoreMummifiedCorpse(Character p_target)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.ADORE_MUMMIFIED_CORPSE, p_target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public void TriggerCheckOut(IPointOfInterest p_target)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.CHECK_OUT, p_target, owner);
		ActualGoapNode p_action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.CHECK_OUT], owner, p_target, null, 0);
		GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(p_action, p_target);
		goapPlan.SetDoNotRecalculate(state: true);
		goapPlanJob.SetCannotBePushedBack(state: true);
		goapPlanJob.SetAssignedPlan(goapPlan);
		owner.jobQueue.AddJobInQueue(goapPlanJob);
	}

	public bool TriggerDestroyHome(JOB_TYPE p_jobType, StructureTileObject p_target)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.DESTROY_HOME, p_target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool TriggerSummonEphemeralBeasts(JOB_TYPE p_jobType, Character p_target)
	{
		if (!owner.jobQueue.HasJob(p_jobType))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.SUMMON_EPHEMERAL_BEASTS, p_target, owner);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}

	public bool TriggerBuildMonsterSpawnerStructure(LocationGridTile targetTile, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_MONSTER_SPAWNER_STRUCTURE, owner, owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BUILD_MONSTER_SPAWNER_STRUCTURE, new object[1] { targetTile });
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void CreateAssassinateTargetJob(Character targetCharacter)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.ASSASSINATE, targetCharacter))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ASSASSINATE, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}

	public void RetrieveStolenItem(TileObject p_item)
	{
		if (p_item is EquipmentItem)
		{
			owner.jobComponent.CreateTakeItemOnSightJob(p_item);
		}
		else if (owner.homeStructure != null)
		{
			owner.jobComponent.CreateDropItemJob(JOB_TYPE.RETURN_STOLEN_THING, p_item, owner.homeStructure);
		}
	}

	public bool CreateKillVillagerJob(Character targetCharacter, out JobQueueItem producedJob)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.DEMON_KILL, targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEMON_KILL, INTERACTION_TYPE.ASSAULT, targetCharacter, owner);
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void TriggerKnockoutJob(Character targetCharacter)
	{
		if (!owner.jobQueue.HasJob(JOB_TYPE.KNOCKOUT, targetCharacter))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, INTERACTION_TYPE.ASSAULT, targetCharacter, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = owner;
	}

	public void CleanUp()
	{
		owner = null;
		ableJobs?.Clear();
		ableJobs = null;
		actionTracker?.Clear();
		actionTracker = null;
		obtainPersonalItemUnownedRandomList?.Clear();
		obtainPersonalItemUnownedRandomList = null;
	}
}
