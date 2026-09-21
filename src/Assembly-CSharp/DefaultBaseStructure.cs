using UnityEngine;

public class DefaultBaseStructure : CharacterBehaviour
{
	public DefaultBaseStructure()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.trapStructure.IsTrappedAndTrapStructureIs(character.currentStructure) && !character.trapStructure.IsTrappedInArea())
		{
			if (Random.Range(0, 100) < 15 && !character.isConversing && character.marker.inVisionCharacters.Count > 0)
			{
				bool flag = false;
				for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
				{
					Character character2 = character.marker.inVisionCharacters[i];
					if (!character2.isConversing && character.nonActionEventsComponent.CanChat(character2) && character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, character2))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
			TileObject unoccupiedBuiltTileObject = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
			if (unoccupiedBuiltTileObject != null)
			{
				character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, unoccupiedBuiltTileObject, out producedJob);
				return true;
			}
			character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
			return true;
		}
		return false;
	}
}
