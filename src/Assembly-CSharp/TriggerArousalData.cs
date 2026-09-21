using Inner_Maps;
using Traits;

public class TriggerArousalData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TRIGGER_AROUSAL;

	public override string name => "Trigger Arousal";

	public override string description => "This Ability can be used to immediately force the Villager to attempt to make love with their object of Arousal.";

	public TriggerArousalData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (!(targetPOI is Character character))
		{
			return;
		}
		Character character2 = character.traitContainer.GetTraitOrStatus<Aroused>("Aroused")?.latestValidArousedTarget;
		if (character2 == null)
		{
			return;
		}
		character.jobComponent.TriggerMakeLoveJob(JOB_TYPE.TRIGGER_AROUSAL, character2, out var producedJob);
		if (producedJob == null || !character.jobQueue.AddJobInQueue(producedJob))
		{
			return;
		}
		LocationGridTile gridTileLocation = character.gridTileLocation;
		if (gridTileLocation != null)
		{
			int p_amount = 2;
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_AROUSAL).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTileLocation.centeredWorldLocation, p_amount, gridTileLocation.parentMap);
			}
		}
		base.ActivateAbility(targetPOI);
		PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_TRIGGER_AROUSAL);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag && target is Character character && (character.isDead || !character.isNormalCharacter))
		{
			return false;
		}
		return flag;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (!targetCharacter.traitContainer.HasTrait("Aroused"))
		{
			return false;
		}
		Character latestValidArousedTarget = targetCharacter.traitContainer.GetTraitOrStatus<Aroused>("Aroused").latestValidArousedTarget;
		if (latestValidArousedTarget == null)
		{
			return false;
		}
		if ((targetCharacter.tileObjectComponent.primaryBed == null || targetCharacter.tileObjectComponent.primaryBed.gridTileLocation == null) && (latestValidArousedTarget.tileObjectComponent.primaryBed == null || latestValidArousedTarget.tileObjectComponent.primaryBed.gridTileLocation == null))
		{
			return false;
		}
		if (targetCharacter.gridTileLocation == null || latestValidArousedTarget.gridTileLocation == null)
		{
			return false;
		}
		if (!targetCharacter.movementComponent.HasPathToEvenIfDiffRegion(latestValidArousedTarget.gridTileLocation))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.traitContainer.HasTrait("Aroused"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Arousal_No_Aroused") + "|";
		}
		else
		{
			Character latestValidArousedTarget = targetCharacter.traitContainer.GetTraitOrStatus<Aroused>("Aroused").latestValidArousedTarget;
			if (latestValidArousedTarget == null)
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Arousal_No_Target") + "|";
			}
			if ((targetCharacter.tileObjectComponent.primaryBed == null || targetCharacter.tileObjectComponent.primaryBed.gridTileLocation == null) && (latestValidArousedTarget == null || latestValidArousedTarget.tileObjectComponent.primaryBed == null || latestValidArousedTarget.tileObjectComponent.primaryBed.gridTileLocation == null))
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Arousal_No_Bed") + "|";
			}
			if (targetCharacter.gridTileLocation == null || (latestValidArousedTarget != null && latestValidArousedTarget.gridTileLocation == null))
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Arousal_No_Location") + "|";
			}
			if (latestValidArousedTarget != null && !targetCharacter.movementComponent.HasPathToEvenIfDiffRegion(latestValidArousedTarget.gridTileLocation))
			{
				text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Arousal_No_Path") + "|";
			}
		}
		return text;
	}
}
