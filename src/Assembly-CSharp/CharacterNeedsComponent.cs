using System.Collections.Generic;
using System.Globalization;
using Inner_Maps.Location_Structures;
using Interrupts;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CharacterNeedsComponent : CharacterComponent
{
	private float tirednessLowerBound;

	public const float TIREDNESS_DEFAULT = 100f;

	public const float EXHAUSTED_UPPER_LIMIT = 20f;

	public const float TIRED_UPPER_LIMIT = 50f;

	public const float REFRESHED_LOWER_LIMIT = 91f;

	private float fullnessLowerBound;

	public const float FULLNESS_DEFAULT = 100f;

	public const float STARVING_UPPER_LIMIT = 20f;

	public const float HUNGRY_UPPER_LIMIT = 50f;

	public const float FULL_LOWER_LIMIT = 91f;

	private float happinessLowerBound;

	public const float HAPPINESS_DEFAULT = 100f;

	public const float SULKING_UPPER_LIMIT = 20f;

	public const float BORED_UPPER_LIMIT = 50f;

	public const float ENTERTAINED_LOWER_LIMIT = 91f;

	private float staminaLowerBound;

	public const float STAMINA_DEFAULT = 100f;

	public const float DRAINED_UPPER_LIMIT = 20f;

	public const float SPENT_UPPER_LIMIT = 50f;

	public const float SPRIGHTLY_LOWER_LIMIT = 91f;

	private float hopeLowerBound;

	public const float HOPE_DEFAULT = 100f;

	public const float HOPELESS_UPPER_LIMIT = 20f;

	public const float DISCOURAGED_UPPER_LIMIT = 40f;

	public const float HOPEFUL_LOWER_LIMIT = 91f;

	private bool _hasTriggeredThisHour;

	public int doNotGetHungry { get; private set; }

	public int doNotGetTired { get; private set; }

	public int doNotGetBored { get; private set; }

	public int doNotGetDrained { get; private set; }

	public bool doesNotGetHungry
	{
		get
		{
			if (doNotGetHungry <= 0)
			{
				if (base.owner.currentActionNode != null && base.owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
				{
					return base.owner.currentActionNode.action.IsFullnessRecoveryAction();
				}
				return false;
			}
			return true;
		}
	}

	public bool doesNotGetTired
	{
		get
		{
			if (doNotGetTired <= 0)
			{
				if (base.owner.currentActionNode != null && base.owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
				{
					return base.owner.currentActionNode.action.IsTirednessRecoveryAction();
				}
				return false;
			}
			return true;
		}
	}

	public bool doesNotGetBored
	{
		get
		{
			if (doNotGetBored <= 0)
			{
				if (base.owner.currentActionNode != null && base.owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)
				{
					return base.owner.currentActionNode.action.IsHappinessRecoveryAction();
				}
				return false;
			}
			return true;
		}
	}

	public bool isStarving
	{
		get
		{
			if (fullness >= 0f)
			{
				return fullness <= 20f;
			}
			return false;
		}
	}

	public bool isExhausted
	{
		get
		{
			if (tiredness >= 0f)
			{
				return tiredness <= 20f;
			}
			return false;
		}
	}

	public bool isSulking
	{
		get
		{
			if (happiness >= 0f)
			{
				return happiness <= 20f;
			}
			return false;
		}
	}

	public bool isDrained
	{
		get
		{
			if (stamina >= 0f)
			{
				return stamina <= 20f;
			}
			return false;
		}
	}

	public bool isHopeless
	{
		get
		{
			if (hope >= 0f)
			{
				return hope <= 20f;
			}
			return false;
		}
	}

	public bool isHungry
	{
		get
		{
			if (fullness > 20f)
			{
				return fullness <= 50f;
			}
			return false;
		}
	}

	public bool isTired
	{
		get
		{
			if (tiredness > 20f)
			{
				return tiredness <= 50f;
			}
			return false;
		}
	}

	public bool isBored
	{
		get
		{
			if (happiness > 20f)
			{
				return happiness <= 50f;
			}
			return false;
		}
	}

	public bool isSpent
	{
		get
		{
			if (stamina > 20f)
			{
				return stamina <= 50f;
			}
			return false;
		}
	}

	public bool isDiscouraged
	{
		get
		{
			if (hope > 20f)
			{
				return hope <= 40f;
			}
			return false;
		}
	}

	public bool isFull
	{
		get
		{
			if (fullness >= 91f)
			{
				return fullness <= 100f;
			}
			return false;
		}
	}

	public bool isRefreshed
	{
		get
		{
			if (tiredness >= 91f)
			{
				return tiredness <= 100f;
			}
			return false;
		}
	}

	public bool isEntertained
	{
		get
		{
			if (happiness >= 91f)
			{
				return happiness <= 100f;
			}
			return false;
		}
	}

	public bool isSprightly
	{
		get
		{
			if (stamina >= 91f)
			{
				return stamina <= 100f;
			}
			return false;
		}
	}

	public bool isHopeful
	{
		get
		{
			if (hope >= 91f)
			{
				return hope <= 100f;
			}
			return false;
		}
	}

	public float tiredness { get; private set; }

	public float tirednessDecreaseRate { get; private set; }

	public float fullness { get; private set; }

	public float fullnessDecreaseRate { get; private set; }

	public float happiness { get; private set; }

	public float happinessDecreaseRate { get; private set; }

	public float happinessDecreaseRateDivisor { get; private set; }

	public float happinessDecreaseRateMultiplier { get; private set; }

	public float stamina { get; private set; }

	public float staminaDecreaseRate { get; private set; }

	public float baseStaminaDecreaseRate { get; private set; }

	public float hope { get; private set; }

	public bool hasForcedFullness { get; set; }

	public bool hasForcedTiredness { get; set; }

	public bool hasForcedSecondHappiness { get; set; }

	public CharacterNeedsComponent()
	{
		SetTirednessLowerBound(0f);
		SetFullnessLowerBound(0f);
		SetHappinessLowerBound(0f);
		SetStaminaLowerBound(0f);
		SetHopeLowerBound(0f);
		happinessDecreaseRateMultiplier = 1f;
	}

	public CharacterNeedsComponent(SaveDataCharacterNeedsComponent data)
	{
		SetSaveDataCharacterNeedsComponent(data);
	}

	public void SetSaveDataCharacterNeedsComponent(SaveDataCharacterNeedsComponent data)
	{
		doNotGetHungry = data.doNotGetHungry;
		doNotGetTired = data.doNotGetTired;
		doNotGetBored = data.doNotGetBored;
		doNotGetDrained = data.doNotGetDrained;
		tiredness = data.tiredness;
		tirednessDecreaseRate = data.tirednessDecreaseRate;
		fullness = data.fullness;
		fullnessDecreaseRate = data.fullnessDecreaseRate;
		happiness = data.happiness;
		happinessDecreaseRate = data.happinessDecreaseRate;
		happinessDecreaseRateDivisor = data.happinessDecreaseRateDivisor;
		happinessDecreaseRateMultiplier = data.happinessDecreaseRateMultiplier;
		if (happinessDecreaseRateMultiplier == 0f)
		{
			happinessDecreaseRateMultiplier = 1f;
		}
		stamina = data.stamina;
		staminaDecreaseRate = data.staminaDecreaseRate;
		baseStaminaDecreaseRate = data.baseStaminaDecreaseRate;
		hope = data.hope;
		hasForcedFullness = data.hasForcedFullness;
		hasForcedTiredness = data.hasForcedTiredness;
		hasForcedSecondHappiness = data.hasForcedSecondHappiness;
	}

	public void SubscribeToSignals()
	{
	}

	public void UnsubscribeToSignals()
	{
	}

	public void DailyGoapProcesses()
	{
		hasForcedFullness = false;
		hasForcedTiredness = false;
		hasForcedSecondHappiness = false;
	}

	public void Initialize()
	{
	}

	public void InitialCharacterPlacement()
	{
		SetHope(50f);
		SetTiredness(Random.Range(50, 101));
		SetFullness(Random.Range(50, 101));
		SetHappiness(Random.Range(50, 101));
		SetStamina(100f);
	}

	public void LoadAllStatsOfCharacter(SaveDataCharacter data)
	{
	}

	public void PerTick()
	{
		if (!base.owner.isDead)
		{
			DecreaseNeeds();
		}
	}

	public void PerTickSummon()
	{
		StaminaAdjustments();
	}

	public void PerHour()
	{
		if (!_hasTriggeredThisHour)
		{
			_hasTriggeredThisHour = true;
			EveryOtherHour();
		}
		else
		{
			_hasTriggeredThisHour = false;
		}
	}

	private void EveryOtherHour()
	{
		if (HasNeeds())
		{
			CheckStarving();
		}
	}

	public void CheckExtremeNeeds(Interrupt interruptThatTriggered = null)
	{
		if (base.owner != null && !base.owner.hasBeenCleanedUp && HasNeeds())
		{
			if (isStarving && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Grieving))
			{
				PlanFullnessRecoveryActions();
			}
			if (isExhausted && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Feeling_Spooked))
			{
				PlanTirednessRecoveryActions();
			}
		}
	}

	public void CheckExtremeNeedsWhileInActiveParty(Interrupt interruptThatTriggered = null)
	{
		if (HasNeeds())
		{
			if ((isStarving || isHungry) && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Grieving))
			{
				PlanFullnessRecoveryActionsWhileInActiveParty();
			}
			if ((isExhausted || isTired) && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Feeling_Spooked))
			{
				PlanTirednessRecoveryActionsWhileInActiveParty();
			}
			if (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Feeling_Brokenhearted)
			{
				PlanHappinessRecoveryWhileInActiveParty();
			}
		}
	}

	private void CheckStarving()
	{
		if (isStarving)
		{
			PlanFullnessRecoveryActions();
		}
	}

	public bool HasNeeds()
	{
		if (base.owner.race != RACE.SKELETON && !base.owner.characterClass.IsZombie() && base.owner.characterClass.className != "Necromancer" && !base.owner.hasBeenRaisedFromDead && base.owner.minion == null && !(base.owner is Summon))
		{
			return !base.owner.traitContainer.HasTrait("Fervor");
		}
		return false;
	}

	private void DecreaseNeeds()
	{
		StaminaAdjustments();
		if (HasNeeds())
		{
			if (!doesNotGetHungry)
			{
				AdjustFullness(0f - (GetBaseFullnessDecreaseRatePerTick() + fullnessDecreaseRate));
			}
			if (!doesNotGetTired)
			{
				AdjustTiredness(0f - (GetBaseTirednessDecreaseRatePerTick() + tirednessDecreaseRate));
			}
			if (!doesNotGetBored)
			{
				AdjustHappiness(0f - GetComputedHappinessDecreaseRatePerTick());
			}
		}
	}

	public void ResetNeeds()
	{
		ResetFullnessMeter();
		ResetTirednessMeter();
		ResetHappinessMeter();
	}

	public void AdjustNeeds(int p_amount)
	{
		AdjustFullness(p_amount);
		AdjustTiredness(p_amount);
		AdjustHappiness(p_amount);
	}

	public float GetBaseFullnessDecreaseRatePerTick()
	{
		if (base.owner.isVagrant || !base.owner.IsInHomeSettlement() || base.owner.traitContainer.HasTrait("Patrolling"))
		{
			return EditableValuesManager.Instance.outsideSettlementFullnessDecreaseRate;
		}
		return EditableValuesManager.Instance.baseFullnessDecreaseRate;
	}

	public float GetBaseTirednessDecreaseRatePerTick()
	{
		if (base.owner.isVagrant || !base.owner.IsInHomeSettlement() || base.owner.traitContainer.HasTrait("Patrolling"))
		{
			return EditableValuesManager.Instance.outsideSettlementTirednessDecreaseRate;
		}
		return EditableValuesManager.Instance.baseTirednessDecreaseRate;
	}

	public float GetBaseHappinessDecreaseRatePerTick()
	{
		if (base.owner.isVagrant || !base.owner.IsInHomeSettlement() || base.owner.traitContainer.HasTrait("Patrolling"))
		{
			return EditableValuesManager.Instance.outsideSettlementHappinessDecreaseRate;
		}
		return EditableValuesManager.Instance.baseHappinessDecreaseRate;
	}

	public float GetComputedHappinessDecreaseRatePerTick()
	{
		float baseHappinessDecreaseRatePerTick = GetBaseHappinessDecreaseRatePerTick();
		float num = ((happinessDecreaseRateDivisor == 0f) ? 1f : happinessDecreaseRateDivisor);
		return (baseHappinessDecreaseRatePerTick + happinessDecreaseRate) * happinessDecreaseRateMultiplier / num;
	}

	private void StaminaAdjustments()
	{
		if (doNotGetDrained > 0)
		{
			return;
		}
		if ((bool)base.owner.marker && base.owner.marker.isMoving)
		{
			if (base.owner.movementComponent.isRunning)
			{
				AdjustStamina(0f - (baseStaminaDecreaseRate + staminaDecreaseRate));
			}
			else
			{
				AdjustStamina(5f);
			}
		}
		else
		{
			AdjustStamina(10f);
		}
	}

	public string GetNeedsSummary()
	{
		string text = "Fullness: " + fullness.ToString(CultureInfo.InvariantCulture) + "/" + 100f.ToString(CultureInfo.InvariantCulture);
		text = text + "\nTiredness: " + tiredness.ToString(CultureInfo.InvariantCulture) + "/" + 100f.ToString(CultureInfo.InvariantCulture);
		text = text + "\nHappiness: " + happiness.ToString(CultureInfo.InvariantCulture) + "/" + 100f.ToString(CultureInfo.InvariantCulture);
		text = text + "\nStamina: " + stamina.ToString(CultureInfo.InvariantCulture) + "/" + 100f.ToString(CultureInfo.InvariantCulture);
		return text + "\nTrust: " + hope.ToString(CultureInfo.InvariantCulture) + "/" + 100f.ToString(CultureInfo.InvariantCulture);
	}

	public void AdjustFullnessDecreaseRate(float amount)
	{
		fullnessDecreaseRate += amount;
	}

	public void AdjustTirednessDecreaseRate(float amount)
	{
		tirednessDecreaseRate += amount;
	}

	public void AdjustHappinessDecreaseRate(float amount)
	{
		happinessDecreaseRate += amount;
	}

	public void AdjustHappinessDecreaseRateDivisor(float p_amount)
	{
		happinessDecreaseRateDivisor += p_amount;
	}

	public void AdjustHappinessDecreaseRateMultiplier(float p_amount)
	{
		happinessDecreaseRateMultiplier += p_amount;
	}

	public void AdjustStaminaDecreaseRate(float amount)
	{
		staminaDecreaseRate += amount;
	}

	private void SetTirednessLowerBound(float amount)
	{
		tirednessLowerBound = amount;
	}

	private void SetFullnessLowerBound(float amount)
	{
		fullnessLowerBound = amount;
	}

	private void SetHappinessLowerBound(float amount)
	{
		happinessLowerBound = amount;
	}

	private void SetStaminaLowerBound(float amount)
	{
		staminaLowerBound = amount;
	}

	private void SetHopeLowerBound(float amount)
	{
		hopeLowerBound = amount;
	}

	public void ResetTirednessMeter()
	{
		bool wasTired = isTired;
		bool wasExhausted = isExhausted;
		bool wasRefreshed = isRefreshed;
		tiredness = 100f;
		OnRefreshed(wasRefreshed, wasTired, wasExhausted);
	}

	public void AdjustTiredness(float adjustment)
	{
		if (adjustment < 0f && base.owner.traitContainer.HasTrait("Vampire"))
		{
			return;
		}
		bool wasTired = isTired;
		bool wasExhausted = isExhausted;
		bool wasRefreshed = isRefreshed;
		bool flag = tiredness == 0f;
		tiredness += adjustment;
		tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, 100f);
		if (tiredness == 0f)
		{
			if (!flag)
			{
				base.owner.traitContainer.AddTrait(base.owner, "Unconscious");
			}
			OnExhausted(wasRefreshed, wasTired, wasExhausted);
		}
		else if (isRefreshed)
		{
			OnRefreshed(wasRefreshed, wasTired, wasExhausted);
		}
		else if (isTired)
		{
			OnTired(wasRefreshed, wasTired, wasExhausted);
		}
		else if (isExhausted)
		{
			OnExhausted(wasRefreshed, wasTired, wasExhausted);
		}
		else
		{
			OnNormalEnergy(wasRefreshed, wasTired, wasExhausted);
		}
	}

	public void SetTiredness(float amount)
	{
		bool wasTired = isTired;
		bool wasExhausted = isExhausted;
		bool wasRefreshed = isRefreshed;
		bool flag = tiredness == 0f;
		tiredness = amount;
		tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, 100f);
		if (tiredness == 0f)
		{
			if (!flag)
			{
				base.owner.traitContainer.AddTrait(base.owner, "Unconscious");
			}
			OnExhausted(wasRefreshed, wasTired, wasExhausted);
		}
		else if (isRefreshed)
		{
			OnRefreshed(wasRefreshed, wasTired, wasExhausted);
		}
		else if (isTired)
		{
			OnTired(wasRefreshed, wasTired, wasExhausted);
		}
		else if (isExhausted)
		{
			OnExhausted(wasRefreshed, wasTired, wasExhausted);
		}
		else
		{
			OnNormalEnergy(wasRefreshed, wasTired, wasExhausted);
		}
	}

	private void OnRefreshed(bool wasRefreshed, bool wasTired, bool wasExhausted)
	{
		if (!wasRefreshed)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Refreshed");
		}
		if (wasExhausted)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Exhausted");
		}
		if (wasTired)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Tired");
		}
	}

	private void OnTired(bool wasRefreshed, bool wasTired, bool wasExhausted)
	{
		if (!wasTired)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Tired");
		}
		if (wasExhausted)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Exhausted");
		}
		if (wasRefreshed)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Refreshed");
		}
	}

	private void OnExhausted(bool wasRefreshed, bool wasTired, bool wasExhausted)
	{
		if (!wasExhausted)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Exhausted");
		}
		if (wasTired)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Tired");
		}
		if (wasRefreshed)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Refreshed");
		}
	}

	private void OnNormalEnergy(bool wasRefreshed, bool wasTired, bool wasExhausted)
	{
		if (wasExhausted)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Exhausted");
		}
		if (wasTired)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Tired");
		}
		if (wasRefreshed)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Refreshed");
		}
	}

	private void RemoveTiredOrExhausted()
	{
		if (!base.owner.traitContainer.RemoveTrait(base.owner, "Tired"))
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Exhausted");
		}
	}

	public void AdjustDoNotGetTired(int amount)
	{
		doNotGetTired += amount;
	}

	public bool PlanTirednessRecoveryActions()
	{
		if (!base.owner.limiterComponent.canPerform)
		{
			return false;
		}
		if (isExhausted && !base.owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT))
		{
			JobQueueItem job = base.owner.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
			if (job != null)
			{
				if (base.owner.currentJob == job)
				{
					return false;
				}
				job.CancelJob();
			}
			JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
			PlanTirednessRecovery(jobType);
			return true;
		}
		return false;
	}

	public bool PlanTirednessRecoveryActionsForSleepBehaviour(out JobQueueItem producedJob)
	{
		if (!base.owner.limiterComponent.canPerform)
		{
			producedJob = null;
			return false;
		}
		if (!base.owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT, JOB_TYPE.ENERGY_RECOVERY_NORMAL))
		{
			return PlanTirednessRecovery(JOB_TYPE.ENERGY_RECOVERY_NORMAL, out producedJob);
		}
		producedJob = null;
		return false;
	}

	private bool PlanTirednessRecoveryActionsWhileInActiveParty()
	{
		if (!base.owner.limiterComponent.canPerform)
		{
			return false;
		}
		if (isExhausted)
		{
			if (!base.owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT))
			{
				JobQueueItem job = base.owner.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
				if (job != null)
				{
					if (base.owner.currentJob == job)
					{
						return false;
					}
					job.CancelJob();
				}
				JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
				PlanTirednessRecoveryBase(jobType);
				return true;
			}
		}
		else if (isTired && !base.owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL))
		{
			PlanTirednessRecoveryBase(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
			return true;
		}
		return false;
	}

	public bool PlanExtremeTirednessRecoveryActionsForCannotPerform()
	{
		if (base.owner.partyComponent.isActiveMember)
		{
			return false;
		}
		if (!base.owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT))
		{
			JobQueueItem job = base.owner.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
			if (job != null)
			{
				if (base.owner.currentJob == job)
				{
					return false;
				}
				job.CancelJob();
			}
			JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
			Spooked traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
			if (traitOrStatus == null || !traitOrStatus.TryTriggerFeelingSpooked(base.owner))
			{
				GoapPlanJob job2 = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP_OUTSIDE, base.owner, base.owner);
				base.owner.jobQueue.AddJobInQueue(job2);
			}
			return true;
		}
		return false;
	}

	private GoapPlanJob PlanTirednessRecovery(JOB_TYPE jobType)
	{
		if (base.owner.partyComponent.isActiveMember)
		{
			return null;
		}
		return PlanTirednessRecoveryBase(jobType);
	}

	private bool PlanTirednessRecovery(JOB_TYPE jobType, out JobQueueItem producedJob)
	{
		if (base.owner.partyComponent.isActiveMember)
		{
			producedJob = null;
			return false;
		}
		return PlanTirednessRecoveryBase(jobType, out producedJob);
	}

	private GoapPlanJob PlanTirednessRecoveryBase(JOB_TYPE jobType)
	{
		if (base.owner == null || base.owner.hasBeenCleanedUp)
		{
			return null;
		}
		if (!base.owner.limiterComponent.canDoTirednessRecovery)
		{
			return null;
		}
		if (base.owner.traitContainer.HasTrait("Burning", "Poisoned"))
		{
			return null;
		}
		Spooked traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
		if (traitOrStatus == null || !traitOrStatus.TryTriggerFeelingSpooked(base.owner))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), base.owner, base.owner);
			if (!base.owner.traitContainer.HasTrait("Travelling") && base.owner.homeStructure != null)
			{
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.SLEEP, base.owner.homeStructure);
			}
			if (base.owner.currentSettlement != null && base.owner.currentSettlement.HasStructure(STRUCTURE_TYPE.TAVERN))
			{
				List<LocationStructure> structuresOfType = base.owner.currentSettlement.GetStructuresOfType(STRUCTURE_TYPE.TAVERN);
				for (int i = 0; i < structuresOfType.Count; i++)
				{
					LocationStructure location = structuresOfType[i];
					goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.SLEEP, location);
				}
			}
			base.owner.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return null;
	}

	private bool PlanTirednessRecoveryBase(JOB_TYPE jobType, out JobQueueItem producedJob)
	{
		if (base.owner == null || base.owner.hasBeenCleanedUp)
		{
			producedJob = null;
			return false;
		}
		if (!base.owner.limiterComponent.canDoTirednessRecovery)
		{
			producedJob = null;
			return false;
		}
		if (base.owner.traitContainer.HasTrait("Burning", "Poisoned", "Alerted"))
		{
			producedJob = null;
			return false;
		}
		Spooked traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
		if (traitOrStatus == null || !traitOrStatus.TryTriggerFeelingSpooked(base.owner))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), base.owner, base.owner);
			if (!base.owner.traitContainer.HasTrait("Travelling") && base.owner.homeStructure != null)
			{
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.SLEEP, base.owner.homeStructure);
			}
			if (base.owner.currentSettlement != null && base.owner.currentSettlement.HasStructure(STRUCTURE_TYPE.TAVERN))
			{
				List<LocationStructure> structuresOfType = base.owner.currentSettlement.GetStructuresOfType(STRUCTURE_TYPE.TAVERN);
				for (int i = 0; i < structuresOfType.Count; i++)
				{
					LocationStructure location = structuresOfType[i];
					goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.SLEEP, location);
				}
			}
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	public void WakeUpFromNoise()
	{
		if (base.owner.traitContainer.HasTrait("Resting") && GameUtilities.RollChance(50))
		{
			base.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Noise_Wake_Up, base.owner);
		}
	}

	public void ResetHappinessMeter()
	{
		if (!base.owner.traitContainer.HasTrait("Psychopath"))
		{
			bool wasBored = isBored;
			bool wasSulking = isSulking;
			bool wasEntertained = isEntertained;
			happiness = 100f;
			OnEntertained(wasEntertained, wasBored, wasSulking);
		}
	}

	public void AdjustHappiness(float adjustment)
	{
		if (!base.owner.traitContainer.HasTrait("Psychopath") && HasNeeds())
		{
			bool wasBored = isBored;
			bool wasSulking = isSulking;
			bool wasEntertained = isEntertained;
			happiness += adjustment;
			happiness = Mathf.Clamp(happiness, happinessLowerBound, 100f);
			if (isEntertained)
			{
				OnEntertained(wasEntertained, wasBored, wasSulking);
			}
			else if (isBored)
			{
				OnBored(wasEntertained, wasBored, wasSulking);
			}
			else if (isSulking)
			{
				OnSulking(wasEntertained, wasBored, wasSulking);
			}
			else
			{
				OnNormalHappiness(wasEntertained, wasBored, wasSulking);
			}
		}
	}

	public void SetHappiness(float amount, bool bypassPsychopathChecking = false)
	{
		if (bypassPsychopathChecking || !base.owner.traitContainer.HasTrait("Psychopath"))
		{
			bool wasBored = isBored;
			bool wasSulking = isSulking;
			bool wasEntertained = isEntertained;
			happiness = amount;
			happiness = Mathf.Clamp(happiness, happinessLowerBound, 100f);
			if (isEntertained)
			{
				OnEntertained(wasEntertained, wasBored, wasSulking);
			}
			else if (isBored)
			{
				OnBored(wasEntertained, wasBored, wasSulking);
			}
			else if (isSulking)
			{
				OnSulking(wasEntertained, wasBored, wasSulking);
			}
			else
			{
				OnNormalHappiness(wasEntertained, wasBored, wasSulking);
			}
		}
	}

	private void OnEntertained(bool wasEntertained, bool wasBored, bool wasSulking)
	{
		if (!wasEntertained)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Entertained");
		}
		if (wasBored)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Bored");
		}
		if (wasSulking)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Sulking");
		}
	}

	private void OnBored(bool wasEntertained, bool wasBored, bool wasSulking)
	{
		if (!wasBored)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Bored");
		}
		if (wasEntertained)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Entertained");
		}
		if (wasSulking)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Sulking");
		}
	}

	private void OnSulking(bool wasEntertained, bool wasBored, bool wasSulking)
	{
		if (!wasSulking)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Sulking");
		}
		if (wasEntertained)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Entertained");
		}
		if (wasBored)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Bored");
		}
	}

	private void OnNormalHappiness(bool wasEntertained, bool wasBored, bool wasSulking)
	{
		if (wasSulking)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Sulking");
		}
		if (wasEntertained)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Entertained");
		}
		if (wasBored)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Bored");
		}
	}

	private void RemoveBoredOrSulking()
	{
		if (!base.owner.traitContainer.RemoveTrait(base.owner, "Bored"))
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Sulking");
		}
	}

	public void AdjustDoNotGetBored(int amount)
	{
		doNotGetBored += amount;
	}

	private void PlanHappinessRecoveryWhileInActiveParty()
	{
		PlanHappinessRecoveryBase();
	}

	public bool PlanHappinessRecoveryForFreeTime(out JobQueueItem p_producedJob)
	{
		return PlanHappinessRecoveryBase(out p_producedJob);
	}

	private bool PlanHappinessRecoveryBase(out JobQueueItem p_producedJob)
	{
		if (base.owner == null || base.owner.hasBeenCleanedUp)
		{
			p_producedJob = null;
			return false;
		}
		if (!base.owner.limiterComponent.canDoHappinessRecovery)
		{
			p_producedJob = null;
			return false;
		}
		if (!base.owner.limiterComponent.canPerform)
		{
			p_producedJob = null;
			return false;
		}
		if (!base.owner.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY))
		{
			bool flag = false;
			Heartbroken traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
			if (traitOrStatus != null)
			{
				flag = Random.Range(0, 100) < 25 * base.owner.traitContainer.stacks[traitOrStatus.name];
			}
			if (!flag)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), base.owner, base.owner);
				JobUtilities.PopulatePriorityLocationsForHappinessRecovery(base.owner, goapPlanJob);
				goapPlanJob.SetDoNotRecalculate(state: true);
				p_producedJob = goapPlanJob;
				return true;
			}
			p_producedJob = null;
			traitOrStatus.TriggerBrokenhearted();
			return false;
		}
		p_producedJob = null;
		return false;
	}

	private bool PlanHappinessRecoveryBase()
	{
		if (PlanHappinessRecoveryBase(out var p_producedJob))
		{
			return base.owner.jobQueue.AddJobInQueue(p_producedJob);
		}
		return false;
	}

	public void ResetFullnessMeter()
	{
		bool wasHungry = isHungry;
		bool wasStarving = isStarving;
		bool wasFull = isFull;
		bool wasMalnourished = fullness == 0f;
		fullness = 100f;
		OnFull(wasFull, wasHungry, wasStarving, wasMalnourished);
	}

	public void AdjustFullness(float adjustment, float hpRecoveryPercentage = 0.05f)
	{
		bool wasHungry = isHungry;
		bool wasStarving = isStarving;
		bool wasFull = isFull;
		bool flag = fullness == 0f;
		fullness += adjustment;
		fullness = Mathf.Clamp(fullness, fullnessLowerBound, 100f);
		if (adjustment > 0f)
		{
			base.owner.PassiveHPRecovery(hpRecoveryPercentage);
		}
		if (fullness == 0f)
		{
			if (!flag)
			{
				base.owner.traitContainer.AddTrait(base.owner, "Malnourished");
			}
			OnStarving(wasFull, wasHungry, wasStarving);
		}
		else if (isFull)
		{
			OnFull(wasFull, wasHungry, wasStarving, flag);
		}
		else if (isHungry)
		{
			OnHungry(wasFull, wasHungry, wasStarving);
		}
		else if (isStarving)
		{
			OnStarving(wasFull, wasHungry, wasStarving);
		}
		else
		{
			OnNormalFullness(wasFull, wasHungry, wasStarving, flag);
		}
	}

	public void SetFullness(float amount)
	{
		bool wasHungry = isHungry;
		bool wasStarving = isStarving;
		bool wasFull = isFull;
		bool flag = fullness == 0f;
		fullness = amount;
		fullness = Mathf.Clamp(fullness, fullnessLowerBound, 100f);
		if (fullness == 0f)
		{
			if (!flag)
			{
				base.owner.traitContainer.AddTrait(base.owner, "Malnourished");
			}
			OnStarving(wasFull, wasHungry, wasStarving);
		}
		else if (isFull)
		{
			OnFull(wasFull, wasHungry, wasStarving, flag);
		}
		else if (isHungry)
		{
			OnHungry(wasFull, wasHungry, wasStarving);
		}
		else if (isStarving)
		{
			OnStarving(wasFull, wasHungry, wasStarving);
		}
		else
		{
			OnNormalFullness(wasFull, wasHungry, wasStarving, flag);
		}
	}

	private void OnFull(bool wasFull, bool wasHungry, bool wasStarving, bool wasMalnourished)
	{
		if (!wasFull)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Full");
		}
		if (wasHungry)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Hungry");
		}
		if (wasStarving)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Starving");
		}
		base.owner.traitContainer.RemoveTrait(base.owner, "Malnourished");
	}

	private void OnHungry(bool wasFull, bool wasHungry, bool wasStarving)
	{
		if (!wasHungry)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Hungry");
		}
		if (wasFull)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Full");
		}
		if (wasStarving)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Starving");
		}
	}

	private void OnStarving(bool wasFull, bool wasHungry, bool wasStarving)
	{
		if (!wasStarving)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Starving");
		}
		if (wasFull)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Full");
		}
		if (wasHungry)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Hungry");
		}
	}

	private void OnNormalFullness(bool wasFull, bool wasHungry, bool wasStarving, bool wasMalnourished)
	{
		if (wasStarving)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Starving");
		}
		if (wasFull)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Full");
		}
		if (wasHungry)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Hungry");
		}
		base.owner.traitContainer.RemoveTrait(base.owner, "Malnourished");
	}

	private void RemoveHungryOrStarving()
	{
		if (!base.owner.traitContainer.RemoveTrait(base.owner, "Hungry"))
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Starving");
		}
	}

	public void AdjustDoNotGetHungry(int amount)
	{
		doNotGetHungry += amount;
	}

	public bool PlanFullnessRecoveryActions()
	{
		if (!base.owner.limiterComponent.canPerform)
		{
			return false;
		}
		if (base.owner.traitContainer.HasTrait("Vampire"))
		{
			return false;
		}
		if (isStarving && !base.owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT))
		{
			JobQueueItem job = base.owner.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
			if (job != null)
			{
				if (base.owner.currentJob == job)
				{
					return false;
				}
				job.CancelJob();
			}
			JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
			GoapPlanJob goapPlanJob = PlanFullnessRecovery(jobType);
			if (goapPlanJob != null)
			{
				base.owner.jobQueue.AddJobInQueue(goapPlanJob);
			}
			return true;
		}
		return false;
	}

	public bool PlanFullnessRecoveryActionsForFreeTime(out JobQueueItem producedJob)
	{
		if (!base.owner.limiterComponent.canPerform)
		{
			producedJob = null;
			return false;
		}
		if (base.owner.traitContainer.HasTrait("Vampire"))
		{
			producedJob = null;
			return false;
		}
		if ((isStarving || isHungry) && !base.owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, JOB_TYPE.FULLNESS_RECOVERY_NORMAL))
		{
			JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
			if (isHungry)
			{
				jobType = JOB_TYPE.FULLNESS_RECOVERY_NORMAL;
			}
			GoapPlanJob goapPlanJob = PlanFullnessRecovery(jobType);
			if (goapPlanJob != null)
			{
				producedJob = goapPlanJob;
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	public bool PlanFullnessRecoveryActionsVampire()
	{
		JobQueueItem producedJob;
		bool result = PlanFullnessRecoveryActionsVampire(out producedJob);
		if (producedJob != null)
		{
			base.owner.jobQueue.AddJobInQueue(producedJob);
		}
		return result;
	}

	public bool PlanFullnessRecoveryActionsVampire(out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!base.owner.limiterComponent.canPerform)
		{
			return false;
		}
		if (isStarving)
		{
			if (!base.owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT))
			{
				JobQueueItem job = base.owner.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
				if (job != null)
				{
					if (base.owner.currentJob == job)
					{
						return false;
					}
					job.CancelJob();
				}
				JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
				GoapPlanJob goapPlanJob = PlanFullnessRecovery(jobType);
				if (goapPlanJob != null)
				{
					producedJob = goapPlanJob;
				}
				return true;
			}
		}
		else if (isHungry && !base.owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL))
		{
			GoapPlanJob goapPlanJob2 = PlanFullnessRecovery(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
			if (goapPlanJob2 != null)
			{
				producedJob = goapPlanJob2;
			}
			return true;
		}
		return false;
	}

	private bool PlanFullnessRecoveryActionsWhileInActiveParty()
	{
		if (!base.owner.limiterComponent.canPerform)
		{
			return false;
		}
		if (base.owner.traitContainer.HasTrait("Vampire"))
		{
			bool flag = false;
			if (!isStarving)
			{
				if (base.owner.partyComponent.isMemberThatJoinedQuest && !base.owner.partyComponent.currentParty.partyFaction.GetCrimeSeverity(base.owner, base.owner, CRIME_TYPE.Vampire).IsConsideredACrime())
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				return false;
			}
		}
		if (isStarving)
		{
			if (!base.owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT))
			{
				JobQueueItem job = base.owner.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
				if (job != null)
				{
					if (base.owner.currentJob == job)
					{
						return false;
					}
					job.CancelJob();
				}
				JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
				GoapPlanJob goapPlanJob = PlanFullnessRecoveryBase(jobType);
				if (goapPlanJob != null)
				{
					base.owner.jobQueue.AddJobInQueue(goapPlanJob);
				}
				return true;
			}
		}
		else if (isHungry && !base.owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL))
		{
			GoapPlanJob goapPlanJob2 = PlanFullnessRecoveryBase(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
			if (goapPlanJob2 != null)
			{
				base.owner.jobQueue.AddJobInQueue(goapPlanJob2);
			}
			return true;
		}
		return false;
	}

	public void PlanFullnessRecoveryGlutton(out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!base.owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL))
		{
			JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_NORMAL;
			producedJob = PlanFullnessRecovery(jobType);
		}
	}

	private GoapPlanJob PlanFullnessRecovery(JOB_TYPE jobType)
	{
		if (base.owner.partyComponent.isActiveMember)
		{
			return null;
		}
		return PlanFullnessRecoveryBase(jobType);
	}

	private GoapPlanJob PlanFullnessRecoveryBase(JOB_TYPE jobType)
	{
		if (base.owner == null || base.owner.hasBeenCleanedUp)
		{
			return null;
		}
		if (!base.owner.limiterComponent.canDoFullnessRecovery && jobType != JOB_TYPE.TRIGGER_FLAW)
		{
			return null;
		}
		if (base.owner.traitContainer.HasTrait("Burning"))
		{
			return null;
		}
		if (base.owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD, JOB_TYPE.PRODUCE_FOOD_FOR_CAMP))
		{
			return null;
		}
		bool flag = false;
		Griefstricken traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Griefstricken>("Griefstricken");
		if (traitOrStatus != null)
		{
			flag = Random.Range(0, 100) < 25 * base.owner.traitContainer.stacks[traitOrStatus.name];
		}
		if (!flag)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), base.owner, base.owner);
			JobUtilities.PopulatePriorityLocationsForFullnessRecovery(base.owner, goapPlanJob);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { 20 });
			return goapPlanJob;
		}
		traitOrStatus.TriggerGrieving();
		return null;
	}

	public void ResetStaminaMeter()
	{
		bool wasSpent = isSpent;
		bool wasDrained = isDrained;
		bool wasSprightly = isSprightly;
		stamina = 100f;
		OnSprightly(wasSprightly, wasSpent, wasDrained);
	}

	public void AdjustStamina(float amount)
	{
		bool wasSpent = isSpent;
		bool wasDrained = isDrained;
		bool wasSprightly = isSprightly;
		stamina += amount;
		stamina = Mathf.Clamp(stamina, staminaLowerBound, 100f);
		if (isSprightly)
		{
			OnSprightly(wasSprightly, wasSpent, wasDrained);
		}
		else if (isSpent)
		{
			OnSpent(wasSprightly, wasSpent, wasDrained);
		}
		else if (isDrained)
		{
			OnDrained(wasSprightly, wasSpent, wasDrained);
		}
		else
		{
			OnNormalStamina(wasSprightly, wasSpent, wasDrained);
		}
	}

	public void SetStamina(float amount)
	{
		bool wasSpent = isSpent;
		bool wasDrained = isDrained;
		bool wasSprightly = isSprightly;
		stamina = amount;
		stamina = Mathf.Clamp(stamina, staminaLowerBound, 100f);
		if (isSprightly)
		{
			OnSprightly(wasSprightly, wasSpent, wasDrained);
		}
		else if (isSpent)
		{
			OnSpent(wasSprightly, wasSpent, wasDrained);
		}
		else if (isDrained)
		{
			OnDrained(wasSprightly, wasSpent, wasDrained);
		}
		else
		{
			OnNormalStamina(wasSprightly, wasSpent, wasDrained);
		}
	}

	private void OnSprightly(bool wasSprightly, bool wasSpent, bool wasDrained)
	{
		if (!wasSprightly)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Sprightly");
			base.owner.movementComponent.SetNoRunExceptCombat(state: false);
			base.owner.movementComponent.SetNoRunWithoutException(state: false);
		}
		if (wasSpent)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Spent");
		}
		if (wasDrained)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Drained");
		}
		base.owner.movementComponent.UpdateSpeed();
	}

	private void OnSpent(bool wasSprightly, bool wasSpent, bool wasDrained)
	{
		if (!wasSpent)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Spent");
			base.owner.movementComponent.SetNoRunExceptCombat(state: true);
		}
		if (wasSprightly)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Sprightly");
		}
		if (wasDrained)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Drained");
		}
		base.owner.movementComponent.UpdateSpeed();
	}

	private void OnDrained(bool wasSprightly, bool wasSpent, bool wasDrained)
	{
		if (!wasDrained)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Drained");
			base.owner.movementComponent.SetNoRunExceptCombat(state: true);
			base.owner.movementComponent.SetNoRunWithoutException(state: true);
		}
		if (wasSprightly)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Sprightly");
		}
		if (wasSpent)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Spent");
		}
		base.owner.movementComponent.UpdateSpeed();
	}

	private void OnNormalStamina(bool wasSprightly, bool wasSpent, bool wasDrained)
	{
		if (wasDrained)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Drained");
		}
		if (wasSprightly)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Sprightly");
		}
		if (wasSpent)
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Spent");
		}
		base.owner.movementComponent.UpdateSpeed();
	}

	public void AdjustDoNotGetDrained(int amount)
	{
		doNotGetDrained += amount;
	}

	public void UpdateBaseStaminaDecreaseRate()
	{
		baseStaminaDecreaseRate = Mathf.RoundToInt(base.owner.characterClass.staminaReduction * ((base.owner.raceSetting.staminaReductionMultiplier == 0f) ? 1f : base.owner.raceSetting.staminaReductionMultiplier));
	}

	public void ResetHopeMeter()
	{
		bool wasDiscouraged = isDiscouraged;
		bool wasHopeless = isHopeless;
		bool wasHopeful = isHopeful;
		hope = 100f;
		OnHopeful(wasHopeful, wasDiscouraged, wasHopeless);
	}

	public void AdjustHope(float amount)
	{
		bool wasDiscouraged = isDiscouraged;
		bool wasHopeless = isHopeless;
		bool wasHopeful = isHopeful;
		hope += amount;
		hope = Mathf.Clamp(hope, hopeLowerBound, 100f);
		if (isHopeful)
		{
			OnHopeful(wasHopeful, wasDiscouraged, wasHopeless);
		}
		else if (isDiscouraged)
		{
			OnDiscouraged(wasHopeful, wasDiscouraged, wasHopeless);
		}
		else if (isHopeless)
		{
			OnHopeless(wasHopeful, wasDiscouraged, wasHopeless);
		}
		else
		{
			OnNormalHope(wasHopeful, wasDiscouraged, wasHopeless);
		}
	}

	public void SetHope(float amount)
	{
		bool wasDiscouraged = isDiscouraged;
		bool wasHopeless = isHopeless;
		bool wasHopeful = isHopeful;
		hope = amount;
		hope = Mathf.Clamp(hope, hopeLowerBound, 100f);
		if (isHopeful)
		{
			OnHopeful(wasHopeful, wasDiscouraged, wasHopeless);
		}
		else if (isDiscouraged)
		{
			OnDiscouraged(wasHopeful, wasDiscouraged, wasHopeless);
		}
		else if (isHopeless)
		{
			OnHopeless(wasHopeful, wasDiscouraged, wasHopeless);
		}
		else
		{
			OnNormalHope(wasHopeful, wasDiscouraged, wasHopeless);
		}
	}

	private void OnHopeful(bool wasHopeful, bool wasDiscouraged, bool wasHopeless)
	{
	}

	private void OnDiscouraged(bool wasHopeful, bool wasDiscouraged, bool wasHopeless)
	{
	}

	private void OnHopeless(bool wasHopeful, bool wasDiscouraged, bool wasHopeless)
	{
	}

	private void OnNormalHope(bool wasHopeful, bool wasDiscouraged, bool wasHopeless)
	{
	}

	public void OnCharacterFinishedJob(JobQueueItem job)
	{
		if (job.jobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT)
		{
			if (base.owner.traitContainer.HasTrait("Pest") || base.owner is Rat)
			{
				base.owner.traitContainer.AddTrait(base.owner, "Abstain Fullness");
			}
			PlanFullnessRecoveryActions();
		}
		else if (job.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT)
		{
			PlanTirednessRecoveryActions();
		}
		else if (job.jobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL && (base.owner.traitContainer.HasTrait("Pest") || base.owner is Rat))
		{
			base.owner.traitContainer.AddTrait(base.owner, "Abstain Fullness");
		}
	}

	public bool TriggerFlawFullnessRecovery(Character character, bool isTriggeredByPlayer)
	{
		GoapPlanJob goapPlanJob = PlanFullnessRecoveryBase(JOB_TYPE.TRIGGER_FLAW);
		if (goapPlanJob != null)
		{
			goapPlanJob.SetIsTriggeredByPlayer(isTriggeredByPlayer);
			return character.jobQueue.AddJobInQueue(goapPlanJob);
		}
		return false;
	}

	public void LoadReferences(SaveDataCharacterNeedsComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
