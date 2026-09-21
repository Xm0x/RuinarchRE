using System;
using Steamworks;

namespace LapinerTools.Steam.Data;

public class WorkshopItem
{
	public class SteamNativeData
	{
		public PublishedFileId_t m_nPublishedFileId { get; set; }

		public SteamUGCDetails_t m_details { get; set; }

		public EItemState m_itemState { get; set; }

		public SteamNativeData()
		{
		}

		public SteamNativeData(PublishedFileId_t p_nPublishedFileId)
		{
			m_nPublishedFileId = p_nPublishedFileId;
		}
	}

	public string Name { get; set; }

	public string Description { get; set; }

	public string OwnerName { get; set; }

	public string PreviewImageURL { get; set; }

	public uint VotesUp { get; set; }

	public uint VotesDown { get; set; }

	public ulong Subscriptions { get; set; }

	public ulong Favorites { get; set; }

	public bool IsSubscribed { get; set; }

	public bool IsFavorited { get; set; }

	public bool IsVotedUp { get; set; }

	public bool IsVotedDown { get; set; }

	public bool IsVoteSkipped { get; set; }

	public bool IsOwned { get; set; }

	public bool IsInstalled { get; set; }

	public bool IsDownloading { get; set; }

	public bool IsUpdateNeeded { get; set; }

	public string InstalledLocalFolder { get; set; }

	public ulong InstalledSizeOnDisk { get; set; }

	public DateTime InstalledTimestamp { get; set; }

	public SteamNativeData SteamNative { get; set; }

	public WorkshopItem()
	{
		SteamNative = new SteamNativeData();
	}
}
