using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PetBehaviour : CharacterBehaviour
{
	public PetBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		Character petOwner = character.petComponent.petOwner;
		if (petOwner.gridTileLocation != null && petOwner.gridTileLocation.structure is TortureChambers)
		{
			TileObject tileObject = null;
			IDamageable nearestDamageableThatContributeToHP = petOwner.gridTileLocation.structure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
			if (nearestDamageableThatContributeToHP != null && nearestDamageableThatContributeToHP is TileObject tileObject2)
			{
				tileObject = tileObject2;
			}
			if (tileObject != null)
			{
				character.combatComponent.Fight(tileObject, "Clear_Demonic_Intrusion");
				producedJob = null;
				return true;
			}
		}
		if (character.hasMarker && character.marker.IsPOIInVision(petOwner))
		{
			if (petOwner.combatComponent.isInActualCombat && petOwner.stateComponent.currentState is CombatState { currentClosestHostile: not null, currentClosestHostile: var currentClosestHostile })
			{
				CombatData combatData = petOwner.combatComponent.GetCombatData(currentClosestHostile);
				if (currentClosestHostile != null && combatData != null)
				{
					character.combatComponent.Fight(currentClosestHostile, combatData.reasonForCombat, null, combatData.isLethal);
					producedJob = null;
					return true;
				}
			}
			if (petOwner.gridTileLocation != null && character.gridTileLocation != null && character.gridTileLocation.structure != petOwner.gridTileLocation.structure && character.jobComponent.CreateFollowJob(petOwner, out producedJob))
			{
				return true;
			}
			if (petOwner.traitContainer.HasTrait("Resting") && character.jobComponent.TriggerMonsterSleep(out producedJob))
			{
				return true;
			}
			if (!petOwner.IsAtHome() && petOwner.currentActionNode != null && petOwner.hasMarker && petOwner.marker.isMoving && character.jobComponent.CreateFollowJob(petOwner, out producedJob))
			{
				return true;
			}
			if (GameUtilities.RollChance(10))
			{
				if (character.jobComponent.TriggerStand(out producedJob))
				{
					return true;
				}
			}
			else if (character.jobComponent.TriggerRoamAroundStructure(out producedJob))
			{
				return true;
			}
			producedJob = null;
			return false;
		}
		if (character.jobComponent.CreateFollowJob(petOwner, out producedJob))
		{
			return true;
		}
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.OnCharacterBecameAPet();
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.behaviourComponent.OnCharacterNoLongerAPet();
	}

	public override void OnLoadBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.OnCharacterBecameAPet();
	}
}
