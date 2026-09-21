using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class FactionTemplate
{
	public string name { get; private set; }

	public string factionTypeString { get; private set; }

	public Sprite factionEmblem { get; private set; }

	public List<VillageSetting> villageSettings { get; }

	public FACTION_TYPE factionType { get; private set; }

	public FactionTemplate(int p_villageCount)
	{
		name = RandomNameGenerator.GenerateFactionName();
		factionTypeString = FACTION_TYPE.Human_Empire.ToStringEnum();
		factionType = FACTION_TYPE.Human_Empire;
		villageSettings = new List<VillageSetting>();
		for (int i = 0; i < p_villageCount; i++)
		{
			villageSettings.Add(VillageSetting.Default);
		}
	}

	public void AddVillageSetting(VillageSetting p_villageSetting)
	{
		villageSettings.Add(p_villageSetting);
	}

	public void SetFactionEmblem(Sprite p_emblem)
	{
		factionEmblem = p_emblem;
	}

	public void ChangeFactionType(int p_index)
	{
		FACTION_TYPE p_type = ((p_index < GameUtilities.customWorldFactionTypeChoices.Length) ? GameUtilities.customWorldFactionTypeChoices[p_index] : FACTION_TYPE.None);
		factionTypeString = p_type.ToStringEnum();
		SetFactionType(p_type);
	}

	public void SetFactionType(FACTION_TYPE p_factionType)
	{
		factionType = p_factionType;
	}

	public void ChangeName(string p_newName)
	{
		Debug.Log("Changed name of " + name + " to " + p_newName);
		name = p_newName;
	}
}
