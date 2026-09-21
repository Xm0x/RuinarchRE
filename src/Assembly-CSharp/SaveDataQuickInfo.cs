using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataQuickInfo
{
	public string saveVersion;

	public string scenarioName;

	public OMNIPOTENT_MODE omnipotentMode;

	public PLAYER_ARCHETYPE archetype;

	public int portalLevel;

	public List<SaveDataInstalledModData> appliedMods;

	public void SaveAppliedMods()
	{
		appliedMods = new List<SaveDataInstalledModData>();
		List<InstalledModData> installedModCollection = ExternalFileManager.Instance.GetInstalledModCollection();
		for (int i = 0; i < installedModCollection.Count; i++)
		{
			InstalledModData installedModData = installedModCollection[i];
			if (installedModData.applyMod)
			{
				if (installedModData.isLocal)
				{
					appliedMods.Add(new SaveDataInstalledModData
					{
						localID = installedModData.localID,
						isLocal = true
					});
				}
				else
				{
					appliedMods.Add(new SaveDataInstalledModData
					{
						publishedFileID = installedModData.itemInfo.PublishedFileId,
						isLocal = false
					});
				}
			}
		}
	}

	public bool IsModInSaveFile(InstalledModData p_modData)
	{
		if (p_modData.isLocal)
		{
			for (int i = 0; i < appliedMods.Count; i++)
			{
				SaveDataInstalledModData saveDataInstalledModData = appliedMods[i];
				if (saveDataInstalledModData.isLocal && saveDataInstalledModData.localID == p_modData.localID)
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < appliedMods.Count; j++)
			{
				SaveDataInstalledModData saveDataInstalledModData2 = appliedMods[j];
				if (!saveDataInstalledModData2.isLocal && saveDataInstalledModData2.publishedFileID == p_modData.itemInfo.PublishedFileId)
				{
					return true;
				}
			}
		}
		return false;
	}
}
