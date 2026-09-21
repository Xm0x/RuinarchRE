namespace Plague.Transmission;

public static class TransmissionExtensions
{
	public static string GetTransmissionTooltip(this PLAGUE_TRANSMISSION p_transmissionType)
	{
		return LocalizationManager.Instance.GetLocalizedValue("Plague_Table", p_transmissionType.ToStringEnum() + "_Tooltip");
	}
}
