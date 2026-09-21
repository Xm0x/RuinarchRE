public class DefendBehaviour : CharacterBehaviour
{
	public DefendBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.IsAtHome())
		{
			Character firstHostileIntruder = GetFirstHostileIntruder(character);
			if (firstHostileIntruder != null)
			{
				character.combatComponent.Fight(firstHostileIntruder, "Hostility");
				producedJob = null;
				return true;
			}
			return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
		}
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}

	private Character GetFirstHostileIntruder(Character actor)
	{
		if (actor.homeStructure != null)
		{
			for (int i = 0; i < actor.homeStructure.charactersHere.Count; i++)
			{
				Character character = actor.homeStructure.charactersHere[i];
				if (actor != character && actor.IsHostileWith(character) && !character.isDead && !character.isAlliedWithPlayer && (bool)character.marker && character.marker.isMainVisualActive && actor.movementComponent.HasPathTo(character.gridTileLocation) && !character.isInLimbo && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried())
				{
					return character;
				}
			}
		}
		else
		{
			Area areaLocation = actor.areaLocation;
			if (areaLocation != null)
			{
				Character firstCharacterInsideHexThatIsAliveHostileThatHasPathTo = areaLocation.locationCharacterTracker.GetFirstCharacterInsideHexThatIsAliveHostileThatHasPathTo(actor);
				if (firstCharacterInsideHexThatIsAliveHostileThatHasPathTo != null)
				{
					return firstCharacterInsideHexThatIsAliveHostileThatHasPathTo;
				}
			}
		}
		return null;
	}
}
