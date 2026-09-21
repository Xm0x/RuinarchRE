# Character AI & Job System

How a character decides what to do and carries it out. The spine:
**Behaviour picks a goal → a Job carries it → GOAP plans the action chain →
actions tick out on a per-tick state machine**, with **Interrupts** and
**Traits** modulating everything. Paths relative to `src/Assembly-CSharp/`.

## The pipeline (one glance)

```
tick → BehaviourComponent.RunBehaviour()          (priority loop)
     → CharacterBehaviour.TryDoBehaviour(...)       → produces JobQueueItem
     → JobQueue.AddJobInQueue(job)                   (priority-sorted)
     → GoapPlanJob.ProcessJob()                      → planner.StartGOAP(...)
     → GoapThread (off main thread) searches plan
     → GoapPlanner.ReceivePlanFromGoapThread(plan)   (main thread) assigns/cancels
     → owner.PerformJob(job) → GoapPlan node chain executes over ticks
     → ActualGoapNode Pre/PerTick/After states → SUCCESS/FAIL → next node
Interrupts can preempt at any point; Traits gate eligibility throughout.
```

## 1. Behaviour selection: `BehaviourComponent.cs` + `CharacterBehaviour.cs`

`BehaviourComponent` (1700+ lines) holds `currentBehaviourComponents`
(`List<CharacterBehaviour>`) sorted by `priority` (descending).

`RunBehaviour()`:
1. Iterate behaviours in priority order.
2. Call `characterBehaviour.TryDoBehaviour(owner, ref string log, out JobQueueItem producedJob)`.
3. First behaviour returning **true** wins (or `STOPS_BEHAVIOUR_LOOP` attribute
   ends the loop early).
4. Validate `producedJob` via `IsProducedJobValid` (target reachable, job type
   allowed), then `jobQueue.AddJobInQueue(producedJob)`.
5. `PostProcessAfterSuccessfulDoBehaviour` applies `ONCE_PER_DAY` scheduling.

`CharacterBehaviour` (abstract base): each subclass is one behaviour
(`ArsonistBehaviour`, `AttackVillageBehaviour`, `PatrolBehaviour`, …).
- `abstract bool TryDoBehaviour(Character, ref string log, out JobQueueItem producedJob)`
  Returns true iff it acted.
- `priority` (sort key), `attributes` (`BEHAVIOUR_COMPONENT_ATTRIBUTE[]`:
  `ONCE_PER_DAY`, `DO_NOT_SKIP_PROCESSING`, `STOPS_BEHAVIOUR_LOOP`).
- Hooks: `OnAddBehaviourToCharacter` / `OnRemoveBehaviourFromCharacter`.

Behaviour objects also stash per-character AI state: `attackVillageTarget`,
`arsonVillageTarget`, `nest`, combat-mode snapshots, cooldowns.

## 2. Jobs: `JobQueueItem.cs`, `GoapPlanJob.cs`, `JobQueue.cs`, `JobManager.cs`

**`JobQueueItem`** (abstract, ~615 lines): the job contract:
- `persistentID`, `jobType` (`JOB_TYPE`), `originalOwner` (`IJobOwner`),
  `assignedCharacter`, `priority` (via virtual `GetPriority`),
  `blacklistedCharacters`.
- `bool ProcessJob()` (true = enqueued for execution),
  `CanCharacterTakeThisJob(...)` (owner type + `canTakeJobChecker`).
- Hooks: `OnAddJobToQueue`, `OnRemoveJobFromQueue`, `UnassignJob(reason)`,
  `PushedBack(jobThatPushed)` (cancels after ~3 push-backs).
- Broadcasts `JOB_ADDED_TO_QUEUE` / `JOB_REMOVED_FROM_QUEUE` on the Messenger bus.

**`GoapPlanJob : JobQueueItem`**: the common GOAP-backed job:
- `goal` (`GoapEffect`), `targetInteractionType` (`INTERACTION_TYPE`),
  `assignedPlan` (`GoapPlan`), `targetPOI` (`IPointOfInterest`).
- `ProcessJob()`: if `assignedPlan == null`, call
  `character.planner.StartGOAP(goal | targetInteractionType, target, this, isPersonalPlan)`.
- `otherData` `Dictionary<INTERACTION_TYPE, OtherData[]>`, `priorityLocations`
  (settlement/structure/area hints for the planner).

**`JobQueue`**: per-character priority queue:
- `jobsInQueue` (main, priority-sorted) + `pendingTopPriorityJobs` (awaiting a
  GOAP plan).
- `AddJobInQueue` → state checks + `CanJobBeAddedToQueue` →
  `InsertJobToMainJobQueue` (priority insert).
- `ProcessFirstJobInQueue()` runs `jobs[0].ProcessJob()` when it's top priority.

**`JobManager`** (singleton): eligibility registries:
- `CanTakeJobChecker` dict (40+: `CanTakeBury`, `CanTakeRemoveStatus`, …).
- `JobApplicabilityChecker` dict (30+: `IsDestroyApplicable`,
  `IsBurySettlementApplicable`, …).
- Concrete checkers live in `Goap/Job_Checkers/` (50+ files); each answers
  `CanTakeJob(Character, JobQueueItem)` or `StillApplicable(JobQueueItem)`.
- Broadcast `CHECK_JOB_APPLICABILITY` re-validates queued jobs against world
  state (target still exists/reachable) and culls the invalid.

## 3. GOAP core: `Goap/`

Goal-Oriented Action Planning: given a goal (an effect to achieve), search a
chain of actions whose preconditions/effects bridge current → goal state.

- **`GoapPlanner.cs`**: orchestrator:
  - `StartGOAP(goal|INTERACTION_TYPE, target, job, isPersonalPlan)` spawns a
    `GoapThread` on `MultiThreadPool` (planning is **off the main thread**);
    sets `job.SetIsInMultithread(true)`.
  - `ReceivePlanFromGoapThread(plan)` (back on main thread, ~300 lines):
    - plan found → move job pending→main, `SetAssignedPlan`, and if still
      `queue[0]` call `owner.PerformJob(job)`.
    - plan null → fallback logic (add `Abstain Fullness/Tiredness/Happiness`
      traits; force `PRODUCE_FOOD` to avoid starvation), log
      unable-to-do (or report-crime for cultists), `job.CancelJob()`.
- **`GoapPlan.cs`**: the chain: `allNodes` (`List<JobNode>`), `currentNode`
  pointer, `target`. `SetActionNodes(...)` (1-5 overloads) builds it;
  `SetNextNode()` advances; `isEnd`, `isBeingRecalculated`, `doNotRecalculate`.
- **`GoapAction.cs`**: abstract action def: `goapType` (`INTERACTION_TYPE`),
  `basePrecondition` (`Precondition`), `baseExpectedEffects` (`List<GoapEffect>`),
  `states` (`Dictionary<string, GoapActionState>`). `CreateStates()` binds
  Pre/PerTick/After handlers via reflection; `DetermineActionDuration`,
  `IsInvalid` hooks.
- **`ActualGoapNode.cs`**: a concrete action instance being executed:
  `actor`, `poiTarget`, `action`, `actionStatus` (`PERFORMING`/`SUCCESS`/`FAIL`),
  `cost`, `currentStateName`, `ticksPerformingCurrentState`,
  `expectedActionStateDuration`, `uniqueActionData` (`OtherData[]`). Also carries
  `crimeType`, `rumor`, `assumption`, `awareCharacters`, the hooks into the
  social/crime systems. Implements `ISavable`.
- **`Precondition.cs`**: `goapEffect` + `Func<Character, IPointOfInterest,
  OtherData[], JOB_TYPE, bool>`; `CanSatisfyCondition` gates plan viability.
- **`GoapEffect.cs`**: a goal/effect spec: `conditionType`
  (`GOAP_EFFECT_CONDITION`: `ADD_TRAIT`, `REMOVE_TRAIT`, `HEAL_CHARACTER`,
  `INCREASE_STAT`, …), `conditionKey` (trait/stat name), `isKeyANumber`,
  `target` (`GOAP_EFFECT_TARGET`: SELF/TARGET/NEARBY). `IsSameGoal` compares
  type+key.
- **`Goap/Unique_Action_Data/`** (15 files): `OtherData` subclasses
  (`CraftEquipmentUAD`, `FeedUAD`, `HealSelfUAD`, `CureCharacterUAD`, …) carrying
  action-specific parameters; each has a SaveData twin.

## 4. Execution (per tick)

`Character.PerformJob(job)` sets `currentJob`. Each tick the plan's `currentNode`
(`ActualGoapNode`) runs its current `GoapActionState` (Pre → PerTick → After).
`DetermineActionDuration` sets `expectedActionStateDuration`; on completion
`SetState(next, node)` transitions. When `currentNode == null` the plan ends
(`isEnd = true`) and the job's completion handler fires.

## 5. Interrupts: `InterruptComponent.cs` + `Interrupts/Interrupt.cs`

Short, event-driven reactions that can preempt the plan:
- `InterruptComponent.TriggerInterrupt(INTERRUPT type, IPointOfInterest target)`
  builds an `InterruptHolder`, runs `ExecuteInterruptStartEffect`, broadcasts
  `INTERRUPT_STARTED`.
  - `doesDropCurrentJob` → `currentJob.CancelJob()`;
    `doesStopCurrentAction` → `currentJob.StopJobNotDrop()`.
- `OnTickEnded` counts `currentDuration`, calls `PerTickInterrupt`, and at expiry
  runs `ExecuteInterruptEndEffect` (add/remove traits, trigger crime, opinion
  swings) then `EndInterrupt` (broadcast `INTERRUPT_ENDED`).
- `isSimultaneous` allows parallel (non-exclusive) interrupts.
- `Interrupt` (abstract) mirrors the `GoapAction` reaction pattern with
  `ReactionToActor` / `ReactionToTarget` for social/crime consequences.

## 6. Traits: `Traits/Trait.cs` + `TraitContainer.cs`

Traits are the pervasive modifier layer (150+ subclasses: `Paralyzed`,
`Quarantined`, `Pyrophobic`, `Kleptomaniac`, `Abstain Fullness`, …):
- `TraitContainer.AddTrait/RemoveTrait`, `HasTrait(name)`.
- Gate execution via `limiterComponent.canPerform` (paralysis/quarantine block
  jobs); the planner cancels jobs when `!canPerform`.
- Recovery-failure fallback adds `Abstain *` traits to stop thrashing; clearing a
  blocking trait broadcasts `CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE` to
  re-enable culled jobs.

## 7. Persistence hooks

GOAP/job state is fully savable: `GoapPlan` (currentNodeIndex, isEnd, nodes,
target ID), `GoapPlanJob` (goal, targetPOI ID, plan SaveData, otherData),
`JobQueueItem` (persistentID, type, priority, owner/character IDs, blacklist),
`InterruptComponent` (durations). Post-load `LoadReferences` re-links owner /
character / target by `persistentID` and reattaches ongoing actions
(`LoadCharactersCurrentAction`). See `docs/systems/SAVE_FORMAT.md`.

## Content enums (the game's vocabulary)

- `JOB_TYPE`: **229 values** (recovery: `ENERGY_RECOVERY_URGENT`,
  `FULLNESS_RECOVERY_NORMAL`; work: `PRODUCE_WOOD/FOOD/STONE/METAL`,
  `CRAFT_OBJECT`, `REPAIR`; social: `VISIT_FRIEND`, `PREACH`; combat: `COMBAT`,
  `BERSERK_ATTACK`; special: `TRIGGER_FLAW`, `CULTIST_TRANSFORM`, `PLAGUE_CARE`).
  Helpers: `IsFullnessRecoveryTypeJob()`, `IsCultistJob()`, …
- `INTERACTION_TYPE`: **~250 values** (`EAT`, `SLEEP`, `STEAL`, `ASSAULT`,
  `SLAY_CHARACTER`, `MAKE_LOVE`, `CHOP_WOOD`, `BUILD_BLUEPRINT`,
  `RITUAL_KILLING`, …). This is `GoapAction.goapType`, indexed in
  `InteractionManager.goapActionData`.
- `GOAP_EFFECT_CONDITION`, `GOAP_EFFECT_TARGET`: goal/effect matching.
- `INTERRUPT`: 100+ interrupt types (`InteractionManager.interruptData`).
- `BEHAVIOUR_COMPONENT_ATTRIBUTE`, `CHARACTER_STATE`: loop/state control.
