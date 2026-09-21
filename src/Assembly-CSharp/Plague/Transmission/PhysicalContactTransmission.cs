namespace Plague.Transmission;

public class PhysicalContactTransmission : Transmission<PhysicalContactTransmission>
{
	public override PLAGUE_TRANSMISSION transmissionType => PLAGUE_TRANSMISSION.Physical_Contact;

	protected override int GetTransmissionRate(int level)
	{
		return level switch
		{
			0 => 0, 
			1 => 12, 
			2 => 24, 
			3 => 36, 
			_ => 0, 
		};
	}

	protected override int GetTransmissionNextLevelCost(int p_currentLevel)
	{
		return p_currentLevel switch
		{
			0 => 20, 
			1 => 40, 
			2 => 60, 
			_ => -1, 
		};
	}

	public override void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl)
	{
		TryTransmitToSingleTarget(p_infector, p_target, p_transmissionLvl);
	}
}
