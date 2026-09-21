using System;

namespace LapinerTools.Steam.Data;

public class EventArgsBase : EventArgs
{
	public bool IsError { get; set; }

	public string ErrorMessage { get; set; }

	public EventArgsBase()
	{
	}

	public EventArgsBase(EventArgsBase p_copyFromArgs)
	{
		if (p_copyFromArgs != null)
		{
			IsError = p_copyFromArgs.IsError;
			ErrorMessage = p_copyFromArgs.ErrorMessage;
		}
	}
}
