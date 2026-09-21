using Inner_Maps.Location_Structures;

public class AttackDemonicStructureBehaviour : CharacterBehaviour
{
	public AttackDemonicStructureBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.partyComponent.hasParty)
		{
			Party currentParty = character.partyComponent.currentParty;
			if (currentParty.isActive)
			{
				PartyQuest currentQuest = currentParty.currentQuest;
				LocationStructure firstStructureOfType = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
				if (firstStructureOfType == null || firstStructureOfType.hasBeenDestroyed || firstStructureOfType.objectsThatContributeToDamage.Count <= 0)
				{
					currentQuest.SetIsSuccessful(state: true);
					if (currentParty.targetDestination != currentParty.partySettlement)
					{
						currentParty.GoBackHomeAndEndQuest();
					}
					else
					{
						currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
					}
					return true;
				}
				if (currentParty.partyState == PARTY_STATE.Working)
				{
					if (firstStructureOfType.objectsThatContributeToDamage.Count > 0 && !firstStructureOfType.hasBeenDestroyed)
					{
						TileObject tileObject = null;
						IDamageable nearestDamageableThatContributeToHP = firstStructureOfType.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
						if (nearestDamageableThatContributeToHP != null && nearestDamageableThatContributeToHP is TileObject tileObject2)
						{
							tileObject = tileObject2;
						}
						if (tileObject != null)
						{
							character.combatComponent.Fight(tileObject, "Clear_Demonic_Intrusion");
							return true;
						}
						currentParty.GoBackHomeAndEndQuest();
						return true;
					}
					currentParty.GoBackHomeAndEndQuest();
					return true;
				}
			}
			return false;
		}
		LocationStructure attackDemonicStructureTarget = character.behaviourComponent.attackDemonicStructureTarget;
		if (attackDemonicStructureTarget == null || attackDemonicStructureTarget.hasBeenDestroyed)
		{
			character.behaviourComponent.SetIsAttackingDemonicStructure(state: false, null);
			return true;
		}
		if (attackDemonicStructureTarget.objectsThatContributeToDamage.Count > 0 && !attackDemonicStructureTarget.hasBeenDestroyed)
		{
			TileObject tileObject3 = null;
			IDamageable nearestDamageableThatContributeToHP2 = attackDemonicStructureTarget.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
			if (nearestDamageableThatContributeToHP2 != null && nearestDamageableThatContributeToHP2 is TileObject tileObject4)
			{
				tileObject3 = tileObject4;
			}
			if (tileObject3 != null)
			{
				character.combatComponent.Fight(tileObject3, "Clear_Demonic_Intrusion");
				return true;
			}
			character.behaviourComponent.SetIsAttackingDemonicStructure(state: false, null);
			return true;
		}
		character.behaviourComponent.SetIsAttackingDemonicStructure(state: false, null);
		return true;
	}
}
