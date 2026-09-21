using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class FactionSettings
{
	public List<FactionTemplate> factionTemplates;

	public bool disableNewFactions;

	public bool disableFactionIdeologyChanges;

	public FactionSettings()
	{
		factionTemplates = new List<FactionTemplate>();
	}

	public int GetCurrentTotalVillageCountBasedOnFactions()
	{
		int num = 0;
		for (int i = 0; i < factionTemplates.Count; i++)
		{
			num += factionTemplates[i].villageSettings.Count;
		}
		return num;
	}

	public FactionTemplate AddFactionSetting(int p_villageCount)
	{
		FactionTemplate factionTemplate = new FactionTemplate(p_villageCount);
		AddFactionSetting(factionTemplate);
		return factionTemplate;
	}

	private void AddFactionSetting(FactionTemplate p_FactionTemplate)
	{
		factionTemplates.Add(p_FactionTemplate);
	}

	public void RemoveFactionSetting(FactionTemplate p_FactionTemplate)
	{
		factionTemplates.Remove(p_FactionTemplate);
	}

	public void ClearFactionSettings()
	{
		factionTemplates.Clear();
	}

	public void AllowNewFactions()
	{
		disableNewFactions = false;
	}

	public void BlockNewFactions()
	{
		disableNewFactions = true;
	}

	public void AllowFactionIdeologyChanges()
	{
		disableFactionIdeologyChanges = false;
	}

	public void BlockFactionIdeologyChanges()
	{
		disableFactionIdeologyChanges = true;
	}

	public void FinalizeFactionTemplates()
	{
		for (int i = 0; i < factionTemplates.Count; i++)
		{
			FactionTemplate factionTemplate = factionTemplates[i];
			if (factionTemplate.factionType == FACTION_TYPE.None)
			{
				factionTemplate.SetFactionType(GameUtilities.RollChance(50) ? FACTION_TYPE.Elven_Kingdom : FACTION_TYPE.Human_Empire);
				Debug.Log(factionTemplate.name + " random faction type set to " + factionTemplate.factionType.ToStringEnum());
			}
		}
	}
}
