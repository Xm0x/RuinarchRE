using Inner_Maps.Location_Structures;

public class StructureProtectorBehaviour : CharacterBehaviour
{
	public StructureProtectorBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Character character2 = null;
		if (character.homeStructure != null)
		{
			character2 = GetFirstHostileIntruderOf(character, character.homeStructure);
		}
		if (character2 != null)
		{
			character.combatComponent.Fight(character2, "Defending_Home");
			return true;
		}
		return character.jobComponent.TriggerRoamAroundTile(out producedJob);
	}

	private Character GetFirstHostileIntruderOf(Character actor, LocationStructure p_structure)
	{
		return p_structure.GetFirstCharacterInsideStructureThatIsAliveHostileThatHasPathTo(actor);
	}
}
