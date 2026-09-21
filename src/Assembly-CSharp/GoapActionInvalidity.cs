public class GoapActionInvalidity
{
	public bool isInvalid;

	public string stateName;

	public string reason;

	public string debugLog;

	public bool shouldLogInvalidity;

	public GoapActionInvalidity()
	{
		isInvalid = false;
		shouldLogInvalidity = true;
	}

	public bool IsReasonForCancellationShouldDropJob()
	{
		if (!string.IsNullOrEmpty(reason))
		{
			if (!(reason == "target_carried") && !(reason == "target_inactive") && !(reason == "target_dead"))
			{
				return reason == "already_being_removed";
			}
			return true;
		}
		return false;
	}

	public void AppendDebugLog(string p_log)
	{
		debugLog += p_log;
	}

	public void Reset()
	{
		isInvalid = false;
		stateName = string.Empty;
		reason = string.Empty;
		debugLog = string.Empty;
		shouldLogInvalidity = true;
	}
}
