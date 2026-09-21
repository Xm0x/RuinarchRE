namespace LapinerTools.Steam.Data;

public class WorkshopItemEventArgs : EventArgsBase
{
	public WorkshopItem Item { get; set; }

	public WorkshopItemEventArgs()
	{
	}

	public WorkshopItemEventArgs(WorkshopItem p_item)
	{
		Item = p_item;
	}

	public WorkshopItemEventArgs(EventArgsBase p_errorEventArgs)
		: base(p_errorEventArgs)
	{
	}
}
