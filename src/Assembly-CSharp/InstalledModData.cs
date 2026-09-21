using System.IO;
using System.Xml;
using LapinerTools.Steam.Data.Internal;
using Steamworks;

public class InstalledModData
{
	public ulong publishFieldID;

	public string fullFolderPath;

	public bool applyMod;

	public WorkshopItemInfo itemInfo;

	public string localID;

	public PublishedFileId_t publishedField { get; private set; }

	public bool isLocal => !string.IsNullOrEmpty(localID);

	public bool HasTag(string p_tag)
	{
		if (itemInfo.Tags != null)
		{
			for (int i = 0; i < itemInfo.Tags.Length; i++)
			{
				if (itemInfo.Tags[i] == p_tag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetPublishField(PublishedFileId_t p_publishField)
	{
		publishedField = p_publishField;
	}

	public void UpdateWorkshopItemInfo()
	{
		string text = Path.Combine(fullFolderPath, "WorkshopItemInfo.xml");
		if (File.Exists(text))
		{
			ParseItemInfo(text, itemInfo);
		}
	}

	private void ParseItemInfo(string p_path, WorkshopItemInfo p_itemInfo)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(p_path);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("WorkshopItemInfo");
		p_itemInfo.PublishedFileId = ulong.Parse(xmlNode.SelectSingleNode("descendant::PublishedFileId").InnerText);
		p_itemInfo.Name = xmlNode.SelectSingleNode("descendant::Name").InnerText;
		p_itemInfo.Description = xmlNode.SelectSingleNode("descendant::Description").InnerText;
		p_itemInfo.IconFileName = xmlNode.SelectSingleNode("descendant::IconFileName").InnerText;
		p_itemInfo.AuthorName = xmlNode.SelectSingleNode("descendant::AuthorName").InnerText;
		p_itemInfo.OwnerLocalPath = xmlNode.SelectSingleNode("descendant::OwnerLocalPath").InnerText;
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("descendant::Tags");
		if (xmlNode2 != null)
		{
			XmlNodeList xmlNodeList = xmlNode2.SelectNodes("descendant::string");
			p_itemInfo.Tags = new string[xmlNodeList.Count];
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				XmlNode xmlNode3 = xmlNodeList[i];
				p_itemInfo.Tags[i] = xmlNode3.InnerText;
			}
		}
	}
}
