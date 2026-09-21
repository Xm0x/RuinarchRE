using Locations.Settlements;

public class SettlementProtectorBehaviour : CharacterBehaviour
{
	public SettlementProtectorBehaviour()
	{
		base.priority = 19;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Character character2 = null;
		if (character.homeSettlement != null)
		{
			character2 = GetFirstHostileIntruderOfHome(character, character.homeSettlement);
		}
		if (character2 != null)
		{
			character.combatComponent.Fight(character2, "Defending_Home");
			return true;
		}
		return character.jobComponent.TriggerRoamAroundTile(out producedJob);
	}

	private Character GetFirstHostileIntruderOfHome(Character actor, BaseSettlement homeSettlement)
	{
		for (int i = 0; i < homeSettlement.areas.Count; i++)
		{
			Area p_area = homeSettlement.areas[i];
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
}
