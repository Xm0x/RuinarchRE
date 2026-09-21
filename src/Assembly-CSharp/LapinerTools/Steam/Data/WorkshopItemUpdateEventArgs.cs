namespace LapinerTools.Steam.Data;

public class WorkshopItemUpdateEventArgs : EventArgsBase
{
	public WorkshopItemUpdate Item { get; set; }

	public WorkshopItemUpdateEventArgs()
	{
	}

	public WorkshopItemUpdateEventArgs(EventArgsBase p_errorEventArgs)
		: base(p_errorEventArgs)
	{
	}
}
