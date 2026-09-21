public class UndeadBehaviour : CharacterBehaviour
{
	public UndeadBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.race == RACE.SKELETON)
		{
			Faction undeadFaction = FactionManager.Instance.undeadFaction;
			if (undeadFaction.leader != null && undeadFaction.leader is Character { homeStructure: var homeStructure } character2)
			{
				if (homeStructure != null)
				{
					if (character.homeStructure != homeStructure)
					{
						character.MigrateHomeStructureTo(homeStructure);
						character.ClearTerritory();
					}
					if (character.currentStructure == homeStructure && character2.currentStructure == homeStructure)
					{
						if (character2.combatComponent.isInCombat)
						{
							bool flag = false;
							CombatState combatState = character2.stateComponent.currentState as CombatState;
							if (combatState.currentClosestHostile != null)
							{
								CombatData combatData = character2.combatComponent.GetCombatData(combatState.currentClosestHostile);
								character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
								flag = true;
							}
							else if (character2.combatComponent.avoidInRange.Count > 0)
							{
								for (int i = 0; i < character2.combatComponent.avoidInRange.Count; i++)
								{
									if (character2.combatComponent.avoidInRange[i] is Character target)
									{
										character.combatComponent.Fight(target, "Hostility");
										flag = true;
									}
								}
							}
							if (flag)
							{
								producedJob = null;
								return true;
							}
						}
						character.jobComponent.TriggerRoamAroundTile(out producedJob);
						return true;
					}
					if (character2.isBeingSeized || !character2.marker || character2.grave != null || character2.gridTileLocation == null || character2.isDead || character.gridTileLocation == null || !character.movementComponent.HasPathToEvenIfDiffRegion(character2.gridTileLocation))
					{
						if (character.currentStructure != homeStructure)
						{
							character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
							return true;
						}
						character.jobComponent.TriggerRoamAroundTile(out producedJob);
						return true;
					}
					if (character.marker.IsPOIInVision(character2))
					{
						if (character2.combatComponent.isInCombat)
						{
							CombatState combatState2 = character2.stateComponent.currentState as CombatState;
							if (combatState2.currentClosestHostile != null)
							{
								CombatData combatData2 = character2.combatComponent.GetCombatData(combatState2.currentClosestHostile);
								character.combatComponent.Fight(combatState2.currentClosestHostile, combatData2.reasonForCombat, combatData2.connectedAction, combatData2.isLethal);
							}
							else if (character2.combatComponent.avoidInRange.Count > 0)
							{
								for (int j = 0; j < character2.combatComponent.avoidInRange.Count; j++)
								{
									if (character2.combatComponent.avoidInRange[j] is Character target2)
									{
										character.combatComponent.Fight(target2, "Hostility");
									}
								}
							}
						}
						producedJob = null;
						return true;
					}
					if (character.jobComponent.CreateGoToJob(character2))
					{
						producedJob = null;
						return true;
					}
				}
				else
				{
					if (character2.isDead)
					{
						if (character.currentStructure != homeStructure)
						{
							character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
							return true;
						}
						character.jobComponent.TriggerRoamAroundTile(out producedJob);
						return true;
					}
					if (character.marker.IsPOIInVision(character2))
					{
						producedJob = null;
						return true;
					}
					if (character.jobComponent.CreateGoToJob(character2))
					{
						producedJob = null;
						return true;
					}
				}
			}
		}
		producedJob = null;
		return false;
	}
}
