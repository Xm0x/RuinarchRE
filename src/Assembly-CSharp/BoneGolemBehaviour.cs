public class BoneGolemBehaviour : CharacterBehaviour
{
	public BoneGolemBehaviour()
	{
		base.priority = 8;
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
			return character.jobComponent.TriggerRoamAroundTile(out producedJob);
		}
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}

	private Character GetFirstHostileIntruder(Character actor)
	{
		if (actor.homeSettlement != null && actor.homeSettlement.region != null)
		{
			for (int i = 0; i < actor.homeSettlement.region.charactersAtLocation.Count; i++)
			{
				Character character = actor.homeSettlement.region.charactersAtLocation[i];
				if (CharacterManager.Instance.IsCharacterConsideredTargetOfBoneGolem(actor, character) && character.gridTileLocation.IsPartOfSettlement(actor.homeSettlement))
				{
					return character;
				}
			}
		}
		else if (actor.homeStructure != null)
		{
			for (int j = 0; j < actor.homeStructure.charactersHere.Count; j++)
			{
				Character character2 = actor.homeStructure.charactersHere[j];
				if (CharacterManager.Instance.IsCharacterConsideredTargetOfBoneGolem(actor, character2))
				{
					return character2;
				}
			}
		}
		else
		{
			Area areaLocation = actor.areaLocation;
			if (areaLocation != null)
			{
				Character firstCharacterInsideHexForBoneGolemBehaviour = areaLocation.locationCharacterTracker.GetFirstCharacterInsideHexForBoneGolemBehaviour(actor);
				if (firstCharacterInsideHexForBoneGolemBehaviour != null)
				{
					return firstCharacterInsideHexForBoneGolemBehaviour;
				}
			}
		}
		return null;
	}
}
