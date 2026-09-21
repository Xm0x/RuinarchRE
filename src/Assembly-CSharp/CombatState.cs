using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CombatState : CharacterState
{
	private const float Wall_Attack_Range_Tolerance = 0.4f;

	private const float Moving_Target_Tolerance = 0.9f;

	public bool isExecutingAttack;

	private int _timesHitCurrentTarget;

	public bool isAttacking { get; private set; }

	public IPointOfInterest currentClosestHostile { get; private set; }

	public Character forcedTarget { get; private set; }

	public List<Character> allCharactersThatDegradedRel { get; private set; }

	public List<Character> allCharactersThatReactedToThisCombat { get; private set; }

	public IPointOfInterest lastFledFrom { get; private set; }

	public LocationStructure lastFledFromStructure { get; private set; }

	public bool isBeingApprehended { get; private set; }

	public GraphNode repositioningTo { get; private set; }

	public bool isRepositioning => repositioningTo != null;

	public CombatState(CharacterStateComponent characterComp)
		: base(characterComp)
	{
		base.stateName = "Combat State";
		base.characterState = CHARACTER_STATE.COMBAT;
		base.duration = 0;
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		isAttacking = true;
		allCharactersThatDegradedRel = new List<Character>();
		allCharactersThatReactedToThisCombat = new List<Character>();
	}

	protected override void DoMovementBehavior()
	{
		base.DoMovementBehavior();
		base.stateComponent.owner.combatComponent.SetWillProcessCombat(state: false);
		StartCombatMovement();
	}

	protected override void StartState()
	{
		base.stateComponent.owner.isBeingCarriedBy?.StopCurrentActionNode();
		base.stateComponent.owner.marker.ShowHPBar(base.stateComponent.owner);
		base.stateComponent.owner.marker.SetAnimationBool("InCombat", value: true);
		base.stateComponent.owner.marker.visionColliderComponent.VoteToUnFilterVision();
		if (base.stateComponent.owner.gatheringComponent.hasGathering && base.stateComponent.owner.gatheringComponent.currentGathering is SocialGathering)
		{
			base.stateComponent.owner.gatheringComponent.currentGathering.RemoveAttendee(base.stateComponent.owner);
		}
		Messenger.AddListener<Character>(CharacterSignals.DETERMINE_COMBAT_REACTION, DetermineReaction);
		Messenger.AddListener<Character>(CharacterSignals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
		Messenger.AddListener<Character>(CharacterSignals.START_FLEE, OnCharacterStartFleeing);
		Messenger.AddListener<GenericTileObject>(TileObjectSignals.TILE_DESTROYED, OnTileDestroyed);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		base.StartState();
		base.stateComponent.owner.UncarryPOI();
	}

	protected override void EndState()
	{
		base.stateComponent.owner.marker.pathfindingAI.ClearAllCurrentPathData();
		base.stateComponent.owner.marker.SetHasFleePath(state: false);
		base.stateComponent.owner.marker.HideHPBar();
		base.stateComponent.owner.marker.SetAnimationBool("InCombat", value: false);
		base.stateComponent.owner.marker.visionColliderComponent.VoteToFilterVision();
		Messenger.RemoveListener<Character>(CharacterSignals.DETERMINE_COMBAT_REACTION, DetermineReaction);
		Messenger.RemoveListener<Character>(CharacterSignals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
		Messenger.RemoveListener<Character>(CharacterSignals.START_FLEE, OnCharacterStartFleeing);
		Messenger.RemoveListener<GenericTileObject>(TileObjectSignals.TILE_DESTROYED, OnTileDestroyed);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		if (base.stateComponent.owner.isNormalCharacter)
		{
			List<LocationStructure> list = new List<LocationStructure>(base.stateComponent.owner.movementComponent.structuresToAvoid);
			for (int i = 0; i < list.Count; i++)
			{
				LocationStructure locationStructure = list[i];
				if (base.stateComponent.owner.currentStructure == locationStructure)
				{
					base.stateComponent.owner.movementComponent.RemoveStructureToAvoid(locationStructure);
				}
			}
		}
		base.EndState();
	}

	public override void AfterExitingState()
	{
		base.AfterExitingState();
		if ((bool)base.stateComponent.owner.marker)
		{
			base.stateComponent.owner.marker.visionColliderComponent.ReCategorizeVision();
		}
		if (!base.stateComponent.owner.isDead)
		{
			if (isBeingApprehended && base.stateComponent.owner.traitContainer.HasTrait("Criminal") && base.stateComponent.owner.limiterComponent.canPerform && base.stateComponent.owner.limiterComponent.canMove && !base.stateComponent.owner.traitContainer.HasTrait("Berserked") && base.stateComponent.owner.jobComponent.TryCreateFleeCrimeJob())
			{
				return;
			}
			if ((bool)base.stateComponent.owner.marker)
			{
				for (int i = 0; i < base.stateComponent.owner.marker.inVisionPOIs.Count; i++)
				{
					IPointOfInterest pointOfInterest = base.stateComponent.owner.marker.inVisionPOIs[i];
					if (!base.stateComponent.owner.marker.unprocessedVisionPOIs.Contains(pointOfInterest))
					{
						base.stateComponent.owner.marker.AddUnprocessedPOI(pointOfInterest);
					}
				}
			}
			base.stateComponent.owner.needsComponent.CheckExtremeNeeds();
		}
		base.stateComponent.owner.combatComponent.ClearCombatData();
		if (base.stateComponent.owner.traitContainer.HasTrait("Subterranean"))
		{
			base.stateComponent.owner.behaviourComponent.SetSubterraneanJustExitedCombat(state: true);
		}
	}

	private void DetermineReaction(Character character)
	{
		if (base.stateComponent.owner != character || base.stateComponent.currentState != this || base.isPaused || base.isDone)
		{
			return;
		}
		DetermineIsBeingApprehended();
		string debugLog = string.Empty;
		if (character.marker.hasFleePath)
		{
			CheckFlee(ref debugLog);
		}
		else if (HasStillAvoidPOIThatIsInRange())
		{
			IPointOfInterest pointOfInterest = base.stateComponent.owner.combatComponent.avoidInRange[base.stateComponent.owner.combatComponent.avoidInRange.Count - 1];
			string avoidReasonLogKey = GetAvoidReasonLogKey(pointOfInterest);
			string text = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", avoidReasonLogKey);
			if (string.IsNullOrEmpty(text))
			{
				text = avoidReasonLogKey;
			}
			int num;
			switch (avoidReasonLogKey)
			{
			default:
				num = ((avoidReasonLogKey == "Unkillable") ? 1 : 0);
				break;
			case "Avoiding Witnesses":
			case "Encountered_Hostile":
			case "Vulnerable":
				num = 1;
				break;
			}
			bool flag = (byte)num != 0;
			if (pointOfInterest is Character { isNormalCharacter: not false } && base.stateComponent.owner.traitContainer.HasTrait("Enslaved") && base.stateComponent.owner.isNormalCharacter)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, text);
			}
			else if (character.homeStructure != null)
			{
				if (character.homeStructure == character.currentStructure)
				{
					if (Random.Range(0, 2) == 0 && !flag)
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, text);
					}
					else
					{
						SetIsAttacking(state: false);
					}
					return;
				}
				int num2 = Random.Range(0, 100);
				if (num2 < 40)
				{
					SetIsAttacking(state: false);
				}
				else if ((num2 >= 40 && num2 < 80) || flag)
				{
					SetIsAttacking(state: false);
				}
				else
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, text);
				}
			}
			else if (character is Summon summon && summon.HasTerritory())
			{
				if (summon.IsInTerritory())
				{
					if (Random.Range(0, 2) == 0 && !flag)
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, text);
					}
					else
					{
						SetIsAttacking(state: false);
					}
					return;
				}
				int num3 = Random.Range(0, 100);
				if (num3 < 40)
				{
					SetIsAttacking(state: false);
				}
				else if ((num3 >= 40 && num3 < 80) || flag)
				{
					SetIsAttacking(state: false);
				}
				else
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, text);
				}
			}
			else if (Random.Range(0, 2) == 0 && !flag)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, "", null, text);
			}
			else
			{
				SetIsAttacking(state: false);
			}
		}
		else if (character.combatComponent.hostilesInRange.Count > 0)
		{
			SetIsAttacking(state: true);
		}
		else
		{
			character.combatComponent.ClearAvoidInRange(processCombatBehavior: false);
			character.stateComponent.ExitCurrentState();
		}
	}

	private void OnUpdateMovementState(Character character)
	{
		Character owner = base.stateComponent.owner;
		if (character == owner && base.stateComponent.currentState == this && !base.isPaused && !base.isDone && currentClosestHostile != null && currentClosestHostile is Character character2 && isAttacking && character2.combatComponent.isInCombat && !(character2.stateComponent.currentState as CombatState).isAttacking && !owner.movementComponent.CanStillPursueTarget(character2) && (!owner.combatComponent.combatDataDictionary.ContainsKey(character2) || !(owner.combatComponent.combatDataDictionary[character2].reasonForCombat == "Demon Kill")))
		{
			owner.combatComponent.RemoveHostileInRange(character2);
		}
	}

	private void OnCharacterStartFleeing(Character characterThatFlee)
	{
		Character owner = base.stateComponent.owner;
		if (base.stateComponent.currentState == this && !base.isPaused && !base.isDone && owner.combatComponent.IsHostileInRange(characterThatFlee) && !owner.movementComponent.CanStillPursueTarget(characterThatFlee))
		{
			if (owner.behaviourComponent.HasBehaviour(typeof(DefendBehaviour)) || owner.behaviourComponent.HasBehaviour(typeof(DemonDefendBehaviour)))
			{
				owner.combatComponent.RemoveHostileInRange(characterThatFlee);
			}
			else if (!owner.combatComponent.combatDataDictionary.ContainsKey(characterThatFlee) || !(owner.combatComponent.combatDataDictionary[characterThatFlee].reasonForCombat == "Demon Kill"))
			{
				owner.combatComponent.RemoveHostileInRange(characterThatFlee);
			}
		}
	}

	private void CheckFlee(ref string debugLog)
	{
		if (!HasStillAvoidPOIThatIsInRange())
		{
			if (HasStillHostilePOIThatIsInRange())
			{
				SetIsAttacking(state: true);
			}
			else
			{
				FinishedTravellingFleePath();
			}
		}
		else
		{
			UpdateFleePath();
		}
	}

	private bool HasStillAvoidPOIThatIsInRange()
	{
		for (int i = 0; i < base.stateComponent.owner.combatComponent.avoidInRange.Count; i++)
		{
			IPointOfInterest poi = base.stateComponent.owner.combatComponent.avoidInRange[i];
			if ((bool)base.stateComponent.owner.marker && base.stateComponent.owner.marker.IsStillInRange(poi))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasStillHostilePOIThatIsInRange()
	{
		for (int i = 0; i < base.stateComponent.owner.combatComponent.hostilesInRange.Count; i++)
		{
			IPointOfInterest poi = base.stateComponent.owner.combatComponent.hostilesInRange[i];
			if ((bool)base.stateComponent.owner.marker && base.stateComponent.owner.marker.IsStillInRange(poi))
			{
				return true;
			}
		}
		return false;
	}

	private void SetIsAttacking(bool state)
	{
		isAttacking = state;
		if (isAttacking && base.stateComponent.owner.isLycanthrope && base.stateComponent.owner.lycanData.isMaster && base.stateComponent.owner.lycanData.CanTransformIntoWerewolf() && !base.stateComponent.owner.isInWerewolfForm)
		{
			string log = string.Empty;
			SetClosestHostileProcessing(out var _, ref log);
			if (!base.stateComponent.owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Werewolf, currentClosestHostile as Character))
			{
				if (base.stateComponent.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Werewolf, base.stateComponent.owner))
				{
					base.stateComponent.owner.combatComponent.SetWillProcessCombat(state: true);
					return;
				}
			}
			else if (currentClosestHostile is Character target)
			{
				CombatData combatData = base.stateComponent.owner.combatComponent.GetCombatData(target);
				if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.LYCAN_HUNT_PREY && base.stateComponent.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Werewolf, base.stateComponent.owner))
				{
					base.stateComponent.owner.combatComponent.SetWillProcessCombat(state: true);
					return;
				}
			}
		}
		if (base.stateComponent.owner.combatComponent.combatBehaviourParent.TryDoCombatBehaviour(base.stateComponent.owner, this))
		{
			base.stateComponent.owner.combatComponent.SetWillProcessCombat(state: true);
			return;
		}
		DoCombatBehavior();
		if (base.isDone)
		{
			return;
		}
		if (isAttacking)
		{
			base.actionIconString = base.stateComponent.owner.combatComponent.GetCombatStateIconString(currentClosestHostile);
			string combatLogKeyReason = base.stateComponent.owner.combatComponent.GetCombatLogKeyReason(currentClosestHostile);
			if (string.IsNullOrEmpty(combatLogKeyReason))
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "CharacterState", "CharacterStates_Table", "Combat State thought_bubble_no_reason", LOG_TAG.Combat);
				log2.AddToFillers(base.stateComponent.owner, base.stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				SetThoughtBubbleLog(log2);
			}
			else
			{
				Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "CharacterState", "CharacterStates_Table", "Combat State thought_bubble_with_reason", LOG_TAG.Combat);
				log3.AddToFillers(base.stateComponent.owner, base.stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", combatLogKeyReason);
				if (LocalizationManager.Instance.HasLocalizedValue(localizedValue))
				{
					log3.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
				}
				SetThoughtBubbleLog(log3);
			}
		}
		else
		{
			base.actionIconString = GoapActionStateDB.Flee_Icon;
		}
		base.stateComponent.owner.marker.UpdateActionIcon();
	}

	private void DetermineIsBeingApprehended()
	{
		if (isBeingApprehended)
		{
			return;
		}
		for (int i = 0; i < base.stateComponent.owner.combatComponent.hostilesInRange.Count; i++)
		{
			if (base.stateComponent.owner.combatComponent.hostilesInRange[i] is Character character && character.combatComponent.isInCombat)
			{
				CombatData combatData = character.combatComponent.GetCombatData(base.stateComponent.owner);
				if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType.IsApprehendTypeJob() && character.faction == base.stateComponent.owner.faction)
				{
					isBeingApprehended = true;
					return;
				}
			}
		}
		for (int j = 0; j < base.stateComponent.owner.combatComponent.avoidInRange.Count; j++)
		{
			if (base.stateComponent.owner.combatComponent.avoidInRange[j] is Character character2 && character2.combatComponent.isInCombat)
			{
				CombatData combatData2 = character2.combatComponent.GetCombatData(base.stateComponent.owner);
				if (combatData2 != null && combatData2.connectedAction != null && combatData2.connectedAction.associatedJobType.IsApprehendTypeJob() && character2.faction == base.stateComponent.owner.faction)
				{
					isBeingApprehended = true;
					break;
				}
			}
		}
	}

	private void StartCombatMovement()
	{
		DetermineReaction(base.stateComponent.owner);
	}

	private void SetClosestHostileProcessing(out bool shouldStillProcessAfterwards, ref string log)
	{
		shouldStillProcessAfterwards = true;
		if (forcedTarget != null && forcedTarget.mapObjectVisual != null)
		{
			SetClosestHostile(forcedTarget);
			SetForcedTarget(null);
			return;
		}
		if (currentClosestHostile != null && !base.stateComponent.owner.combatComponent.IsHostileInRange(currentClosestHostile))
		{
			SetClosestHostile();
			return;
		}
		if (currentClosestHostile != null && currentClosestHostile.isDead)
		{
			base.stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile, processCombatBehavior: false);
			SetClosestHostile();
			return;
		}
		if (currentClosestHostile != null && (!currentClosestHostile.mapObjectVisual || !currentClosestHostile.mapObjectVisual.gameObject))
		{
			base.stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile, processCombatBehavior: false);
			SetClosestHostile();
			return;
		}
		if (currentClosestHostile != null && CombatManager.Instance.IsImmuneToElement(currentClosestHostile, base.stateComponent.owner.combatComponent.currentElement.type))
		{
			TrySetNearestNonResistantHostile();
			return;
		}
		if (currentClosestHostile != null && currentClosestHostile is Character character && character.combatComponent.isInCombat && !(character.stateComponent.currentState as CombatState).isAttacking)
		{
			if (base.stateComponent.owner.behaviourComponent.HasBehaviour(typeof(DefendBehaviour)))
			{
				base.stateComponent.owner.combatComponent.RemoveHostileInRange(character, processCombatBehavior: false);
				SetClosestHostile();
			}
			else
			{
				SetClosestHostilePriorityNotFleeing();
			}
			return;
		}
		if (currentClosestHostile == null)
		{
			SetClosestHostile();
			return;
		}
		IPointOfInterest nearestValidHostile = base.stateComponent.owner.combatComponent.GetNearestValidHostile();
		if (nearestValidHostile != null && currentClosestHostile != nearestValidHostile)
		{
			SetClosestHostile(nearestValidHostile);
		}
		else if ((bool)base.stateComponent.owner.marker && base.stateComponent.owner.marker.isMoving && currentClosestHostile != null && base.stateComponent.owner.marker.targetPOI == currentClosestHostile)
		{
			shouldStillProcessAfterwards = false;
		}
	}

	private void DoCombatBehavior()
	{
		if (base.stateComponent.currentState != this)
		{
			return;
		}
		string log = string.Empty;
		if (isAttacking)
		{
			if ((bool)base.stateComponent.owner.marker && base.stateComponent.owner.marker.hasFleePath)
			{
				base.stateComponent.owner.marker.SetHasFleePath(state: false);
			}
			else
			{
				_ = (bool)base.stateComponent.owner.marker;
			}
			SetClosestHostileProcessing(out var shouldStillProcessAfterwards, ref log);
			if (shouldStillProcessAfterwards)
			{
				if (currentClosestHostile == null)
				{
					base.stateComponent.ExitCurrentState();
				}
				else if (Vector2.Distance(base.stateComponent.owner.marker.transform.position, currentClosestHostile.worldPosition) > base.stateComponent.owner.combatComponent.attackRange || !base.stateComponent.owner.marker.IsCharacterInLineOfSightWith(currentClosestHostile))
				{
					PursueClosestHostile();
				}
			}
		}
		else if (base.stateComponent.owner.combatComponent.avoidInRange.Count <= 0)
		{
			base.stateComponent.ExitCurrentState();
		}
		else if (!base.stateComponent.owner.marker.hasFleePath && base.stateComponent.owner.limiterComponent.canMove)
		{
			StartFlee();
			if (base.stateComponent.owner.isNormalCharacter && lastFledFrom != null && lastFledFromStructure != null && lastFledFrom is Character { isNormalCharacter: false } character && character.homeStructure == lastFledFromStructure && lastFledFromStructure.structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				base.stateComponent.owner.movementComponent.AddStructureToAvoidAndScheduleRemoval(lastFledFromStructure);
			}
			Messenger.Broadcast(CharacterSignals.START_FLEE, base.stateComponent.owner);
		}
	}

	private string GetAvoidReasonLogKey(IPointOfInterest objToAvoid)
	{
		string result = "Got_Scared";
		CombatData combatData = base.stateComponent.owner.combatComponent.GetCombatData(objToAvoid);
		if (combatData != null && combatData.avoidReasonLogKey != string.Empty)
		{
			result = combatData.avoidReasonLogKey;
		}
		return result;
	}

	private void PursueClosestHostile()
	{
		if (!base.stateComponent.owner.movementComponent.isStationary && base.stateComponent.owner.hasMarker)
		{
			if (!base.stateComponent.owner.marker.isMoving || base.stateComponent.owner.marker.targetPOI != currentClosestHostile)
			{
				base.stateComponent.owner.marker.GoToPOI(currentClosestHostile);
			}
			else if (!base.stateComponent.owner.marker.pathfindingAI.pathPending && base.stateComponent.owner.marker.pathfindingAI.currentPath == null)
			{
				base.stateComponent.owner.marker.pathfindingAI.SetCanSearchPathAgain(p_state: true);
			}
		}
	}

	private void SetClosestHostilePriorityNotFleeing()
	{
		IPointOfInterest nearestValidHostilePriorityNotFleeing = base.stateComponent.owner.combatComponent.GetNearestValidHostilePriorityNotFleeing();
		SetClosestHostile(nearestValidHostilePriorityNotFleeing);
	}

	private void SetClosestHostile()
	{
		IPointOfInterest nearestValidHostile = base.stateComponent.owner.combatComponent.GetNearestValidHostile();
		SetClosestHostile(nearestValidHostile);
	}

	private void TrySetNearestNonResistantHostile()
	{
		IPointOfInterest nearestNonResistantValidHostile = base.stateComponent.owner.combatComponent.GetNearestNonResistantValidHostile();
		SetClosestHostile(nearestNonResistantValidHostile);
	}

	private void SetClosestHostile(IPointOfInterest poi)
	{
		if (poi == currentClosestHostile)
		{
			return;
		}
		IPointOfInterest pointOfInterest = currentClosestHostile;
		currentClosestHostile = poi;
		if (currentClosestHostile != null && pointOfInterest != currentClosestHostile)
		{
			_timesHitCurrentTarget = 0;
			CreateNewCombatTargetLog();
			if (currentClosestHostile is Character p_responsibleCharacter)
			{
				base.stateComponent.owner.classComponent.OnCharacterStartedCombatWith(p_responsibleCharacter);
			}
		}
		if (pointOfInterest == null && currentClosestHostile != null && base.stateComponent.owner.partyComponent.hasParty && base.stateComponent.owner.partyComponent.currentParty.currentQuest is DemonDefendPartyQuest demonDefendPartyQuest)
		{
			demonDefendPartyQuest.OnDefenderStartedAttackingAnIntruder(base.stateComponent.owner, currentClosestHostile);
		}
	}

	private void CreateNewCombatTargetLog()
	{
		CombatData combatData = base.stateComponent.owner.combatComponent.GetCombatData(currentClosestHostile);
		if (combatData == null || combatData.connectedAction == null)
		{
			string combatLogKeyReason = base.stateComponent.owner.combatComponent.GetCombatLogKeyReason(currentClosestHostile);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", combatLogKeyReason);
			Log log;
			if (!string.IsNullOrEmpty(combatLogKeyReason) && LocalizationManager.Instance.HasLocalizedValue(localizedValue))
			{
				log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCombat_Table", "new_combat_target_with_reason", LOG_TAG.Combat);
				log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			}
			else
			{
				log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCombat_Table", "new_combat_target", LOG_TAG.Combat);
			}
			log.AddToFillers(base.stateComponent.owner, base.stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			base.stateComponent.owner.logComponent.RegisterLog(log, releaseAfter: true);
		}
	}

	public void LateUpdate()
	{
		if (GameManager.Instance.isPaused || base.stateComponent.owner.combatComponent.ExecuteSpecialSkill(currentClosestHostile) || currentClosestHostile == null)
		{
			return;
		}
		if (currentClosestHostile.isDead || currentClosestHostile.currentHP <= 0)
		{
			base.stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
		}
		else
		{
			if (!isAttacking || isExecutingAttack)
			{
				return;
			}
			CombatData combatData = base.stateComponent.owner.combatComponent.GetCombatData(currentClosestHostile);
			if (combatData != null && !combatData.isLethal && currentClosestHostile.traitContainer.HasTrait("Unconscious"))
			{
				base.stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
			}
			else
			{
				if (isRepositioning || currentClosestHostile is Character { hasMarker: false })
				{
					return;
				}
				float num = Vector2.Distance(base.stateComponent.owner.worldPosition, currentClosestHostile.attackRangePosition);
				if (base.stateComponent.owner.combatComponent.rangeType == RANGE_TYPE.MELEE)
				{
					if (currentClosestHostile.IsUnpassable())
					{
						num -= 0.4f;
					}
					else if (currentClosestHostile is Character { hasMarker: not false } character2 && character2.marker.isMoving)
					{
						num -= 0.9f;
					}
				}
				if (base.stateComponent.owner.combatComponent.attackRange >= num)
				{
					EvaluateAttackOrReposition();
					return;
				}
				LocationGridTile tileFromWorldPosition = GridMap.Instance.mainRegion.innerMap.GetTileFromWorldPosition(currentClosestHostile.worldPosition);
				if (base.stateComponent.owner.gridTileLocation == tileFromWorldPosition)
				{
					EvaluateAttackOrReposition();
				}
				else
				{
					PursueClosestHostile();
				}
			}
		}
	}

	private void EvaluateAttackOrReposition()
	{
		if (base.stateComponent.owner.movementComponent.isStationary)
		{
			AttackOrReposition();
		}
		else if (base.stateComponent.owner.marker.IsCharacterInLineOfSightWith(currentClosestHostile, base.stateComponent.owner.combatComponent.attackRange) || base.stateComponent.owner.movementComponent.isStationary)
		{
			AttackOrReposition();
		}
		else
		{
			PursueClosestHostile();
		}
	}

	private void AttackOrReposition()
	{
		if (base.stateComponent.owner.movementComponent.IsCurrentGridNodeOccupiedByOtherNonRepositioningActiveCharacter() || !base.stateComponent.owner.movementComponent.IsInWalkableNode())
		{
			if (!TryReposition())
			{
				base.stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
			}
		}
		else
		{
			Attack();
		}
	}

	private void SetGridNodeToReposition(GraphNode p_gridNode)
	{
		if (repositioningTo == p_gridNode)
		{
			return;
		}
		repositioningTo = p_gridNode;
		if (isRepositioning)
		{
			if (base.stateComponent.owner.hasMarker)
			{
				base.stateComponent.owner.marker.pathfindingAI.SetEndReachedDistance(0.05f);
			}
		}
		else if (base.stateComponent.owner.hasMarker)
		{
			base.stateComponent.owner.marker.pathfindingAI.ResetEndReachedDistance();
		}
	}

	private bool TryReposition()
	{
		string summary = string.Empty;
		return RepositionToAnotherUnoccupiedNodeWithinDistance(base.stateComponent.owner.combatComponent.attackRange, currentClosestHostile, ref summary);
	}

	private bool RepositionToAnotherUnoccupiedNodeWithinDistance(float p_distanceLimit, IPointOfInterest p_relativePOI, ref string summary)
	{
		if (base.stateComponent.owner.hasMarker && !isRepositioning)
		{
			LocationGridTile gridTileLocation = base.stateComponent.owner.gridTileLocation;
			LocationGridTile gridTileLocation2 = p_relativePOI.gridTileLocation;
			if (gridTileLocation != null && gridTileLocation2 != null)
			{
				LocationGridTile p_chosenPositionGridTile = null;
				Vector3 positionToReposition = GetPositionToReposition(gridTileLocation2, p_distanceLimit, p_relativePOI, base.stateComponent.owner.marker.lineOfSightHitObjects, ref p_chosenPositionGridTile);
				if (positionToReposition.Equals(Vector3.positiveInfinity))
				{
					return false;
				}
				GraphNode gridNodeByWorldPosition = p_chosenPositionGridTile.GetGridNodeByWorldPosition(positionToReposition);
				SetGridNodeToReposition(gridNodeByWorldPosition);
				base.stateComponent.owner.marker.GoTo(positionToReposition, OnArriveAfterCombatRepositioning);
			}
		}
		return true;
	}

	private Vector3 GetPositionToReposition(LocationGridTile p_gridTile, float p_distanceLimit, IPointOfInterest p_target, RaycastHit2D[] p_lineOfSightObjects, ref LocationGridTile p_chosenPositionGridTile)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		p_gridTile.PopulateTilesInRadius(list, Mathf.CeilToInt(p_distanceLimit), 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		Vector3 result = Vector3.positiveInfinity;
		if (list.Count > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				LocationGridTile locationGridTile = list[num];
				if (locationGridTile.IsPassable() && locationGridTile.HasWalkableNode() && locationGridTile.HasPathOutside())
				{
					Vector3 unoccupiedWalkablePositionInTileThatIsInLineOfSightWith = locationGridTile.GetUnoccupiedWalkablePositionInTileThatIsInLineOfSightWith(p_target, p_distanceLimit, p_gridTile.centeredWorldLocation, p_lineOfSightObjects);
					if (!unoccupiedWalkablePositionInTileThatIsInLineOfSightWith.Equals(Vector3.positiveInfinity))
					{
						p_chosenPositionGridTile = locationGridTile;
						result = unoccupiedWalkablePositionInTileThatIsInLineOfSightWith;
						break;
					}
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	private void OnArriveAfterCombatRepositioning()
	{
		SetGridNodeToReposition(null);
	}

	private void Attack()
	{
		if (base.stateComponent.owner.marker.isMoving)
		{
			if (currentClosestHostile is Character character)
			{
				if (character.hasMarker)
				{
					if (!character.marker.isMoving)
					{
						base.stateComponent.owner.marker.StopMovement();
					}
				}
				else
				{
					base.stateComponent.owner.marker.StopMovement();
				}
			}
			else
			{
				base.stateComponent.owner.marker.StopMovement();
			}
			base.stateComponent.owner.marker.SetTargetPOI(null);
		}
		if (!base.stateComponent.owner.marker.CanAttackByAttackSpeed())
		{
			return;
		}
		InnerMapManager.Instance.FaceTarget(base.stateComponent.owner, currentClosestHostile);
		if (!isExecutingAttack)
		{
			isExecutingAttack = true;
			if (base.stateComponent.owner.marker.GetAnimationBool("InCombat"))
			{
				base.stateComponent.owner.marker.PlayAttackAnimation();
			}
		}
		base.stateComponent.owner.marker.ResetAttackSpeed();
	}

	public void OnAttackHit(IDamageable damageable)
	{
		string attackSummary = string.Empty;
		if (damageable == null || currentClosestHostile == null)
		{
			return;
		}
		_ = currentClosestHostile;
		damageable.OnHitByAttackFrom(base.stateComponent.owner, this, ref attackSummary);
		if (damageable.currentHP > 0 && damageable is Character character)
		{
			if (character.faction != null && !character.faction.IsHostileWith(base.stateComponent.owner.faction) && !character.combatComponent.IsHostileInRange(base.stateComponent.owner) && !character.combatComponent.IsAvoidInRange(base.stateComponent.owner) && !allCharactersThatDegradedRel.Contains(character))
			{
				character.relationshipContainer.AdjustOpinion(character, base.stateComponent.owner, "Base", -15);
				AddCharacterThatDegradedRel(character);
			}
			if (damageable == currentClosestHostile && (character.combatComponent.combatMode == COMBAT_MODE.Defend || character.combatComponent.combatMode == COMBAT_MODE.Aggressive) && character.limiterComponent.canPerform && character.limiterComponent.canWitness && (character.currentJob == null || character.currentJob.jobType != JOB_TYPE.FLEE_CRIME))
			{
				bool flag = base.stateComponent.owner.combatComponent.GetLethalityFromCombatData(character);
				if (flag)
				{
					flag = character.combatComponent.ShouldCombatBeLethalAgainst(base.stateComponent.owner);
				}
				character.combatComponent.FightOrFlight(base.stateComponent.owner, "Retaliation", null, flag);
			}
		}
		if (damageable == currentClosestHostile)
		{
			if (!damageable.CanBeDamaged() && !(damageable is GenericTileObject) && GameUtilities.RollChance(10 * _timesHitCurrentTarget))
			{
				base.stateComponent.owner.combatComponent.Flight(currentClosestHostile, "Got_Scared");
			}
			_timesHitCurrentTarget++;
			if (damageable.gridTileLocation != null && damageable.gridTileLocation.structure is DemonicStructure demonicStructure && demonicStructure.objectsThatContributeToDamage.Contains(damageable))
			{
				demonicStructure.AddAttacker(base.stateComponent.owner);
			}
		}
	}

	public void FinishedTravellingFleePath()
	{
		base.stateComponent.owner.marker.SetHasFleePath(state: false);
		EvaluateFleeingBecauseOfVulnerability(processCombatBehaviour: false);
		DetermineReaction(base.stateComponent.owner);
		base.stateComponent.owner.marker.UpdateAnimation();
		base.stateComponent.owner.marker.UpdateActionIcon();
	}

	private void UpdateFleePath()
	{
		StartFlee(shouldLog: false);
	}

	private void StartFlee(bool shouldLog = true)
	{
		if (base.stateComponent.owner.combatComponent.avoidInRange.Count == 0)
		{
			return;
		}
		ResetClosestHostile();
		List<IPointOfInterest> avoidInRange = base.stateComponent.owner.combatComponent.avoidInRange;
		IPointOfInterest pointOfInterest = (lastFledFrom = avoidInRange[avoidInRange.Count - 1]);
		lastFledFromStructure = pointOfInterest.gridTileLocation?.structure;
		if ((bool)base.stateComponent.owner.marker && !base.stateComponent.owner.marker.hasFleePath)
		{
			List<Trait> traitOverrideFunctions = base.stateComponent.owner.traitContainer.GetTraitOverrideFunctions("Before_Start_Flee");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					traitOverrideFunctions[i].OnBeforeStartFlee(base.stateComponent.owner);
				}
			}
		}
		string avoidReasonLogKey = GetAvoidReasonLogKey(pointOfInterest);
		string value = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", avoidReasonLogKey);
		if (string.IsNullOrEmpty(value))
		{
			value = avoidReasonLogKey;
		}
		if (avoidReasonLogKey == "Coward" && base.stateComponent.owner.HasAfflictedByPlayerWith("Coward"))
		{
			base.stateComponent.owner.traitContainer.GetTraitOrStatus<Coward>("Coward").DispenseChaosOrbsForAffliction(base.stateComponent.owner, PLAYER_SKILL_TYPE.COWARDICE, 1);
		}
		if (avoidReasonLogKey == "Vulnerable")
		{
			base.stateComponent.owner.marker.OnStartFleeToPartyMate();
		}
		else if (base.stateComponent.owner.currentStructure != null && base.stateComponent.owner.currentStructure.structureType.IsSpecialStructure())
		{
			base.stateComponent.owner.marker.OnStartFleeToOutside();
		}
		else
		{
			base.stateComponent.owner.marker.OnStartFlee();
		}
		if (avoidReasonLogKey == "critically low health" && base.stateComponent.owner.partyComponent.hasParty)
		{
			Party currentParty = base.stateComponent.owner.partyComponent.currentParty;
			if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Raid)
			{
				base.stateComponent.owner.partyComponent.currentParty.RemoveMemberThatJoinedQuest(base.stateComponent.owner);
			}
		}
		if (shouldLog)
		{
			if (pointOfInterest is GenericTileObject)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Fleeing", LOG_TAG.Combat);
				log.AddToFillers(base.stateComponent.owner, base.stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				base.stateComponent.owner.logComponent.RegisterLog(log);
				SetThoughtBubbleLog(log);
			}
			else
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "start_flee", LOG_TAG.Combat);
				log2.AddToFillers(base.stateComponent.owner, base.stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(pointOfInterest, pointOfInterest.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
				base.stateComponent.owner.logComponent.RegisterLog(log2);
				SetThoughtBubbleLog(log2);
			}
		}
	}

	private void EvaluateFleeingBecauseOfVulnerability(bool processCombatBehaviour)
	{
		bool num = base.stateComponent.owner.partyComponent.HasPartymateInVision();
		bool flag = false;
		if (num)
		{
			for (int i = 0; i < base.stateComponent.owner.combatComponent.avoidInRange.Count; i++)
			{
				IPointOfInterest pointOfInterest = base.stateComponent.owner.combatComponent.avoidInRange[i];
				if (base.stateComponent.owner.combatComponent.GetCombatData(pointOfInterest).avoidReasonLogKey == "Vulnerable" && base.stateComponent.owner.combatComponent.RemoveAvoidInRange(pointOfInterest, processCombatBehavior: false))
				{
					flag = true;
					i--;
				}
			}
		}
		if (flag && processCombatBehaviour)
		{
			base.stateComponent.owner.combatComponent.SetWillProcessCombat(state: true);
		}
	}

	public void ResetClosestHostile()
	{
		IPointOfInterest pointOfInterest = currentClosestHostile;
		currentClosestHostile = null;
		if (pointOfInterest != null && base.stateComponent.owner.marker.targetPOI == pointOfInterest && (bool)base.stateComponent.owner.marker)
		{
			base.stateComponent.owner.marker.SetTargetPOI(null);
		}
	}

	public void SetForcedTarget(Character character)
	{
		forcedTarget = character;
	}

	public void AddCharacterThatDegradedRel(Character character)
	{
		if (!allCharactersThatDegradedRel.Contains(character))
		{
			allCharactersThatDegradedRel.Add(character);
		}
	}

	public void AddCharacterThatReactedToThisCombat(Character character)
	{
		allCharactersThatReactedToThisCombat.Add(character);
	}

	public bool DidCharacterAlreadyReactToThisCombat(Character character)
	{
		for (int i = 0; i < allCharactersThatReactedToThisCombat.Count; i++)
		{
			if (allCharactersThatReactedToThisCombat[i] == character)
			{
				return true;
			}
		}
		return false;
	}

	private void OnTileDestroyed(GenericTileObject p_tileObject)
	{
		if (currentClosestHostile == p_tileObject)
		{
			base.stateComponent.owner.combatComponent.RemoveHostileInRange(p_tileObject);
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		if (lastFledFrom == p_character)
		{
			lastFledFrom = null;
		}
		if (currentClosestHostile == p_character)
		{
			SetClosestHostile(null);
		}
	}

	private void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (lastFledFromStructure == p_structure)
		{
			lastFledFromStructure = null;
		}
	}

	public override void Reset()
	{
		base.Reset();
		isAttacking = false;
		currentClosestHostile = null;
		forcedTarget = null;
		allCharactersThatDegradedRel.Clear();
		allCharactersThatReactedToThisCombat.Clear();
		lastFledFrom = null;
		lastFledFromStructure = null;
		isBeingApprehended = false;
		_timesHitCurrentTarget = 0;
		isExecutingAttack = false;
		repositioningTo = null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = lastFledFromStructure;
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = currentClosestHostile;
		_ = forcedTarget;
		allCharactersThatDegradedRel.Contains(p_character);
		allCharactersThatDegradedRel.Contains(p_character);
		_ = lastFledFrom;
	}
}
