using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations;
using Locations.Settlements;
using Maccima_Games.Util;
using UnityEngine;
using UtilityScripts;

public class GoapPlanner
{
	private const int CACHED_GOAP_NODE_CAPACITY = 15;

	private List<GoapNode> _rawPlan;

	private GoapThread _goapThreadInProcess;

	private List<GoapNode> _cachedGoapNodes;

	public Character owner { get; private set; }

	public GOAP_PLANNING_STATUS status { get; private set; }

	private Precondition failedPrecondition { get; set; }

	public INTERACTION_TYPE failedPreconditionActionType { get; private set; }

	public GoapPlanner(Character owner)
	{
		this.owner = owner;
		_rawPlan = new List<GoapNode>();
		_cachedGoapNodes = new List<GoapNode>();
	}

	public void StartGOAP(GoapEffect goal, IPointOfInterest target, GoapPlanJob job, bool isPersonalPlan = true)
	{
		if (status != GOAP_PLANNING_STATUS.RUNNING)
		{
			job?.SetAssignedPlan(null);
			status = GOAP_PLANNING_STATUS.RUNNING;
			Character actor = owner;
			CreateGoapNodeCache();
			_goapThreadInProcess = ObjectPoolManager.Instance.CreateNewGoapThread();
			_goapThreadInProcess.Initialize(actor, target, goal, isPersonalPlan, job);
			MultiThreadPool.Instance.AddToThreadPool(_goapThreadInProcess);
			job.SetIsInMultithread(state: true);
		}
	}

	public void StartGOAP(INTERACTION_TYPE goalType, IPointOfInterest target, GoapPlanJob job, bool isPersonalPlan = true)
	{
		if (status != GOAP_PLANNING_STATUS.RUNNING)
		{
			job?.SetAssignedPlan(null);
			status = GOAP_PLANNING_STATUS.RUNNING;
			CreateGoapNodeCache();
			_goapThreadInProcess = ObjectPoolManager.Instance.CreateNewGoapThread();
			_goapThreadInProcess.Initialize(owner, goalType, target, isPersonalPlan, job);
			MultiThreadPool.Instance.AddToThreadPool(_goapThreadInProcess);
			job.SetIsInMultithread(state: true);
		}
	}

	public void RecalculateJob(GoapPlanJob job)
	{
		if (status != GOAP_PLANNING_STATUS.RUNNING && job.assignedPlan != null)
		{
			job.assignedPlan.SetIsBeingRecalculated(state: true);
			status = GOAP_PLANNING_STATUS.RUNNING;
			CreateGoapNodeCache();
			_goapThreadInProcess = ObjectPoolManager.Instance.CreateNewGoapThread();
			_goapThreadInProcess.InitializeForRecalculation(owner, job.assignedPlan, job);
			MultiThreadPool.Instance.AddToThreadPool(_goapThreadInProcess);
			job.SetIsInMultithread(state: true);
		}
	}

	public void ReceivePlanFromGoapThread(GoapPlan createdPlan)
	{
		status = GOAP_PLANNING_STATUS.NONE;
		if (_goapThreadInProcess.job != null)
		{
			_goapThreadInProcess.job.SetIsInMultithread(state: false);
			if (_goapThreadInProcess.job.shouldForceCancelUponReceiving)
			{
				ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
				if (_goapThreadInProcess.recalculationPlan != null && _goapThreadInProcess.recalculationPlan.resetPlanOnFinishRecalculation)
				{
					ObjectPoolManager.Instance.ReturnGoapPlanToPool(_goapThreadInProcess.recalculationPlan);
				}
				ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
				_goapThreadInProcess = null;
				return;
			}
		}
		if (owner.isDead || !owner.marker)
		{
			ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
			ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
			_goapThreadInProcess = null;
			return;
		}
		if (_goapThreadInProcess.recalculationPlan != null)
		{
			if (_goapThreadInProcess.recalculationPlan.isEnd)
			{
				ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
				if (_goapThreadInProcess.recalculationPlan.resetPlanOnFinishRecalculation)
				{
					ObjectPoolManager.Instance.ReturnGoapPlanToPool(_goapThreadInProcess.recalculationPlan);
				}
				ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
				_goapThreadInProcess = null;
				return;
			}
			if (_goapThreadInProcess.recalculationPlan.resetPlanOnFinishRecalculation)
			{
				ObjectPoolManager.Instance.ReturnGoapPlanToPool(_goapThreadInProcess.recalculationPlan);
				ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
				_goapThreadInProcess = null;
				return;
			}
		}
		_ = string.Empty;
		if (_goapThreadInProcess.job.originalOwner == null)
		{
			ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
			ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
			_goapThreadInProcess = null;
			return;
		}
		JOB_TYPE jobType = _goapThreadInProcess.job.jobType;
		GOAP_EFFECT_CONDITION gOAP_EFFECT_CONDITION = GOAP_EFFECT_CONDITION.NONE;
		if (_goapThreadInProcess.job.goal != null)
		{
			gOAP_EFFECT_CONDITION = _goapThreadInProcess.job.goal.conditionType;
		}
		bool isTriggeredByPlayer = _goapThreadInProcess.job.isTriggeredByPlayer;
		if (createdPlan != null)
		{
			if (owner.jobQueue.pendingTopPriorityJobs.Contains(_goapThreadInProcess.job))
			{
				owner.jobQueue.TransferPendingJobToMainJobQueue(_goapThreadInProcess.job, createdPlan);
			}
			if (jobType == JOB_TYPE.PRODUCE_FOOD && owner.traitContainer.HasTrait("Abstain Fullness"))
			{
				owner.traitContainer.RemoveTrait(owner, "Abstain Fullness");
			}
			createdPlan.SetDoNotRecalculate(_goapThreadInProcess.job.doNotRecalculate);
			if (_goapThreadInProcess.recalculationPlan != null)
			{
				createdPlan.SetIsBeingRecalculated(state: false);
			}
			if (!owner.limiterComponent.canPerform && (owner.limiterComponent.canPerformValue != -1 || (!owner.traitContainer.HasTrait("Paralyzed") && !owner.traitContainer.HasTrait("Quarantined"))))
			{
				_goapThreadInProcess.job.CancelJob();
				ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
				_goapThreadInProcess = null;
				return;
			}
			int jobQueueIndex = owner.jobQueue.GetJobQueueIndex(_goapThreadInProcess.job);
			if (jobQueueIndex != -1)
			{
				_goapThreadInProcess.job.SetAssignedPlan(createdPlan);
				if (jobQueueIndex != 0)
				{
					ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
					_goapThreadInProcess = null;
					owner.jobQueue.ProcessFirstJobInQueue();
					return;
				}
			}
			JobQueueItem job = _goapThreadInProcess.job;
			ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
			_goapThreadInProcess = null;
			owner.PerformJob(job);
			return;
		}
		if (jobType == JOB_TYPE.REPORT_CRIME)
		{
			_goapThreadInProcess?.job?.LogUnableToDoReportCrime(owner);
		}
		if (jobType.IsFullnessRecoveryTypeJob())
		{
			owner.trapStructure.ResetAllTrappedValues();
			if (jobType != JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)
			{
				owner.traitContainer.AddTrait(owner, "Abstain Fullness");
			}
		}
		else if (jobType.IsTirednessRecoveryTypeJob())
		{
			owner.trapStructure.ResetAllTrappedValues();
			owner.traitContainer.AddTrait(owner, "Abstain Tiredness");
		}
		else if (jobType.IsHappinessRecoveryTypeJob())
		{
			owner.trapStructure.ResetAllTrappedValues();
			owner.traitContainer.AddTrait(owner, "Abstain Happiness");
		}
		if (_goapThreadInProcess.recalculationPlan == null)
		{
			if (_goapThreadInProcess.job.targetPOI != null && (jobType != JOB_TYPE.DOUSE_FIRE || _goapThreadInProcess.job.targetPOI.gridTileLocation != null) && !CharacterManager.Instance.lessenCharacterLogs)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "cancel_job_no_plan", LOG_TAG.Work, null);
				log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, _goapThreadInProcess.job.GetJobDetailString(), LOG_IDENTIFIER.STRING_1);
				owner.logComponent.RegisterLog(log, releaseAfter: true);
			}
			if (_goapThreadInProcess.job.originalOwner.ownerType != JOB_OWNER.CHARACTER && _goapThreadInProcess.job.originalOwner.ownerType != JOB_OWNER.PARTY)
			{
				_goapThreadInProcess.job.AddBlacklistedCharacter(owner);
			}
		}
		if (owner.carryComponent.IsNotBeingCarried())
		{
			if (owner.carryComponent.isCarryingAnyPOI)
			{
				IPointOfInterest carriedPOI = owner.carryComponent.carriedPOI;
				_ = string.Empty;
				if (!(carriedPOI is ResourcePile))
				{
					_ = carriedPOI is Table;
				}
			}
			owner.UncarryPOI();
		}
		if (_goapThreadInProcess.job != null && _goapThreadInProcess.job.jobType.IsCultistJob())
		{
			string cultistUnableToDoJobReason = owner.GetCultistUnableToDoJobReason(_goapThreadInProcess.job, failedPrecondition, failedPreconditionActionType);
			owner.LogUnableToDoJob(cultistUnableToDoJobReason);
		}
		_goapThreadInProcess.job.CancelJob();
		switch (jobType)
		{
		case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
		case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
			if (!owner.traitContainer.HasTrait("Vampire") && owner.isNormalCharacter && !owner.isConsideredRatman && (owner.homeSettlement == null || !owner.homeSettlement.HasPathTowardsTileInSettlement(owner, 2)) && !owner.partyComponent.isMemberThatJoinedQuest)
			{
				ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
				_goapThreadInProcess = null;
				owner.jobComponent.CreateProduceFoodJob();
				return;
			}
			if (owner.traitContainer.HasTrait("Pest") || owner is Rat)
			{
				owner.behaviourComponent.SetPestHasFailedEat(p_state: true);
			}
			break;
		case JOB_TYPE.RECOVER_HP:
			owner.jobComponent.SetDoNotDoRecoverHPJob(state: true);
			break;
		case JOB_TYPE.TRIGGER_FLAW:
			if (isTriggeredByPlayer)
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Flaw_Failed");
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("actorName", owner.name);
				string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Flaw_Failed_Description", dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
				PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, localizedValue2);
			}
			if (gOAP_EFFECT_CONDITION == GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY && !owner.traitContainer.HasTrait("Vampire") && owner.isNormalCharacter && !owner.isConsideredRatman && !owner.partyComponent.isMemberThatJoinedQuest)
			{
				ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
				_goapThreadInProcess = null;
				owner.jobComponent.CreateProduceFoodJob();
				return;
			}
			break;
		}
		ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
		_goapThreadInProcess = null;
	}

	private void ForceCancelJobAndReturnToObjectPool(JobQueueItem job)
	{
		if (job != null)
		{
			job.ForceCancelJob();
			if (!string.IsNullOrEmpty(job.persistentID))
			{
				JobManager.Instance.ReleaseJob(job);
			}
		}
	}

	public void PlanActions(IPointOfInterest target, GoapEffect goalEffect, bool isPersonalPlan, ref string log, GoapPlanJob job, GoapThread goapThread)
	{
		Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffectCondition = InteractionManager.Instance.actionsCategorizedByEffectCondition;
		_rawPlan.Clear();
		failedPrecondition = null;
		failedPreconditionActionType = INTERACTION_TYPE.NONE;
		owner.logComponent.ClearCostLog();
		if (goalEffect.target == GOAP_EFFECT_TARGET.TARGET)
		{
			int cost = 0;
			if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(owner))
			{
				GoapAction goapAction = target.AdvertiseActionsToActor(owner, goalEffect, job, ref cost, ref log);
				if (goapAction != null)
				{
					GoapNode node = SetGoapNodeCacheData(cost, 0, goapAction, target);
					BuildGoapTree(node, owner, job, _rawPlan, actionsCategorizedByEffectCondition, ref log);
				}
			}
		}
		else if (goalEffect.target == GOAP_EFFECT_TARGET.ACTOR)
		{
			GoapAction lowestCostAction = null;
			IPointOfInterest lowestCostTarget = null;
			int lowestCost = 0;
			ProcessFindingLowestCostActionAndTarget(job, goalEffect, target, actionsCategorizedByEffectCondition, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
			if (lowestCostAction != null)
			{
				GoapNode node2 = SetGoapNodeCacheData(lowestCost, 0, lowestCostAction, lowestCostTarget);
				BuildGoapTree(node2, owner, job, _rawPlan, actionsCategorizedByEffectCondition, ref log);
			}
		}
		_ = _rawPlan.Count;
		_ = 0;
	}

	public void PlanActions(IPointOfInterest target, GoapAction goalAction, bool isPersonalPlan, ref string log, GoapPlanJob job, GoapThread goapThread)
	{
		Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffectCondition = InteractionManager.Instance.actionsCategorizedByEffectCondition;
		_rawPlan.Clear();
		failedPrecondition = null;
		failedPreconditionActionType = INTERACTION_TYPE.NONE;
		owner.logComponent.ClearCostLog();
		if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(owner))
		{
			int cost = goalAction.GetCost(owner, target, job);
			GoapNode node = SetGoapNodeCacheData(cost, 0, goalAction, target);
			BuildGoapTree(node, owner, job, _rawPlan, actionsCategorizedByEffectCondition, ref log);
			_ = _rawPlan.Count;
			_ = 0;
		}
	}

	public bool RecalculatePathForPlan(GoapPlan currentPlan, GoapPlanJob job, ref string log)
	{
		Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffectCondition = InteractionManager.Instance.actionsCategorizedByEffectCondition;
		_rawPlan.Clear();
		failedPrecondition = null;
		failedPreconditionActionType = INTERACTION_TYPE.NONE;
		ActualGoapNode singleNode = currentPlan.currentNode.singleNode;
		GoapAction action = singleNode.action;
		IPointOfInterest poiTarget = singleNode.poiTarget;
		_ = singleNode.otherData;
		if ((poiTarget == job.targetPOI || poiTarget.IsStillConsideredPartOfAwarenessByCharacter(owner)) && poiTarget.CanAdvertiseActionToActor(owner, action, job) && poiTarget.Advertises(action.goapType))
		{
			action.GetCost(owner, poiTarget, job);
			GoapNode node = SetGoapNodeCacheData(singleNode.cost, currentPlan.currentNodeIndex, action, poiTarget);
			BuildGoapTree(node, owner, job, _rawPlan, actionsCategorizedByEffectCondition, ref log);
			if (_rawPlan.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public GoapPlan TransformRawPlanToActualPlan()
	{
		GoapPlan result = null;
		if (_rawPlan.Count > 0)
		{
			_ = string.Empty;
			if (_goapThreadInProcess.recalculationPlan != null)
			{
				GoapPlan recalculationPlan = _goapThreadInProcess.recalculationPlan;
				if (_goapThreadInProcess.isRecalculationSuccess)
				{
					List<JobNode> nodes = TransformRawPlanToActualNodes(_rawPlan, _goapThreadInProcess.job, recalculationPlan);
					recalculationPlan.SetNodes(nodes);
					result = recalculationPlan;
				}
			}
			else
			{
				List<JobNode> list = TransformRawPlanToActualNodes(_rawPlan, _goapThreadInProcess.job);
				GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(list, _goapThreadInProcess.target);
				goapPlan.SetNodes(list);
				goapPlan.SetTarget(_goapThreadInProcess.target);
				goapPlan.SetIsPersonalPlan(_goapThreadInProcess.isPersonalPlan);
				if (goapPlan != null)
				{
					result = goapPlan;
				}
			}
		}
		ResetGoapNodeCache();
		return result;
	}

	private void BuildGoapTree(GoapNode node, Character actor, GoapPlanJob job, List<GoapNode> rawPlan, Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffect, ref string log)
	{
		GoapAction action = node.action;
		IPointOfInterest target = node.target;
		rawPlan.Add(node);
		if (rawPlan.Sum((GoapNode x) => x.cost) > 1000)
		{
			rawPlan.Clear();
			return;
		}
		Precondition precondition = null;
		OtherData[] otherDataFor = job.GetOtherDataFor(action.goapType);
		bool isOverridden = false;
		precondition = action.GetPrecondition(actor, target, otherDataFor, job.jobType, out isOverridden);
		if (precondition == null || precondition.CanSatisfyCondition(actor, target, otherDataFor, job.jobType))
		{
			return;
		}
		GoapEffect goapEffect = precondition.goapEffect;
		if (goapEffect.target == GOAP_EFFECT_TARGET.TARGET)
		{
			int cost = 0;
			GoapAction goapAction = null;
			if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(actor))
			{
				goapAction = target.AdvertiseActionsToActor(actor, goapEffect, job, ref cost, ref log);
			}
			if (goapAction != null)
			{
				GoapNode node2 = SetGoapNodeCacheData(cost, node.level + 1, goapAction, target);
				BuildGoapTree(node2, actor, job, rawPlan, actionsCategorizedByEffect, ref log);
			}
			else
			{
				rawPlan.Clear();
				failedPrecondition = precondition;
				failedPreconditionActionType = action.goapType;
			}
		}
		else if (goapEffect.target == GOAP_EFFECT_TARGET.ACTOR)
		{
			GoapAction lowestCostAction = null;
			IPointOfInterest lowestCostTarget = null;
			int lowestCost = 0;
			ProcessFindingLowestCostActionAndTarget(job, goapEffect, target, actionsCategorizedByEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
			if (lowestCostAction != null)
			{
				GoapNode node3 = SetGoapNodeCacheData(lowestCost, node.level + 1, lowestCostAction, lowestCostTarget);
				BuildGoapTree(node3, actor, job, rawPlan, actionsCategorizedByEffect, ref log);
			}
			else
			{
				rawPlan.Clear();
				failedPrecondition = precondition;
				failedPreconditionActionType = action.goapType;
			}
		}
	}

	private void ProcessFindingLowestCostActionAndTarget(GoapPlanJob job, GoapEffect goalEffect, IPointOfInterest target, Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffect, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log)
	{
		if (!actionsCategorizedByEffect.ContainsKey(goalEffect.conditionType))
		{
			return;
		}
		List<GoapAction> list = actionsCategorizedByEffect[goalEffect.conditionType];
		for (int i = 0; i < list.Count; i++)
		{
			GoapAction goapAction = list[i];
			bool isJobTargetEvaluated = false;
			SetLowestCostActionBasedOnLocationAwareness(job, goapAction, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
			if (!isJobTargetEvaluated && (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(owner)) && target.Advertises(goapAction.goapType))
			{
				PopulateLowestCostAction(target, goapAction, job, goalEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
			}
		}
	}

	private void SetLowestCostActionBasedOnLocationAwareness(GoapPlanJob job, GoapAction action, GoapEffect goalEffect, ref bool isJobTargetEvaluated, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log)
	{
		List<ILocation> priorityLocationsFor = job.GetPriorityLocationsFor(action.goapType);
		if (priorityLocationsFor != null)
		{
			bool flag = false;
			for (int i = 0; i < priorityLocationsFor.Count; i++)
			{
				ILocation location = priorityLocationsFor[i];
				if (location is BaseSettlement baseSettlement)
				{
					for (int j = 0; j < baseSettlement.allStructures.Count; j++)
					{
						LocationStructure locationStructure = baseSettlement.allStructures[j];
						if (SetLowestCostActionGivenLocationAwareness(locationStructure.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
						{
							flag = true;
						}
					}
					for (int k = 0; k < baseSettlement.areas.Count; k++)
					{
						Area area = baseSettlement.areas[k];
						if (SetLowestCostActionGivenLocationAwareness(area.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
						{
							flag = true;
						}
					}
				}
				else if (location is LocationStructure locationStructure2)
				{
					if (SetLowestCostActionGivenLocationAwareness(locationStructure2.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
					{
						flag = true;
					}
				}
				else if (location is Area area2 && SetLowestCostActionGivenLocationAwareness(area2.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
				{
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
		}
		if (owner.items.Count > 0 && SetLowestCostActionGivenActorInventory(owner, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
		{
			return;
		}
		LocationGridTile gridTileLocation = owner.gridTileLocation;
		if (gridTileLocation == null)
		{
			return;
		}
		LocationStructure structure = gridTileLocation.structure;
		if (structure.structureType != STRUCTURE_TYPE.WILDERNESS && structure.structureType != STRUCTURE_TYPE.OCEAN && SetLowestCostActionGivenLocationAwareness(structure.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
		{
			return;
		}
		Area area3 = gridTileLocation.area;
		if (area3 != null && !SetLowestCostActionGivenLocationAwareness(area3.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
		{
			List<Area> neighbours = area3.neighbourComponent.neighbours;
			for (int l = 0; l < neighbours.Count; l++)
			{
				Area area4 = neighbours[l];
				SetLowestCostActionGivenLocationAwareness(area4.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
			}
		}
	}

	private bool SetLowestCostActionGivenLocationAwareness(ILocationAwareness locationAwareness, GoapPlanJob job, GoapAction action, GoapEffect goalEffect, ref bool isJobTargetEvaluated, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log)
	{
		bool result = false;
		lock (MultiThreadPool.THREAD_LOCKER)
		{
			List<IPointOfInterest> listOfPOIBasedOnActionType = locationAwareness.GetListOfPOIBasedOnActionType(action.goapType);
			if (listOfPOIBasedOnActionType != null)
			{
				for (int i = 0; i < listOfPOIBasedOnActionType.Count; i++)
				{
					IPointOfInterest pointOfInterest = listOfPOIBasedOnActionType[i];
					if (pointOfInterest == job.targetPOI)
					{
						isJobTargetEvaluated = true;
					}
					if (pointOfInterest.IsStillConsideredPartOfAwarenessByCharacter(owner) && PopulateLowestCostAction(pointOfInterest, action, job, goalEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
					{
						result = true;
					}
				}
			}
			List<Character> allCharacters = CharacterManager.Instance.allCharacters;
			for (int j = 0; j < allCharacters.Count; j++)
			{
				Character character = allCharacters[j];
				if (character.currentLocationAwareness == locationAwareness && character.Advertises(action.goapType))
				{
					if (character == job.targetPOI)
					{
						isJobTargetEvaluated = true;
					}
					if (character.IsStillConsideredPartOfAwarenessByCharacter(owner) && PopulateLowestCostAction(character, action, job, goalEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
					{
						result = true;
					}
				}
			}
			return result;
		}
	}

	private bool SetLowestCostActionGivenActorInventory(Character actor, GoapPlanJob job, GoapAction action, GoapEffect goalEffect, ref bool isJobTargetEvaluated, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log)
	{
		bool result = false;
		lock (MultiThreadPool.THREAD_LOCKER)
		{
			List<IPointOfInterest> listOfInventoryItemsBasedOnActionType = actor.GetListOfInventoryItemsBasedOnActionType(action.goapType);
			if (listOfInventoryItemsBasedOnActionType != null)
			{
				for (int i = 0; i < listOfInventoryItemsBasedOnActionType.Count; i++)
				{
					IPointOfInterest pointOfInterest = listOfInventoryItemsBasedOnActionType[i];
					if (pointOfInterest == job.targetPOI)
					{
						isJobTargetEvaluated = true;
					}
					if (pointOfInterest.IsStillConsideredPartOfAwarenessByCharacter(owner) && PopulateLowestCostAction(pointOfInterest, action, job, goalEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log))
					{
						result = true;
					}
				}
				RuinarchListPool<IPointOfInterest>.Release(listOfInventoryItemsBasedOnActionType);
			}
		}
		return result;
	}

	private bool PopulateLowestCostAction(IPointOfInterest poiTarget, GoapAction action, GoapPlanJob job, GoapEffect goalEffect, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log)
	{
		if (CanActionBeDoneOn(poiTarget, action, job, goalEffect))
		{
			int cost = action.GetCost(owner, poiTarget, job);
			if (cost < 1000 && (lowestCostAction == null || cost < lowestCost))
			{
				lowestCostAction = action;
				lowestCostTarget = poiTarget;
				lowestCost = cost;
				return true;
			}
		}
		return false;
	}

	private bool CanActionBeDoneOn(IPointOfInterest poiTarget, GoapAction action, GoapPlanJob job, GoapEffect goalEffect)
	{
		if (poiTarget.CanAdvertiseActionToActor(owner, action, job))
		{
			return action.WillEffectsSatisfyPrecondition(goalEffect, owner, poiTarget, job);
		}
		return false;
	}

	private List<JobNode> TransformRawPlanToActualNodes(List<GoapNode> rawPlan, GoapPlanJob job, GoapPlan currentPlan = null)
	{
		int num = 0;
		List<JobNode> list;
		if (currentPlan == null)
		{
			list = RuinarchListPool<JobNode>.Claim();
		}
		else
		{
			list = currentPlan.allNodes;
			if (list.Count > 0 && currentPlan.currentNodeIndex + 1 < list.Count)
			{
				list.RemoveRange(0, currentPlan.currentNodeIndex + 1);
			}
			num = currentPlan.currentNodeIndex;
		}
		List<int> list2 = RuinarchListPool<int>.Claim();
		List<GoapNode> list3 = RuinarchListPool<GoapNode>.Claim();
		while (rawPlan.Count > 0)
		{
			list2.Clear();
			for (int i = 0; i < rawPlan.Count; i++)
			{
				if (rawPlan[i].level == num)
				{
					list2.Add(i);
				}
			}
			if (list2.Count > 0)
			{
				int index = list2[0];
				GoapNode goapNode = rawPlan[index];
				if (goapNode.action == null)
				{
					Debug.LogError("Null action in raw plan");
				}
				OtherData[] otherDataFor = job.GetOtherDataFor(goapNode.action.goapType);
				ActualGoapNode actionNode = ObjectPoolManager.Instance.CreateNewAction(goapNode.action, owner, goapNode.target, otherDataFor, goapNode.cost);
				SingleJobNode singleJobNode = ObjectPoolManager.Instance.CreateNewSingleJobNode();
				singleJobNode.SetActionNode(actionNode);
				list.Insert(0, singleJobNode);
				rawPlan.RemoveAt(index);
			}
			num++;
		}
		RuinarchListPool<int>.Release(list2);
		RuinarchListPool<GoapNode>.Release(list3);
		return list;
	}

	private void CreateGoapNodeCache()
	{
		if (_cachedGoapNodes.Count > 0)
		{
			Debug.LogError("Creating cache but there is still cached data");
			return;
		}
		for (int i = 0; i < 15; i++)
		{
			_cachedGoapNodes.Add(ObjectPoolManager.Instance.CreateNewGoapNode());
		}
	}

	private void ResetGoapNodeCache()
	{
		for (int i = 0; i < _cachedGoapNodes.Count; i++)
		{
			ObjectPoolManager.Instance.ReturnGoapNodeToPool(_cachedGoapNodes[i]);
		}
		_cachedGoapNodes.Clear();
	}

	private GoapNode SetGoapNodeCacheData(int cost, int level, GoapAction action, IPointOfInterest target)
	{
		for (int i = 0; i < _cachedGoapNodes.Count; i++)
		{
			GoapNode goapNode = _cachedGoapNodes[i];
			if (goapNode.action == null)
			{
				goapNode.Initialize(cost, level, action, target);
				return goapNode;
			}
		}
		throw new Exception("Cached goap nodes are fully filled up, need to adjust max capacity");
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = owner;
	}
}
