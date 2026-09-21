namespace Plague.Transmission;

public class AirborneTransmission : Transmission<AirborneTransmission>
{
	public override PLAGUE_TRANSMISSION transmissionType => PLAGUE_TRANSMISSION.Airborne;

	protected override int GetTransmissionRate(int level)
	{
		return level switch
		{
			0 => 0, 
			1 => 30, 
			2 => 40, 
			3 => 50, 
			_ => 0, 
		};
	}

	protected override int GetTransmissionNextLevelCost(int p_currentLevel)
	{
		return p_currentLevel switch
		{
			0 => 10, 
			1 => 30, 
			2 => 60, 
			_ => -1, 
		};
	}

	public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl)
	{
		TryTransmitToInRange(p_infector, p_transmissionLvl);
	}
}
