using UtilityScripts;

public class SleepBehaviour : CharacterBehaviour
{
	public SleepBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Sleep)
		{
			if (character.traitContainer.HasTrait("Alerted") && character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob))
			{
				return true;
			}
			if (GameUtilities.RollChance(20, ref log) && character.homeSettlement != null && character.IsInHomeSettlement() && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE && (character is Summon || character.characterClass.IsCombatant()) && character.jobComponent.TriggerPersonalPatrol(out producedJob))
			{
				return true;
			}
			if (!character.needsComponent.doesNotGetTired)
			{
				bool flag = false;
				if (character.hasMarker)
				{
					for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
					{
						if (character.marker.inVisionCharacters[i].combatComponent.isInActualCombat)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return character.needsComponent.PlanTirednessRecoveryActionsForSleepBehaviour(out producedJob);
				}
			}
		}
		producedJob = null;
		return false;
	}
}
