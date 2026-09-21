namespace Plague.Transmission;

public class ConsumptionTransmission : Transmission<ConsumptionTransmission>
{
	public override PLAGUE_TRANSMISSION transmissionType => PLAGUE_TRANSMISSION.Consumption;

	protected override int GetTransmissionRate(int level)
	{
		return level switch
		{
			0 => 20, 
			1 => 20, 
			2 => 30, 
			3 => 40, 
			_ => 0, 
		};
	}

	protected override int GetTransmissionNextLevelCost(int p_currentLevel)
	{
		return p_currentLevel switch
		{
			0 => -1, 
			1 => 10, 
			2 => 50, 
			_ => -1, 
		};
	}

	public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl)
	{
		TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
	}
}
