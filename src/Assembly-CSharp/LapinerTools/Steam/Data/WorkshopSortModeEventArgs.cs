namespace LapinerTools.Steam.Data;

public class WorkshopSortModeEventArgs : EventArgsBase
{
	public WorkshopSortMode SortMode { get; set; }

	public WorkshopSortModeEventArgs()
	{
	}

	public WorkshopSortModeEventArgs(WorkshopSortMode p_sortMode)
	{
		SortMode = p_sortMode;
	}

	public WorkshopSortModeEventArgs(EventArgsBase p_errorEventArgs)
		: base(p_errorEventArgs)
	{
	}
}
