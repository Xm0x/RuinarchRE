using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using Traits;
using UnityEngine;
using UtilityScripts;

public class MovementComponent : CharacterComponent
{
	public bool isRunning { get; private set; }

	public bool noRunExceptCombat { get; private set; }

	public bool noRunWithoutException { get; private set; }

	public int useRunSpeed { get; private set; }

	public float speedModifier { get; private set; }

	public float walkSpeedModifier { get; private set; }

	public float runSpeedModifier { get; private set; }

	public bool hasMovedOnCorruption { get; private set; }

	public bool isStationary { get; private set; }

	public bool cameFromWurmHole { get; private set; }

	public bool isTravellingInWorld { get; private set; }

	public bool isFlying => IsFlying();

	public List<LocationStructure> structuresToAvoid { get; }

	public int enableDiggingCounter { get; private set; }

	public int avoidSettlementsCounter { get; private set; }

	public int traversableTags { get; private set; }

	public int[] tagPenalties { get; private set; }

	public int previousTraversableTags { get; private set; }

	public int[] previousTagPenalties { get; private set; }

	public float walkSpeed => base.owner.raceSetting.walkSpeed + base.owner.raceSetting.walkSpeed * walkSpeedModifier;

	public float runSpeed => base.owner.raceSetting.runSpeed + base.owner.raceSetting.runSpeed * runSpeedModifier;

	public bool enableDigging => enableDiggingCounter > 0;

	public bool avoidSettlements => avoidSettlementsCounter > 0;

	public MovementComponent()
	{
		structuresToAvoid = new List<LocationStructure>();
		tagPenalties = new int[32];
		traversableTags = -1;
		SetTagAsUnTraversable(2);
	}

	public MovementComponent(SaveDataMovementComponent data)
	{
		structuresToAvoid = new List<LocationStructure>();
		isRunning = data.isRunning;
		noRunExceptCombat = data.noRunExceptCombat;
		noRunWithoutException = data.noRunWithoutException;
		useRunSpeed = data.useRunSpeed;
		speedModifier = data.speedModifier;
		walkSpeedModifier = data.walkSpeedModifier;
		runSpeedModifier = data.runSpeedModifier;
		hasMovedOnCorruption = data.hasMovedOnCorruption;
		isStationary = data.isStationary;
		cameFromWurmHole = data.cameFromWurmHole;
		isTravellingInWorld = data.isTravellingInWorld;
		enableDiggingCounter = data.enableDiggingCounter;
		avoidSettlementsCounter = data.avoidSettlementsCounter;
		traversableTags = data.traversableTags;
		tagPenalties = data.tagPenalties;
		previousTraversableTags = data.previousTraversableTags;
		previousTagPenalties = data.previousTagPenalties;
	}

	public void SubscribeToSignals()
	{
		Messenger.AddListener<Character>(CharacterSignals.STARTED_TRAVELLING, OnStartedTravelling);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);
	}

	public void UnsubscribeFromSignals()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.STARTED_TRAVELLING, OnStartedTravelling);
		Messenger.RemoveListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);
	}

	public void UpdateSpeed()
	{
		if ((bool)base.owner.marker)
		{
			SetMovementState();
			base.owner.marker.pathfindingAI.speed = GetSpeed();
			Messenger.Broadcast(CharacterSignals.UPDATE_MOVEMENT_STATE, base.owner);
		}
	}

	public void SetIsRunning(bool state)
	{
		isRunning = state;
	}

	public void SetNoRunExceptCombat(bool state)
	{
		noRunExceptCombat = state;
	}

	public void SetNoRunWithoutException(bool state)
	{
		noRunWithoutException = state;
	}

	public void AdjustSpeedModifier(float amount)
	{
		speedModifier += amount;
		UpdateSpeed();
	}

	public void AdjustWalkSpeedModifier(float amount)
	{
		walkSpeedModifier += amount;
	}

	public void AdjustRunSpeedModifier(float amount)
	{
		runSpeedModifier += amount;
	}

	private float GetSpeed()
	{
		float num = runSpeed;
		float num2 = walkSpeed;
		if (base.owner.mountComponent.IsMounting())
		{
			num = base.owner.mountComponent.mountedCharacter.movementComponent.runSpeed;
			num2 = base.owner.mountComponent.mountedCharacter.movementComponent.walkSpeed;
		}
		float num3 = num;
		bool flag = base.owner.partyComponent.isMemberThatJoinedQuest && base.owner.partyComponent.currentParty.isPlayerParty;
		if (base.owner.partyComponent.hasParty && base.owner.partyComponent.currentParty.isActive && base.owner.partyComponent.currentParty.currentQuest is DemonDefendPartyQuest)
		{
			flag = false;
		}
		if (!isRunning || base.owner.traitContainer.HasTrait("Bloated"))
		{
			num3 = num2;
		}
		num3 += num3 * speedModifier;
		if (num3 <= 0f)
		{
			num3 = 0.5f;
		}
		if (flag && !isRunning)
		{
			num3 = Mathf.Min(base.owner.partyComponent.currentParty.partyWalkSpeed, num3);
		}
		if ((bool)base.owner.marker)
		{
			return num3 * base.owner.marker.progressionSpeedMultiplier;
		}
		throw new Exception("Trying to get speed for " + base.owner.name + " without a marker, this canot happen!");
	}

	private void SetMovementState()
	{
		SetIsRunning(state: false);
		if (noRunWithoutException || (noRunExceptCombat && !base.owner.combatComponent.isInCombat))
		{
			return;
		}
		if (useRunSpeed > 0)
		{
			SetIsRunning(state: true);
		}
		else if (base.owner.combatComponent.isInActualCombat)
		{
			SetIsRunning(state: true);
		}
		else if (base.owner.currentActionNode != null)
		{
			if (base.owner.currentActionNode.associatedJobType == JOB_TYPE.ENERGY_RECOVERY_URGENT || base.owner.currentActionNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT || base.owner.currentActionNode.associatedJobType == JOB_TYPE.DOUSE_FIRE || base.owner.currentActionNode.associatedJobType == JOB_TYPE.DOUSE_FIRE_SELF || base.owner.currentActionNode.associatedJobType == JOB_TYPE.REMOVE_STATUS || base.owner.currentActionNode.associatedJobType == JOB_TYPE.NEUTRALIZE_DANGER || base.owner.currentActionNode.associatedJobType == JOB_TYPE.APPREHEND || base.owner.currentActionNode.associatedJobType == JOB_TYPE.APPREHEND_RESTRAINED || base.owner.currentActionNode.associatedJobType == JOB_TYPE.REPORT_CORRUPTED_STRUCTURE || base.owner.currentActionNode.associatedJobType == JOB_TYPE.RESTRAIN || (base.owner.currentActionNode.associatedJobType == JOB_TYPE.CAPTURE_CHARACTER && base.owner.race == RACE.HARPY) || base.owner.currentActionNode.associatedJobType == JOB_TYPE.TRITON_KIDNAP || base.owner.currentActionNode.associatedJobType == JOB_TYPE.REMOVE_TRAP || base.owner.currentActionNode.associatedJobType == JOB_TYPE.REPORT_CRIME || base.owner.currentActionNode.associatedJobType == JOB_TYPE.FLEE_CRIME || base.owner.currentActionNode.associatedJobType == JOB_TYPE.RETURN_HOME_URGENT || base.owner.currentActionNode.associatedJobType == JOB_TYPE.VISIT_DIFFERENT_VILLAGE || base.owner.currentActionNode.associatedJobType == JOB_TYPE.IDLE_RETURN_HOME || base.owner.currentActionNode.associatedJobType == JOB_TYPE.HAUL || base.owner.currentActionNode.associatedJobType == JOB_TYPE.STOCKPILE_FOOD || base.owner.currentActionNode.associatedJobType == JOB_TYPE.PARTY_GO_TO || base.owner.currentActionNode.associatedJobType == JOB_TYPE.HAUL_ON_SIGHT)
			{
				SetIsRunning(state: true);
			}
		}
		else if (base.owner.partyComponent.isFollowingBeacon)
		{
			SetIsRunning(state: true);
		}
	}

	public void AdjustUseRunSpeed(int amount)
	{
		useRunSpeed += amount;
		useRunSpeed = Mathf.Max(0, useRunSpeed);
	}

	public bool CanStillPursueTarget(Character target)
	{
		if (isRunning)
		{
			return true;
		}
		return false;
	}

	public void SetHasMovedOnCorruption(bool state)
	{
		hasMovedOnCorruption = state;
	}

	public void SetIsStationary(bool state)
	{
		isStationary = state;
	}

	public void SetAvoidSettlements(bool state)
	{
		if (state)
		{
			avoidSettlementsCounter++;
		}
		else
		{
			avoidSettlementsCounter--;
		}
	}

	public void SetCameFromWurmHole(bool state)
	{
		cameFromWurmHole = state;
	}

	public void SetToFlying()
	{
		base.owner.traitContainer.AddTrait(base.owner, "Flying");
	}

	public void SetToNonFlying()
	{
		base.owner.traitContainer.RemoveTrait(base.owner, "Flying");
	}

	private void OnStartedTravelling(Character character)
	{
		OnCharacterStartedTravelling(character);
	}

	public void OnAssignedClass(CharacterClass characterClass)
	{
		if (characterClass.className == "Ratman")
		{
			AvoidAllFactions();
		}
	}

	public void OnChangeFactionTo(Faction newFaction)
	{
		if (newFaction != null)
		{
			DoNotAvoidFaction(newFaction);
		}
	}

	private void OnFactionDisbanded(Faction p_faction)
	{
		DoNotAvoidFaction(p_faction);
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		RemoveStructureToAvoid(p_structure);
	}

	public bool MoveToAnotherRegion(Region targetRegion, Action doneAction = null)
	{
		doneAction?.Invoke();
		return true;
	}

	private void TravelToAnotherRegion(Region targetRegion, Action doneAction = null)
	{
	}

	private void StartTravellingToRegion(Region targetRegion, Action doneAction = null)
	{
	}

	private void FinishTravellingToRegion(Action doneAction = null)
	{
	}

	public bool HasPathTo(LocationGridTile toTile)
	{
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (!CanDig() && !isFlying)
		{
			Vampire traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus != null && traitOrStatus.CanTransformIntoBat())
			{
				if (gridTileLocation == null || toTile == null)
				{
					return false;
				}
				return true;
			}
			return PathfindingManager.Instance.HasPath(gridTileLocation, toTile);
		}
		if (gridTileLocation == null || toTile == null)
		{
			return false;
		}
		if (gridTileLocation == toTile)
		{
			return true;
		}
		if (toTile.IsWater() && !isFlying)
		{
			return false;
		}
		return true;
	}

	public bool HasPathTo(Area toArea)
	{
		LocationGridTile centerGridTile = toArea.gridTileComponent.centerGridTile;
		if (centerGridTile != null && centerGridTile.IsPassable() && HasPathTo(centerGridTile))
		{
			return true;
		}
		LocationGridTile randomElement = CollectionUtilities.GetRandomElement(toArea.gridTileComponent.gridTiles);
		if (randomElement.IsPassable())
		{
			return HasPathTo(randomElement);
		}
		return false;
	}

	public bool HasPathToEvenIfDiffRegion(LocationGridTile toTile)
	{
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (CanDig() || isFlying)
		{
			if (gridTileLocation == null || toTile == null)
			{
				return false;
			}
			if (gridTileLocation == toTile)
			{
				return true;
			}
			if (toTile.IsWater() && !isFlying)
			{
				return false;
			}
			return true;
		}
		Vampire traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
		if (traitOrStatus != null && traitOrStatus.CanTransformIntoBat())
		{
			if (gridTileLocation == null || toTile == null)
			{
				return false;
			}
			return true;
		}
		return PathfindingManager.Instance.HasPathEvenDiffRegion(gridTileLocation, toTile);
	}

	public bool HasPathToEvenIfDiffRegion(LocationStructure locationStructure)
	{
		if (locationStructure.passableTiles.Count > 0)
		{
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationStructure.passableTiles);
			return HasPathToEvenIfDiffRegion(randomElement);
		}
		if (locationStructure.tiles.Count > 0)
		{
			LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(locationStructure.tiles);
			return HasPathToEvenIfDiffRegion(randomElement2);
		}
		return false;
	}

	public bool HasPathToEvenIfDiffRegion(LocationGridTile toTile, NNConstraint constraint)
	{
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (CanDig() || isFlying)
		{
			if (gridTileLocation == null || toTile == null)
			{
				return false;
			}
			if (gridTileLocation == toTile)
			{
				return true;
			}
			if (toTile.IsWater() && !isFlying)
			{
				return false;
			}
			return true;
		}
		Vampire traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
		if (traitOrStatus != null && traitOrStatus.CanTransformIntoBat())
		{
			if (gridTileLocation == null || toTile == null)
			{
				return false;
			}
			return true;
		}
		return PathfindingManager.Instance.HasPathEvenDiffRegion(gridTileLocation, toTile, constraint);
	}

	public bool CanReturnHome()
	{
		if (base.owner.HasHome())
		{
			LocationGridTile locationGridTile = null;
			if (base.owner.homeSettlement != null)
			{
				locationGridTile = base.owner.homeSettlement.GetRandomPassableTile();
			}
			else if (base.owner.homeStructure != null)
			{
				locationGridTile = base.owner.homeStructure.GetRandomPassableTile();
			}
			else if (base.owner.HasTerritory())
			{
				locationGridTile = base.owner.territory.GetRandomPassableTile();
			}
			if (locationGridTile != null)
			{
				return HasPathToEvenIfDiffRegion(locationGridTile);
			}
		}
		return false;
	}

	public bool CanDig()
	{
		if (base.owner.currentJob != null)
		{
			if (base.owner.currentJob.jobType == JOB_TYPE.RESCUE_MOVE_CHARACTER)
			{
				return true;
			}
			if (base.owner.currentJob.jobType == JOB_TYPE.RESTRAIN && base.owner.currentJob.originalOwner is NPCSettlement)
			{
				return true;
			}
		}
		if (enableDigging && base.owner.currentStructure != null && base.owner.currentStructure.structureType != STRUCTURE_TYPE.KENNEL)
		{
			if (base.owner.combatComponent.isInCombat && (!(base.owner.stateComponent.currentState as CombatState).isAttacking || ((bool)base.owner.marker && base.owner.marker.hasFleePath)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void SetEnableDigging(bool state)
	{
		if (state)
		{
			enableDiggingCounter++;
		}
		else
		{
			enableDiggingCounter--;
		}
	}

	public LocationGridTile GetBlockerTargetTileOnReachEndPath(Path path, LocationGridTile lastGridTileInPath, LocationGridTile actualDestinationTile)
	{
		LocationGridTile locationGridTile = null;
		if (!base.owner.hasMarker)
		{
			return null;
		}
		if (lastGridTileInPath.tileObjectComponent.HasWalls() || actualDestinationTile.centeredWorldLocation == lastGridTileInPath.centeredWorldLocation)
		{
			locationGridTile = lastGridTileInPath;
		}
		else
		{
			Vector2 vector = actualDestinationTile.centeredWorldLocation - lastGridTileInPath.centeredWorldLocation;
			locationGridTile = ((vector.y > 0f) ? lastGridTileInPath.GetNeighbourAtDirection(GridNeighbourDirection.North) : ((vector.y < 0f) ? lastGridTileInPath.GetNeighbourAtDirection(GridNeighbourDirection.South) : ((!(vector.x > 0f)) ? lastGridTileInPath.GetNeighbourAtDirection(GridNeighbourDirection.West) : lastGridTileInPath.GetNeighbourAtDirection(GridNeighbourDirection.East))));
		}
		if (locationGridTile != null && !locationGridTile.tileObjectComponent.HasWalls())
		{
			LocationGridTile locationGridTile2 = null;
			for (int i = 0; i < lastGridTileInPath.neighbourList.Count; i++)
			{
				LocationGridTile locationGridTile3 = lastGridTileInPath.neighbourList[i];
				if (locationGridTile3.tileObjectComponent.HasWalls())
				{
					locationGridTile2 = locationGridTile3;
					break;
				}
			}
			locationGridTile = locationGridTile2;
		}
		return locationGridTile;
	}

	public bool DigOnReachEndPath(Path path, LocationGridTile lastGridTileInPath, LocationGridTile actualDestinationTile)
	{
		LocationGridTile blockerTargetTileOnReachEndPath = GetBlockerTargetTileOnReachEndPath(path, lastGridTileInPath, actualDestinationTile);
		if (blockerTargetTileOnReachEndPath != null && blockerTargetTileOnReachEndPath.tileObjectComponent.HasWalls())
		{
			TileObject firstWall = blockerTargetTileOnReachEndPath.tileObjectComponent.GetFirstWall();
			if (!base.owner.jobQueue.HasJob(JOB_TYPE.DIG_THROUGH) && firstWall != null && firstWall.Advertises(INTERACTION_TYPE.DIG))
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DIG_THROUGH, INTERACTION_TYPE.DIG, firstWall, base.owner);
				goapPlanJob.SetCannotBePushedBack(state: true);
				return base.owner.jobQueue.AddJobInQueue(goapPlanJob);
			}
		}
		return false;
	}

	public bool AttackBlockersOnReachEndPath(Path path, LocationGridTile lastGridTileInPath, LocationGridTile actualDestinationTile)
	{
		LocationGridTile blockerTargetTileOnReachEndPath = GetBlockerTargetTileOnReachEndPath(path, lastGridTileInPath, actualDestinationTile);
		if (blockerTargetTileOnReachEndPath != null && blockerTargetTileOnReachEndPath.tileObjectComponent.HasWalls())
		{
			TileObject firstWall = blockerTargetTileOnReachEndPath.tileObjectComponent.GetFirstWall();
			if (base.owner.combatComponent.IsHostileInRange(firstWall))
			{
				base.owner.combatComponent.SetWillProcessCombat(state: true);
			}
			else
			{
				base.owner.combatComponent.Fight(firstWall, "Dig");
			}
			return true;
		}
		return false;
	}

	private bool AddStructureToAvoid(LocationStructure locationStructure)
	{
		if (!structuresToAvoid.Contains(locationStructure))
		{
			structuresToAvoid.Add(locationStructure);
			List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
			list.AddRange(base.owner.jobQueue.jobsInQueue);
			for (int i = 0; i < list.Count; i++)
			{
				JobQueueItem jobQueueItem = list[i];
				if (jobQueueItem.poiTarget != null && jobQueueItem.poiTarget != base.owner && jobQueueItem.poiTarget.gridTileLocation != null && jobQueueItem.poiTarget.gridTileLocation.structure == locationStructure)
				{
					jobQueueItem.ForceCancelJob();
				}
				else
				{
					if (!(jobQueueItem is GoapPlanJob { assignedPlan: not null } goapPlanJob) || goapPlanJob.assignedPlan.allNodes == null)
					{
						continue;
					}
					bool flag = false;
					for (int j = 0; j < goapPlanJob.assignedPlan.allNodes.Count; j++)
					{
						JobNode jobNode = goapPlanJob.assignedPlan.allNodes[j];
						if (jobNode.singleNode != null && jobNode.singleNode.target != null && jobNode.singleNode.target.gridTileLocation != null && jobNode.singleNode.target.gridTileLocation.structure == locationStructure)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						jobQueueItem.ForceCancelJob();
					}
				}
			}
			RuinarchListPool<JobQueueItem>.Release(list);
			return true;
		}
		return false;
	}

	public void RemoveStructureToAvoid(LocationStructure locationStructure)
	{
		structuresToAvoid.Remove(locationStructure);
	}

	public void AddStructureToAvoidAndScheduleRemoval(LocationStructure locationStructure)
	{
		if (AddStructureToAvoid(locationStructure))
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(8));
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				RemoveStructureToAvoid(locationStructure);
			}, base.owner);
		}
		if (locationStructure is Cave cave && base.owner.homeSettlement != null && base.owner.faction != null && !base.owner.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, locationStructure) && cave.IsConnectedToSettlement(base.owner.homeSettlement))
		{
			base.owner.faction.partyQuestBoard.CreateExterminatePartyQuest(base.owner, base.owner.homeSettlement, locationStructure);
		}
	}

	public bool ShouldAvoidStructureLocationOfTarget(IPointOfInterest p_target)
	{
		if (p_target.gridTileLocation != null && base.owner.gridTileLocation != null && structuresToAvoid.Contains(p_target.gridTileLocation.structure) && p_target.gridTileLocation.structure != base.owner.gridTileLocation.structure)
		{
			return true;
		}
		return false;
	}

	public void SetTagAsTraversable(int tag)
	{
		traversableTags |= tag;
		if (base.owner != null && base.owner.hasMarker)
		{
			base.owner.marker.UpdateTraversableTags();
		}
	}

	public void SetTagAsUnTraversable(int tag)
	{
		traversableTags &= ~tag;
		if (base.owner != null && base.owner.hasMarker)
		{
			base.owner.marker.UpdateTraversableTags();
		}
	}

	public void SetPenaltyForTag(int tag, int penalty)
	{
		tag--;
		if (tag > 0)
		{
			tagPenalties[tag] = penalty;
			if (base.owner != null && base.owner.hasMarker)
			{
				base.owner.marker.UpdateTagPenalties();
			}
		}
	}

	private void AvoidAllFactions()
	{
		for (int i = 19; i < 32; i++)
		{
			SetPenaltyForTag(i, 100);
		}
	}

	private void DoNotAvoidFaction(Faction p_faction)
	{
		if (DoesFactionUsePathfindingTag(p_faction))
		{
			int pathfindingTag = (int)p_faction.pathfindingTag;
			SetPenaltyForTag(pathfindingTag, 0);
			SetPenaltyForTag((int)p_faction.pathfindingDoorTag, 0);
		}
	}

	private void AvoidFaction(Faction p_faction)
	{
		if (DoesFactionUsePathfindingTag(p_faction))
		{
			int pathfindingTag = (int)p_faction.pathfindingTag;
			SetPenaltyForTag(pathfindingTag, 500);
			SetPenaltyForTag((int)p_faction.pathfindingDoorTag, 500);
		}
	}

	public void RedetermineFactionsToAvoid(Character p_character)
	{
		if (p_character.faction == null)
		{
			return;
		}
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (p_character.faction.IsHostileWith(faction) || p_character.crimeComponent.IsWantedBy(faction))
			{
				AvoidFaction(faction);
			}
			else
			{
				DoNotAvoidFaction(faction);
			}
		}
	}

	private bool DoesFactionUsePathfindingTag(Faction p_faction)
	{
		if (!p_faction.isMajorFaction)
		{
			if (p_faction.factionType.type != FACTION_TYPE.Ratmen)
			{
				return p_faction.factionType.type == FACTION_TYPE.Undead;
			}
			return true;
		}
		return true;
	}

	public void UpdateMovement()
	{
		if (base.owner.mountComponent.IsMounting())
		{
			previousTraversableTags = traversableTags;
			previousTagPenalties = tagPenalties;
			traversableTags = base.owner.mountComponent.mountedCharacter.movementComponent.traversableTags;
			tagPenalties = base.owner.mountComponent.mountedCharacter.movementComponent.tagPenalties;
		}
		else
		{
			traversableTags = previousTraversableTags;
			tagPenalties = previousTagPenalties;
		}
		if (base.owner != null && base.owner.hasMarker)
		{
			base.owner.marker.UpdateTraversableTags();
			base.owner.marker.UpdateTagPenalties();
		}
		UpdateSpeed();
	}

	public bool IsFlying()
	{
		if (base.owner.mountComponent.IsMounting())
		{
			return base.owner.mountComponent.mountedCharacter.traitContainer.HasTrait("Flying");
		}
		return base.owner.traitContainer.HasTrait("Flying");
	}

	private void OnCharacterStartedTravelling(Character character)
	{
		if (base.owner != character && base.owner.currentActionNode != null && base.owner.currentActionNode.poiTarget == character && base.owner.currentActionNode.actionStatus == ACTION_STATUS.STARTED && (base.owner.currentActionNode.associatedJobType == JOB_TYPE.RITUAL_KILLING || base.owner.currentActionNode.goapType == INTERACTION_TYPE.SHARE_INFORMATION || base.owner.currentActionNode.goapType == INTERACTION_TYPE.REPORT_CRIME))
		{
			base.owner.currentActionNode.associatedJob?.ForceCancelJob();
		}
	}

	public bool IsCurrentGridNodeOccupiedByOtherNonRepositioningActiveCharacter()
	{
		return base.owner.gridTileLocation?.IsGridNodeOccupiedByNonRepositioningActiveCharacterOtherThan(base.owner) ?? false;
	}

	public bool IsInWalkableNode()
	{
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (gridTileLocation != null && base.owner.hasMarker)
		{
			return gridTileLocation.IsPositionInWalkableNode(base.owner.worldPosition);
		}
		return false;
	}

	public GraphNode GetNearestGridNode()
	{
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (gridTileLocation != null && base.owner.hasMarker)
		{
			return gridTileLocation.GetNearestGridNodeByWorldPos(base.owner.worldPosition);
		}
		return null;
	}

	public void LetGo(bool becomeDazed = false)
	{
		LocationStructure currentStructure = base.owner.currentStructure;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < currentStructure.tiles.Count; i++)
		{
			LocationGridTile locationGridTile = currentStructure.tiles.ElementAt(i);
			for (int j = 0; j < locationGridTile.neighbourList.Count; j++)
			{
				LocationGridTile locationGridTile2 = locationGridTile.neighbourList[j];
				if (locationGridTile2.structure is Wilderness && !list.Contains(locationGridTile2))
				{
					list.Add(locationGridTile2);
					if (locationGridTile2.IsPassable())
					{
						list2.Add(locationGridTile2);
					}
				}
			}
		}
		LocationGridTile randomElement = CollectionUtilities.GetRandomElement((list2.Count > 0) ? list2 : list);
		if (becomeDazed)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Dazed");
		}
		CharacterManager.Instance.Teleport(base.owner, randomElement);
		GameManager.Instance.CreateParticleEffectAt(randomElement, PARTICLE_EFFECT.Minion_Dissipate);
		base.owner.traitContainer.RemoveRestrainAndImprison(base.owner);
		if (base.owner.isLycanthrope)
		{
			base.owner.lycanData.limboForm.traitContainer.RemoveRestrainAndImprison(base.owner.lycanData.limboForm);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		RuinarchListPool<LocationGridTile>.Release(list2);
	}

	public void Unstuck()
	{
		if (isFlying || !base.owner.hasMarker)
		{
			return;
		}
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (gridTileLocation != null && !gridTileLocation.HasWalkableNode())
		{
			LocationGridTile nearestTileWithWalkableNode = gridTileLocation.GetNearestTileWithWalkableNode();
			if (nearestTileWithWalkableNode != null)
			{
				base.owner.marker.PlaceMarkerAt(nearestTileWithWalkableNode);
			}
		}
	}

	public void OnCharacterEntersArea(Area p_area)
	{
		base.owner.behaviourComponent.OnCharacterEnteredArea(p_area);
		base.owner.jobComponent.OnCharacterEnteredArea(p_area);
		if (base.owner.traitContainer.HasTrait("Restrained", "Paralyzed", "Stoned"))
		{
			Messenger.Broadcast(CharacterSignals.UPDATE_CHARACTER_AWARENESS_STATE, base.owner);
		}
		for (int i = 0; i < p_area.settlementsOnArea.Count; i++)
		{
			BaseSettlement baseSettlement = p_area.settlementsOnArea[i];
			if (baseSettlement.owner != null)
			{
				baseSettlement.owner.charactersComponent.UpdateCharacterAwarenessData(base.owner);
			}
		}
	}

	public void LoadReferences(SaveDataMovementComponent data)
	{
		for (int i = 0; i < data.structuresToAvoid.Count; i++)
		{
			LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.structuresToAvoid[i]);
			if (structure != null)
			{
				structuresToAvoid.Add(structure);
				GameDate gameDate = GameManager.Instance.Today();
				gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(8));
				SchedulingManager.Instance.AddEntry(gameDate, delegate
				{
					RemoveStructureToAvoid(structure);
				}, base.owner);
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		structuresToAvoid.Contains(p_structure);
	}
}
