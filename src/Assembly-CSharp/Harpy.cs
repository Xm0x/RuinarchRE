using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

public class Harpy : Summon
{
	public const string ClassName = "Harpy";

	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;

	public override Type serializedData => typeof(SaveDataHarpy);

	public bool hasCapturedForTheDay { get; private set; }

	public GameDate nextCaptureDate { get; private set; }

	public Harpy()
		: base(SUMMON_TYPE.Harpy, "Harpy", RACE.HARPY, Utilities.GetRandomGender())
	{
	}

	public Harpy(string className)
		: base(SUMMON_TYPE.Harpy, className, RACE.HARPY, Utilities.GetRandomGender())
	{
	}

	public Harpy(SaveDataHarpy data)
		: base(data)
	{
		hasCapturedForTheDay = data.hasCapturedForTheDay;
		nextCaptureDate = data.nextCaptureDate;
	}

	private LocationStructure GetDestinationToDropCapturedCharacter(Character actor, Region region)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		for (int i = 0; i < region.allSpecialStructures.Count; i++)
		{
			LocationStructure locationStructure = region.allSpecialStructures[i];
			if (locationStructure != actor.homeStructure && locationStructure.passableTiles.Count > 0)
			{
				list.Add(locationStructure);
			}
		}
		LocationStructure result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	private Character GetTargetForCapture(Character actor, Region region)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < region.charactersAtLocation.Count; i++)
		{
			Character character = region.charactersAtLocation[i];
			if (character != actor && character.race != actor.race && !character.isHidden && !character.isDead && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried())
			{
				list.Add(character);
			}
		}
		Character result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		p_agitateJob = null;
		Region region = base.currentRegion;
		if (region != null)
		{
			Character targetForCapture = GetTargetForCapture(this, region);
			if (targetForCapture != null)
			{
				LocationStructure destinationToDropCapturedCharacter = GetDestinationToDropCapturedCharacter(this, region);
				if (destinationToDropCapturedCharacter != null)
				{
					base.jobComponent.TryTriggerCaptureCharacter(JOB_TYPE.CAPTURE_CHARACTER, targetForCapture, destinationToDropCapturedCharacter, out p_agitateJob, doNotRecalculate: true);
					if (p_agitateJob is GoapPlanJob goapPlanJob)
					{
						goapPlanJob.SetIsAgitateJob(p_state: true);
						CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
						return true;
					}
				}
				else
				{
					CreateAgitateLog(AGITATE_MESSAGE_TYPE.Abduct_Character_No_Target);
				}
			}
		}
		return false;
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Abduct_Villager_Tooltip.ToStringEnum();
	}

	public override void Initialize()
	{
		base.Initialize();
		base.movementComponent.SetToFlying();
		hasCapturedForTheDay = true;
		int num = UnityEngine.Random.Range(0, 288);
		if (num == 0)
		{
			SetHasCapturedForTheDay(state: false);
			return;
		}
		nextCaptureDate = GameManager.Instance.Today().AddTicks(num);
		SchedulingManager.Instance.AddEntry(nextCaptureDate, delegate
		{
			SetHasCapturedForTheDay(state: false);
		}, this);
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<Character, GoapPlanJob>(JobSignals.CHARACTER_WILL_DO_JOB, OnCharacterWillDoJob);
			Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
			Messenger.AddListener<Character>(CharacterSignals.HEALTH_CRITICALLY_LOW, OnHealthCriticallyLow);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<Character, GoapPlanJob>(JobSignals.CHARACTER_WILL_DO_JOB, OnCharacterWillDoJob);
			Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
			Messenger.RemoveListener<Character>(CharacterSignals.HEALTH_CRITICALLY_LOW, OnHealthCriticallyLow);
		}
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		base.LoadReferences(data);
		if (hasCapturedForTheDay)
		{
			SchedulingManager.Instance.AddEntry(nextCaptureDate, delegate
			{
				SetHasCapturedForTheDay(state: false);
			}, this);
		}
	}

	private void OnCharacterWillDoJob(Character character, GoapPlanJob job)
	{
		if (character == this && job.jobType == JOB_TYPE.CAPTURE_CHARACTER)
		{
			base.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
		}
	}

	private void OnJobRemovedFromQueue(JobQueueItem job, Character character)
	{
		if (character != this || job.jobType != JOB_TYPE.CAPTURE_CHARACTER)
		{
			return;
		}
		IPointOfInterest poiTarget = job.poiTarget;
		if (poiTarget != null)
		{
			Prisoner traitOrStatus = poiTarget.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus != null && traitOrStatus.prisonerOfCharacter == this)
			{
				poiTarget.traitContainer.RemoveRestrainAndImprison(poiTarget);
			}
		}
		base.combatComponent.SetCombatMode(defaultCombatMode);
	}

	private void OnHealthCriticallyLow(Character character)
	{
		if (character == this && character.currentJob != null && character.currentJob.jobType == JOB_TYPE.CAPTURE_CHARACTER)
		{
			character.jobQueue.CancelFirstJob();
		}
	}

	public void SetHasCapturedForTheDay(bool state)
	{
		if (hasCapturedForTheDay == state)
		{
			return;
		}
		hasCapturedForTheDay = state;
		if (hasCapturedForTheDay)
		{
			nextCaptureDate = GameManager.Instance.Today().AddDays(1);
			SchedulingManager.Instance.AddEntry(nextCaptureDate, delegate
			{
				SetHasCapturedForTheDay(state: false);
			}, this);
		}
	}
}
