namespace Plague.Transmission;

public class CombatRateTransmission : Transmission<CombatRateTransmission>
{
	public override PLAGUE_TRANSMISSION transmissionType => PLAGUE_TRANSMISSION.Combat;

	protected override int GetTransmissionRate(int level)
	{
		return level switch
		{
			0 => 0, 
			1 => 10, 
			2 => 20, 
			3 => 30, 
			_ => 0, 
		};
	}

	protected override int GetTransmissionNextLevelCost(int p_currentLevel)
	{
		return p_currentLevel switch
		{
			0 => 10, 
			1 => 20, 
			2 => 30, 
			_ => -1, 
		};
	}

	public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl)
	{
		TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
	}
}
