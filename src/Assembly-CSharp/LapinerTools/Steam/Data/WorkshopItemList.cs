using System.Collections.Generic;

namespace LapinerTools.Steam.Data;

public class WorkshopItemList
{
	public uint Page { get; set; }

	public uint PagesItems { get; set; }

	public List<WorkshopItem> Items { get; set; }

	public uint PagesItemsFavorited { get; set; }

	public List<WorkshopItem> ItemsFavorited { get; set; }

	public uint PagesItemsVoted { get; set; }

	public List<WorkshopItem> ItemsVoted { get; set; }

	public WorkshopItemList()
	{
		Page = 1u;
		PagesItems = 1u;
		Items = new List<WorkshopItem>();
		PagesItemsFavorited = 1u;
		ItemsFavorited = new List<WorkshopItem>();
		PagesItemsVoted = 1u;
		ItemsVoted = new List<WorkshopItem>();
	}
}
