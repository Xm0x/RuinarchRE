using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.Data.Internal;
using Steamworks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UtilityScripts;

public class ExternalFileManager : MonoBehaviour
{
	public static ExternalFileManager Instance;

	private List<string> _subscribedModFolders;

	private Action _onAfterInitialization;

	private bool _hasInitialized;

	private Dictionary<string, CharacterClass> _classesCollectionFromFile;

	private Dictionary<string, List<string>> _villagerHairCollection;

	private Dictionary<string, CharacterSpritesPerAnimation> _specialCharacterSpriteCollection;

	private CharacterPortraitRaceCollection _villagerPortraitCollectionFromFile;

	private string _installedModsXmlPath;

	private XmlDocument _installedModsXML;

	private List<InstalledModData> _installedModCollection = new List<InstalledModData>(10);

	public bool hasInitialized => _hasInitialized;

	public event Action<InstalledModData> OnModAddedToCollection;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnSubscribed += OnModSubscribed;
			SteamWorkshopMain.Instance.OnInstalled += OnModInstalled;
		}
	}

	private void OnDestroy()
	{
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnSubscribed -= OnModSubscribed;
			SteamWorkshopMain.Instance.OnInstalled -= OnModInstalled;
		}
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
	}

	public void InitializeAllFiles(Action p_callbackAfterInitialization)
	{
		_classesCollectionFromFile = new Dictionary<string, CharacterClass>();
		_onAfterInitialization = p_callbackAfterInitialization;
		StartCoroutine(InitializeAllFilesWithDelay());
	}

	private IEnumerator InitializeAllFilesWithDelay()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Initializing_External_Data");
		MainMenuUI.Instance.ShowHeaderText(localizedValue);
		yield return GameUtilities.waitForHalfSecond;
		ConstructInstalledModsData();
		LoadCoreVillagerClasses();
		LoadCoreMonsterClasses();
		LoadCoreSpecialCharacterSprites();
		LoadCoreVillagerHairs();
		LoadCoreVillagerPortraits();
		LoadAllMods();
		LoadAllVillagerPortraitSprites();
		MainMenuUI.Instance.InitializeLatestSaveFile();
		yield return StartCoroutine(LoadAllCharacterTexturesIntoAtlas());
		_onAfterInitialization?.Invoke();
		MainMenuUI.Instance.HideHeaderText();
		_hasInitialized = true;
	}

	private void OnLocaleChanged(Locale p_obj)
	{
		if (_classesCollectionFromFile == null)
		{
			return;
		}
		foreach (KeyValuePair<string, CharacterClass> item in _classesCollectionFromFile)
		{
			item.Value.OnLocaleChanged(p_obj);
		}
	}

	private void LoadCoreVillagerClasses()
	{
		string text = Utilities.coreStreamingDataPath + "/VillagerClasses.xml";
		if (File.Exists(text))
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(text);
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/VillagerClasses/ClassItem");
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				XmlNode xmlNode = xmlNodeList[i];
				string value = xmlNode.Attributes["ID"].Value;
				CharacterClass value2 = new CharacterClass(value, xmlNode);
				_classesCollectionFromFile.Add(value, value2);
			}
		}
	}

	private void LoadCoreMonsterClasses()
	{
		string text = Utilities.coreStreamingDataPath + "/MonsterClasses.xml";
		if (File.Exists(text))
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(text);
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/MonsterClasses/ClassItem");
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				XmlNode xmlNode = xmlNodeList[i];
				string value = xmlNode.Attributes["ID"].Value;
				CharacterClass value2 = new CharacterClass(value, xmlNode);
				_classesCollectionFromFile.Add(value, value2);
			}
		}
	}

	private void LoadCoreVillagerHairs()
	{
		_villagerHairCollection = new Dictionary<string, List<string>>();
		TextureAtlas2D characterSpritesAtlas = TextureManager.Instance.characterSpritesAtlas;
		string text = Path.Combine(Utilities.coreStreamingDataPath, "VillagerHairs.xml");
		if (!File.Exists(text))
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(text);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/VillagerHairs/SpriteCollection");
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlNode xmlNode = xmlNodeList[i];
			string value = xmlNode.Attributes["ID"].Value;
			_villagerHairCollection.Add(value, new List<string>());
			XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("descendant::ImgSprite");
			for (int j = 0; j < xmlNodeList2.Count; j++)
			{
				XmlNode xmlNode2 = xmlNodeList2[j];
				string value2 = xmlNode2.Attributes["Path"].Value;
				string text2 = Path.Combine(Application.streamingAssetsPath, value2);
				if (File.Exists(text2))
				{
					string value3 = xmlNode2.Attributes["ID"].Value;
					_villagerHairCollection[value].Add(value3);
					if (characterSpritesAtlas.HasTextureID(value3))
					{
						characterSpritesAtlas.GetPackedTextureByKey(value3).SetPath(text2);
					}
					else
					{
						characterSpritesAtlas.AddTexturePathToAtlas(value3, text2, 100);
					}
				}
			}
		}
	}

	private void LoadCoreSpecialCharacterSprites()
	{
		_specialCharacterSpriteCollection = new Dictionary<string, CharacterSpritesPerAnimation>();
		TextureAtlas2D characterSpritesAtlas = TextureManager.Instance.characterSpritesAtlas;
		string text = Path.Combine(Utilities.coreStreamingDataPath, "SpecialCharacterSprites.xml");
		if (!File.Exists(text))
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(text);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/SpecialCharacterSprites/Character");
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlNode xmlNode = xmlNodeList[i];
			string value = xmlNode.Attributes["ID"].Value;
			_specialCharacterSpriteCollection.Add(value, new CharacterSpritesPerAnimation());
			XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("descendant::Animation");
			for (int j = 0; j < xmlNodeList2.Count; j++)
			{
				XmlNode xmlNode2 = xmlNodeList2[j];
				string value2 = xmlNode2.Attributes["ID"].Value;
				XmlNodeList xmlNodeList3 = xmlNode2.SelectNodes("descendant::ImgSprite");
				for (int k = 0; k < xmlNodeList3.Count; k++)
				{
					XmlNode xmlNode3 = xmlNodeList3[k];
					string value3 = xmlNode3.Attributes["Path"].Value;
					string text2 = Path.Combine(Application.streamingAssetsPath, value3);
					if (File.Exists(text2))
					{
						string value4 = xmlNode3.Attributes["ID"].Value;
						string text3 = value + "_" + value4;
						_specialCharacterSpriteCollection[value].Add(value2, value4, text3);
						characterSpritesAtlas.AddTexturePathToAtlas(text3, text2, 80);
					}
				}
			}
		}
	}

	private void LoadCoreVillagerPortraits()
	{
		_villagerPortraitCollectionFromFile = new CharacterPortraitRaceCollection();
		string text = Utilities.coreStreamingDataPath + "/VillagerPortraits.xml";
		if (!File.Exists(text))
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(text);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/VillagerPortraits/PortraitCollection");
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlNode xmlNode = xmlNodeList[i];
			if (!int.TryParse(xmlNode.Attributes["Race"].Value, out var result))
			{
				continue;
			}
			RACE p_race = (RACE)result;
			XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("descendant::SpriteCollection");
			if (xmlNodeList2 == null || xmlNodeList2.Count <= 0)
			{
				continue;
			}
			for (int j = 0; j < xmlNodeList2.Count; j++)
			{
				XmlNode xmlNode2 = xmlNodeList2[j];
				GENDER p_gender = (GENDER)int.Parse(xmlNode2.Attributes["Gender"].Value);
				HAIR_COLOR p_hairColor = (HAIR_COLOR)Enum.Parse(typeof(HAIR_COLOR), xmlNode2.Attributes["HairColor"].Value);
				string value = xmlNode2.Attributes["FolderPath"].Value;
				string text2 = Path.Combine(Application.streamingAssetsPath, value);
				if (Directory.Exists(text2))
				{
					_villagerPortraitCollectionFromFile.SetToCollection(p_race, p_gender, p_hairColor, text2);
				}
			}
		}
	}

	private void LoadAllMods()
	{
		for (int num = _installedModCollection.Count - 1; num >= 0; num--)
		{
			InstalledModData installedModData = _installedModCollection[num];
			if (installedModData.applyMod)
			{
				if (installedModData.HasTag("Character Class"))
				{
					string fullFolderPath = installedModData.fullFolderPath;
					string text = Path.Combine(fullFolderPath, "VillagerClasses.xml");
					if (File.Exists(text))
					{
						LoadClassFromFile("VillagerClasses", text, fullFolderPath);
					}
					text = Path.Combine(fullFolderPath, "MonsterClasses.xml");
					if (File.Exists(text))
					{
						LoadClassFromFile("MonsterClasses", text, fullFolderPath);
					}
				}
				else if (installedModData.HasTag("Villager Portraits"))
				{
					string fullFolderPath2 = installedModData.fullFolderPath;
					string text2 = Path.Combine(fullFolderPath2, "VillagerPortraits.xml");
					if (File.Exists(text2))
					{
						LoadVillagerPortraitFromFile(text2, fullFolderPath2);
					}
				}
			}
		}
	}

	private void LoadClassFromFile(string p_rootName, string p_fullPath, string p_folderPath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(p_fullPath);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/" + p_rootName + "/ClassItem");
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlNode xmlNode = xmlNodeList[i];
			string value = xmlNode.Attributes["ID"].Value;
			if (_classesCollectionFromFile.ContainsKey(value))
			{
				_classesCollectionFromFile[value].SetData(value, xmlNode, p_folderPath);
				continue;
			}
			CharacterClass value2 = new CharacterClass(value, xmlNode);
			_classesCollectionFromFile.Add(value, value2);
		}
	}

	private void LoadVillagerPortraitFromFile(string p_fullPath, string p_modPath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(p_fullPath);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/VillagerPortraits/PortraitCollection");
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlNode xmlNode = xmlNodeList[i];
			if (!int.TryParse(xmlNode.Attributes["Race"].Value, out var result))
			{
				continue;
			}
			RACE p_race = (RACE)result;
			XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("descendant::SpriteCollection");
			if (xmlNodeList2 == null || xmlNodeList2.Count <= 0)
			{
				continue;
			}
			for (int j = 0; j < xmlNodeList2.Count; j++)
			{
				XmlNode xmlNode2 = xmlNodeList2[j];
				GENDER p_gender = (GENDER)int.Parse(xmlNode2.Attributes["Gender"].Value);
				HAIR_COLOR p_hairColor = (HAIR_COLOR)Enum.Parse(typeof(HAIR_COLOR), xmlNode2.Attributes["HairColor"].Value);
				string value = xmlNode2.Attributes["FolderPath"].Value;
				string text = Path.Combine(p_modPath, value);
				if (Directory.Exists(text))
				{
					_villagerPortraitCollectionFromFile.SetToCollection(p_race, p_gender, p_hairColor, text);
				}
			}
		}
	}

	private void LoadAllVillagerPortraitSprites()
	{
		_villagerPortraitCollectionFromFile.LoadAllSpriteKeys();
	}

	private IEnumerator LoadAllCharacterTexturesIntoAtlas()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Loading_Character_Sprites");
		TextureAtlas2D spriteAtlas = TextureManager.Instance.characterSpritesAtlas;
		MainMenuUI.Instance.ShowHeaderText(localizedValue);
		yield return StartCoroutine(spriteAtlas.LoadTexturePathsFromFile());
		Rect[] p_rects = spriteAtlas.PackAtlasRaw();
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Packing_Atlas");
		yield return StartCoroutine(spriteAtlas.SetPackedAtlasData(p_rects, localizedValue2));
		spriteAtlas.ReleaseMemory();
		yield return GameUtilities.waitForHalfSecond;
	}

	public Dictionary<string, CharacterClass> GetCharacterClassCollection()
	{
		return _classesCollectionFromFile;
	}

	public Dictionary<string, List<string>> GetVillagerHairCollection()
	{
		return _villagerHairCollection;
	}

	public Dictionary<string, CharacterSpritesPerAnimation> GetSpecialCharacterSpriteCollection()
	{
		return _specialCharacterSpriteCollection;
	}

	public CharacterPortraitRaceCollection GetVillagerPortraitCollection()
	{
		return _villagerPortraitCollectionFromFile;
	}

	private void ConstructInstalledModsData()
	{
		_installedModsXmlPath = Application.persistentDataPath + "/InstalledMods.xml";
		if (_installedModsXML == null)
		{
			_installedModsXML = new XmlDocument();
		}
		if (File.Exists(_installedModsXmlPath))
		{
			_installedModsXML.Load(_installedModsXmlPath);
			XmlNodeList xmlNodeList = _installedModsXML.SelectNodes("/InstalledMods/ModInfo");
			for (int num = xmlNodeList.Count - 1; num >= 0; num--)
			{
				XmlNode xmlNode = xmlNodeList[num];
				InstalledModData installedModData = CreateDefaultInstalledModData();
				installedModData.publishFieldID = ulong.Parse(xmlNode.Attributes["ID"].Value);
				installedModData.fullFolderPath = xmlNode.Attributes["Path"].Value;
				installedModData.applyMod = bool.Parse(xmlNode.Attributes["Apply"].Value);
				installedModData.localID = xmlNode.Attributes["LocalID"].Value;
				installedModData.UpdateWorkshopItemInfo();
				if (Directory.Exists(installedModData.fullFolderPath))
				{
					AddToInstalledModCollection(installedModData);
				}
			}
			bool flag = false;
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				XmlNode xmlNode2 = xmlNodeList[i];
				if (!Directory.Exists(xmlNode2.Attributes["Path"].Value))
				{
					xmlNode2.ParentNode.RemoveChild(xmlNode2);
					flag = true;
				}
			}
			if (flag)
			{
				_installedModsXML.Save(_installedModsXmlPath);
			}
		}
		else
		{
			XmlElement xmlElement = _installedModsXML.CreateElement("InstalledMods");
			xmlElement.IsEmpty = false;
			_installedModsXML.AppendChild(xmlElement);
			_installedModsXML.Save(_installedModsXmlPath);
		}
		ConstructModsFromSteam();
	}

	private void ConstructModsFromSteam()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		if (numSubscribedItems == 0)
		{
			return;
		}
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		for (int num = array.Length - 1; num >= 0; num--)
		{
			PublishedFileId_t publishedFileId_t = array[num];
			if (SteamUGC.GetItemInstallInfo(publishedFileId_t, out var _, out var pchFolder, 260u, out var _))
			{
				InstalledModData installedModData = GetInstalledModData(publishedFileId_t.m_PublishedFileId);
				if (installedModData == null)
				{
					AddSteamModToCollectionAndXML(publishedFileId_t, pchFolder);
				}
				else
				{
					installedModData.SetPublishField(publishedFileId_t);
				}
			}
		}
	}

	private bool AddToInstalledModCollection(InstalledModData p_data)
	{
		bool flag = false;
		if (!((!p_data.isLocal) ? IsInInstalledModCollection(p_data.publishFieldID) : IsInInstalledModCollection(p_data.localID)))
		{
			if (_installedModCollection.Count > 0)
			{
				_installedModCollection.Insert(0, p_data);
			}
			else
			{
				_installedModCollection.Add(p_data);
			}
			return true;
		}
		return false;
	}

	private void RemoveFromInstalledModCollection(ulong p_key)
	{
		for (int i = 0; i < _installedModCollection.Count; i++)
		{
			if (_installedModCollection[i].publishFieldID == p_key)
			{
				_installedModCollection.RemoveAt(i);
				break;
			}
		}
	}

	private void RemoveFromInstalledModCollection(InstalledModData p_modData)
	{
		_installedModCollection.Remove(p_modData);
	}

	private bool IsInInstalledModCollection(ulong p_key)
	{
		for (int i = 0; i < _installedModCollection.Count; i++)
		{
			if (_installedModCollection[i].publishFieldID == p_key)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsInInstalledModCollection(string p_id)
	{
		for (int i = 0; i < _installedModCollection.Count; i++)
		{
			if (_installedModCollection[i].localID == p_id)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsInInstalledModCollection(InstalledModData p_data)
	{
		return _installedModCollection.Contains(p_data);
	}

	public InstalledModData GetInstalledModData(ulong p_key)
	{
		for (int i = 0; i < _installedModCollection.Count; i++)
		{
			InstalledModData installedModData = _installedModCollection[i];
			if (installedModData.publishFieldID == p_key)
			{
				return installedModData;
			}
		}
		return null;
	}

	public InstalledModData GetInstalledModData(string p_fullFolderPath)
	{
		for (int i = 0; i < _installedModCollection.Count; i++)
		{
			InstalledModData installedModData = _installedModCollection[i];
			if (installedModData.fullFolderPath == p_fullFolderPath)
			{
				return installedModData;
			}
		}
		return null;
	}

	private InstalledModData CreateDefaultInstalledModData()
	{
		return new InstalledModData
		{
			fullFolderPath = string.Empty,
			applyMod = false,
			localID = string.Empty,
			itemInfo = new WorkshopItemInfo()
		};
	}

	private void SaveModInfo(InstalledModData p_data)
	{
		bool flag = true;
		XmlNode xmlNode = ((!p_data.isLocal) ? _installedModsXML.SelectSingleNode($"/InstalledMods/ModInfo[@ID='{p_data.publishFieldID}']") : _installedModsXML.SelectSingleNode("/InstalledMods/ModInfo[@LocalID='" + p_data.localID + "']"));
		if (xmlNode != null)
		{
			xmlNode.Attributes["ID"].Value = p_data.publishFieldID.ToString();
			xmlNode.Attributes["Path"].Value = p_data.fullFolderPath.ToString();
			xmlNode.Attributes["Apply"].Value = p_data.applyMod.ToString();
			xmlNode.Attributes["LocalID"].Value = p_data.localID.ToString();
			flag = false;
			_installedModsXML.Save(_installedModsXmlPath);
		}
		if (flag)
		{
			XmlNode xmlNode2 = _installedModsXML.SelectSingleNode("InstalledMods");
			XmlElement xmlElement = _installedModsXML.CreateElement("ModInfo");
			xmlElement.SetAttribute("ID", p_data.publishFieldID.ToString());
			xmlElement.SetAttribute("Path", p_data.fullFolderPath);
			xmlElement.SetAttribute("Apply", p_data.applyMod.ToString());
			xmlElement.SetAttribute("LocalID", p_data.localID);
			xmlNode2.PrependChild(xmlElement);
			_installedModsXML.Save(_installedModsXmlPath);
		}
	}

	public void IncreasePrioritySaveModInfo(InstalledModData p_data)
	{
		XmlNode xmlNode = ((!p_data.isLocal) ? _installedModsXML.SelectSingleNode($"/InstalledMods/ModInfo[@ID='{p_data.publishFieldID}']") : _installedModsXML.SelectSingleNode("/InstalledMods/ModInfo[@LocalID='" + p_data.localID + "']"));
		if (xmlNode != null)
		{
			XmlNode parentNode = xmlNode.ParentNode;
			XmlNode previousSibling = xmlNode.PreviousSibling;
			parentNode.InsertBefore(xmlNode, previousSibling);
		}
	}

	private InstalledModData AddSteamModToCollectionAndXML(PublishedFileId_t p_publishFieldID, string p_fullFolderPath)
	{
		InstalledModData installedModData = CreateDefaultInstalledModData();
		installedModData.SetPublishField(p_publishFieldID);
		installedModData.publishFieldID = p_publishFieldID.m_PublishedFileId;
		installedModData.fullFolderPath = p_fullFolderPath;
		installedModData.UpdateWorkshopItemInfo();
		AddToInstalledModCollection(installedModData);
		SaveModInfo(installedModData);
		return installedModData;
	}

	private bool AddLocalModToCollectionAndXML(InstalledModData p_modData)
	{
		bool result = AddToInstalledModCollection(p_modData);
		SaveModInfo(p_modData);
		return result;
	}

	public void RemoveSteamModFromCollectionAndXML(ulong p_publishFieldID)
	{
		RemoveFromInstalledModCollection(p_publishFieldID);
		XmlNode xmlNode = _installedModsXML.SelectSingleNode("/InstalledMods/ModInfo[@ID='" + p_publishFieldID + "']");
		xmlNode?.ParentNode.RemoveChild(xmlNode);
		_installedModsXML.Save(_installedModsXmlPath);
	}

	public void RemoveLocalModFromCollectionAndXML(InstalledModData p_modData)
	{
		XmlNode xmlNode = _installedModsXML.SelectSingleNode("/InstalledMods/ModInfo[@LocalID='" + p_modData.localID + "']");
		xmlNode?.ParentNode.RemoveChild(xmlNode);
		_installedModsXML.Save(_installedModsXmlPath);
		RemoveFromInstalledModCollection(p_modData);
	}

	public void SaveAllExistingModInfos()
	{
		for (int i = 0; i < _installedModCollection.Count; i++)
		{
			InstalledModData installedModData = _installedModCollection[i];
			XmlNode xmlNode;
			if (installedModData.isLocal)
			{
				xmlNode = _installedModsXML.SelectSingleNode("/InstalledMods/ModInfo[@LocalID='" + installedModData.localID + "']");
			}
			else
			{
				string text = installedModData.publishFieldID.ToString();
				xmlNode = _installedModsXML.SelectSingleNode("/InstalledMods/ModInfo[@ID='" + text + "']");
			}
			if (xmlNode != null)
			{
				xmlNode.Attributes["Apply"].Value = installedModData.applyMod.ToString();
			}
		}
		_installedModsXML.Save(_installedModsXmlPath);
	}

	public List<InstalledModData> GetInstalledModCollection()
	{
		return _installedModCollection;
	}

	public void SaveToWorkshopItemInfo(InstalledModData p_modData)
	{
		using FileStream stream = new FileStream(Path.Combine(p_modData.fullFolderPath, "WorkshopItemInfo.xml"), FileMode.Create);
		new XmlSerializer(typeof(WorkshopItemInfo)).Serialize(stream, p_modData.itemInfo);
	}

	public void OnLocalModSubscribed(InstalledModData p_modData)
	{
		SaveToWorkshopItemInfo(p_modData);
		AddLocalModToCollectionAndXML(p_modData);
		this.OnModAddedToCollection?.Invoke(p_modData);
	}

	public void OnLocalModUnsubscribed(InstalledModData modData)
	{
		RemoveLocalModFromCollectionAndXML(modData);
	}

	private void OnModSubscribed(WorkshopItemEventArgs p_itemArgs)
	{
		OnModInstalled(p_itemArgs);
	}

	private void OnModInstalled(WorkshopItemEventArgs p_itemArgs)
	{
		WorkshopItem item = p_itemArgs.Item;
		if (item.IsInstalled)
		{
			InstalledModData obj = AddSteamModToCollectionAndXML(item.SteamNative.m_nPublishedFileId, item.InstalledLocalFolder);
			this.OnModAddedToCollection?.Invoke(obj);
		}
	}

	private void OnModUnsubscribed(WorkshopItemEventArgs p_itemArgs)
	{
		WorkshopItem item = p_itemArgs.Item;
		RemoveSteamModFromCollectionAndXML(item.SteamNative.m_nPublishedFileId.m_PublishedFileId);
	}

	public void OnModUnsubscribed(ulong p_publishedFieldID)
	{
		RemoveSteamModFromCollectionAndXML(p_publishedFieldID);
	}
}
