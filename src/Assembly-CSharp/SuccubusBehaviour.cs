using UnityEngine;
using UtilityScripts;

public class SuccubusBehaviour : BaseMonsterBehaviour
{
	public SuccubusBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.reactionComponent.disguisedCharacter != null)
		{
			JOB_TYPE previousJobType = character.previousCharacterDataComponent.previousJobType;
			if ((character.isAtHomeStructure || character.IsInTerritory() || character.IsInHomeSettlement()) && (previousJobType == JOB_TYPE.IDLE_RETURN_HOME || previousJobType == JOB_TYPE.RETURN_TERRITORY))
			{
				character.reactionComponent.SetDisguisedCharacter(null);
				return true;
			}
			if (character.previousCharacterDataComponent.previousActionNodeType == INTERACTION_TYPE.MAKE_LOVE)
			{
				if (character.currentStructure != character.homeStructure && !character.IsInTerritory() && character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob))
				{
					return true;
				}
			}
			else
			{
				Character randomCharacterForSuccubusMakeLove = character.currentRegion.GetRandomCharacterForSuccubusMakeLove(character);
				if (randomCharacterForSuccubusMakeLove == null)
				{
					character.reactionComponent.SetDisguisedCharacter(null);
					return true;
				}
				if (!character.movementComponent.HasPathToEvenIfDiffRegion(randomCharacterForSuccubusMakeLove.gridTileLocation))
				{
					character.reactionComponent.SetDisguisedCharacter(null);
					return true;
				}
				if (character.jobComponent.TriggerMakeLoveJob(JOB_TYPE.IDLE, randomCharacterForSuccubusMakeLove, out producedJob))
				{
					return true;
				}
			}
		}
		else if (character.isAtHomeStructure || character.IsInTerritory() || character.IsInHomeSettlement())
		{
			if (Random.Range(0, 100) < 1)
			{
				Character randomCharacterThatIsThisGenderVillagerAndNotDead = character.currentRegion.GetRandomCharacterThatIsThisGenderVillagerAndNotDead(GENDER.FEMALE);
				if (randomCharacterThatIsThisGenderVillagerAndNotDead != null && character.currentRegion.GetRandomCharacterForSuccubusMakeLove(character) != null && character.jobComponent.TriggerDisguiseJob(randomCharacterThatIsThisGenderVillagerAndNotDead, out producedJob))
				{
					return true;
				}
			}
		}
		else if (character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob))
		{
			return true;
		}
		character.jobComponent.TriggerRoamAroundTile(out producedJob);
		return true;
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		if (DisguisedBehaviourTamed(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob))
		{
			return true;
		}
		if (GameUtilities.RollChance(5) && p_character.currentRegion.GetRandomCharacterThatIsMaleVillagerAndNotDeadAndFactionIsNotTheSameAs(p_character) != null)
		{
			Character randomCharacterThatIsThisGenderVillagerAndNotDead = p_character.currentRegion.GetRandomCharacterThatIsThisGenderVillagerAndNotDead(GENDER.FEMALE);
			if (randomCharacterThatIsThisGenderVillagerAndNotDead != null && p_character.currentRegion.GetRandomCharacterForSuccubusMakeLoveTamed(p_character) != null && p_character.jobComponent.TriggerDisguiseJob(randomCharacterThatIsThisGenderVillagerAndNotDead, out p_producedJob))
			{
				return true;
			}
		}
		return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
	}

	private bool DisguisedBehaviourTamed(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.reactionComponent.disguisedCharacter != null)
		{
			if (character.isAtHomeStructure || character.IsInTerritory() || character.IsInHomeSettlement())
			{
				JOB_TYPE previousJobType = character.previousCharacterDataComponent.previousJobType;
				if (previousJobType == JOB_TYPE.IDLE_RETURN_HOME || previousJobType == JOB_TYPE.RETURN_TERRITORY)
				{
					character.reactionComponent.SetDisguisedCharacter(null);
					return true;
				}
			}
			if (character.previousCharacterDataComponent.previousActionNodeType == INTERACTION_TYPE.MAKE_LOVE)
			{
				if (character.currentStructure != character.homeStructure && !character.IsInTerritory())
				{
					character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
					return true;
				}
				return true;
			}
			Character randomCharacterForSuccubusMakeLoveTamed = character.currentRegion.GetRandomCharacterForSuccubusMakeLoveTamed(character);
			if (randomCharacterForSuccubusMakeLoveTamed != null)
			{
				if (character.movementComponent.HasPathToEvenIfDiffRegion(randomCharacterForSuccubusMakeLoveTamed.gridTileLocation))
				{
					character.jobComponent.TriggerMakeLoveJob(JOB_TYPE.IDLE, randomCharacterForSuccubusMakeLoveTamed, out producedJob);
					return true;
				}
				character.reactionComponent.SetDisguisedCharacter(null);
				return true;
			}
			character.reactionComponent.SetDisguisedCharacter(null);
			return true;
		}
		return false;
	}
}
