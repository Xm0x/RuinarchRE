namespace Plague.Transmission;

public interface IPlagueTransmissionListener
{
	void OnPlagueTransmitted(IPointOfInterest p_target);
}
