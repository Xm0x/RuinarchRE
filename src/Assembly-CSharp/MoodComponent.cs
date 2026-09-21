using System;
using System.Collections.Generic;
using Characters.Components;
using Inner_Maps.Location_Structures;
using UnityEngine.Localization;
using UtilityScripts;

public class MoodComponent : CharacterComponent, LocalizationManagerEventDispatcher.ILocaleChangeListener, CharacterEventDispatcher.IDeathListener
{
	public int moodValue { get; private set; }

	public bool isInNormalMood { get; private set; }

	public bool isInLowMood { get; private set; }

	public bool isInCriticalMood { get; private set; }

	public bool executeMoodChangeEffects { get; private set; }

	public bool isInCriticalBreak { get; private set; }

	public bool hasMoodChanged { get; private set; }

	public CRITICAL_BREAK_ACTION currentCriticalBreak { get; set; }

	public object criticalBreakTarget { get; set; }

	public Type criticalBreakTargetType { get; set; }

	public Dictionary<string, List<MoodModification>> allMoodModifications { get; private set; }

	public MOOD_STATE moodState
	{
		get
		{
			if (isInNormalMood)
			{
				return MOOD_STATE.Normal;
			}
			if (isInLowMood)
			{
				return MOOD_STATE.Bad;
			}
			if (isInCriticalMood)
			{
				return MOOD_STATE.Critical;
			}
			throw new Exception("Problem determining " + base.owner.name + "'s mood. Because all switches are set to false.");
		}
	}

	public string moodStateName
	{
		get
		{
			if (isInNormalMood)
			{
				return LocalizationManager.Instance.GetLocalizedValue("CharacterGeneric_Table", "Normal_Mood");
			}
			if (isInLowMood)
			{
				return LocalizationManager.Instance.GetLocalizedValue("CharacterGeneric_Table", "Bad_Mood");
			}
			if (isInCriticalMood)
			{
				return LocalizationManager.Instance.GetLocalizedValue("CharacterGeneric_Table", "Critical_Mood");
			}
			return "";
		}
	}

	public MoodComponent()
	{
		EnableMoodEffects();
		allMoodModifications = new Dictionary<string, List<MoodModification>>(10);
		isInNormalMood = true;
	}

	public MoodComponent(SaveDataMoodComponent data)
	{
		SetSaveDataMoodComponent(data);
	}

	public void SetSaveDataMoodComponent(SaveDataMoodComponent data)
	{
		moodValue = data.moodValue;
		isInNormalMood = data.isInNormalMood;
		isInLowMood = data.isInLowMood;
		isInCriticalMood = data.isInCriticalMood;
		isInCriticalBreak = data.isInCriticalBreak;
		executeMoodChangeEffects = data.executeMoodChangeEffects;
		hasMoodChanged = data.hasMoodChanged;
		currentCriticalBreak = data.currentCriticalBreak;
		criticalBreakTargetType = data.criticalBreakTargetType;
		if (data.allMoodModifications != null && data.allMoodModifications.Count > 0)
		{
			allMoodModifications = new Dictionary<string, List<MoodModification>>(data.allMoodModifications);
		}
		else
		{
			allMoodModifications = new Dictionary<string, List<MoodModification>>();
		}
	}

	public void SubscribeListeners()
	{
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	public void UnsubscribeListeners()
	{
		Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	private void OnGamePaused(bool p_isPaused)
	{
		if (p_isPaused)
		{
			TryUpdateMoodState();
		}
	}

	public void OnCharacterBecomeMinionOrSummon()
	{
		DisableMoodEffects();
	}

	public void OnCharacterNoLongerMinionOrSummon()
	{
		EnableMoodEffects();
	}

	private void EnableMoodEffects()
	{
		executeMoodChangeEffects = true;
	}

	private void DisableMoodEffects()
	{
		executeMoodChangeEffects = false;
	}

	public void SetMoodValue(int amount)
	{
		moodValue = amount;
		hasMoodChanged = true;
		if (GameManager.Instance.isPaused)
		{
			TryUpdateMoodState();
		}
	}

	public void AddMoodEffect(int amount, IMoodModifier modifier, GameDate expiryDate, Character characterResponsible)
	{
		moodValue += amount;
		AddModificationToSummary(modifier.modifierName, amount, expiryDate, modifier.GetMoodEffectFlavorText(characterResponsible));
		hasMoodChanged = true;
		if (GameManager.Instance.isPaused)
		{
			TryUpdateMoodState();
		}
	}

	public void RescheduleMoodEffect(IMoodModifier p_modifier, GameDate p_rescheduledDate)
	{
		if (!allMoodModifications.ContainsKey(p_modifier.modifierName))
		{
			return;
		}
		List<MoodModification> list = allMoodModifications[p_modifier.modifierName];
		if (list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			MoodModification moodModification = list[i];
			int num = i + 1;
			if (num < list.Count)
			{
				MoodModification moodModification2 = list[num];
				moodModification.expiryDate = moodModification2.expiryDate;
			}
		}
		list[list.Count - 1].expiryDate = p_rescheduledDate;
	}

	public void RemoveMoodEffect(int amount, IMoodModifier modifier)
	{
		moodValue += amount;
		RemoveModificationFromSummary(modifier.modifierName, amount);
		hasMoodChanged = true;
		if (GameManager.Instance.isPaused)
		{
			TryUpdateMoodState();
		}
	}

	public void OnTickEnded()
	{
		TryUpdateMoodState();
	}

	private void TryUpdateMoodState()
	{
		if (hasMoodChanged)
		{
			hasMoodChanged = false;
			OnMoodChanged();
		}
	}

	public void LoadReferences(SaveDataMoodComponent data)
	{
		if (!string.IsNullOrEmpty(data.criticalBreakTargetID))
		{
			if (criticalBreakTargetType == typeof(Character))
			{
				criticalBreakTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.criticalBreakTargetID);
			}
			else if (criticalBreakTargetType == typeof(LocationStructure))
			{
				criticalBreakTarget = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.criticalBreakTargetID);
			}
		}
		if (isInCriticalBreak && currentCriticalBreak == CRITICAL_BREAK_ACTION.Kill_Target)
		{
			(criticalBreakTarget as Character).eventDispatcher.SubscribeToCharacterDied(this);
		}
	}

	private void OnMoodChanged()
	{
		if (moodValue >= EditableValuesManager.Instance.normalMoodMinThreshold)
		{
			if (!isInNormalMood)
			{
				EnterNormalMood();
			}
		}
		else if (moodValue >= EditableValuesManager.Instance.lowMoodMinThreshold && moodValue <= EditableValuesManager.Instance.lowMoodHighThreshold)
		{
			if (!isInLowMood)
			{
				EnterLowMood();
			}
		}
		else if (moodValue <= EditableValuesManager.Instance.criticalMoodHighThreshold && !isInCriticalMood)
		{
			EnterCriticalMood();
		}
	}

	private void EnterNormalMood()
	{
		SwitchMoodStates(MOOD_STATE.Normal);
	}

	private void ExitNormalMood()
	{
		isInNormalMood = false;
	}

	private void EnterLowMood()
	{
		SwitchMoodStates(MOOD_STATE.Bad);
	}

	private void ExitLowMood()
	{
		isInLowMood = false;
	}

	private void EnterCriticalMood()
	{
		SwitchMoodStates(MOOD_STATE.Critical);
	}

	private void ExitCriticalMood()
	{
		isInCriticalMood = false;
	}

	public void OnOwnerDied()
	{
		if (isInCriticalBreak)
		{
			if (currentCriticalBreak == CRITICAL_BREAK_ACTION.Kill_Target)
			{
				(criticalBreakTarget as Character).eventDispatcher.UnsubscribeToCharacterDied(this);
			}
			else if (currentCriticalBreak == CRITICAL_BREAK_ACTION.Destroy_Structure)
			{
				base.owner.behaviourComponent.StopNonInstantCriticalBreak();
			}
		}
	}

	private void SwitchMoodStates(MOOD_STATE moodToEnter)
	{
		MOOD_STATE mOOD_STATE = moodState;
		switch (moodToEnter)
		{
		case MOOD_STATE.Bad:
			isInLowMood = true;
			break;
		case MOOD_STATE.Normal:
			isInNormalMood = true;
			break;
		case MOOD_STATE.Critical:
			isInCriticalMood = true;
			break;
		}
		switch (mOOD_STATE)
		{
		case MOOD_STATE.Bad:
			ExitLowMood();
			break;
		case MOOD_STATE.Normal:
			ExitNormalMood();
			break;
		case MOOD_STATE.Critical:
			ExitCriticalMood();
			break;
		}
		base.owner.eventDispatcher.ExecuteMoodChangedEvent(base.owner, mOOD_STATE, moodToEnter);
	}

	private void AddModificationToSummary(string p_modifierName, int p_modificationValue, GameDate p_expiryDate, Log p_modificationFlavorText)
	{
		MoodModification moodModification = ObjectPoolManager.Instance.CreateNewMoodModification();
		moodModification.SetData(p_modificationValue, p_expiryDate, p_modificationFlavorText);
		if (!allMoodModifications.ContainsKey(p_modifierName))
		{
			List<MoodModification> list = RuinarchListPool<MoodModification>.Claim(8);
			list.Add(moodModification);
			allMoodModifications.Add(p_modifierName, list);
		}
		else
		{
			allMoodModifications[p_modifierName].Add(moodModification);
		}
		Messenger.Broadcast(CharacterSignals.MOOD_SUMMARY_MODIFIED, this);
	}

	private void RemoveModificationFromSummary(string modificationKey, int modificationValue)
	{
		if (!allMoodModifications.ContainsKey(modificationKey))
		{
			return;
		}
		List<MoodModification> list = allMoodModifications[modificationKey];
		if (list != null && list.Count > 0)
		{
			MoodModification data = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			ObjectPoolManager.Instance.ReturnMoodModificationToPool(data);
			if (list.Count <= 0)
			{
				allMoodModifications.Remove(modificationKey);
				RuinarchListPool<MoodModification>.Release(list);
			}
			Messenger.Broadcast(CharacterSignals.MOOD_SUMMARY_MODIFIED, this);
		}
	}

	public void UpdateMoodModificationLog(IMoodModifier p_modifier, Character p_characterResponsible)
	{
		if (!allMoodModifications.ContainsKey(p_modifier.modifierName))
		{
			return;
		}
		List<MoodModification> list = allMoodModifications[p_modifier.modifierName];
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[0].UpdateLog(p_modifier.GetMoodEffectFlavorText(p_characterResponsible));
			}
			Messenger.Broadcast(CharacterSignals.MOOD_SUMMARY_MODIFIED, this);
		}
	}

	public void UpdateMoodSummaryLogsOnCharacterChangedName(Character p_character)
	{
		if (allMoodModifications == null)
		{
			return;
		}
		foreach (List<MoodModification> value in allMoodModifications.Values)
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].flavorText?.TryUpdateLogAfterRename(p_character);
			}
		}
	}

	public void OnLocaleChanged(Locale locale)
	{
		if (allMoodModifications == null)
		{
			return;
		}
		foreach (List<MoodModification> value in allMoodModifications.Values)
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].flavorText?.ReevaluateTextGivenNewLanguage(locale);
			}
		}
	}

	public void OnCriticalBreakStarted(CRITICAL_BREAK_ACTION p_action, object p_target)
	{
		isInCriticalBreak = true;
		currentCriticalBreak = p_action;
		if (p_target != null)
		{
			criticalBreakTarget = p_target;
			criticalBreakTargetType = p_target.GetType();
		}
		switch (p_action)
		{
		case CRITICAL_BREAK_ACTION.Break_Up:
		case CRITICAL_BREAK_ACTION.Abandon_Faction:
		case CRITICAL_BREAK_ACTION.Become_Cannibal:
		case CRITICAL_BREAK_ACTION.Lightning_Storm:
		case CRITICAL_BREAK_ACTION.Expel:
		case CRITICAL_BREAK_ACTION.Become_Bandit:
		case CRITICAL_BREAK_ACTION.Become_Evil:
			OnCriticalBreakFinished();
			break;
		case CRITICAL_BREAK_ACTION.Kill_Target:
			(p_target as Character).eventDispatcher.SubscribeToCharacterDied(this);
			break;
		default:
			throw new ArgumentOutOfRangeException("p_action", p_action, null);
		case CRITICAL_BREAK_ACTION.Commit_Suicide:
		case CRITICAL_BREAK_ACTION.Destroy_Structure:
			break;
		}
	}

	private void OnCriticalBreakFinished()
	{
		isInCriticalBreak = false;
		criticalBreakTarget = null;
		criticalBreakTargetType = null;
		(PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CRITICAL_BREAK) as CriticalBreakData).SpawnChaosOrbs(base.owner, 2, base.owner.gridTileLocation);
		if (!base.owner.isDead)
		{
			base.owner.traitContainer.AddTrait(base.owner, "Catharsis");
		}
	}

	public void EndCriticalBreakAbruptly()
	{
		isInCriticalBreak = false;
		criticalBreakTarget = null;
		criticalBreakTargetType = null;
	}

	private void CheckIfKillTargetIsFulfilled(Character p_character)
	{
		if (p_character == criticalBreakTarget)
		{
			OnCriticalBreakFinished();
			p_character.eventDispatcher.UnsubscribeToCharacterDied(this);
		}
	}

	public void OnCharacterFinishedJob(JobQueueItem p_job)
	{
		if (isInCriticalBreak && currentCriticalBreak == CRITICAL_BREAK_ACTION.Commit_Suicide && p_job.jobType == JOB_TYPE.CRITICAL_BREAK)
		{
			OnCriticalBreakFinished();
		}
	}

	public void DoneDestroyStructureCriticalBreak()
	{
		OnCriticalBreakFinished();
	}

	public void OnCharacterSubscribedToDied(Character p_character)
	{
		CheckIfKillTargetIsFulfilled(p_character);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
