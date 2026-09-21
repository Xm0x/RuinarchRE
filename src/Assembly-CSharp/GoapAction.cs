using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crime_System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class GoapAction
{
	protected TIME_IN_WORDS[] validTimeOfDays;

	private string _localizedName;

	public INTERACTION_TYPE goapType { get; private set; }

	public virtual ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public string goapName { get; protected set; }

	public Precondition basePrecondition { get; private set; }

	public List<GoapEffect> baseExpectedEffects { get; private set; }

	public List<GoapEffectConditionTypeAndTargetType> possibleExpectedEffectsTypeAndTargetMatching { get; private set; }

	public Dictionary<string, GoapActionState> states { get; protected set; }

	public ACTION_LOCATION_TYPE actionLocationType { get; protected set; }

	public bool showNotification { get; protected set; }

	protected bool shouldAddLogs { get; set; }

	public string actionIconString { get; protected set; }

	public string animationName { get; protected set; }

	public bool doesNotStopTargetCharacter { get; protected set; }

	public bool canBeAdvertisedEvenIfTargetIsUnavailable { get; protected set; }

	public bool canBePerformedEvenIfPathImpossible { get; protected set; }

	public bool canBePerformedEvenIfTargetInCombat { get; protected set; }

	public bool canBePerformedEvenIfTargetHasNoTileLocation { get; protected set; }

	public LOG_TAG[] logTags { get; protected set; }

	public string name => goapName;

	public string localizedName
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedName))
			{
				_localizedName = goapType.LocalizedActionName();
			}
			return _localizedName;
		}
	}

	public virtual Type uniqueActionDataType => null;

	public virtual bool shouldShowNotifRegardlessOfWatcher => false;

	public virtual bool isTargetSelf => false;

	public GoapAction(INTERACTION_TYPE goapType)
	{
		this.goapType = goapType;
		goapName = Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToStringEnum());
		showNotification = false;
		shouldAddLogs = true;
		basePrecondition = null;
		baseExpectedEffects = new List<GoapEffect>();
		possibleExpectedEffectsTypeAndTargetMatching = new List<GoapEffectConditionTypeAndTargetType>();
		actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		actionIconString = GoapActionStateDB.No_Icon;
		canBeAdvertisedEvenIfTargetIsUnavailable = false;
		canBePerformedEvenIfPathImpossible = false;
		canBePerformedEvenIfTargetInCombat = false;
		canBePerformedEvenIfTargetHasNoTileLocation = false;
		animationName = "Interact";
		ConstructBasePreconditionsAndEffects();
		CreateStates();
	}

	public void SetState(string stateName, ActualGoapNode actionNode)
	{
		actionNode.OnActionStateSet(stateName);
		Messenger.Broadcast(JobSignals.AFTER_ACTION_STATE_SET, stateName, actionNode);
	}

	public virtual int DetermineActionDuration(GoapActionState p_goapActionState, ActualGoapNode p_actualGoapNode)
	{
		return 1;
	}

	private void CreateStates()
	{
		states = new Dictionary<string, GoapActionState>();
		if (!GoapActionStateDB.goapActionStates.ContainsKey(goapType))
		{
			return;
		}
		StateNameAndDuration[] array = GoapActionStateDB.goapActionStates[goapType];
		for (int i = 0; i < array.Length; i++)
		{
			StateNameAndDuration stateNameAndDuration = array[i];
			string text = Utilities.RemoveAllWhiteSpace(stateNameAndDuration.name);
			Type type = GetType();
			string text2 = "Pre" + text;
			string text3 = "PerTick" + text;
			string text4 = "After" + text;
			MethodInfo method = type.GetMethod(text2, new Type[1] { typeof(ActualGoapNode) });
			MethodInfo method2 = type.GetMethod(text3, new Type[1] { typeof(ActualGoapNode) });
			MethodInfo method3 = type.GetMethod(text4, new Type[1] { typeof(ActualGoapNode) });
			Action<ActualGoapNode> preEffect = null;
			Action<ActualGoapNode> perTickEffect = null;
			Action<ActualGoapNode> afterEffect = null;
			if (method != null)
			{
				preEffect = (Action<ActualGoapNode>)Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, method, throwOnBindFailure: false);
			}
			if (method2 != null)
			{
				perTickEffect = (Action<ActualGoapNode>)Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, method2, throwOnBindFailure: false);
			}
			if (method3 != null)
			{
				afterEffect = (Action<ActualGoapNode>)Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, method3, throwOnBindFailure: false);
			}
			GoapActionState value = new GoapActionState(stateNameAndDuration.name, this, preEffect, perTickEffect, afterEffect, stateNameAndDuration.duration, stateNameAndDuration.status, stateNameAndDuration.animationName);
			states.Add(stateNameAndDuration.name, value);
		}
	}

	protected virtual void ConstructBasePreconditionsAndEffects()
	{
	}

	public virtual void Perform(ActualGoapNode actionNode)
	{
	}

	protected virtual bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		return true;
	}

	protected virtual int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 0;
	}

	public virtual void AddFillersToLog(Log log, ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		LocationStructure targetStructure = node.targetStructure;
		log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		if (targetStructure != null)
		{
			log.AddToFillers(targetStructure, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		}
	}

	public virtual bool IsInvalidOnVision(ActualGoapNode node, out string reason)
	{
		if (node.poiTarget is Character character && character.combatComponent.isInActualCombat && !canBePerformedEvenIfTargetInCombat)
		{
			reason = "target_in_combat";
			return true;
		}
		reason = string.Empty;
		return false;
	}

	public virtual GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		string p_targetMissingLog;
		bool isInvalid = IsTargetMissing(node, out p_targetMissingLog);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unreachable";
		return invalidity;
	}

	public virtual void OnInvalidAction(ActualGoapNode node)
	{
	}

	public virtual void AfterInvalidAction(ActualGoapNode node, GoapActionInvalidity invalidity)
	{
	}

	public virtual LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget.gridTileLocation == null)
		{
			if (poiTarget is TileObject tileObject && tileObject.isBeingCarriedBy?.currentStructure != null)
			{
				return tileObject.isBeingCarriedBy.currentStructure;
			}
			return null;
		}
		return poiTarget.gridTileLocation.structure;
	}

	public virtual IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		return goapNode.poiTarget;
	}

	public virtual LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		return goapNode.poiTarget.gridTileLocation;
	}

	public virtual void OnStopWhilePerforming(ActualGoapNode node)
	{
	}

	public virtual void OnStopWhileStarted(ActualGoapNode node)
	{
	}

	public virtual LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode)
	{
		return null;
	}

	public virtual void PopulateNearbyLocation(List<LocationGridTile> gridTiles, ActualGoapNode goapNode)
	{
	}

	public virtual string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string empty = string.Empty;
		bool flag;
		if (status == REACTION_STATUS.WITNESSED)
		{
			flag = witness.IsHostileWith(actor);
		}
		else
		{
			flag = witness.IsHostileWithCheckingForInformed(actor);
			if (flag && CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType).IsConsideredACrime() && target is Character character && !witness.IsHostileWithCheckingForInformed(character) && witness.relationshipContainer.IsFriendsWith(character))
			{
				LocationGridTile gridTileLocation = witness.gridTileLocation;
				LocationGridTile gridTileLocation2 = actor.gridTileLocation;
				if (gridTileLocation != null && gridTileLocation2 != null && gridTileLocation.area.GetAreaDistanceTo(gridTileLocation2.area) <= 3 && witness.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation2))
				{
					witness.combatComponent.Fight(actor, "Slay_Target");
					return "Attack_Nearby_Actor";
				}
			}
		}
		if (flag)
		{
			CrimeType crimeType = CrimeManager.Instance.GetCrimeType(node.crimeType);
			if (crimeType != null)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "hostile_crime_reaction_with_crime", LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Major);
				log.AddToFillers(witness, witness.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddToFillers(null, crimeType.name, LOG_IDENTIFIER.STRING_1);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(witness, log);
				LogPool.Release(log);
			}
			else
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "hostile_crime_reaction", LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Major);
				log2.AddToFillers(witness, witness.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(witness, log2);
				LogPool.Release(log2);
			}
			return "Stop_Telling_News";
		}
		node.IncreaseReactionCounter();
		empty = CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
		node.DecreaseReactionCounter();
		string text = string.Empty;
		List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
		PopulateEmotionReactionsToActor(list, actor, target, witness, node, status);
		int p_totalOpinionReduction = 0;
		string p_lastStrawReasonKey = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			text += CharacterManager.Instance.TriggerEmotion(list[i], witness, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, node, "", p_triggerOpinionChangesEffect: false);
		}
		if (p_totalOpinionReduction < 0 && !witness.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
		{
			witness.relationshipContainer.CreateJobsOnOpinionReduced(witness, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
		}
		RuinarchListPool<EMOTION>.Release(list);
		if (string.IsNullOrEmpty(empty))
		{
			empty += text;
		}
		return empty;
	}

	public virtual string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string text = string.Empty;
		if (target is Character character)
		{
			if ((status != REACTION_STATUS.WITNESSED) ? witness.IsHostileWithCheckingForInformed(character) : witness.IsHostileWith(character))
			{
				return text;
			}
			List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
			PopulateEmotionReactionsToTarget(list, actor, character, witness, node, status);
			int p_totalOpinionReduction = 0;
			string p_lastStrawReasonKey = string.Empty;
			for (int i = 0; i < list.Count; i++)
			{
				text += CharacterManager.Instance.TriggerEmotion(list[i], witness, character, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, node, "", p_triggerOpinionChangesEffect: false);
			}
			if (p_totalOpinionReduction < 0 && !witness.reactionComponent.isDisguised && !character.reactionComponent.isDisguised)
			{
				witness.relationshipContainer.CreateJobsOnOpinionReduced(witness, character, p_lastStrawReasonKey, p_totalOpinionReduction);
			}
			RuinarchListPool<EMOTION>.Release(list);
		}
		return text;
	}

	public virtual string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		if (target is Character character)
		{
			node.IncreaseReactionCounter();
			CrimeManager.Instance.ReactToCrime(character, actor, target, target.factionOwner, node.crimeType, node, status);
			node.DecreaseReactionCounter();
			List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
			PopulateEmotionReactionsOfTarget(list, actor, target, node, status);
			string text = string.Empty;
			int p_totalOpinionReduction = 0;
			string p_lastStrawReasonKey = string.Empty;
			for (int i = 0; i < list.Count; i++)
			{
				text += CharacterManager.Instance.TriggerEmotion(list[i], character, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, node, "", p_triggerOpinionChangesEffect: false);
			}
			if (p_totalOpinionReduction < 0 && !character.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
			{
				character.relationshipContainer.CreateJobsOnOpinionReduced(character, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
			}
			RuinarchListPool<EMOTION>.Release(list);
			return text;
		}
		return string.Empty;
	}

	public virtual void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
	}

	public virtual void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
	}

	public virtual void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
	}

	public virtual void OnActionStarted(ActualGoapNode node)
	{
	}

	public virtual void OnStoppedInterrupt(ActualGoapNode node)
	{
	}

	public virtual REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Neutral;
	}

	public virtual void OnMoveToDoAction(ActualGoapNode node)
	{
	}

	public virtual string GetActionIconString(ActualGoapNode node)
	{
		return actionIconString;
	}

	public virtual bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return false;
	}

	public virtual bool IsFullnessRecoveryAction()
	{
		return false;
	}

	public virtual bool IsTirednessRecoveryAction()
	{
		return false;
	}

	public virtual bool IsHappinessRecoveryAction()
	{
		return false;
	}

	public virtual bool ShouldAddLogs(ActualGoapNode goapNode)
	{
		return shouldAddLogs;
	}

	public int GetCost(Character actor, IPointOfInterest target, GoapPlanJob job)
	{
		OtherData[] otherDataFor = job.GetOtherDataFor(goapType);
		int baseCost = GetBaseCost(actor, target, job, otherDataFor);
		int distanceCost = GetDistanceCost(actor, target, job);
		return baseCost * PreconditionCostMultiplier() + distanceCost;
	}

	protected bool IsTargetMissing(ActualGoapNode node, out string p_targetMissingLog)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		bool num = poiTarget.IsAvailable();
		p_targetMissingLog = null;
		if ((!num && !canBeAdvertisedEvenIfTargetIsUnavailable) || (poiTarget.gridTileLocation == null && !canBePerformedEvenIfTargetHasNoTileLocation))
		{
			return true;
		}
		if (actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE && actor.currentRegion != poiTarget.gridTileLocation.structure.region)
		{
			return true;
		}
		if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET)
		{
			if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true))
			{
				if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
				{
					return false;
				}
				return true;
			}
		}
		else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET)
		{
			if (actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
			{
				return true;
			}
		}
		else if (actionLocationType == ACTION_LOCATION_TYPE.NEARBY || actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION || actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B || actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE)
		{
			if (actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
			{
				return true;
			}
		}
		else if (actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION && (!actor.hasMarker || !actor.marker.IsPOIInVision(poiTarget)))
		{
			return true;
		}
		return false;
	}

	public bool CanSatisfyRequirements(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job, bool shouldCheckTimeOfDays = true)
	{
		bool flag = true;
		if (poiTarget is TileObject tileObject)
		{
			if (tileObject.traitContainer.HasTrait("Frozen") && (actionCategory == ACTION_CATEGORY.DIRECT || actionCategory == ACTION_CATEGORY.CONSUME))
			{
				flag = false;
			}
			if (actionCategory == ACTION_CATEGORY.CONSUME)
			{
				Poisoned traitOrStatus = tileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
				if (traitOrStatus != null && traitOrStatus.awareCharacters.Contains(actor))
				{
					flag = false;
				}
				BoobyTrapped traitOrStatus2 = tileObject.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
				if (traitOrStatus2 != null && traitOrStatus2.awareCharacters.Contains(actor))
				{
					flag = false;
				}
			}
			else if (actionCategory == ACTION_CATEGORY.DIRECT)
			{
				BoobyTrapped traitOrStatus3 = tileObject.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
				if (traitOrStatus3 != null && traitOrStatus3.awareCharacters.Contains(actor))
				{
					flag = false;
				}
			}
		}
		if (!actor.limiterComponent.canPerform && actionLocationType != ACTION_LOCATION_TYPE.NEARBY && actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE)
		{
			flag = false;
			if ((goapType == INTERACTION_TYPE.SLEEP || goapType == INTERACTION_TYPE.SLEEP_OUTSIDE) && actor.traitContainer.HasTrait("Paralyzed") && actor.gridTileLocation != null && (actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true)))
			{
				flag = true;
			}
		}
		if (poiTarget is Character { hasMarker: false })
		{
			flag = false;
		}
		if (flag)
		{
			flag = AreRequirementsSatisfied(actor, poiTarget, otherData, job);
		}
		TIME_IN_WORDS value = ((goapType != INTERACTION_TYPE.DRINK_BLOOD) ? GameManager.Instance.GetCurrentTimeInWordsOfTick(actor) : GameManager.Instance.GetCurrentTimeInWordsOfTick());
		if (flag)
		{
			if (shouldCheckTimeOfDays && job.jobType != JOB_TYPE.TRIGGER_FLAW && job.jobType != JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT && validTimeOfDays != null)
			{
				return validTimeOfDays.Contains(value);
			}
			return true;
		}
		return false;
	}

	private int GetDistanceCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job)
	{
		if (job.jobType == JOB_TYPE.SNATCH || job.jobType == JOB_TYPE.SNATCH_RESTRAIN || job.jobType == JOB_TYPE.DROP_ITEM_PARTY)
		{
			return 1;
		}
		if (poiTarget == job.poiTarget)
		{
			return 1;
		}
		int distanceMultiplier = GetDistanceMultiplier(job);
		LocationGridTile gridTileLocation = poiTarget.gridTileLocation;
		if (actor.gridTileLocation != null && gridTileLocation != null)
		{
			return Mathf.RoundToInt(actor.gridTileLocation.GetDistanceTo(gridTileLocation)) * distanceMultiplier;
		}
		return 1;
	}

	private int GetDistanceMultiplier(JobQueueItem job)
	{
		if (job.jobType == JOB_TYPE.HAUL_ON_SIGHT || job.jobType == JOB_TYPE.TAKE_ITEM_ON_SIGHT)
		{
			return 50;
		}
		return 2;
	}

	private int PreconditionCostMultiplier()
	{
		return 1;
	}

	public void LogActionInvalid(GoapActionInvalidity goapActionInvalidity, ActualGoapNode node, bool isInvalidStealth)
	{
		string text = goapActionInvalidity.stateName.ToLower() + "_description";
		if (goapActionInvalidity.stateName != "Target Missing" && !string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", name + " " + text)))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", name + " " + text, LOG_TAG.Work, null);
			AddFillersToLog(log, node);
			log.AddLogToDatabase(releaseLogAfter: true);
			return;
		}
		string reason = goapActionInvalidity.reason;
		string value = null;
		string text2 = "Invalid";
		if (isInvalidStealth)
		{
			text2 = "Invalid_Stealth";
		}
		else if (!string.IsNullOrEmpty(reason))
		{
			value = LocalizationManager.Instance.GetLocalizedValue("ActionInvalidReasons_Table", reason);
			if (!string.IsNullOrEmpty(value))
			{
				text2 = "Invalid_with_reason";
			}
		}
		Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "ActionInvalidReasons_Table", text2, LOG_TAG.Work, null);
		log2.AddToFillers(node.actor, node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log2.AddToFillers(node.poiTarget, node.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log2.AddToFillers(null, localizedName, LOG_IDENTIFIER.STRING_1);
		if (text2 == "Invalid_with_reason")
		{
			log2.AddToFillers(null, value, LOG_IDENTIFIER.STRING_2);
		}
		log2.AddLogToDatabase(releaseLogAfter: true);
	}

	protected void SetPrecondition(GoapEffect effect, Func<Character, IPointOfInterest, OtherData[], JOB_TYPE, bool> condition)
	{
		basePrecondition = new Precondition(effect, condition);
	}

	public bool CanSatisfyAllPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType)
	{
		bool isOverridden = false;
		bool result = true;
		Precondition precondition = GetPrecondition(actor, target, otherData, jobType, out isOverridden);
		if (precondition != null && !precondition.CanSatisfyCondition(actor, target, otherData, jobType))
		{
			result = false;
		}
		return result;
	}

	public bool CanSatisfyAllPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out Precondition failedPrecondition)
	{
		bool isOverridden = false;
		bool result = true;
		failedPrecondition = null;
		Precondition precondition = GetPrecondition(actor, target, otherData, jobType, out isOverridden);
		if (precondition != null && !precondition.CanSatisfyCondition(actor, target, otherData, jobType))
		{
			result = false;
			failedPrecondition = precondition;
		}
		return result;
	}

	public virtual Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		isOverridden = false;
		return basePrecondition;
	}

	protected void AddExpectedEffect(GoapEffect effect)
	{
		baseExpectedEffects.Add(effect);
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(effect.conditionType, effect.target));
	}

	protected void AddPossibleExpectedEffectForTypeAndTargetMatching(GoapEffectConditionTypeAndTargetType effect)
	{
		possibleExpectedEffectsTypeAndTargetMatching.Add(effect);
	}

	protected void AddBaseExpectedEffectsToList(List<GoapEffect> p_expectedEffects)
	{
		if (baseExpectedEffects != null && baseExpectedEffects.Count > 0)
		{
			for (int i = 0; i < baseExpectedEffects.Count; i++)
			{
				p_expectedEffects.Add(baseExpectedEffects[i]);
			}
		}
	}

	public bool WillEffectsSatisfyPrecondition(GoapEffect precondition, Character actor, IPointOfInterest target, GoapPlanJob job)
	{
		OtherData[] otherDataFor = job.GetOtherDataFor(goapType);
		bool isOverriden = false;
		List<GoapEffect> expectedEffects = GetExpectedEffects(actor, target, otherDataFor, out isOverriden);
		bool result = false;
		for (int i = 0; i < expectedEffects.Count; i++)
		{
			if (EffectPreconditionMatching(expectedEffects[i], precondition))
			{
				result = true;
				break;
			}
		}
		if (isOverriden)
		{
			RuinarchListPool<GoapEffect>.Release(expectedEffects);
		}
		return result;
	}

	private bool EffectPreconditionMatching(GoapEffect effect, GoapEffect precondition)
	{
		if (effect.conditionType == precondition.conditionType && effect.target == precondition.target)
		{
			if (effect.conditionKey == string.Empty && precondition.conditionKey == string.Empty)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(effect.conditionKey) && !string.IsNullOrEmpty(precondition.conditionKey))
			{
				if (effect.isKeyANumber && precondition.isKeyANumber)
				{
					int num = int.Parse(effect.conditionKey);
					int num2 = int.Parse(precondition.conditionKey);
					return num >= num2;
				}
				if (precondition.conditionKey == "Food Pile")
				{
					if (!(effect.conditionKey == "Animal Meat") && !(effect.conditionKey == "Human Meat") && !(effect.conditionKey == "Elf Meat") && !(effect.conditionKey == "Vegetables") && !(effect.conditionKey == "Fish Pile") && !(effect.conditionKey == "Food Pile") && !(effect.conditionKey == "Potato") && !(effect.conditionKey == "Corn") && !(effect.conditionKey == "Pineapple") && !(effect.conditionKey == "Iceberry"))
					{
						return effect.conditionKey == "Mushroom";
					}
					return true;
				}
				if (precondition.conditionKey == "Cloth Pile")
				{
					if (!(effect.conditionKey == "Mink Cloth") && !(effect.conditionKey == "Moon Thread") && !(effect.conditionKey == "Mooncrawler Cloth") && !(effect.conditionKey == "Rabbit Cloth") && !(effect.conditionKey == "Spider Silk") && !(effect.conditionKey == "Wool"))
					{
						return effect.conditionKey == "Cloth Pile";
					}
					return true;
				}
				if (precondition.conditionKey == "Leather Pile")
				{
					if (!(effect.conditionKey == "Bear Hide") && !(effect.conditionKey == "Boar Hide") && !(effect.conditionKey == "Dragon Hide") && !(effect.conditionKey == "Wolf Hide"))
					{
						return effect.conditionKey == "Leather Pile";
					}
					return true;
				}
				if (precondition.conditionKey == "Metal Pile")
				{
					if (!(effect.conditionKey == "Copper") && !(effect.conditionKey == "Iron") && !(effect.conditionKey == "Mithril") && !(effect.conditionKey == "Orichalcum"))
					{
						return effect.conditionKey == "Metal Pile";
					}
					return true;
				}
			}
			return effect.conditionKey == precondition.conditionKey;
		}
		return false;
	}

	protected virtual List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverriden)
	{
		isOverriden = false;
		return baseExpectedEffects;
	}

	public virtual CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.None;
	}

	public virtual CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.None;
	}

	public virtual BLACKMAIL_TYPE GetBlackMailTypeConsideringTarget(ActualGoapNode p_goapNode, Character p_targetCharacter)
	{
		return BLACKMAIL_TYPE.None;
	}
}
