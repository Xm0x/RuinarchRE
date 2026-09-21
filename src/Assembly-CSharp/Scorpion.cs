using System;
using UtilityScripts;

public class Scorpion : Summon
{
	public const string ClassName = "Scorpion";

	public override Type serializedData => typeof(SaveDataScorpion);

	public Character heldCharacter { get; private set; }

	public bool hasPulledForTheDay { get; private set; }

	public GameDate nextPullDate { get; private set; }

	public Scorpion()
		: base(SUMMON_TYPE.Scorpion, "Scorpion", RACE.SCORPION, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public Scorpion(string className)
		: base(SUMMON_TYPE.Scorpion, className, RACE.SCORPION, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public Scorpion(SaveDataScorpion data)
		: base(data)
	{
		hasPulledForTheDay = data.hasPulledForTheDay;
		nextPullDate = data.nextPullDate;
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		base.LoadReferences(data);
		if (!(data is SaveDataScorpion saveDataScorpion))
		{
			return;
		}
		if (!string.IsNullOrEmpty(saveDataScorpion.heldCharacter))
		{
			heldCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataScorpion.heldCharacter);
		}
		if (hasPulledForTheDay)
		{
			SchedulingManager.Instance.AddEntry(nextPullDate, delegate
			{
				SetHasPulledForTheDay(p_state: false);
			}, this);
		}
	}

	protected override void OnHourStarted()
	{
		base.OnHourStarted();
		if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 || GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 18)
		{
			base.jobQueue.CancelAllJobs();
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}

	public override void OnAgitatedSuccessfully()
	{
		base.OnAgitatedSuccessfully();
		base.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
		base.reactionComponent.SetIsHidden(state: false);
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		if (heldCharacter == p_character)
		{
			SetHeldCharacter(null);
		}
	}

	public void SetHeldCharacter(Character p_character)
	{
		heldCharacter = p_character;
	}

	public void SetHasPulledForTheDay(bool p_state)
	{
		if (hasPulledForTheDay == p_state)
		{
			return;
		}
		hasPulledForTheDay = p_state;
		if (hasPulledForTheDay)
		{
			nextPullDate = GameManager.Instance.Today().AddDays(1);
			SchedulingManager.Instance.AddEntry(nextPullDate, delegate
			{
				SetHasPulledForTheDay(p_state: false);
			}, this);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = heldCharacter;
	}

	public override void CleanUp()
	{
		base.CleanUp();
		heldCharacter = null;
	}
}
