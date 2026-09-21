public class PlayerDevastationComponent
{
	public bool hasMeteorDevastation { get; private set; }

	public int currentTickMeteorDevastation { get; private set; }

	public int durationInTicksMeteorDevastation { get; private set; }

	public PlayerDevastationComponent()
	{
	}

	public PlayerDevastationComponent(SaveDataPlayerDevastationComponent data)
	{
		hasMeteorDevastation = data.hasMeteorDevastation;
		currentTickMeteorDevastation = data.currentTickMeteorDevastation;
		durationInTicksMeteorDevastation = data.durationInTicksMeteorDevastation;
	}

	public void OnTickEnded()
	{
		if (hasMeteorDevastation)
		{
			SpawnMeteorOnPlayerStructure();
			IncreaseCurrentTickMeteorDevastation();
			if (currentTickMeteorDevastation >= durationInTicksMeteorDevastation)
			{
				SetHasMeteorDevastation(p_state: false);
				ResetCurrentMeteorDevastationTicks();
			}
		}
	}

	public void SetHasMeteorDevastation(bool p_state)
	{
		if (hasMeteorDevastation != p_state)
		{
			hasMeteorDevastation = p_state;
		}
	}

	public void SetHasMeteorDevastation(bool p_state, int durationInTicks)
	{
		if (hasMeteorDevastation != p_state)
		{
			hasMeteorDevastation = p_state;
		}
		if (hasMeteorDevastation)
		{
			SetDurationInTicksMeteorDevastation(durationInTicks);
			ResetCurrentMeteorDevastationTicks();
		}
	}

	private void ResetCurrentMeteorDevastationTicks()
	{
		currentTickMeteorDevastation = 0;
	}

	private void SetDurationInTicksMeteorDevastation(int p_amount)
	{
		durationInTicksMeteorDevastation = p_amount;
	}

	private void IncreaseCurrentTickMeteorDevastation()
	{
		currentTickMeteorDevastation++;
	}

	private void SpawnMeteorOnPlayerStructure()
	{
		PlayerManager.Instance.player.playerSettlement.GetRandomStructure()?.GetRandomTile()?.AddNonPlayerMeteor();
	}
}
