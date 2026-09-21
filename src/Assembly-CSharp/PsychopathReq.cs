using System.Collections.Generic;

public class PsychopathReq
{
	public SERIAL_VICTIM_TYPE victimType;

	public List<string> victimDescriptions;

	public string text;

	public PsychopathReq(SERIAL_VICTIM_TYPE victimType, List<string> victimDescriptions, string text)
	{
		this.victimType = victimType;
		this.victimDescriptions = new List<string>(victimDescriptions);
		this.text = text;
	}
}
