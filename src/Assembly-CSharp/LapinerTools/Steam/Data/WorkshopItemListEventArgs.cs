namespace LapinerTools.Steam.Data;

public class WorkshopItemListEventArgs : EventArgsBase
{
	public WorkshopItemList ItemList { get; set; }

	public WorkshopItemListEventArgs()
	{
	}

	public WorkshopItemListEventArgs(EventArgsBase p_errorEventArgs)
		: base(p_errorEventArgs)
	{
	}
}
