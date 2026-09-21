using System;
using System.Collections.Generic;
using System.Linq;
using Goap.Unique_Action_Data;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Traits;
using UtilityScripts;

public class ActualGoapNode : IRumorable, ICrimeable, IReactable, ISavable, IObjectPoolTester
{
	public string instanceID { get; private set; }

	public string persistentID { get; private set; }

	public Character actor { get; private set; }

	public IPointOfInterest poiTarget { get; private set; }

	public Character disguisedActor { get; private set; }

	public Character disguisedTarget { get; private set; }

	public bool isStealth { get; private set; }

	public bool isAvoidWitnesses { get; private set; }

	public bool avoidCombat { get; private set; }

	public OtherData[] otherData { get; private set; }

	public int cost { get; private set; }

	public bool isIntel { get; private set; }

	public bool isNegativeInfo { get; private set; }

	public bool isUsedAsCrime { get; private set; }

	public bool hasBeenReset { get; private set; }

	public bool isSupposedToBeInPool { get; private set; }

	public bool hasStartedPerTickEffect { get; private set; }

	public int stillProcessingCounter { get; private set; }

	public bool isAssigned { get; set; }

	public GoapAction action { get; private set; }

	public ACTION_STATUS actionStatus { get; private set; }

	public Log thoughtBubbleLog { get; private set; }

	public Log thoughtBubbleMovingLog { get; private set; }

	public Log descriptionLog { get; private set; }

	public LocationStructure targetStructure { get; private set; }

	public LocationGridTile targetTile { get; private set; }

	public IPointOfInterest targetPOIToGoTo { get; private set; }

	public JOB_TYPE associatedJobType { get; private set; }

	public JobQueueItem associatedJob { get; private set; }

	public string currentStateName { get; private set; }

	public int ticksPerformingCurrentState { get; private set; }

	public int expectedActionStateDuration { get; private set; }

	public Rumor rumor { get; private set; }

	public Assumption assumption { get; private set; }

	public List<Character> awareCharacters { get; private set; }

	public List<LOG_TAG> logTags { get; private set; }

	public int reactionProcessCounter { get; private set; }

	public bool isIllusion { get; private set; }

	public bool isFabricated { get; private set; }

	public CRIME_TYPE crimeType { get; private set; }

	public UniqueActionData uniqueActionData { get; private set; }

	public GoapActionInvalidity invalidity { get; private set; }

	public GoapActionState currentState
	{
		get
		{
			if (!action.states.ContainsKey(currentStateName))
			{
				throw new Exception(action.goapName + " does not have a state named " + currentStateName);
			}
			return action.states[currentStateName];
		}
	}

	public bool isPerformingActualAction => actionStatus == ACTION_STATUS.PERFORMING;

	public bool isDone
	{
		get
		{
			if (actionStatus != ACTION_STATUS.SUCCESS)
			{
				return actionStatus == ACTION_STATUS.FAIL;
			}
			return true;
		}
	}

	public INTERACTION_TYPE goapType => action.goapType;

	public string goapName => action.goapName;

	public bool isRumor => rumor != null;

	public bool isAssumption => assumption != null;

	public string name => action.goapName;

	public string classificationName => "News";

	public IPointOfInterest target => poiTarget;

	public Log informationLog => descriptionLog;

	public RUMOR_TYPE rumorType => RUMOR_TYPE.Action;

	public CRIMABLE_TYPE crimableType => CRIMABLE_TYPE.Action;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Action;

	public Type serializedData => typeof(SaveDataActualGoapNode);

	public bool isStillProcessing => stillProcessingCounter > 0;

	public ActualGoapNode()
	{
		instanceID = Utilities.GetNewUniqueID();
		persistentID = string.Empty;
		logTags = new List<LOG_TAG>(4);
		awareCharacters = new List<Character>(10);
		invalidity = new GoapActionInvalidity();
	}

	public ActualGoapNode(SaveDataActualGoapNode data)
	{
		instanceID = data.instanceID;
		if (data.logTags != null && data.logTags.Count > 0)
		{
			logTags = new List<LOG_TAG>(data.logTags);
		}
		if (logTags == null)
		{
			logTags = new List<LOG_TAG>(4);
		}
		awareCharacters = new List<Character>(10);
		persistentID = data.persistentID;
		isStealth = data.isStealth;
		isAvoidWitnesses = data.isAvoidWitnesses;
		avoidCombat = data.avoidCombat;
		cost = data.cost;
		action = InteractionManager.Instance.goapActionData[data.action];
		actionStatus = data.actionStatus;
		associatedJobType = data.associatedJobType;
		currentStateName = data.currentStateName;
		ticksPerformingCurrentState = data.ticksPerformingCurrentState;
		expectedActionStateDuration = data.expectedActionStateDuration;
		crimeType = data.crimeType;
		isIntel = data.isIntel;
		isUsedAsCrime = data.isUsedAsCrime;
		isNegativeInfo = data.isNegativeInfo;
		isSupposedToBeInPool = data.isSupposedToBeInPool;
		stillProcessingCounter = data.stillProcessingCounter;
		hasBeenReset = data.hasBeenReset;
		isAssigned = data.isAssigned;
		hasStartedPerTickEffect = data.hasStartedPerTickEffect;
		isIllusion = data.isIllusion;
		isFabricated = data.isFabricated;
		invalidity = new GoapActionInvalidity();
	}

	public void SetActionData(GoapAction action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, int cost)
	{
		persistentID = Utilities.GetNewUniqueID();
		this.action = action;
		this.actor = actor;
		this.poiTarget = poiTarget;
		this.otherData = otherData;
		if (otherData != null)
		{
			for (int i = 0; i < otherData.Length; i++)
			{
				otherData[i]?.IncreaseReferenceCount();
			}
		}
		this.cost = cost;
		uniqueActionData = CreateUniqueActionData(action);
		disguisedActor = actor.reactionComponent.disguisedCharacter;
		if (poiTarget is Character character)
		{
			disguisedTarget = character.reactionComponent.disguisedCharacter;
		}
		SetDefaultLogTags();
		SetAdditionalLogTags();
		SetHasBeenReset(p_state: false);
	}

	public void SetOtherData(OtherData[] otherData)
	{
		this.otherData = otherData;
		for (int i = 0; i < otherData.Length; i++)
		{
			otherData[i]?.IncreaseReferenceCount();
		}
	}

	public virtual void DoAction(JobQueueItem job, GoapPlan plan)
	{
		actionStatus = ACTION_STATUS.STARTED;
		associatedJobType = job.jobType;
		SetJob(job);
		isStealth = IsActionStealth(job);
		isAvoidWitnesses = IsActionAvoidWitness(job);
		avoidCombat = IsActionAvoidCombat(job);
		actor.SetCurrentActionNode(this, job, plan);
		Messenger.Broadcast(JobSignals.CHARACTER_DOING_ACTION, actor, this);
		action.OnActionStarted(this);
		SetCrimeType();
		actor.marker.pathfindingAI.ResetEndReachedDistance();
		SetTargetToGoTo();
		CreateThoughtBubbleLog();
		CheckAndMoveToDoAction(job);
	}

	private void SetTargetToGoTo()
	{
		if (targetStructure == null)
		{
			targetStructure = action.GetTargetStructure(this);
			if (targetStructure == null)
			{
				targetTile = null;
				return;
			}
		}
		if (action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET || action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET)
		{
			IPointOfInterest targetToGoTo = action.GetTargetToGoTo(this);
			if (targetToGoTo == null)
			{
				targetTile = action.GetTargetTileToGoTo(this);
			}
			else
			{
				targetPOIToGoTo = targetToGoTo;
				targetTile = targetToGoTo.gridTileLocation;
			}
			if (actor.movementComponent.isStationary && actor.gridTileLocation != targetTile && !actor.gridTileLocation.IsNeighbour(targetTile, sameStructureOnly: true))
			{
				targetTile = null;
			}
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.IN_PLACE)
		{
			targetTile = actor.gridTileLocation;
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.NEARBY)
		{
			targetTile = GetRandomNearbyTileFromActor(actor);
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION)
		{
			if (actor.movementComponent.isStationary)
			{
				targetTile = null;
				return;
			}
			targetTile = action.GetTargetTileToGoTo(this);
			if (targetTile != null)
			{
				return;
			}
			if (targetStructure is Wilderness)
			{
				Area areaLocation = actor.areaLocation;
				if (areaLocation != null)
				{
					targetTile = areaLocation.neighbourComponent.GetRandomNearbyPassableWildernessGridTileWithPathTo(actor);
				}
			}
			if (targetTile == null)
			{
				List<LocationGridTile> passableTiles = targetStructure.passableTiles;
				if (passableTiles.Count > 0)
				{
					targetTile = passableTiles[Utilities.Rng.Next(0, passableTiles.Count)];
				}
			}
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B)
		{
			if (actor.movementComponent.isStationary)
			{
				targetTile = null;
				return;
			}
			targetTile = action.GetTargetTileToGoTo(this);
			if (targetTile != null)
			{
				return;
			}
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			for (int i = 0; i < targetStructure.unoccupiedTiles.Count; i++)
			{
				LocationGridTile locationGridTile = targetStructure.unoccupiedTiles[i];
				if (locationGridTile.HasUnoccupiedNeighbour(sameStructureOnly: true))
				{
					list.Add(locationGridTile);
				}
			}
			if (list.Count > 0)
			{
				targetTile = list[Utilities.Rng.Next(0, list.Count)];
			}
			else if (targetStructure.HasUnoccupiedTile())
			{
				targetTile = targetStructure.unoccupiedTiles[Utilities.Rng.Next(0, targetStructure.unoccupiedTiles.Count)];
			}
			else if (targetStructure.passableTiles.Count > 0)
			{
				targetTile = CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
			}
			else if (targetStructure.tiles.Count > 0)
			{
				targetTile = CollectionUtilities.GetRandomElement(targetStructure.tiles);
			}
			else if (actor.gridTileLocation != null)
			{
				targetTile = actor.gridTileLocation;
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION)
		{
			if (actor.marker.IsPOIInVision(poiTarget))
			{
				targetTile = actor.gridTileLocation;
				return;
			}
			if (actor.movementComponent.isStationary)
			{
				targetTile = null;
				return;
			}
			IPointOfInterest targetToGoTo2 = action.GetTargetToGoTo(this);
			if (targetToGoTo2 == null)
			{
				targetTile = action.GetTargetTileToGoTo(this);
				return;
			}
			targetPOIToGoTo = targetToGoTo2;
			targetTile = targetToGoTo2.gridTileLocation;
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE)
		{
			LocationGridTile overrideTargetTile = action.GetOverrideTargetTile(this);
			if (overrideTargetTile != null)
			{
				targetTile = overrideTargetTile;
			}
			if (targetTile != null && actor.movementComponent.isStationary && actor.gridTileLocation != targetTile && !actor.gridTileLocation.IsNeighbour(targetTile, sameStructureOnly: true))
			{
				targetTile = null;
			}
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL)
		{
			if (actor.currentStructure == targetStructure && targetStructure.structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				targetTile = actor.gridTileLocation;
				return;
			}
			IPointOfInterest targetToGoTo3 = action.GetTargetToGoTo(this);
			if (targetToGoTo3 == null)
			{
				targetTile = action.GetTargetTileToGoTo(this);
			}
			else
			{
				targetPOIToGoTo = targetToGoTo3;
				targetTile = targetToGoTo3.gridTileLocation;
			}
			if (actor.movementComponent.isStationary && actor.gridTileLocation != targetTile && !actor.gridTileLocation.IsNeighbour(targetTile, sameStructureOnly: true))
			{
				targetTile = null;
			}
		}
		else
		{
			if (action.actionLocationType != ACTION_LOCATION_TYPE.ON_REACH_CORRUPTION)
			{
				return;
			}
			if (actor.gridTileLocation != null && actor.gridTileLocation.corruptionComponent.isCorrupted)
			{
				targetTile = actor.gridTileLocation;
				return;
			}
			IPointOfInterest targetToGoTo4 = action.GetTargetToGoTo(this);
			if (targetToGoTo4 == null)
			{
				targetTile = action.GetTargetTileToGoTo(this);
			}
			else
			{
				targetPOIToGoTo = targetToGoTo4;
				targetTile = targetToGoTo4.gridTileLocation;
			}
			if (actor.movementComponent.isStationary && actor.gridTileLocation != targetTile && actor.gridTileLocation != null && !actor.gridTileLocation.IsNeighbour(targetTile, sameStructureOnly: true))
			{
				targetTile = null;
			}
		}
	}

	private void CheckAndMoveToDoAction(JobQueueItem job)
	{
		if (actor.currentActionNode != this || job.originalOwner == null)
		{
			return;
		}
		actor.movementComponent.Unstuck();
		if (!MoveToDoAction(job))
		{
			if (targetTile != null)
			{
				if (job.originalOwner != null && job.originalOwner.ownerType != JOB_OWNER.CHARACTER)
				{
					job.AddBlacklistedCharacter(actor);
				}
				actor.NoPathToDoJobOrAction(job, this);
				job.CancelJob();
			}
		}
		else
		{
			if (hasBeenReset)
			{
				return;
			}
			if (avoidCombat)
			{
				if (actor.hasMarker)
				{
					actor.marker.SetVisionColliderSize(12);
				}
			}
			else if (actor.hasMarker)
			{
				actor.marker.SetVisionColliderSize(8);
			}
			action.OnMoveToDoAction(this);
		}
	}

	private bool MoveToDoAction(JobQueueItem job)
	{
		if (targetTile == null)
		{
			job.CancelJob();
			return false;
		}
		if (actor.currentRegion != targetTile.structure.region)
		{
			if (!actor.movementComponent.MoveToAnotherRegion(targetTile.structure.region, delegate
			{
				CheckAndMoveToDoAction(job);
			}) || !actor.limiterComponent.canMove)
			{
				return false;
			}
		}
		else if (targetPOIToGoTo == null)
		{
			if (targetTile == actor.gridTileLocation)
			{
				actor.marker.StopMovement();
				actor.PerformGoapAction();
			}
			else
			{
				if ((!action.canBePerformedEvenIfPathImpossible && !actor.movementComponent.HasPathTo(targetTile)) || !actor.limiterComponent.canMove)
				{
					return false;
				}
				actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
			}
		}
		else if (actor.gridTileLocation == targetPOIToGoTo.gridTileLocation)
		{
			actor.marker.StopMovement();
			actor.PerformGoapAction();
		}
		else
		{
			if ((!action.canBePerformedEvenIfPathImpossible && !actor.movementComponent.HasPathTo(targetPOIToGoTo.gridTileLocation)) || !actor.limiterComponent.canMove)
			{
				return false;
			}
			actor.marker.GoToPOI(targetPOIToGoTo, OnArriveAtTargetLocation);
		}
		return true;
	}

	public void OnArriveAtTargetLocation()
	{
		if (hasBeenReset)
		{
			return;
		}
		if (actor.hasMarker && actor.marker.visionColliderComponent != null)
		{
			actor.marker.visionColliderComponent.TransferAllDifferentStructureCharacters();
		}
		if (hasBeenReset)
		{
			return;
		}
		if (action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION)
		{
			if (actor.hasMarker)
			{
				actor.PerformGoapAction();
			}
		}
		else if (action.actionLocationType == ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL)
		{
			if (targetStructure != null && (targetStructure.structureType == STRUCTURE_TYPE.WILDERNESS || actor.currentStructure != targetStructure))
			{
				actor.PerformGoapAction();
			}
		}
		else
		{
			actor.PerformGoapAction();
		}
	}

	public void PerformAction()
	{
		GoapActionInvalidity goapActionInvalidity = action.IsInvalid(this);
		string reason = string.Empty;
		bool flag = action.IsInvalidOnVision(this, out reason);
		bool flag2 = IsInvalidStealthOrAvoidWitnesses();
		if (goapActionInvalidity.isInvalid || flag || flag2)
		{
			if (!string.IsNullOrEmpty(reason) && string.IsNullOrEmpty(goapActionInvalidity.reason))
			{
				goapActionInvalidity.reason = reason;
			}
			if (goapActionInvalidity.shouldLogInvalidity)
			{
				action.LogActionInvalid(goapActionInvalidity, this, flag2);
			}
			SetIsStillProcessing(p_state: true);
			actor.GoapActionResult("Fail", this);
			action.OnInvalidAction(this);
			JobQueueItem jobQueueItem = associatedJob;
			if (jobQueueItem != null)
			{
				if (jobQueueItem.forceCancelOnInvalid || goapActionInvalidity.IsReasonForCancellationShouldDropJob())
				{
					jobQueueItem.ForceCancelJob();
				}
				else if (flag || flag2)
				{
					if (jobQueueItem.originalOwner != null && jobQueueItem.originalOwner.ownerType != JOB_OWNER.CHARACTER)
					{
						jobQueueItem.AddBlacklistedCharacter(actor);
					}
					jobQueueItem.CancelJob();
				}
				else if (goapActionInvalidity.stateName == "Invite Rejected" && action.goapType == INTERACTION_TYPE.INVITE)
				{
					if (jobQueueItem.originalOwner != null && jobQueueItem.originalOwner.ownerType != JOB_OWNER.CHARACTER)
					{
						jobQueueItem.AddBlacklistedCharacter(actor);
					}
					jobQueueItem.CancelJob();
				}
				else if (jobQueueItem.invalidCounter > 0)
				{
					if (jobQueueItem.originalOwner != null && jobQueueItem.originalOwner.ownerType != JOB_OWNER.CHARACTER)
					{
						jobQueueItem.AddBlacklistedCharacter(actor);
					}
					jobQueueItem.CancelJob();
				}
				else
				{
					jobQueueItem.IncreaseInvalidCounter();
				}
			}
			action.AfterInvalidAction(this, goapActionInvalidity);
			SetIsStillProcessing(p_state: false);
			if (isSupposedToBeInPool)
			{
				ProcessReturnToPool();
			}
			return;
		}
		actionStatus = ACTION_STATUS.PERFORMING;
		actor.marker.UpdateAnimation();
		if ((associatedJobType == JOB_TYPE.ENERGY_RECOVERY_NORMAL || associatedJobType == JOB_TYPE.ENERGY_RECOVERY_URGENT) && actor.gatheringComponent.hasGathering && actor.gatheringComponent.currentGathering is SocialGathering)
		{
			actor.gatheringComponent.currentGathering.RemoveAttendee(actor);
		}
		if (poiTarget is Character character)
		{
			if (!action.doesNotStopTargetCharacter && actor != poiTarget)
			{
				if (!character.isDead)
				{
					if (character.marker.isMoving)
					{
						character.marker.StopMovement();
					}
					if (character.stateComponent.currentState != null)
					{
						character.stateComponent.currentState.PauseState();
					}
					if (character.currentActionNode != null)
					{
						character.StopCurrentActionNode();
					}
					character.limiterComponent.DecreaseCanMove();
					InnerMapManager.Instance.FaceTarget(character, actor);
				}
				character.AdjustNumOfNonSecretActionsBeingPerformedOnThis(1);
				for (int i = 0; i < character.allJobsTargetingThis.Count; i++)
				{
					if (character.allJobsTargetingThis[i] is GoapPlanJob { assignedCharacter: { } assignedCharacter } goapPlanJob && assignedCharacter.currentJob == goapPlanJob && assignedCharacter.currentActionNode != null && assignedCharacter.currentActionNode.action != null && assignedCharacter.currentActionNode.action.goapType == INTERACTION_TYPE.RAISE_CORPSE && assignedCharacter.currentActionNode != this && goapPlanJob != associatedJob)
					{
						assignedCharacter.currentJob.CancelJob();
						break;
					}
				}
			}
		}
		else
		{
			poiTarget.AdjustNumOfNonSecretActionsBeingPerformedOnThis(1);
			if (poiTarget is TileObject tileObject)
			{
				tileObject.AdjustRepairCounter(1);
			}
			InnerMapManager.Instance.FaceTarget(actor, poiTarget);
		}
		if (associatedJobType != JOB_TYPE.REMOVE_STATUS && associatedJobType != JOB_TYPE.REPAIR && associatedJobType != JOB_TYPE.FEED && !action.doesNotStopTargetCharacter && actor != target)
		{
			poiTarget.CancelRemoveStatusFeedAndRepairJobsTargetingThis();
		}
		if ((action.actionCategory == ACTION_CATEGORY.DIRECT || action.actionCategory == ACTION_CATEGORY.CONSUME) && poiTarget is BaseMapObject baseMapObject)
		{
			baseMapObject.OnManipulatedBy(actor);
		}
		SetIsStillProcessing(p_state: true);
		action.Perform(this);
		Messenger.Broadcast(JobSignals.STARTED_PERFORMING_ACTION, this);
		SetIsStillProcessing(p_state: false);
		if (isSupposedToBeInPool)
		{
			ProcessReturnToPool();
		}
	}

	public void ActionInterruptedWhilePerforming()
	{
		actionStatus = ACTION_STATUS.FAIL;
		StopPerTickEffect();
		if (poiTarget is Character character)
		{
			if (!action.doesNotStopTargetCharacter && actor != poiTarget)
			{
				if (!character.isDead)
				{
					if (character.stateComponent.currentState != null && character.stateComponent.currentState.isPaused)
					{
						character.stateComponent.currentState.ResumeState();
					}
					character.limiterComponent.IncreaseCanMove();
				}
				character.AdjustNumOfNonSecretActionsBeingPerformedOnThis(-1);
			}
		}
		else
		{
			poiTarget.AdjustNumOfNonSecretActionsBeingPerformedOnThis(-1);
		}
		OnFinishActionTowardsTarget();
		GoapPlanJob obj = actor.currentJob as GoapPlanJob;
		if (actor.currentActionNode == this)
		{
			actor.SetCurrentActionNode(null, null, null);
		}
		Character arg = actor;
		IPointOfInterest arg2 = poiTarget;
		INTERACTION_TYPE arg3 = action.goapType;
		ACTION_STATUS arg4 = actionStatus;
		obj?.CancelJob();
		Messenger.Broadcast(JobSignals.CHARACTER_FINISHED_ACTION, arg, arg2, arg3, arg4);
	}

	private void ActionResult(GoapActionState actionState)
	{
		string stateResult = GoapActionStateDB.GetStateResult(action.goapType, actionState.name);
		actionStatus = ((stateResult == "Success") ? ACTION_STATUS.SUCCESS : ACTION_STATUS.FAIL);
		StopPerTickEffect();
		if (poiTarget is Character character)
		{
			if (!action.doesNotStopTargetCharacter && actor != poiTarget)
			{
				if (!character.isDead)
				{
					if (character.stateComponent.currentState != null && character.stateComponent.currentState.isPaused)
					{
						character.stateComponent.currentState.ResumeState();
					}
					character.limiterComponent.IncreaseCanMove();
				}
				character.AdjustNumOfNonSecretActionsBeingPerformedOnThis(-1);
			}
		}
		else
		{
			poiTarget.AdjustNumOfNonSecretActionsBeingPerformedOnThis(-1);
		}
		OnFinishActionTowardsTarget();
		Character arg = actor;
		IPointOfInterest arg2 = poiTarget;
		INTERACTION_TYPE arg3 = action.goapType;
		ACTION_STATUS arg4 = actionStatus;
		actor.GoapActionResult(stateResult, this);
		Messenger.Broadcast(JobSignals.CHARACTER_FINISHED_ACTION, arg, arg2, arg3, arg4);
	}

	public void StopActionNode()
	{
		if (actionStatus == ACTION_STATUS.PERFORMING)
		{
			SetIsStillProcessing(p_state: true);
			action.OnStopWhilePerforming(this);
			OnCancelActionTowardsTarget();
			SetIsStillProcessing(p_state: false);
			ActionInterruptedWhilePerforming();
		}
		else if (actionStatus == ACTION_STATUS.STARTED)
		{
			action.OnStopWhileStarted(this);
		}
	}

	private bool IsActionStealth(JobQueueItem job)
	{
		if (action.goapType == INTERACTION_TYPE.STEAL || action.goapType == INTERACTION_TYPE.STEAL_ANYTHING || action.goapType == INTERACTION_TYPE.PICKPOCKET || action.goapType == INTERACTION_TYPE.STEAL_COINS)
		{
			return true;
		}
		if (action.goapType == INTERACTION_TYPE.REMOVE_BUFF)
		{
			return true;
		}
		if (action.goapType == INTERACTION_TYPE.DRINK_BLOOD || action.goapType == INTERACTION_TYPE.VAMPIRIC_EMBRACE || action.goapType == INTERACTION_TYPE.STEAL_TRAIT)
		{
			return true;
		}
		if (action.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER && !job.jobType.IsApprehendTypeJob())
		{
			return true;
		}
		if (job.jobType == JOB_TYPE.PLACE_TRAP || job.jobType == JOB_TYPE.POISON_FOOD)
		{
			return true;
		}
		if (job.jobType == JOB_TYPE.ARSON)
		{
			return true;
		}
		if (job.jobType == JOB_TYPE.SNATCH && (action.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER || action.goapType == INTERACTION_TYPE.ASSAULT))
		{
			return true;
		}
		if (action.goapType == INTERACTION_TYPE.PRAY && action.GetCrimeType(actor, poiTarget, this) != CRIME_TYPE.None)
		{
			return true;
		}
		return false;
	}

	private bool IsActionAvoidCombat(JobQueueItem job)
	{
		if (job.jobType == JOB_TYPE.STEAL_CORPSE)
		{
			return true;
		}
		return false;
	}

	private bool IsActionAvoidWitness(JobQueueItem job)
	{
		if (action != null && (action.goapType == INTERACTION_TYPE.EVANGELIZE || action.goapType == INTERACTION_TYPE.DARK_RITUAL || action.goapType == INTERACTION_TYPE.LIBERATE))
		{
			return true;
		}
		if (job is GoapPlanJob { assignedPlan: not null } goapPlanJob && goapPlanJob.assignedPlan.HasNodeWithAction(INTERACTION_TYPE.BUTCHER))
		{
			bool flag = action != null && (action.goapType == INTERACTION_TYPE.BUTCHER || action.goapType == INTERACTION_TYPE.ASSAULT);
			if (flag && poiTarget is Character character && character.race.IsSapient())
			{
				return true;
			}
		}
		GoapAction goapAction3 = action;
		if (goapAction3 != null && goapAction3.goapType == INTERACTION_TYPE.BUTCHER && poiTarget is Character character2 && character2.race.IsSapient())
		{
			return true;
		}
		return false;
	}

	public void SetTargetStructure(LocationStructure structure)
	{
		targetStructure = structure;
	}

	public void OnActionStateSet(string stateName)
	{
		currentStateName = stateName;
		SetActionDuration();
		OnPerformActualActionToTarget();
		ExecuteCurrentActionState();
	}

	private void SetActionDuration()
	{
		GoapActionState goapActionState = currentState;
		if (goapActionState.duration >= 0)
		{
			expectedActionStateDuration = goapActionState.duration;
		}
		else
		{
			expectedActionStateDuration = action.DetermineActionDuration(goapActionState, this);
		}
	}

	private void ExecuteCurrentActionState()
	{
		GoapActionState goapActionState = currentState;
		_ = poiTarget;
		if (ShouldDoVigilantEffect())
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "vigilant", LOG_TAG.Social, this);
			action.AddFillersToLog(log, this);
			log.AddToFillers(null, action.localizedName, LOG_IDENTIFIER.STRING_1);
			OverrideDescriptionLog(log);
			actor.marker.UpdateAnimation();
			if (expectedActionStateDuration != -1)
			{
				ticksPerformingCurrentState = expectedActionStateDuration;
				EndPerTickEffect();
			}
			return;
		}
		CreateDescriptionLog(goapActionState);
		goapActionState.preEffect?.Invoke(this);
		List<Trait> traitOverrideFunctions = actor.traitContainer.GetTraitOverrideFunctions("Execute_Pre_Effect_Trait");
		List<Trait> traitOverrideFunctions2 = poiTarget.traitContainer.GetTraitOverrideFunctions("Execute_Pre_Effect_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].ExecuteActionPreEffects(action.goapType, this);
			}
		}
		if (traitOverrideFunctions2 != null)
		{
			for (int j = 0; j < traitOverrideFunctions2.Count; j++)
			{
				traitOverrideFunctions2[j].ExecuteActionPreEffects(action.goapType, this);
			}
		}
		actor.marker.UpdateAnimation();
		if (expectedActionStateDuration > 0)
		{
			ticksPerformingCurrentState = 0;
			StartPerTickEffect();
		}
		else if (expectedActionStateDuration != -1)
		{
			EndPerTickEffect();
		}
	}

	private void StartPerTickEffect()
	{
		hasStartedPerTickEffect = true;
	}

	public void StopPerTickEffect()
	{
		hasStartedPerTickEffect = false;
	}

	public void EndPerTickEffect(bool shouldDoAfterEffect = true)
	{
		if (actionStatus != ACTION_STATUS.FAIL && actionStatus != ACTION_STATUS.SUCCESS)
		{
			if (ShouldDoVigilantEffect())
			{
				EndEffectVigilant();
			}
			else
			{
				EndEffectNormal(shouldDoAfterEffect);
			}
		}
	}

	private bool ShouldDoVigilantEffect()
	{
		if (isStealth && target.traitContainer.HasTrait("Vigilant") && !target.traitContainer.HasTrait(InteractionManager.Instance.vigilantCancellingTraits) && !target.isDead && target != actor)
		{
			if (target.traitContainer.HasTrait("Hemophiliac") && action.GetCrimeType(actor, target, this) == CRIME_TYPE.Vampire)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void LogAction(Log p_log, bool ignoreShouldAddLog = false)
	{
		if (p_log == null || !(action.ShouldAddLogs(this) || ignoreShouldAddLog) || !CharacterManager.Instance.CanAddCharacterLogOrShowNotif(action.goapType))
		{
			return;
		}
		p_log.AddLogToDatabase();
		if (action.ShouldActionBeAnIntel(this))
		{
			Character character = poiTarget as Character;
			if (actor.isNormalCharacter || (character != null && character.isNormalCharacter))
			{
				if (action.shouldShowNotifRegardlessOfWatcher)
				{
					PlayerManager.Instance.player.ShowNotificationFromPlayer(InteractionManager.Instance.CreateNewIntel(this));
				}
				else if (PlayerManager.Instance.player.ShouldShowNotificationFrom(actor))
				{
					PlayerManager.Instance.player.ShowNotificationFrom(actor, InteractionManager.Instance.CreateNewIntel(this));
				}
			}
		}
		else if (action.showNotification)
		{
			if (action.shouldShowNotifRegardlessOfWatcher)
			{
				PlayerManager.Instance.player.ShowNotificationFromPlayer(p_log);
			}
			else
			{
				PlayerManager.Instance.player.ShowNotificationFrom(actor, p_log);
			}
		}
	}

	private void EndEffectNormal(bool shouldDoAfterEffect)
	{
		if (shouldDoAfterEffect)
		{
			LogAction(descriptionLog);
		}
		GoapActionState goapActionState = currentState;
		Character character = actor;
		IPointOfInterest pointOfInterest = poiTarget;
		GoapAction goapAction = action;
		JobQueueItem currentJob = actor.currentJob;
		SetIsStillProcessing(p_state: true);
		ActionResult(goapActionState);
		if (shouldDoAfterEffect)
		{
			goapActionState.afterEffect?.Invoke(this);
			bool flag = false;
			List<Trait> traitOverrideFunctions = character.traitContainer.GetTraitOverrideFunctions("Execute_Pre_Effect_Trait");
			List<Trait> traitOverrideFunctions2 = pointOfInterest.traitContainer.GetTraitOverrideFunctions("Execute_Pre_Effect_Trait");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					Trait trait = traitOverrideFunctions[i];
					flag = false;
					trait.ExecuteActionAfterEffects(goapAction.goapType, character, pointOfInterest, goapAction.actionCategory, ref flag);
					if (flag)
					{
						i--;
					}
				}
			}
			if (traitOverrideFunctions2 != null)
			{
				for (int j = 0; j < traitOverrideFunctions2.Count; j++)
				{
					Trait trait2 = traitOverrideFunctions2[j];
					flag = false;
					trait2.ExecuteActionAfterEffects(goapAction.goapType, character, pointOfInterest, goapAction.actionCategory, ref flag);
					if (flag)
					{
						j--;
					}
				}
			}
			if (currentJob != null && currentJob.hasBeenReset)
			{
				currentJob?.CleanUpAfterActionIsDone();
			}
		}
		SetIsStillProcessing(p_state: false);
		if (isSupposedToBeInPool)
		{
			ProcessReturnToPool();
		}
	}

	private void EndEffectVigilant()
	{
		if (descriptionLog != null && action.ShouldAddLogs(this) && CharacterManager.Instance.CanAddCharacterLogOrShowNotif(action.goapType))
		{
			descriptionLog.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(actor.gridTileLocation, descriptionLog);
		}
		JobQueueItem currentJob = actor.currentJob;
		GoapActionState actionState = currentState;
		ActionResult(actionState);
		if (currentJob != null)
		{
			if (currentJob.hasBeenReset)
			{
				currentJob?.CleanUpAfterActionIsDone();
			}
			else if (currentJob.jobType != JOB_TYPE.NONE)
			{
				currentJob.CancelJob();
			}
		}
	}

	public void PerTickEffect()
	{
		if (hasBeenReset)
		{
			StopPerTickEffect();
		}
		else
		{
			if (!hasStartedPerTickEffect)
			{
				return;
			}
			GoapActionState goapActionState = currentState;
			ticksPerformingCurrentState++;
			IPointOfInterest pointOfInterest = poiTarget;
			if (!actor.interruptComponent.hasTriggeredSimultaneousInterrupt)
			{
				InnerMapManager.Instance.FaceTarget(actor, pointOfInterest);
			}
			goapActionState.perTickEffect?.Invoke(this);
			if (hasBeenReset)
			{
				StopPerTickEffect();
				return;
			}
			List<Trait> traitOverrideFunctions = actor.traitContainer.GetTraitOverrideFunctions("Execute_Pre_Effect_Trait");
			List<Trait> traitOverrideFunctions2 = poiTarget.traitContainer.GetTraitOverrideFunctions("Execute_Pre_Effect_Trait");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					Trait trait = traitOverrideFunctions[i];
					if (hasBeenReset)
					{
						break;
					}
					trait?.ExecuteActionPerTickEffects(action.goapType, this);
				}
			}
			if (traitOverrideFunctions2 != null)
			{
				for (int j = 0; j < traitOverrideFunctions2.Count; j++)
				{
					Trait trait2 = traitOverrideFunctions2[j];
					if (hasBeenReset)
					{
						break;
					}
					trait2?.ExecuteActionPerTickEffects(action.goapType, this);
				}
			}
			if (hasBeenReset)
			{
				StopPerTickEffect();
			}
			else if (ticksPerformingCurrentState >= expectedActionStateDuration)
			{
				EndPerTickEffect();
			}
		}
	}

	private void OnPerformActualActionToTarget()
	{
		if (!(GoapActionStateDB.GetStateResult(action.goapType, currentStateName) != "Success") && poiTarget is TileObject tileObject)
		{
			tileObject.OnDoActionToObject(this);
		}
	}

	private void OnFinishActionTowardsTarget()
	{
		if (poiTarget is TileObject tileObject)
		{
			tileObject.AdjustRepairCounter(-1);
			if (actionStatus != ACTION_STATUS.FAIL)
			{
				tileObject.OnDoneActionToObject(this);
			}
		}
	}

	private void OnCancelActionTowardsTarget()
	{
		if (poiTarget is TileObject tileObject)
		{
			tileObject.OnCancelActionTowardsObject(this);
		}
	}

	public void OverrideCurrentStateDuration(int val)
	{
		ticksPerformingCurrentState = val;
	}

	private void CreateDescriptionLog(GoapActionState actionState)
	{
		if (descriptionLog == null)
		{
			descriptionLog = actionState.CreateDescriptionLog(this);
		}
	}

	private void CreateThoughtBubbleLog()
	{
		if (thoughtBubbleLog == null)
		{
			string key = action.goapName + " thought_bubble";
			if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", key)))
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", key, LOG_TAG.Major);
				action.AddFillersToLog(log, this);
				thoughtBubbleLog = log;
			}
		}
		if (thoughtBubbleMovingLog == null)
		{
			string key2 = action.goapName + " thought_bubble_m";
			if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", key2)))
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", action.goapName + " thought_bubble_m");
				action.AddFillersToLog(log2, this);
				thoughtBubbleMovingLog = log2;
			}
		}
	}

	public Log GetCurrentLog()
	{
		if (actionStatus == ACTION_STATUS.STARTED)
		{
			return thoughtBubbleMovingLog;
		}
		if (actionStatus == ACTION_STATUS.PERFORMING)
		{
			if (thoughtBubbleLog == null)
			{
				return thoughtBubbleMovingLog;
			}
			return thoughtBubbleLog;
		}
		return descriptionLog;
	}

	public void OverrideDescriptionLog(Log log)
	{
		if (descriptionLog != null)
		{
			LogPool.Release(descriptionLog);
		}
		descriptionLog = log;
	}

	private void SetDefaultLogTags()
	{
		logTags.Clear();
		if (action.logTags != null)
		{
			for (int i = 0; i < action.logTags.Length; i++)
			{
				logTags.Add(action.logTags[i]);
			}
		}
	}

	private void SetAdditionalLogTags()
	{
		if (action.ShouldActionBeAnIntel(this))
		{
			Character character = poiTarget as Character;
			if ((actor.isNormalCharacter || (character != null && character.isNormalCharacter)) && !logTags.Contains(LOG_TAG.Intel))
			{
				logTags.Add(LOG_TAG.Intel);
			}
		}
	}

	public void OnAttachPlanToJob(GoapPlanJob job)
	{
	}

	public void OnUnattachPlanToJob(GoapPlanJob job)
	{
		if (associatedJob == job)
		{
			SetJob(null);
		}
	}

	public void SetJob(JobQueueItem job)
	{
		associatedJob = job;
	}

	public void AddAwareCharacter(Character character)
	{
		awareCharacters.Add(character);
	}

	public bool IsAware(Character character)
	{
		return awareCharacters.Contains(character);
	}

	public override string ToString()
	{
		return "Action: " + (action?.name ?? "Null") + ". Actor: " + actor?.name + " . Target: " + (poiTarget?.name ?? "Null");
	}

	public void SetAsRumor(Rumor newRumor)
	{
		if (rumor != newRumor)
		{
			rumor = newRumor;
			if (rumor != null)
			{
				rumor.SetRumorable(this);
				actionStatus = ACTION_STATUS.SUCCESS;
				currentStateName = GoapActionStateDB.goapActionStates[goapType][0].name;
				CreateDescriptionLog(currentState);
				SetIsFabricated(p_state: true);
			}
		}
	}

	public void SetAsAssumption(Assumption newAssumption)
	{
		if (assumption != newAssumption)
		{
			assumption = newAssumption;
			if (assumption != null)
			{
				assumption.SetAssumedAction(this);
				actionStatus = ACTION_STATUS.SUCCESS;
				currentStateName = GoapActionStateDB.goapActionStates[goapType][0].name;
				CreateDescriptionLog(currentState);
			}
		}
	}

	public void SetIsIntel(bool p_state)
	{
		isIntel = p_state;
	}

	public void SetIsNegativeInfo(bool p_state)
	{
		isNegativeInfo = p_state;
	}

	public void SetAsIllusion()
	{
		isIllusion = true;
		actionStatus = ACTION_STATUS.SUCCESS;
		currentStateName = GoapActionStateDB.goapActionStates[goapType][0].name;
		CreateDescriptionLog(currentState);
	}

	public void SetAsIllusion(string p_stateName)
	{
		isIllusion = true;
		actionStatus = ACTION_STATUS.SUCCESS;
		currentStateName = p_stateName;
		CreateDescriptionLog(currentState);
	}

	public void SetAsIllusionForTesting(string p_stateName)
	{
		isIllusion = true;
		actionStatus = ACTION_STATUS.SUCCESS;
		currentStateName = p_stateName;
		CreateThoughtBubbleLog();
		CreateDescriptionLog(currentState);
	}

	public void SetIsFabricated(bool p_state)
	{
		isFabricated = p_state;
	}

	public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return action.ReactionToActor(actor, target, witness, this, status);
	}

	public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return action.ReactionToTarget(actor, target, witness, this, status);
	}

	public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		return action.ReactionOfTarget(actor, target, this, status);
	}

	public void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		action.PopulateEmotionReactionsToActor(reactions, actor, target, witness, this, status);
	}

	public void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		action.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, this, status);
	}

	public void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		action.PopulateEmotionReactionsOfTarget(reactions, actor, target, this, status);
	}

	public REACTABLE_EFFECT GetReactableEffect(Character witness)
	{
		return action.GetReactableEffect(this, witness);
	}

	public void SetIsUsedAsCrime(bool p_state)
	{
		isUsedAsCrime = p_state;
	}

	public void SetCrimeType()
	{
		if (crimeType == CRIME_TYPE.Unset)
		{
			Character disguisedCharacter = actor;
			IPointOfInterest disguisedCharacter2 = poiTarget;
			if (actor.reactionComponent.disguisedCharacter != null)
			{
				disguisedCharacter = actor.reactionComponent.disguisedCharacter;
			}
			if (poiTarget is Character character && character.reactionComponent.disguisedCharacter != null)
			{
				disguisedCharacter2 = character.reactionComponent.disguisedCharacter;
			}
			crimeType = action.GetCrimeType(disguisedCharacter, disguisedCharacter2, this);
			if (crimeType != CRIME_TYPE.None && !logTags.Contains(LOG_TAG.Crimes))
			{
				logTags.Add(LOG_TAG.Crimes);
			}
			SetAdditionalLogTags();
		}
	}

	private UniqueActionData CreateUniqueActionData(GoapAction action)
	{
		if (action.uniqueActionDataType != null)
		{
			return Activator.CreateInstance(action.uniqueActionDataType) as UniqueActionData;
		}
		return null;
	}

	public T GetConvertedUniqueActionData<T>() where T : UniqueActionData
	{
		return uniqueActionData as T;
	}

	private bool IsInvalidStealthOrAvoidWitnesses()
	{
		bool flag = false;
		if (actor.isNormalCharacter)
		{
			if (isAvoidWitnesses)
			{
				flag = true;
			}
			else if (isStealth)
			{
				flag = true;
				if (associatedJob != null && associatedJob.isTriggeredFlaw)
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			IPointOfInterest poi = poiTarget;
			if (poiTarget != actor)
			{
				if (actor.hasMarker && actor.marker.IsPOIInVision(poi) && !actor.marker.CanDoStealthCrimeToTarget(poi, crimeType))
				{
					return true;
				}
			}
			else if (actor.hasMarker && !actor.marker.CanDoStealthCrimeToTarget(poi, crimeType))
			{
				return true;
			}
		}
		return false;
	}

	public LocationGridTile GetRandomNearbyTileFromActor(Character p_actor)
	{
		LocationGridTile result;
		if (p_actor.limiterComponent.canMove && !p_actor.movementComponent.isStationary)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			action.PopulateNearbyLocation(list, this);
			if (list.Count <= 0)
			{
				p_actor.gridTileLocation.PopulateTilesInRadius(list, 3, 0, includeCenterTile: false, includeTilesInDifferentStructure: false, includeImpassable: false);
			}
			result = ((list.Count <= 0) ? p_actor.gridTileLocation : list[Utilities.Rng.Next(0, list.Count)]);
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		else
		{
			result = p_actor.gridTileLocation;
		}
		return result;
	}

	public void DoActionUponLoadingSavedGame()
	{
		if (actionStatus != ACTION_STATUS.STARTED)
		{
			if (actionStatus != ACTION_STATUS.PERFORMING)
			{
				throw new Exception("Action " + action.name + " of " + actor.name + " is being done again after loading but the status is " + actionStatus);
			}
			actor.marker.UpdateAnimation();
			if (expectedActionStateDuration > 0)
			{
				StartPerTickEffect();
			}
			else if (expectedActionStateDuration != -1)
			{
				EndPerTickEffect();
			}
		}
	}

	public bool LoadReferences(SaveDataActualGoapNode data)
	{
		bool result = true;
		actor = CharacterManager.Instance.GetCharacterByPersistentID(data.actor);
		if (actor == null)
		{
			result = false;
		}
		if (data.poiTargetType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			poiTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.poiTarget);
			if (poiTarget == null)
			{
				result = false;
			}
		}
		else if (data.poiTargetType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			poiTarget = InnerMapManager.Instance.GetTileObjectByPersistentID(data.poiTarget);
			if (poiTarget == null)
			{
				result = false;
			}
		}
		if (!string.IsNullOrEmpty(data.disguisedActor))
		{
			disguisedActor = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedActor);
		}
		if (!string.IsNullOrEmpty(data.disguisedTarget))
		{
			disguisedTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedTarget);
		}
		if (data.thoughtBubbleLog != null)
		{
			thoughtBubbleLog = LogPool.Claim();
			thoughtBubbleLog.Copy(data.thoughtBubbleLog);
		}
		if (data.thoughtBubbleMovingLog != null)
		{
			thoughtBubbleMovingLog = LogPool.Claim();
			thoughtBubbleMovingLog.Copy(data.thoughtBubbleMovingLog);
		}
		if (data.descriptionLog != null)
		{
			descriptionLog = LogPool.Claim();
			descriptionLog.Copy(data.descriptionLog);
		}
		if (!string.IsNullOrEmpty(data.targetStructure))
		{
			targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.targetStructure);
		}
		if (!string.IsNullOrEmpty(data.targetPOIToGoTo))
		{
			if (data.targetPOIToGoToType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				targetPOIToGoTo = CharacterManager.Instance.GetCharacterByPersistentID(data.targetPOIToGoTo);
			}
			else if (data.targetPOIToGoToType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				targetPOIToGoTo = InnerMapManager.Instance.GetTileObjectByPersistentID(data.targetPOIToGoTo);
			}
			if (targetPOIToGoTo == null)
			{
				result = false;
			}
		}
		if (data.awareCharacters != null)
		{
			for (int i = 0; i < data.awareCharacters.Count; i++)
			{
				Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(data.awareCharacters[i]);
				if (characterByPersistentID != null)
				{
					awareCharacters.Add(characterByPersistentID);
				}
			}
		}
		if (data.hasRumor)
		{
			rumor = data.rumor.Load();
			rumor.SetRumorable(this);
		}
		if (data.hasAssumption)
		{
			assumption = data.assumption.Load();
			assumption.SetAssumedAction(this);
		}
		if (data.otherData != null)
		{
			otherData = new OtherData[data.otherData.Length];
			for (int j = 0; j < otherData.Length; j++)
			{
				SaveDataOtherData saveDataOtherData = data.otherData[j];
				if (saveDataOtherData != null)
				{
					otherData[j] = saveDataOtherData.Load();
				}
			}
		}
		if (!string.IsNullOrEmpty(data.associatedJobID))
		{
			associatedJob = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.associatedJobID);
		}
		if (data.uniqueActionData != null)
		{
			uniqueActionData = data.uniqueActionData.Load();
		}
		return result;
	}

	public void LoadAdditionalReferences(SaveDataActualGoapNode data)
	{
		if (this.otherData == null)
		{
			return;
		}
		for (int i = 0; i < this.otherData.Length; i++)
		{
			OtherData otherData = this.otherData[i];
			SaveDataOtherData saveDataOtherData = data.otherData.ElementAtOrDefault(i);
			if (otherData != null && saveDataOtherData != null)
			{
				otherData.LoadAdditionalData(saveDataOtherData);
			}
		}
	}

	public static bool operator ==(ActualGoapNode left, ActualGoapNode right)
	{
		return left?.persistentID == right?.persistentID;
	}

	public static bool operator !=(ActualGoapNode left, ActualGoapNode right)
	{
		return left?.persistentID != right?.persistentID;
	}

	public static bool operator ==(ActualGoapNode left, ICrimeable right)
	{
		return left?.persistentID == right?.persistentID;
	}

	public static bool operator !=(ActualGoapNode left, ICrimeable right)
	{
		return left?.persistentID != right?.persistentID;
	}

	public static bool operator ==(ActualGoapNode left, IRumorable right)
	{
		return left?.persistentID == right?.persistentID;
	}

	public static bool operator !=(ActualGoapNode left, IRumorable right)
	{
		return left?.persistentID != right?.persistentID;
	}

	public override bool Equals(object obj)
	{
		if (obj is ActualGoapNode actualGoapNode)
		{
			return persistentID.Equals(actualGoapNode.persistentID);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public void SetIsStillProcessing(bool p_state)
	{
		if (p_state)
		{
			stillProcessingCounter++;
		}
		else
		{
			stillProcessingCounter--;
		}
	}

	public void SetIsSupposedToBeInPool(bool p_state)
	{
		isSupposedToBeInPool = p_state;
	}

	public void SetHasBeenReset(bool p_state)
	{
		hasBeenReset = p_state;
	}

	public void IncreaseReactionCounter()
	{
		reactionProcessCounter++;
	}

	public void DecreaseReactionCounter()
	{
		reactionProcessCounter--;
		if (isSupposedToBeInPool)
		{
			ProcessReturnToPool();
		}
	}

	public bool ProcessReturnToPool()
	{
		if (reactionProcessCounter <= 0 && !isRumor && !isIntel && !isAssumption && !isStillProcessing && !isNegativeInfo && !isUsedAsCrime)
		{
			ObjectPoolManager.Instance.ReturnActionToPool(this);
			return true;
		}
		return false;
	}

	public void Reset()
	{
		persistentID = string.Empty;
		actor = null;
		poiTarget = null;
		disguisedActor = null;
		disguisedTarget = null;
		isStealth = false;
		avoidCombat = false;
		if (this.otherData != null)
		{
			for (int i = 0; i < this.otherData.Length; i++)
			{
				OtherData otherData = this.otherData[i];
				if (otherData != null)
				{
					otherData.DecreaseReferenceCount();
					if (otherData.actionReferenceCount <= 0)
					{
						otherData.CleanUp();
					}
				}
			}
		}
		this.otherData = null;
		cost = 0;
		action = null;
		if (thoughtBubbleLog != null)
		{
			LogPool.Release(thoughtBubbleLog);
		}
		if (thoughtBubbleMovingLog != null)
		{
			LogPool.Release(thoughtBubbleMovingLog);
		}
		if (descriptionLog != null)
		{
			LogPool.Release(descriptionLog);
		}
		thoughtBubbleLog = null;
		thoughtBubbleMovingLog = null;
		descriptionLog = null;
		targetStructure = null;
		targetTile = null;
		targetPOIToGoTo = null;
		associatedJobType = JOB_TYPE.NONE;
		associatedJob = null;
		currentStateName = string.Empty;
		ticksPerformingCurrentState = 0;
		rumor = null;
		assumption = null;
		awareCharacters.Clear();
		logTags.Clear();
		crimeType = CRIME_TYPE.Unset;
		isUsedAsCrime = false;
		uniqueActionData = null;
		actionStatus = ACTION_STATUS.NONE;
		isSupposedToBeInPool = false;
		isIntel = false;
		isNegativeInfo = false;
		stillProcessingCounter = 0;
		isIllusion = false;
		isFabricated = false;
		invalidity.Reset();
		SetHasBeenReset(p_state: true);
		StopPerTickEffect();
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		awareCharacters.Remove(p_character);
		rumor?.DisconnectFromCharacter(p_character);
		assumption?.DisconnectFromCharacter(p_character);
	}

	public bool IsImportantDataNull()
	{
		if (action == null || actor == null || poiTarget == null)
		{
			return true;
		}
		if (assumption != null && (assumption.characterThatCreatedAssumption == null || assumption.targetCharacter == null))
		{
			return true;
		}
		if (rumor != null && (rumor.characterThatCreatedRumor == null || rumor.targetCharacter == null))
		{
			return true;
		}
		return false;
	}

	public bool IsNodeObjectInvalid()
	{
		if (hasBeenReset)
		{
			return true;
		}
		if (IsImportantDataNull())
		{
			return true;
		}
		if (otherData != null)
		{
			for (int i = 0; i < otherData.Length; i++)
			{
				if (otherData[i].IsOtherDataInvalid())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		IsStructureReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		IsCharacterReferenced(p_character);
	}

	public bool IsStructureReferenced(LocationStructure p_structure)
	{
		if (targetStructure == p_structure)
		{
			return true;
		}
		if (uniqueActionData != null && uniqueActionData.IsStructureReferenced(p_structure))
		{
			return true;
		}
		if (otherData != null)
		{
			for (int i = 0; i < otherData.Length; i++)
			{
				if (otherData[i].IsStructureReferenced(p_structure))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsCharacterReferenced(Character p_character)
	{
		if (actor == p_character)
		{
			return true;
		}
		if (poiTarget == p_character)
		{
			return true;
		}
		if (disguisedActor == p_character)
		{
			return true;
		}
		if (disguisedTarget == p_character)
		{
			return true;
		}
		if (targetPOIToGoTo == p_character)
		{
			return true;
		}
		if (awareCharacters.Contains(p_character))
		{
			return true;
		}
		if (assumption != null && assumption.IsCharacterReferenced(p_character))
		{
			return true;
		}
		if (rumor != null && rumor.IsCharacterReferenced(p_character))
		{
			return true;
		}
		if (uniqueActionData != null && uniqueActionData.IsCharacterReferenced(p_character))
		{
			return true;
		}
		if (otherData != null)
		{
			for (int i = 0; i < otherData.Length; i++)
			{
				if (otherData[i].IsCharacterReferenced(p_character))
				{
					return true;
				}
			}
		}
		return false;
	}
}
