using System;
using UnityEngine;

public enum ACHIEVEMENT
{
	GOAL_POISON_FOOD,
	GOAL_IMPRISON_VILLAGER,
	GOAL_CAPTURE_TRITON,
	GOAL_FALLEN_ANGEL,
	GOAL_DEMON_CULTIST,
	GOAL_DESTROY_RESOURCE,
	GOAL_CREATE_PSYCHO,
	GOAL_REMOVE_BUFF,
	GOAL_KILL_ELF,
	GOAL_KILL_HUMAN,
	GOAL_MAKE_VILLAGER_EVIL,
	GOAL_SHARE_CRIME_INTEL,
	GOAL_SNATCH_OBJECT,
	GOAL_TRIGGER_FLAW,
	GOAL_TRIGGER_AROUSAL,
	GOAL_POISON_CLOUD,
	GOAL_FROSTY_FOG,
	GOAL_BALL_LIGHTNING,
	BABYARCH,
	TEENARCH,
	ADULTARCH,
	RUINARCH,
	UNDEAD_SUPREMACY,
	CREATURES_OF_THE_NIGHT,
	IDOL_WORSHIP,
	TERRORIZED_VILLAGERS,
	OUTBREAK,
	DEATH_AND_DESTRUCTION
}
[Serializable]
public class Achievement
{
	public ACHIEVEMENT achievementIDEnum;

	public ACHIEVEMENT_STAT statIDEnum;

	public string achievementID;

	public string statID;

	public string nameKey;

	public string descriptionKey;

	public int currentProgress;

	public int maxProgress;

	[SerializeField]
	private bool _isAchieved;

	public bool isAchieved => _isAchieved;

	public Achievement(ACHIEVEMENT p_achievementIDEnum)
	{
		achievementIDEnum = p_achievementIDEnum;
		achievementID = achievementIDEnum.ToString();
		statID = string.Empty;
		nameKey = achievementID + "_NAME";
		descriptionKey = achievementID + "_DESC";
		_isAchieved = false;
	}

	public Achievement(ACHIEVEMENT p_achievementIDEnum, ACHIEVEMENT_STAT p_statIDEnum, int p_maxProgress)
	{
		achievementIDEnum = p_achievementIDEnum;
		achievementID = achievementIDEnum.ToString();
		statIDEnum = p_statIDEnum;
		statID = statIDEnum.ToString();
		nameKey = achievementID + "_NAME";
		descriptionKey = achievementID + "_DESC";
		_isAchieved = false;
		currentProgress = 0;
		maxProgress = p_maxProgress;
	}

	public void SetNameAndDescriptionKey(string p_nameKey, string p_descKey)
	{
		nameKey = p_nameKey;
		descriptionKey = p_descKey;
	}

	public void SetIsAchieved(bool p_state)
	{
		_isAchieved = p_state;
	}

	public void SetCurrentProgress(int p_currentProgress)
	{
		currentProgress = p_currentProgress;
	}

	public void SetMaxProgress(int p_maxProgress)
	{
		maxProgress = p_maxProgress;
	}

	public void ResetAchievement()
	{
		currentProgress = 0;
		_isAchieved = false;
	}
}
