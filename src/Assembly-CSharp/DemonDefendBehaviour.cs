using Inner_Maps;
using Inner_Maps.Location_Structures;

public class DemonDefendBehaviour : CharacterBehaviour
{
	public DemonDefendBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working)
		{
			DemonDefendPartyQuest demonDefendPartyQuest = currentParty.currentQuest as DemonDefendPartyQuest;
			Character partyMemberInCombatExcept = GetPartyMemberInCombatExcept(currentParty, character);
			if (partyMemberInCombatExcept != null)
			{
				bool flag = false;
				CombatState combatState = partyMemberInCombatExcept.stateComponent.currentState as CombatState;
				if (combatState.currentClosestHostile != null && demonDefendPartyQuest.IsDefenderNearEnoughToTarget(character, combatState.currentClosestHostile) && demonDefendPartyQuest.IsPOIInPlayerSettlementOrCorruptedTileAndNotInPrisonOrKennel(combatState.currentClosestHostile) && demonDefendPartyQuest.IsPOIInPlayerSettlementOrCorruptedTileAndNotInPrisonOrKennel(partyMemberInCombatExcept))
				{
					CombatData combatData = partyMemberInCombatExcept.combatComponent.GetCombatData(combatState.currentClosestHostile);
					if (combatData != null && (!character.movementComponent.isStationary || character.marker.IsCharacterInLineOfSightWith(combatState.currentClosestHostile)))
					{
						character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
						flag = true;
					}
				}
				if (flag)
				{
					producedJob = null;
					return true;
				}
			}
			for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.allStructures.Count; i++)
			{
				if (!(PlayerManager.Instance.player.playerSettlement.allStructures[i] is DemonicStructure { hasBeenDestroyed: false } demonicStructure) || demonicStructure.currentAttackers.Count <= 0)
				{
					continue;
				}
				Character character2 = null;
				for (int j = 0; j < demonicStructure.currentAttackers.Count; j++)
				{
					Character character3 = demonicStructure.currentAttackers[j];
					LocationGridTile gridTileLocation = character3.gridTileLocation;
					if (gridTileLocation != null && !character3.isBeingSeized && !character3.isDead && gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS && gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && character3.combatComponent.IsCurrentlyAttackingDemonicStructure())
					{
						character2 = character3;
						break;
					}
				}
				if (character2 != null && (!character.movementComponent.isStationary || character.marker.IsCharacterInLineOfSightWith(character2)))
				{
					character.combatComponent.Fight(character2, "Defending_Home");
					producedJob = null;
					return true;
				}
			}
			Character firstHostileIntruderOfPlayerSettlement = GetFirstHostileIntruderOfPlayerSettlement(character);
			if (firstHostileIntruderOfPlayerSettlement != null && demonDefendPartyQuest.IsDefenderNearEnoughToTarget(character, firstHostileIntruderOfPlayerSettlement))
			{
				if (!character.movementComponent.isStationary || character.marker.IsCharacterInLineOfSightWith(firstHostileIntruderOfPlayerSettlement))
				{
					character.combatComponent.Fight(firstHostileIntruderOfPlayerSettlement, "Defending_Home");
					producedJob = null;
					return true;
				}
			}
			else
			{
				if (character.movementComponent.isStationary)
				{
					return true;
				}
				LocationGridTile randomPassableTileThatIsCorruptedWithPathTo = PlayerManager.Instance.player.playerSettlement.GetRandomPassableTileThatIsCorruptedWithPathTo(character);
				result = ((randomPassableTileThatIsCorruptedWithPathTo == null) ? character.jobComponent.TriggerRoamAroundTile(out producedJob) : character.jobComponent.CreatePartyGoToSpecificTileJob(randomPassableTileThatIsCorruptedWithPathTo, out producedJob));
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return result;
	}

	private Character GetFirstHostileIntruderOfPlayerSettlement(Character actor)
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.areas.Count; i++)
		{
			Area p_area = PlayerManager.Instance.player.playerSettlement.areas[i];
			Character firstHostileIntruderOf = GetFirstHostileIntruderOf(actor, p_area);
			if (firstHostileIntruderOf != null)
			{
				return firstHostileIntruderOf;
			}
		}
		return null;
	}

	private Character GetFirstHostileIntruderOf(Character actor, Area p_area)
	{
		return p_area.locationCharacterTracker.GetFirstCharacterInsideHexThatIsAliveHostileAndInCorruptedTileThatHasPathTo(actor);
	}

	private Character GetPartyMemberInCombatExcept(Party party, Character exception)
	{
		for (int i = 0; i < party.membersThatJoinedQuest.Count; i++)
		{
			Character character = party.membersThatJoinedQuest[i];
			LocationGridTile gridTileLocation = character.gridTileLocation;
			if (character != exception && gridTileLocation != null && gridTileLocation.IsPartOfSettlement(out var settlement) && settlement == PlayerManager.Instance.player.playerSettlement && character.combatComponent.isInCombat)
			{
				return character;
			}
		}
		return null;
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeDemonicDefender();
		if (character is Dragon dragon)
		{
			dragon.OnBecomeDefender();
		}
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.behaviourComponent.OnNoLongerDemonicDefender();
	}

	public override void OnLoadBehaviourToCharacter(Character character)
	{
		base.OnLoadBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeDemonicDefender();
	}
}
