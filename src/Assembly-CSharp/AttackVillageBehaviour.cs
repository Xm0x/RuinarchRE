using Inner_Maps;
using Locations.Settlements;
using UnityEngine;

public class AttackVillageBehaviour : CharacterBehaviour
{
	public AttackVillageBehaviour()
	{
		base.priority = 31;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.behaviourComponent.attackAreaTarget == null && character.behaviourComponent.attackVillageTarget == null)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
			return false;
		}
		if (character.behaviourComponent.attackVillageTarget != null && character.behaviourComponent.attackVillageTarget.areas.Count <= 0)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
			return false;
		}
		if (character.gridTileLocation.area.HasSettlementOnArea(character.behaviourComponent.attackVillageTarget) || character.gridTileLocation.area == character.behaviourComponent.attackAreaTarget)
		{
			BaseSettlement attackVillageTarget = character.behaviourComponent.attackVillageTarget;
			if (attackVillageTarget != null)
			{
				Character character2 = null;
				Character character3 = null;
				for (int i = 0; i < attackVillageTarget.residents.Count; i++)
				{
					Character character4 = attackVillageTarget.residents[i];
					if (character != character4 && !character4.isDead && character4.gridTileLocation != null && character4.gridTileLocation.IsPartOfSettlement(attackVillageTarget))
					{
						if (character4.traitContainer.HasTrait("Combatant"))
						{
							character3 = character4;
							break;
						}
						if (character2 == null)
						{
							character2 = character4;
						}
					}
				}
				if (character3 != null)
				{
					character.combatComponent.Fight(character3, "Hostility");
				}
				else if (character2 != null)
				{
					character.combatComponent.Fight(character2, "Hostility");
				}
				else
				{
					character.behaviourComponent.SetAttackVillageTarget(null);
					character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
				}
			}
			else if (character.behaviourComponent.attackAreaTarget != null)
			{
				Character randomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory = character.behaviourComponent.attackAreaTarget.locationCharacterTracker.GetRandomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory(character.behaviourComponent.attackAreaTarget);
				if (randomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory != null)
				{
					character.combatComponent.Fight(randomCharacterInsideHexThatIsAliveAndConsidersAreaAsTerritory, "Hostility");
				}
				else
				{
					character.behaviourComponent.SetAttackVillageTarget(null);
					character.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
				}
			}
			else
			{
				character.jobComponent.TriggerRoamAroundTile(out producedJob);
			}
		}
		else
		{
			Area area = character.behaviourComponent.attackAreaTarget;
			if (character.behaviourComponent.attackVillageTarget != null)
			{
				area = character.behaviourComponent.attackVillageTarget.areas[Random.Range(0, character.behaviourComponent.attackVillageTarget.areas.Count)];
			}
			LocationGridTile tile = area.gridTileComponent.gridTiles[Random.Range(0, area.gridTileComponent.gridTiles.Count)];
			character.jobComponent.CreateGoToJob(tile, out producedJob);
		}
		return true;
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.SetCombatModeBeforeAttackVillageBehaviour(character.combatComponent.combatMode);
		character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.combatComponent.SetCombatMode(character.behaviourComponent.combatModeBeforeAttackVillageBehaviour);
	}
}
