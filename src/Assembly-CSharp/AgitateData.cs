using Inner_Maps.Location_Structures;

public class AgitateData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.AGITATE;

	public override string name => "Agitate";

	public override string description => "This Ability may be used on a wild monster to force it to enter a state of frenzy. It will terrorize nearby Villagers if possible.\nAgitating a monster produces 1 Chaos Orb. Additional 2 Chaos Orbs are produced each time the monster kills a Villager.";

	public AgitateData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Summon summon)
		{
			JobQueueItem p_agitateJob = null;
			if (summon.Agitate(ref p_agitateJob) && p_agitateJob != null)
			{
				summon.jobQueue.CancelAllJobs();
				summon.behaviourComponent.SetIsAgitated(state: false);
				summon.behaviourComponent.SetIsAgitated(state: true);
				if (summon.jobQueue.AddJobInQueue(p_agitateJob))
				{
					summon.OnAgitatedSuccessfully();
				}
				else
				{
					summon.behaviourComponent.SetIsAgitated(state: false);
				}
				Messenger.Broadcast(PlayerSkillSignals.AGITATE_ACTIVATED, targetPOI);
			}
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (targetCharacter.currentStructure is Kennel)
			{
				return false;
			}
			if (targetCharacter.movementComponent.isStationary)
			{
				return false;
			}
			if (targetCharacter.traitContainer.HasTrait("Agitated"))
			{
				return false;
			}
			if (targetCharacter is Summon summon)
			{
				if (targetCharacter.characterClass.cantBeAgitated)
				{
					return false;
				}
				if (summon.isTamed)
				{
					return false;
				}
			}
			if (!PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !targetCharacter.traitContainer.HasTrait("Hibernating") && !targetCharacter.isDead)
			{
				return targetCharacter.limiterComponent.canPerform;
			}
			return false;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.currentStructure is Kennel)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Kennel_Cannot_Agitate", targetCharacter) + "|";
		}
		if (targetCharacter is Summon { isTamed: not false })
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Tamed_Cannot_Agitate", targetCharacter) + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Summon summon)
		{
			if (base.IsValid(target))
			{
				if (summon.faction != null)
				{
					return !summon.faction.isPlayerFaction;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
