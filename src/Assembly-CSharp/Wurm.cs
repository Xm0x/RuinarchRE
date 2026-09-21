using UtilityScripts;

public class Wurm : Summon
{
	public Wurm()
		: base(SUMMON_TYPE.Wurm, "Wurm", RACE.WURM, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public Wurm(string className)
		: base(SUMMON_TYPE.Wurm, className, RACE.WURM, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
	}

	public Wurm(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!base.traitContainer.HasTrait("Tower"))
		{
			base.reactionComponent.SetIsHidden(state: true);
		}
		base.traitContainer.AddTrait(this, "Subterranean");
		base.movementComponent.SetIsStationary(state: true);
	}

	public override void LoadReferencesMainThread(SaveDataCharacter data)
	{
		base.LoadReferencesMainThread(data);
		base.visuals.SetHasBlood(state: false);
	}

	public override void OnSummonAsPlayerMonster()
	{
		base.OnSummonAsPlayerMonster();
		base.traitContainer.RemoveTrait(this, "Subterranean");
	}

	public override void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
		base.OnJobAddedToCharacterJobQueue(job, character);
		if (character == this)
		{
			base.reactionComponent.SetIsHidden(state: false);
		}
	}

	public override void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		base.OnJobRemovedFromCharacterJobQueue(job, character, shouldBlacklist);
		if (character == this && character.jobQueue.jobsInQueue.Count <= 0 && !base.traitContainer.HasTrait("Tower"))
		{
			base.reactionComponent.SetIsHidden(state: true);
		}
	}
}
